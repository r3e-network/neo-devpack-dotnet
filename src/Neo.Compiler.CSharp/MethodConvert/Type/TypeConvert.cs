// Copyright (C) 2015-2024 The Neo Project.
//
// TypeConvert.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

extern alias scfx;

using Microsoft.CodeAnalysis;
using Neo.VM;
using System;

namespace Neo.Compiler;

internal static class TypeConvert
{
    /// <summary>
    /// Converts a type to another type.
    /// </summary>
    /// <param name="convert">The method convert instance.</param>
    /// <param name="model">The semantic model.</param>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>True if the conversion was successful; otherwise, false.</returns>
    public static bool ConvertType(MethodConvert convert, SemanticModel model, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // Handle enum conversions
        if (sourceType.TypeKind == TypeKind.Enum || targetType.TypeKind == TypeKind.Enum)
        {
            return ConvertEnumType(convert, model, sourceType, targetType);
        }

        // Handle record type conversions
        if (sourceType is INamedTypeSymbol sourceNamedType && sourceNamedType.IsRecord ||
            targetType is INamedTypeSymbol targetNamedType && targetNamedType.IsRecord)
        {
            return RecordTypeConvert.ConvertType(convert, model, sourceType, targetType);
        }

        return false;
    }

    /// <summary>
    /// Converts an enum type to another type.
    /// </summary>
    /// <param name="convert">The method convert instance.</param>
    /// <param name="model">The semantic model.</param>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>True if the conversion was successful; otherwise, false.</returns>
    private static bool ConvertEnumType(MethodConvert convert, SemanticModel model, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // If the source type is an enum
        if (sourceType.TypeKind == TypeKind.Enum)
        {
            // Get the underlying type of the enum
            INamedTypeSymbol enumType = (INamedTypeSymbol)sourceType;
            ITypeSymbol underlyingType = enumType.EnumUnderlyingType;

            // If the target type is the underlying type of the enum, no conversion is needed
            if (SymbolEqualityComparer.Default.Equals(targetType, underlyingType))
                return true;

            // If the target type is a numeric type, convert the enum to its underlying type first
            if (IsNumericType(targetType))
            {
                // No additional conversion needed for Neo VM as enums are stored as their underlying values
                return true;
            }
        }
        // If the target type is an enum
        else if (targetType.TypeKind == TypeKind.Enum)
        {
            // Get the underlying type of the enum
            INamedTypeSymbol enumType = (INamedTypeSymbol)targetType;
            ITypeSymbol underlyingType = enumType.EnumUnderlyingType;

            // If the source type is the underlying type of the enum, no conversion is needed
            if (SymbolEqualityComparer.Default.Equals(sourceType, underlyingType))
                return true;

            // If the source type is a numeric type, convert it to the underlying type of the enum
            if (IsNumericType(sourceType))
            {
                // No additional conversion needed for Neo VM as enums are stored as their underlying values
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if a type is a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a numeric type; otherwise, false.</returns>
    private static bool IsNumericType(ITypeSymbol type)
    {
        switch (type.SpecialType)
        {
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                return true;
            default:
                return false;
        }
    }
}
