// Copyright (C) 2015-2024 The Neo Project.
//
// LocalFunctionStatement.cs file belongs to the neo project and is free
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

internal partial class MethodConvert
{
    /// <summary>
    /// Converts a local function statement to OpCodes.
    /// </summary>
    /// <param name="model">The semantic model providing context and information about local function statement.</param>
    /// <param name="syntax">The syntax representation of the local function statement being converted.</param>
    /// <remarks>
    /// Local functions are methods declared within another method. They are only visible within the containing method.
    /// This implementation converts local functions to Neo VM opcodes by:
    /// 1. Creating a unique label for the local function
    /// 2. Jumping over the local function code during normal execution
    /// 3. Implementing the local function code after the jump
    /// 4. When the local function is called, jumping to its label
    /// </remarks>
    /// <example>
    /// <code>
    /// public static int Calculate(int x, int y)
    /// {
    ///     int Add(int a, int b) // This is a local function
    ///     {
    ///         return a + b;
    ///     }
    ///     
    ///     return Add(x, y);
    /// }
    /// </code>
    /// </example>
    private void ConvertLocalFunctionStatement(SemanticModel model, LocalFunctionStatementSyntax syntax)
    {
        // Get the method symbol for the local function
        IMethodSymbol methodSymbol = model.GetDeclaredSymbol(syntax) as IMethodSymbol;
        if (methodSymbol == null)
            throw new CompilationException(syntax, DiagnosticId.SyntaxNotSupported, $"Failed to get method symbol for local function: {syntax.Identifier.Text}");

        // Create a unique label for the local function
        JumpTarget functionStart = new JumpTarget();
        JumpTarget functionEnd = new JumpTarget();

        // Jump over the local function code during normal execution
        AddInstruction(OpCode.JMP, functionEnd);

        // Mark the start of the local function
        functionStart.Instruction = _currentInstruction;

        // Store the local function in the context for later use
        string functionName = methodSymbol.Name;
        _localFunctions ??= new Dictionary<string, JumpTarget>();
        _localFunctions[functionName] = functionStart;

        // Convert the local function parameters
        List<(string, bool)> parameters = new List<(string, bool)>();
        foreach (ParameterSyntax parameter in syntax.ParameterList.Parameters)
        {
            string paramName = parameter.Identifier.Text;
            bool isByRef = parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
            parameters.Add((paramName, isByRef));
            
            // Add the parameter to the scope
            IParameterSymbol paramSymbol = model.GetDeclaredSymbol(parameter) as IParameterSymbol;
            if (paramSymbol != null)
            {
                AddLocalVariable(paramSymbol);
            }
        }

        // Convert the local function body
        if (syntax.Body != null)
        {
            ConvertBlockStatement(model, syntax.Body);
        }
        else if (syntax.ExpressionBody != null)
        {
            // For expression-bodied local functions (e.g., int Add(int a, int b) => a + b;)
            ConvertExpression(model, syntax.ExpressionBody.Expression);
            AddInstruction(OpCode.RET);
        }

        // Mark the end of the local function
        functionEnd.Instruction = _currentInstruction;
    }

    // Dictionary to store local functions for the current method
    private Dictionary<string, JumpTarget> _localFunctions;

    /// <summary>
    /// Calls a local function by name.
    /// </summary>
    /// <param name="functionName">The name of the local function to call.</param>
    /// <returns>True if the local function was found and called; otherwise, false.</returns>
    internal bool CallLocalFunction(string functionName)
    {
        if (_localFunctions == null || !_localFunctions.TryGetValue(functionName, out JumpTarget target))
            return false;

        // Jump to the local function
        AddInstruction(OpCode.CALL, target);
        return true;
    }
}
