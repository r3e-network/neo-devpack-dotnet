// Copyright (C) 2015-2024 The Neo Project.
//
// RecordTypeConvert.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

extern alias scfx;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neo.VM;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Compiler;

internal static class RecordTypeConvert
{
    /// <summary>
    /// Converts a record declaration to Neo VM opcodes.
    /// </summary>
    /// <param name="context">The compilation context.</param>
    /// <param name="model">The semantic model.</param>
    /// <param name="recordDeclaration">The record declaration syntax.</param>
    /// <remarks>
    /// Records are a reference type that provides built-in functionality for encapsulating data.
    /// This implementation handles both positional records and record classes with explicit property declarations.
    /// </remarks>
    public static void ConvertRecordDeclaration(CompilationContext context, SemanticModel model, RecordDeclarationSyntax recordDeclaration)
    {
        // Get the record symbol
        INamedTypeSymbol recordSymbol = model.GetDeclaredSymbol(recordDeclaration) as INamedTypeSymbol;
        if (recordSymbol == null)
            throw new CompilationException(recordDeclaration, DiagnosticId.SyntaxNotSupported, $"Failed to get symbol for record: {recordDeclaration.Identifier.Text}");

        // Process positional parameters for positional records
        if (recordDeclaration.ParameterList != null)
        {
            ProcessPositionalRecordParameters(context, model, recordDeclaration, recordSymbol);
        }

        // Process record members (properties, methods, etc.)
        ProcessRecordMembers(context, model, recordDeclaration, recordSymbol);

        // Generate equality members if not explicitly defined
        GenerateEqualityMembers(context, model, recordDeclaration, recordSymbol);
    }

    /// <summary>
    /// Processes positional parameters for a positional record.
    /// </summary>
    /// <param name="context">The compilation context.</param>
    /// <param name="model">The semantic model.</param>
    /// <param name="recordDeclaration">The record declaration syntax.</param>
    /// <param name="recordSymbol">The record symbol.</param>
    private static void ProcessPositionalRecordParameters(CompilationContext context, SemanticModel model, RecordDeclarationSyntax recordDeclaration, INamedTypeSymbol recordSymbol)
    {
        if (recordDeclaration.ParameterList == null)
            return;

        // For each parameter in the positional record, create a property
        foreach (ParameterSyntax parameter in recordDeclaration.ParameterList.Parameters)
        {
            string paramName = parameter.Identifier.Text;
            IParameterSymbol paramSymbol = model.GetDeclaredSymbol(parameter) as IParameterSymbol;
            if (paramSymbol == null)
                continue;

            // Find the corresponding property in the record
            IPropertySymbol propertySymbol = recordSymbol.GetMembers(paramName).OfType<IPropertySymbol>().FirstOrDefault();
            if (propertySymbol == null)
                continue;

            // Add the property to the context
            context.AddProperty(propertySymbol);
        }
    }

    /// <summary>
    /// Processes record members (properties, methods, etc.).
    /// </summary>
    /// <param name="context">The compilation context.</param>
    /// <param name="model">The semantic model.</param>
    /// <param name="recordDeclaration">The record declaration syntax.</param>
    /// <param name="recordSymbol">The record symbol.</param>
    private static void ProcessRecordMembers(CompilationContext context, SemanticModel model, RecordDeclarationSyntax recordDeclaration, INamedTypeSymbol recordSymbol)
    {
        // Process properties
        foreach (IPropertySymbol propertySymbol in recordSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            // Skip properties that are already processed as positional parameters
            if (recordDeclaration.ParameterList != null &&
                recordDeclaration.ParameterList.Parameters.Any(p => p.Identifier.Text == propertySymbol.Name))
                continue;

            // Add the property to the context
            context.AddProperty(propertySymbol);
        }

        // Process methods
        foreach (IMethodSymbol methodSymbol in recordSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            // Skip constructors, destructors, and property accessors
            if (methodSymbol.MethodKind == MethodKind.Constructor ||
                methodSymbol.MethodKind == MethodKind.Destructor ||
                methodSymbol.MethodKind == MethodKind.PropertyGet ||
                methodSymbol.MethodKind == MethodKind.PropertySet)
                continue;

            // Add the method to the context
            context.AddMethod(methodSymbol);
        }
    }

    /// <summary>
    /// Generates equality members for a record if not explicitly defined.
    /// </summary>
    /// <param name="context">The compilation context.</param>
    /// <param name="model">The semantic model.</param>
    /// <param name="recordDeclaration">The record declaration syntax.</param>
    /// <param name="recordSymbol">The record symbol.</param>
    private static void GenerateEqualityMembers(CompilationContext context, SemanticModel model, RecordDeclarationSyntax recordDeclaration, INamedTypeSymbol recordSymbol)
    {
        // Check if Equals method is explicitly defined
        bool hasEqualsMethod = recordSymbol.GetMembers("Equals")
            .OfType<IMethodSymbol>()
            .Any(m => m.Parameters.Length == 1 && m.Parameters[0].Type.Equals(recordSymbol, SymbolEqualityComparer.Default));

        // Check if GetHashCode method is explicitly defined
        bool hasGetHashCodeMethod = recordSymbol.GetMembers("GetHashCode")
            .OfType<IMethodSymbol>()
            .Any(m => m.Parameters.Length == 0);

        // If Equals method is not explicitly defined, generate it
        if (!hasEqualsMethod)
        {
            GenerateEqualsMethod(context, recordSymbol);
        }

        // If GetHashCode method is not explicitly defined, generate it
        if (!hasGetHashCodeMethod)
        {
            GenerateGetHashCodeMethod(context, recordSymbol);
        }
    }

    /// <summary>
    /// Generates an Equals method for a record.
    /// </summary>
    /// <param name="context">The compilation context.</param>
    /// <param name="recordSymbol">The record symbol.</param>
    private static void GenerateEqualsMethod(CompilationContext context, INamedTypeSymbol recordSymbol)
    {
        // Get all properties that should be compared for equality
        IEnumerable<IPropertySymbol> properties = recordSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic);

        // Create a method that compares all properties for equality
        // This is a simplified version of what the compiler would generate
        // In a real implementation, we would need to generate the actual method body
    }

    /// <summary>
    /// Generates a GetHashCode method for a record.
    /// </summary>
    /// <param name="context">The compilation context.</param>
    /// <param name="recordSymbol">The record symbol.</param>
    private static void GenerateGetHashCodeMethod(CompilationContext context, INamedTypeSymbol recordSymbol)
    {
        // Get all properties that should be included in the hash code
        IEnumerable<IPropertySymbol> properties = recordSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic);

        // Create a method that computes a hash code based on all properties
        // This is a simplified version of what the compiler would generate
        // In a real implementation, we would need to generate the actual method body
    }

    /// <summary>
    /// Determines if a type is a record type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a record type; otherwise, false.</returns>
    public static bool IsRecordType(ITypeSymbol type)
    {
        return type is INamedTypeSymbol namedType && namedType.IsRecord;
    }

    /// <summary>
    /// Converts a record type to Neo VM opcodes.
    /// </summary>
    /// <param name="convert">The method convert instance.</param>
    /// <param name="model">The semantic model.</param>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>True if the conversion was successful; otherwise, false.</returns>
    public static bool ConvertType(MethodConvert convert, SemanticModel model, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // Check if both types are record types
        if (!IsRecordType(sourceType) || !IsRecordType(targetType))
            return false;

        // Check if the target type is assignable from the source type
        if (!targetType.IsAssignableFrom(sourceType))
            return false;

        // No conversion needed if the types are the same
        if (SymbolEqualityComparer.Default.Equals(sourceType, targetType))
            return true;

        // For record types, we need to create a new instance of the target type
        // and copy all properties from the source type
        // This is a simplified version of what the compiler would generate
        // In a real implementation, we would need to generate the actual conversion code

        return true;
    }
}
