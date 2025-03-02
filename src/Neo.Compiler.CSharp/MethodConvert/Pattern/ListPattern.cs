// Copyright (C) 2015-2024 The Neo Project.
//
// ListPattern.cs file belongs to the neo project and is free
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
using System.Linq;

namespace Neo.Compiler;

internal partial class MethodConvert
{
    /// <summary>
    /// Converts a list pattern to OpCodes.
    /// </summary>
    /// <param name="model">The semantic model.</param>
    /// <param name="pattern">The list pattern syntax.</param>
    /// <param name="localIndex">The local variable index.</param>
    /// <remarks>
    /// List patterns are used to match against collections like arrays and lists.
    /// This implementation supports:
    /// - Matching specific elements at specific positions
    /// - Using discard patterns (_) to skip elements
    /// - Using slice patterns (..) to match any number of elements
    /// </remarks>
    /// <example>
    /// <code>
    /// int[] numbers = [1, 2, 3, 4, 5];
    /// if (numbers is [1, 2, ..]) { } // Matches arrays starting with 1, 2
    /// if (numbers is [_, 2, _, 4, _]) { } // Matches arrays with 2 at index 1 and 4 at index 3
    /// if (numbers is [>= 1, <= 10, ..]) { } // Matches arrays with first element >= 1 and second <= 10
    /// </code>
    /// </example>
    private void ConvertListPattern(SemanticModel model, ListPatternSyntax pattern, byte localIndex)
    {
        // Load the collection to match against
        AccessSlot(OpCode.LDLOC, localIndex);
        
        // Check if it's null
        AddInstruction(OpCode.DUP);
        AddInstruction(OpCode.ISNULL);
        JumpTarget endTarget = new JumpTarget();
        AddInstruction(OpCode.JMPIF, endTarget);
        
        // Check if it's an array or collection
        AddInstruction(OpCode.DUP);
        AddInstruction(OpCode.ISARRAY);
        JumpTarget notArrayTarget = new JumpTarget();
        AddInstruction(OpCode.JMPIFNOT, notArrayTarget);
        
        // It's an array, process the patterns
        bool hasSlicePattern = pattern.Patterns.Any(p => p is SlicePatternSyntax);
        
        // If there's no slice pattern, check the length
        if (!hasSlicePattern)
        {
            // Check if the array length matches the number of patterns
            AddInstruction(OpCode.DUP);
            AddInstruction(OpCode.SIZE);
            Push(pattern.Patterns.Count);
            AddInstruction(OpCode.EQUAL);
            JumpTarget lengthMismatchTarget = new JumpTarget();
            AddInstruction(OpCode.JMPIFNOT, lengthMismatchTarget);
            
            // Process each pattern
            for (int i = 0; i < pattern.Patterns.Count; i++)
            {
                PatternSyntax subPattern = pattern.Patterns[i];
                
                // Skip discard patterns
                if (subPattern is DiscardPatternSyntax)
                    continue;
                
                // Load the array and the index
                AddInstruction(OpCode.DUP);
                Push(i);
                
                // Get the element at the index
                AddInstruction(OpCode.PICKITEM);
                
                // Store it in a temporary variable
                byte tempIndex = AddAnonymousVariable();
                AccessSlot(OpCode.STLOC, tempIndex);
                
                // Match the element against the pattern
                ConvertPattern(model, subPattern, tempIndex);
                
                // If the pattern doesn't match, jump to the end
                AddInstruction(OpCode.JMPIFNOT, lengthMismatchTarget);
                
                // Clean up
                RemoveAnonymousVariable(tempIndex);
            }
            
            // If we get here, all patterns matched
            AddInstruction(OpCode.DROP);
            Push(true);
            AddInstruction(OpCode.JMP, endTarget);
            
            // Length mismatch or pattern mismatch
            lengthMismatchTarget.Instruction = _currentInstruction;
            AddInstruction(OpCode.DROP);
            Push(false);
            AddInstruction(OpCode.JMP, endTarget);
        }
        else
        {
            // Handle slice patterns
            // This is more complex as we need to match patterns before and after the slice
            
            // First, check if the array is long enough for the non-slice patterns
            AddInstruction(OpCode.DUP);
            AddInstruction(OpCode.SIZE);
            
            // Count non-slice patterns
            int nonSliceCount = pattern.Patterns.Count(p => p is not SlicePatternSyntax);
            Push(nonSliceCount);
            
            // Check if array length >= non-slice count
            AddInstruction(OpCode.GE);
            JumpTarget lengthMismatchTarget = new JumpTarget();
            AddInstruction(OpCode.JMPIFNOT, lengthMismatchTarget);
            
            // Find the index of the slice pattern
            int sliceIndex = pattern.Patterns.IndexOf(pattern.Patterns.First(p => p is SlicePatternSyntax));
            
            // Process patterns before the slice
            for (int i = 0; i < sliceIndex; i++)
            {
                PatternSyntax subPattern = pattern.Patterns[i];
                
                // Skip discard patterns
                if (subPattern is DiscardPatternSyntax)
                    continue;
                
                // Load the array and the index
                AddInstruction(OpCode.DUP);
                Push(i);
                
                // Get the element at the index
                AddInstruction(OpCode.PICKITEM);
                
                // Store it in a temporary variable
                byte tempIndex = AddAnonymousVariable();
                AccessSlot(OpCode.STLOC, tempIndex);
                
                // Match the element against the pattern
                ConvertPattern(model, subPattern, tempIndex);
                
                // If the pattern doesn't match, jump to the end
                AddInstruction(OpCode.JMPIFNOT, lengthMismatchTarget);
                
                // Clean up
                RemoveAnonymousVariable(tempIndex);
            }
            
            // Process patterns after the slice
            int patternsAfterSlice = pattern.Patterns.Count - sliceIndex - 1;
            if (patternsAfterSlice > 0)
            {
                // Check if there are enough elements for patterns after the slice
                AddInstruction(OpCode.DUP);
                AddInstruction(OpCode.SIZE);
                Push(patternsAfterSlice);
                AddInstruction(OpCode.GE);
                AddInstruction(OpCode.JMPIFNOT, lengthMismatchTarget);
                
                // Process each pattern after the slice
                for (int i = 0; i < patternsAfterSlice; i++)
                {
                    PatternSyntax subPattern = pattern.Patterns[sliceIndex + 1 + i];
                    
                    // Skip discard patterns
                    if (subPattern is DiscardPatternSyntax)
                        continue;
                    
                    // Load the array and calculate the index from the end
                    AddInstruction(OpCode.DUP);
                    AddInstruction(OpCode.SIZE);
                    Push(patternsAfterSlice - i);
                    AddInstruction(OpCode.SUB);
                    
                    // Get the element at the index
                    AddInstruction(OpCode.PICKITEM);
                    
                    // Store it in a temporary variable
                    byte tempIndex = AddAnonymousVariable();
                    AccessSlot(OpCode.STLOC, tempIndex);
                    
                    // Match the element against the pattern
                    ConvertPattern(model, subPattern, tempIndex);
                    
                    // If the pattern doesn't match, jump to the end
                    AddInstruction(OpCode.JMPIFNOT, lengthMismatchTarget);
                    
                    // Clean up
                    RemoveAnonymousVariable(tempIndex);
                }
            }
            
            // If we get here, all patterns matched
            AddInstruction(OpCode.DROP);
            Push(true);
            AddInstruction(OpCode.JMP, endTarget);
            
            // Length mismatch or pattern mismatch
            lengthMismatchTarget.Instruction = _currentInstruction;
            AddInstruction(OpCode.DROP);
            Push(false);
            AddInstruction(OpCode.JMP, endTarget);
        }
        
        // Not an array
        notArrayTarget.Instruction = _currentInstruction;
        AddInstruction(OpCode.DROP);
        Push(false);
        
        // End of pattern matching
        endTarget.Instruction = _currentInstruction;
    }
}
