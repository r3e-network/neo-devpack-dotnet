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

/// <summary>
/// Provides methods for converting between different types in the Neo VM.
/// </summary>
internal static class TypeConvert
{
    /// <summary>
    /// Converts a value from one type to another in the Neo VM.
    /// </summary>
    /// <param name="convert">The method convert instance.</param>
    /// <param name="model">The semantic model.</param>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>True if the conversion was successful, false otherwise.</returns>
    public static bool ConvertType(MethodConvert convert, SemanticModel model, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // Handle enum conversions
        if (sourceType.TypeKind == TypeKind.Enum && targetType.SpecialType == SpecialType.System_Int32)
        {
            // Enum to int conversion - no additional operations needed
            // as enums are already represented as integers in the Neo VM
            return true;
        }
        else if (targetType.TypeKind == TypeKind.Enum && sourceType.SpecialType == SpecialType.System_Int32)
        {
            // Int to enum conversion - no additional operations needed
            // as enums are represented as integers in the Neo VM
            return true;
        }
        else if (sourceType.TypeKind == TypeKind.Enum && targetType.TypeKind == TypeKind.Enum)
        {
            // Enum to enum conversion - check if the underlying types are compatible
            INamedTypeSymbol sourceEnum = (INamedTypeSymbol)sourceType;
            INamedTypeSymbol targetEnum = (INamedTypeSymbol)targetType;
            
            // Get the underlying types
            ITypeSymbol sourceUnderlyingType = sourceEnum.EnumUnderlyingType!;
            ITypeSymbol targetUnderlyingType = targetEnum.EnumUnderlyingType!;
            
            // If the underlying types are the same, no conversion is needed
            if (sourceUnderlyingType.Equals(targetUnderlyingType, SymbolEqualityComparer.Default))
            {
                return true;
            }
            
            // Otherwise, we need to convert between the underlying types
            return ConvertNumericTypes(convert, sourceUnderlyingType, targetUnderlyingType);
        }
        
        // Handle other type conversions
        return false;
    }
    
    /// <summary>
    /// Converts between numeric types in the Neo VM.
    /// </summary>
    /// <param name="convert">The method convert instance.</param>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>True if the conversion was successful, false otherwise.</returns>
    private static bool ConvertNumericTypes(MethodConvert convert, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // Check if both types are numeric
        if (!IsNumericType(sourceType) || !IsNumericType(targetType))
        {
            return false;
        }
        
        // No conversion needed for same types
        if (sourceType.Equals(targetType, SymbolEqualityComparer.Default))
        {
            return true;
        }
        
        // Ensure the value is within the range of the target type
        convert.EnsureIntegerInRange(targetType);
        
        return true;
    }
    
    /// <summary>
    /// Checks if a type is a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is numeric, false otherwise.</returns>
    private static bool IsNumericType(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_SByte => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            _ => false
        };
    }
}
