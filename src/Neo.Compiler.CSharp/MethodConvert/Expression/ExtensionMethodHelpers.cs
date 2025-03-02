// Copyright (C) 2015-2024 The Neo Project.
//
// ExtensionMethodHelpers.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

extern alias scfx;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Neo.Compiler;

internal static class ExtensionMethodHelpers
{
    /// <summary>
    /// Determines whether a method is an extension method.
    /// </summary>
    /// <param name="method">The method symbol to check.</param>
    /// <returns>True if the method is an extension method; otherwise, false.</returns>
    public static bool IsExtensionMethod(this IMethodSymbol method)
    {
        return method.IsStatic && method.IsExtensionMethod;
    }

    /// <summary>
    /// Determines whether a method invocation is using extension method syntax.
    /// </summary>
    /// <param name="method">The method symbol.</param>
    /// <param name="expression">The expression syntax.</param>
    /// <returns>True if the method invocation is using extension method syntax; otherwise, false.</returns>
    public static bool IsExtensionMethodSyntax(this IMethodSymbol method, ExpressionSyntax expression)
    {
        if (!method.IsExtensionMethod)
            return false;

        // Extension method syntax is used when the method is accessed through a member access expression
        // where the expression is the first parameter of the extension method
        return expression is MemberAccessExpressionSyntax;
    }

    /// <summary>
    /// Gets the reduced form of an extension method if it's being called with extension method syntax.
    /// </summary>
    /// <param name="method">The method symbol.</param>
    /// <param name="expression">The expression syntax.</param>
    /// <returns>The reduced method symbol if the method is an extension method being called with extension method syntax; otherwise, the original method symbol.</returns>
    public static IMethodSymbol GetReducedExtensionMethod(this IMethodSymbol method, ExpressionSyntax expression)
    {
        if (method.IsExtensionMethodSyntax(expression) && method.ReducedFrom != null)
        {
            return method.ReducedFrom;
        }

        return method;
    }
}
