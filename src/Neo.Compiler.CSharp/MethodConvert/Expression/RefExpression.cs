// Copyright (C) 2015-2024 The Neo Project.
//
// RefExpression.cs file belongs to the neo project and is free
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
using Neo.VM;

namespace Neo.Compiler
{
    internal partial class MethodConvert
    {
        /// <summary>
        /// Converts a ref expression to Neo VM instructions.
        /// </summary>
        /// <param name="model">The semantic model.</param>
        /// <param name="syntax">The ref expression syntax.</param>
        /// <remarks>
        /// A ref expression is used to return the address of a variable in a ref return statement.
        /// Example: return ref myVariable;
        /// </remarks>
        private void ConvertRefExpression(SemanticModel model, RefExpressionSyntax syntax)
        {
            // Get the symbol for the expression
            var expression = syntax.Expression;
            var symbolInfo = model.GetSymbolInfo(expression);
            
            if (symbolInfo.Symbol is ILocalSymbol localSymbol)
            {
                // For local variables, we need to load the address
                if (IsRefVariable(localSymbol))
                {
                    // If it's already a ref variable, just load its value (which is an address)
                    var index = _localVariables[localSymbol];
                    AccessSlot(OpCode.LDLOC, index);
                }
                else
                {
                    // For regular local variables, we need to load their address
                    var index = _localVariables[localSymbol];
                    AccessSlot(OpCode.LDLOCA, index);
                }
            }
            else if (symbolInfo.Symbol is IParameterSymbol parameterSymbol)
            {
                // For parameters, we need to load the address
                var index = _parameters[parameterSymbol];
                AccessSlot(OpCode.LDARGA, index);
            }
            else if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
            {
                // For fields, we need to load the address
                if (fieldSymbol.IsStatic)
                {
                    // Static fields
                    var staticIndex = _context.GetStaticFieldIndex(fieldSymbol);
                    AccessSlot(OpCode.LDSFLD, staticIndex);
                }
                else
                {
                    // Instance fields
                    // First, load the instance
                    if (expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        ConvertExpression(model, memberAccess.Expression);
                    }
                    else
                    {
                        // This is an implicit this access
                        AccessSlot(OpCode.LDARG, 0);
                    }
                    
                    // Then, load the field address
                    AddInstruction(OpCode.LDFLDA);
                    Push(fieldSymbol.Name);
                }
            }
            else if (expression is ElementAccessExpressionSyntax elementAccess)
            {
                // For array elements, we need to load the address
                // First, load the array
                ConvertExpression(model, elementAccess.Expression);
                
                // Then, load the indices
                foreach (var argument in elementAccess.ArgumentList.Arguments)
                {
                    ConvertExpression(model, argument.Expression);
                }
                
                // Finally, load the element address
                AddInstruction(OpCode.PICKITEM);
            }
            else
            {
                // For other expressions, just convert them normally
                // This might not be correct for all cases, but it's a fallback
                ConvertExpression(model, expression);
            }
        }
    }
}
