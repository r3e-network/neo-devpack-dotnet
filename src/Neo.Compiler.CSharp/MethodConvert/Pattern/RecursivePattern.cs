// Copyright (C) 2015-2024 The Neo Project.
//
// RecursivePattern.cs file belongs to the neo project and is free
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

namespace Neo.Compiler;

internal partial class MethodConvert
{
    private void ConvertRecursivePattern(SemanticModel model, RecursivePatternSyntax pattern, byte localIndex)
    {
        // Check if the value is null
        AccessSlot(OpCode.LDLOC, localIndex);
        AddInstruction(OpCode.ISNULL);
        JumpTarget endTarget = new JumpTarget();
        JumpTarget nullTarget = new JumpTarget();
        AddInstruction(OpCode.JMPIF, nullTarget);
        
        // Process property pattern clause
        if (pattern.PropertyPatternClause is { } propertyClause)
        {
            // For each subpattern, check if the property matches the pattern
            foreach (var subpattern in propertyClause.Subpatterns)
            {
                // Get the property symbol
                var propertySymbol = model.GetSymbolInfo(subpattern.NameColon!.Name).Symbol!;
                
                if (propertySymbol is IPropertySymbol property)
                {
                    // Load the object
                    AccessSlot(OpCode.LDLOC, localIndex);
                    
                    // Call the property getter
                    CallMethodWithConvention(model, property.GetMethod!);
                    
                    // Store the property value in a temporary variable
                    byte tempIndex = AddAnonymousVariable();
                    AccessSlot(OpCode.STLOC, tempIndex);
                    
                    // Match the property value against the pattern
                    ConvertPattern(model, subpattern.Pattern, tempIndex);
                    
                    // If the pattern doesn't match, jump to the end
                    AddInstruction(OpCode.JMPIFNOT, nullTarget);
                    
                    // Clean up
                    RemoveAnonymousVariable(tempIndex);
                }
                else if (propertySymbol is IFieldSymbol field)
                {
                    // Load the object
                    AccessSlot(OpCode.LDLOC, localIndex);
                    
                    // Get the field
                    AddInstruction(OpCode.DUP);
                    Push(field.Name);
                    AddInstruction(OpCode.PICKITEM);
                    
                    // Store the field value in a temporary variable
                    byte tempIndex = AddAnonymousVariable();
                    AccessSlot(OpCode.STLOC, tempIndex);
                    
                    // Match the field value against the pattern
                    ConvertPattern(model, subpattern.Pattern, tempIndex);
                    
                    // If the pattern doesn't match, jump to the end
                    AddInstruction(OpCode.JMPIFNOT, nullTarget);
                    
                    // Clean up
                    RemoveAnonymousVariable(tempIndex);
                }
                else
                {
                    throw new CompilationException(subpattern, DiagnosticId.SyntaxNotSupported, $"Unsupported property or field: {subpattern.NameColon.Name}");
                }
            }
            
            // If we get here, all patterns matched
            Push(true);
            AddInstruction(OpCode.JMP, endTarget);
        }
        // Process positional pattern clause
        else if (pattern.PositionalPatternClause is { } positionalClause)
        {
            // Check if the object is a tuple or has deconstruct method
            AccessSlot(OpCode.LDLOC, localIndex);
            
            // For now, we only support tuples
            // TODO: Add support for objects with Deconstruct method
            
            // Process each subpattern
            for (int i = 0; i < positionalClause.Subpatterns.Count; i++)
            {
                var subpattern = positionalClause.Subpatterns[i];
                
                // Load the tuple
                AccessSlot(OpCode.LDLOC, localIndex);
                
                // Get the item at index i
                Push(i);
                AddInstruction(OpCode.PICKITEM);
                
                // Store the item in a temporary variable
                byte tempIndex = AddAnonymousVariable();
                AccessSlot(OpCode.STLOC, tempIndex);
                
                // Match the item against the pattern
                ConvertPattern(model, subpattern.Pattern, tempIndex);
                
                // If the pattern doesn't match, jump to the end
                AddInstruction(OpCode.JMPIFNOT, nullTarget);
                
                // Clean up
                RemoveAnonymousVariable(tempIndex);
            }
            
            // If we get here, all patterns matched
            Push(true);
            AddInstruction(OpCode.JMP, endTarget);
        }
        else
        {
            // No property or positional pattern clause
            // This is a type pattern with a designation
            
            // Check if the type matches
            if (pattern.Type != null)
            {
                AccessSlot(OpCode.LDLOC, localIndex);
                ITypeSymbol typeSymbol = model.GetTypeInfo(pattern.Type).Type!;
                EmitIsType(typeSymbol);
                AddInstruction(OpCode.JMPIFNOT, nullTarget);
            }
            
            // If we get here, the type matches
            Push(true);
            AddInstruction(OpCode.JMP, endTarget);
        }
        
        // Pattern doesn't match
        nullTarget.Instruction = _currentInstruction;
        Push(false);
        
        // End of pattern matching
        endTarget.Instruction = _currentInstruction;
    }
}
