// Copyright (C) 2015-2024 The Neo Project.
//
// NameOfExpression.cs file belongs to the neo project and is free
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

namespace Neo.Compiler;

internal partial class MethodConvert
{
    /// <summary>
    /// This method converts a nameof expression to OpCodes.
    /// </summary>
    /// <param name="model">The semantic model providing context and information about nameof expression.</param>
    /// <param name="expression">The syntax representation of the nameof expression statement being converted.</param>
    /// <remarks>
    /// The nameof expression returns the name of a variable, type, or member as a string constant.
    /// This is evaluated at compile-time and replaced with a string literal.
    /// </remarks>
    /// <example>
    /// <code>
    /// string name = nameof(System.String); // "String"
    /// string length = nameof(System.String.Length); // "Length"
    /// </code>
    /// </example>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/nameof">nameof expression</seealso>
    private void ConvertNameOfExpression(SemanticModel model, NameOfExpressionSyntax expression)
    {
        // Get the argument of the nameof expression
        ExpressionSyntax argument = expression.Argument;
        
        // Get the name of the argument
        string name = GetNameOfArgument(model, argument);
        
        // Push the name as a string constant
        Push(name);
    }
    
    /// <summary>
    /// Gets the name of the argument in a nameof expression.
    /// </summary>
    /// <param name="model">The semantic model.</param>
    /// <param name="expression">The expression to get the name of.</param>
    /// <returns>The name of the expression.</returns>
    private string GetNameOfArgument(SemanticModel model, ExpressionSyntax expression)
    {
        switch (expression)
        {
            case IdentifierNameSyntax identifierName:
                return identifierName.Identifier.Text;
                
            case MemberAccessExpressionSyntax memberAccess:
                // For member access expressions like A.B.C, we only want the last part (C)
                return memberAccess.Name.Identifier.Text;
                
            case GenericNameSyntax genericName:
                // For generic types like List<T>, we only want the name without the type parameters
                return genericName.Identifier.Text;
                
            case PredefinedTypeSyntax predefinedType:
                // For predefined types like int, string, etc.
                return predefinedType.Keyword.Text;
                
            default:
                // For other expressions, try to get the symbol and use its name
                ISymbol? symbol = model.GetSymbolInfo(expression).Symbol;
                if (symbol != null)
                {
                    return symbol.Name;
                }
                
                // If we can't get the symbol, use the expression text as a fallback
                return expression.ToString();
        }
    }
}
