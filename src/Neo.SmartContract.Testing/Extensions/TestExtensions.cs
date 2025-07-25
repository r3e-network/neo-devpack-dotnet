// Copyright (C) 2015-2025 The Neo Project.
//
// TestExtensions.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.Cryptography.ECC;
using Neo.SmartContract.Testing.Attributes;
using Neo.SmartContract.Testing.Interpreters;
using Neo.VM.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace Neo.SmartContract.Testing.Extensions
{
    public static class TestExtensions
    {
        private static readonly Dictionary<Type, Dictionary<int, PropertyInfo>> _propertyCache = new();
        private static readonly Dictionary<Type, FieldInfo[]> _fieldCache = new();

        /// <summary>
        /// Convert Array stack item to dotnet array
        /// </summary>
        /// <param name="state">Item</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Object</returns>
        public static object?[]? ConvertTo(this VM.Types.Array state, ParameterInfo[] parameters)
        {
            return ConvertTo(state, parameters, StringInterpreter.StrictUTF8);
        }

        /// <summary>
        /// Convert Array stack item to dotnet array
        /// </summary>
        /// <param name="state">Item</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="stringInterpreter">String interpreter</param>
        /// <returns>Object</returns>
        public static object?[]? ConvertTo(this VM.Types.Array state, ParameterInfo[] parameters, IStringInterpreter stringInterpreter)
        {
            if (parameters.Length > 0)
            {
                object?[] args = new object[parameters.Length];

                for (int x = 0; x < parameters.Length; x++)
                {
                    args[x] = state[x].ConvertTo(parameters[x].ParameterType, stringInterpreter);
                }

                return args;
            }

            return null;
        }

        /// <summary>
        /// Convert stack item to dotnet
        /// </summary>
        /// <param name="stackItem">Item</param>
        /// <param name="type">Type</param>
        /// <returns>Object</returns>
        public static object? ConvertTo(this StackItem stackItem, Type type)
        {
            return ConvertTo(stackItem, type, StringInterpreter.StrictUTF8);
        }

        /// <summary>
        /// Convert stack item to dotnet
        /// </summary>
        /// <param name="stackItem">Item</param>
        /// <param name="type">Type</param>
        /// <param name="stringInterpreter">String interpreter</param>
        /// <returns>Object</returns>
        public static object? ConvertTo(this StackItem stackItem, Type type, IStringInterpreter stringInterpreter)
        {
            if (stackItem is null || stackItem.IsNull) return null;

            return type switch
            {
                _ when type == stackItem.GetType() => stackItem,
                _ when type == typeof(object) => stackItem,
                _ when type == typeof(string) => stringInterpreter.GetString(stackItem.GetSpan()),
                _ when type == typeof(byte[]) => stackItem.GetSpan().ToArray(),

                _ when type == typeof(bool) || type == typeof(bool?) => stackItem.GetBoolean(),
                _ when type == typeof(byte) || type == typeof(byte?) => (byte)stackItem.GetInteger(),
                _ when type == typeof(sbyte) || type == typeof(sbyte?) => (sbyte)stackItem.GetInteger(),
                _ when type == typeof(short) || type == typeof(short?) => (short)stackItem.GetInteger(),
                _ when type == typeof(ushort) || type == typeof(ushort?) => (ushort)stackItem.GetInteger(),
                _ when type == typeof(int) || type == typeof(int?) => (int)stackItem.GetInteger(),
                _ when type == typeof(uint) || type == typeof(uint?) => (uint)stackItem.GetInteger(),
                _ when type == typeof(long) || type == typeof(long?) => (long)stackItem.GetInteger(),
                _ when type == typeof(ulong) || type == typeof(ulong?) => (ulong)stackItem.GetInteger(),

                _ when type.IsEnum => Enum.ToObject(type, (int)stackItem.GetInteger()),
                _ when type == typeof(BigInteger) || type == typeof(BigInteger?) => stackItem.GetInteger(),
                _ when type == typeof(UInt160) => new UInt160(stackItem.GetSpan().ToArray()),
                _ when type == typeof(UInt256) => new UInt256(stackItem.GetSpan().ToArray()),
                _ when type == typeof(ECPoint) => ECPoint.FromBytes(stackItem.GetSpan().ToArray(), ECCurve.Secp256r1),
                _ when typeof(IInteroperable).IsAssignableFrom(type) => CreateInteroperable(stackItem, type),
                _ when stackItem is InteropInterface it && it.GetInterface<object>().GetType() == type => it.GetInterface<object>(),

                _ when stackItem is VM.Types.Array ar => type switch
                {
                    _ when
                        type == typeof(IList<object>) || type == typeof(IList<object?>) ||
                        type == typeof(List<object>) || type == typeof(List<object?>)
                        => new List<object?>(ar.SubItems.Select(ConvertToBaseValue)), // SubItems in StackItem type except bool, buffer and int
                    _ when type.IsArray => CreateTypeArray(ar.SubItems, type.GetElementType()!, stringInterpreter),
                    _ when type.IsClass => CreateObject(ar.SubItems, type, stringInterpreter),
                    _ when type.IsValueType => CreateValueType(ar.SubItems, type, stringInterpreter),
                    _ => throw new FormatException($"Impossible to convert {stackItem} to {type}"),
                },
                _ when stackItem is Map mp => type switch
                {
                    _ when
                        type == typeof(IDictionary<object, object>) ||
                        type == typeof(Dictionary<object, object>)
                        => ToDictionary(mp), // SubItems in StackItem type except bool, buffer and int
                    _ => throw new FormatException($"Impossible to convert {stackItem} to {type}"),
                },

                _ => throw new FormatException($"Impossible to convert {stackItem} to {type}"),
            };
        }

        private static object? ConvertToBaseValue(StackItem u)
        {
            if (u is Null) return null;
            if (u is Integer i) return i.GetInteger();
            if (u is VM.Types.Boolean b) return b.GetBoolean();
            if (u is VM.Types.Buffer bf) return bf.GetSpan().ToArray();
            // if (u is ByteString s) return s.GetString(); // it could be a byte[]

            return u;
        }

        private static object CreateObject(IEnumerable<StackItem> subItems, Type type, IStringInterpreter stringInterpreter)
        {
            var index = 0;
            var obj = Activator.CreateInstance(type) ?? throw new FormatException($"Impossible create {type}");

            // Cache the object properties by offset

            if (!_propertyCache.TryGetValue(type, out var cache))
            {
                cache = new Dictionary<int, PropertyInfo>();

                foreach (var property in type.GetProperties())
                {
                    var fieldOffset = property.GetCustomAttribute<FieldOrderAttribute>();
                    if (fieldOffset is null) continue;
                    if (!property.CanWrite) continue;

                    cache.Add(fieldOffset.Order, property);
                }

                if (cache.Count == 0)
                {
                    // Without FieldOrderAttribute, by order

                    foreach (var property in type.GetProperties())
                    {
                        if (!property.CanWrite) continue;
                        cache.Add(index, property);
                        index++;
                    }
                    index = 0;
                }

                _propertyCache[type] = cache;
            }

            // Fill the object

            foreach (var item in subItems)
            {
                if (cache.TryGetValue(index, out var property))
                {
                    property.SetValue(obj, ConvertTo(item, property.PropertyType, stringInterpreter));
                }
                else
                {
                    throw new FormatException($"Error converting {type}, the property with the offset {index} was not found.");
                }

                index++;
            }

            return obj;
        }

        private static IDictionary<object, object> ToDictionary(Map map)
        {
            Dictionary<object, object> dictionary = new();

            foreach (var entry in map)
            {
                dictionary.Add(ConvertToBaseValue(entry.Key)!, ConvertToBaseValue(entry.Value)!);
            }

            return dictionary;
        }

        private static object CreateValueType(IEnumerable<StackItem> objects, Type valueType, IStringInterpreter stringInterpreter)
        {
            var arr = objects.ToArray();
            var value = Activator.CreateInstance(valueType) ?? new NoNullAllowedException("Impossible create value type");

            // Cache the object properties by offset

            if (!_fieldCache.TryGetValue(valueType, out var cache))
            {
                cache = valueType.GetFields().ToArray();
                _fieldCache[valueType] = cache;
            }

            if (cache.Length != arr.Length)
            {
                throw new FormatException($"Error converting {valueType}, field count doesn't match.");
            }

            for (int x = 0; x < arr.Length; x++)
            {
                cache[x].SetValue(value, ConvertTo(arr[x], cache[x].FieldType, stringInterpreter));
            }

            return value;
        }

        private static System.Array CreateTypeArray(IEnumerable<StackItem> objects, Type elementType, IStringInterpreter stringInterpreter)
        {
            var obj = objects.ToArray();

            if (elementType != typeof(object))
            {
                var arr = System.Array.CreateInstance(elementType, obj.Length);

                for (int x = 0; x < arr.Length; x++)
                {
                    arr.SetValue(ConvertTo(obj[x], elementType, stringInterpreter), x);
                }

                return arr;
            }

            return obj;
        }

        private static IInteroperable CreateInteroperable(StackItem stackItem, Type type)
        {
            var interoperable = (IInteroperable)Activator.CreateInstance(type)!;
            interoperable.FromStackItem(stackItem);
            return interoperable;
        }
    }
}
