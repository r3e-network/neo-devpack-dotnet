// Copyright (C) 2015-2024 The Neo Project.
//
// SystemCall.Trig.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neo.VM;

namespace Neo.Compiler;

internal partial class MethodConvert
{
    /// <summary>
    /// Handler for Math.Sin methods
    /// Implements sin using Taylor series approximation:
    /// sin(x) ≈ x - x^3/3! + x^5/5! - x^7/7! + ...
    /// </summary>
    private static void HandleMathSin(MethodConvert methodConvert, SemanticModel model, IMethodSymbol symbol, ExpressionSyntax? instanceExpression, IReadOnlyList<SyntaxNode>? arguments)
    {
        if (arguments is not null)
            methodConvert.PrepareArgumentsForMethod(model, symbol, arguments);
        
        // Store original x for later use
        methodConvert.AddInstruction(OpCode.DUP);  // x, x
        
        // Calculate x^3
        methodConvert.AddInstruction(OpCode.DUP);  // x, x, x
        methodConvert.AddInstruction(OpCode.DUP);  // x, x, x, x
        methodConvert.AddInstruction(OpCode.MUL);  // x, x, x^2
        methodConvert.AddInstruction(OpCode.MUL);  // x, x^3
        
        // Calculate x^3/6 (3!)
        methodConvert.Push(6);  // x, x^3, 6
        methodConvert.AddInstruction(OpCode.DIV);  // x, x^3/6
        
        // Subtract from x
        methodConvert.AddInstruction(OpCode.SUB);  // x - x^3/6
        
        // Calculate x^5
        methodConvert.Push(5);  // result, 5
        methodConvert.AddInstruction(OpCode.PICK);  // result, x
        methodConvert.AddInstruction(OpCode.DUP);  // result, x, x
        methodConvert.AddInstruction(OpCode.DUP);  // result, x, x, x
        methodConvert.AddInstruction(OpCode.MUL);  // result, x, x^2
        methodConvert.AddInstruction(OpCode.DUP);  // result, x, x^2, x^2
        methodConvert.AddInstruction(OpCode.MUL);  // result, x, x^4
        methodConvert.AddInstruction(OpCode.MUL);  // result, x^5
        
        // Calculate x^5/120 (5!)
        methodConvert.Push(120);  // result, x^5, 120
        methodConvert.AddInstruction(OpCode.DIV);  // result, x^5/120
        
        // Add to result
        methodConvert.AddInstruction(OpCode.ADD);  // result + x^5/120
        
        // Clean up the stack
        methodConvert.AddInstruction(OpCode.NIP);  // Remove the original x
    }
    
    /// <summary>
    /// Handler for Math.Cos methods
    /// Implements cos using Taylor series approximation:
    /// cos(x) ≈ 1 - x^2/2! + x^4/4! - x^6/6! + ...
    /// </summary>
    private static void HandleMathCos(MethodConvert methodConvert, SemanticModel model, IMethodSymbol symbol, ExpressionSyntax? instanceExpression, IReadOnlyList<SyntaxNode>? arguments)
    {
        if (arguments is not null)
            methodConvert.PrepareArgumentsForMethod(model, symbol, arguments);
        
        // Store x for later use
        methodConvert.AddInstruction(OpCode.DUP);  // x, x
        
        // Calculate x^2
        methodConvert.AddInstruction(OpCode.MUL);  // x^2
        
        // Store x^2 for later use
        methodConvert.AddInstruction(OpCode.DUP);  // x^2, x^2
        
        // Calculate x^2/2 (2!)
        methodConvert.Push(2);  // x^2, x^2, 2
        methodConvert.AddInstruction(OpCode.DIV);  // x^2, x^2/2
        
        // Calculate 1 - x^2/2
        methodConvert.Push(1);  // x^2, x^2/2, 1
        methodConvert.AddInstruction(OpCode.SWAP);  // x^2, 1, x^2/2
        methodConvert.AddInstruction(OpCode.SUB);  // x^2, 1 - x^2/2
        
        // Calculate x^4
        methodConvert.AddInstruction(OpCode.SWAP);  // 1 - x^2/2, x^2
        methodConvert.AddInstruction(OpCode.DUP);  // 1 - x^2/2, x^2, x^2
        methodConvert.AddInstruction(OpCode.MUL);  // 1 - x^2/2, x^4
        
        // Calculate x^4/24 (4!)
        methodConvert.Push(24);  // 1 - x^2/2, x^4, 24
        methodConvert.AddInstruction(OpCode.DIV);  // 1 - x^2/2, x^4/24
        
        // Add to result
        methodConvert.AddInstruction(OpCode.ADD);  // 1 - x^2/2 + x^4/24
    }
    
    /// <summary>
    /// Handler for Math.Tan methods
    /// Implements tan using the identity: tan(x) = sin(x) / cos(x)
    /// </summary>
    private static void HandleMathTan(MethodConvert methodConvert, SemanticModel model, IMethodSymbol symbol, ExpressionSyntax? instanceExpression, IReadOnlyList<SyntaxNode>? arguments)
    {
        if (arguments is not null)
            methodConvert.PrepareArgumentsForMethod(model, symbol, arguments);
        
        // Duplicate x for sin and cos calculations
        methodConvert.AddInstruction(OpCode.DUP);  // x, x
        
        // Calculate cos(x) first
        HandleMathCos(methodConvert, model, symbol, instanceExpression, arguments);  // x, cos(x)
        
        // Swap to get x on top
        methodConvert.AddInstruction(OpCode.SWAP);  // cos(x), x
        
        // Calculate sin(x)
        HandleMathSin(methodConvert, model, symbol, instanceExpression, arguments);  // cos(x), sin(x)
        
        // Calculate sin(x) / cos(x)
        methodConvert.AddInstruction(OpCode.SWAP);  // sin(x), cos(x)
        methodConvert.AddInstruction(OpCode.DIV);  // sin(x) / cos(x)
    }
}
