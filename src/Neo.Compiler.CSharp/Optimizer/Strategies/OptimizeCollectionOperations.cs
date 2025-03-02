// Copyright (C) 2015-2024 The Neo Project.
//
// OptimizeCollectionOperations.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.SmartContract;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Neo.Optimizer
{
    public static partial class Peephole
    {
        /// <summary>
        /// Optimizes collection operations to reduce gas costs.
        /// </summary>
        /// <param name="nef">The NEF file to optimize.</param>
        /// <param name="manifest">The contract manifest.</param>
        /// <param name="debugInfo">The debug information.</param>
        /// <returns>The optimized NEF file, manifest, and debug information.</returns>
        public static (NefFile, ContractManifest, JObject?) OptimizeCollectionOperations(NefFile nef, ContractManifest manifest, JObject? debugInfo = null)
        {
            if (nef is null) throw new ArgumentNullException(nameof(nef));
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));

            Script script = nef.Script;
            Instruction[] instructions = script.GetInstructions().ToArray();
            if (instructions.Length <= 3) return (nef, manifest, debugInfo);

            List<Instruction> optimized = new(instructions);
            bool modified = false;

            // Optimize pattern: NEWARRAY_T + multiple APPEND operations
            modified |= OptimizeAppendOperations(optimized);

            // Optimize pattern: NEWARRAY_T + multiple SETITEM operations with consecutive indices
            modified |= OptimizeSetItemOperations(optimized);

            if (!modified) return (nef, manifest, debugInfo);

            // Create a new script with the optimized instructions
            using MemoryStream ms = new();
            using BinaryWriter writer = new(ms);
            foreach (Instruction instruction in optimized)
                instruction.Write(writer);
            
            NefFile result = new()
            {
                Compiler = nef.Compiler,
                Source = nef.Source,
                Tokens = nef.Tokens,
                Script = ms.ToArray(),
                CheckSum = 0
            };
            result.CheckSum = result.CalculateChecksum();
            
            return (result, manifest, debugInfo);
        }

        /// <summary>
        /// Optimizes NEWARRAY_T + multiple APPEND operations by pre-allocating the array and using SETITEM.
        /// </summary>
        /// <param name="instructions">The list of instructions to optimize.</param>
        /// <returns>True if any optimization was applied, false otherwise.</returns>
        private static bool OptimizeAppendOperations(List<Instruction> instructions)
        {
            bool modified = false;

            for (int i = 0; i < instructions.Count - 2; i++)
            {
                // Look for NEWARRAY_T or NEWARRAY
                if (instructions[i].OpCode != OpCode.NEWARRAY_T && instructions[i].OpCode != OpCode.NEWARRAY)
                    continue;

                // Find consecutive APPEND operations
                List<int> appendIndices = new();
                int j = i + 1;
                
                while (j < instructions.Count && IsAppendOperation(instructions, j))
                {
                    appendIndices.Add(j);
                    j += 2; // Skip the APPEND instruction and the value before it
                }

                // If we found at least 3 APPEND operations, optimize
                if (appendIndices.Count >= 3)
                {
                    // Replace NEWARRAY_T with PUSH<count> + NEWARRAY_T
                    instructions[i] = new Instruction { OpCode = GetPushOpCode(appendIndices.Count) };
                    instructions.Insert(i + 1, new Instruction { 
                        OpCode = instructions[i + 1].OpCode == OpCode.NEWARRAY ? OpCode.NEWARRAY : OpCode.NEWARRAY_T,
                        Operand = instructions[i + 1].Operand
                    });

                    // Replace APPEND operations with SETITEM operations
                    for (int k = 0; k < appendIndices.Count; k++)
                    {
                        int appendIdx = appendIndices[k] + k + 1; // +k because we inserted an instruction
                        
                        // Replace the APPEND with SWAP + PUSH<index> + SWAP + SETITEM
                        instructions[appendIdx] = new Instruction { OpCode = OpCode.SWAP };
                        instructions.Insert(appendIdx + 1, new Instruction { OpCode = GetPushOpCode(k) });
                        instructions.Insert(appendIdx + 2, new Instruction { OpCode = OpCode.SWAP });
                        instructions.Insert(appendIdx + 3, new Instruction { OpCode = OpCode.SETITEM });
                        
                        // Update the indices of the remaining APPEND operations
                        for (int m = k + 1; m < appendIndices.Count; m++)
                            appendIndices[m] += 3;
                    }

                    modified = true;
                    i = appendIndices[appendIndices.Count - 1] + 4; // Skip ahead
                }
            }

            return modified;
        }

        /// <summary>
        /// Optimizes NEWARRAY_T + multiple SETITEM operations with consecutive indices.
        /// </summary>
        /// <param name="instructions">The list of instructions to optimize.</param>
        /// <returns>True if any optimization was applied, false otherwise.</returns>
        private static bool OptimizeSetItemOperations(List<Instruction> instructions)
        {
            bool modified = false;

            for (int i = 0; i < instructions.Count - 6; i++)
            {
                // Look for NEWARRAY_T or NEWARRAY
                if (instructions[i].OpCode != OpCode.NEWARRAY_T && instructions[i].OpCode != OpCode.NEWARRAY)
                    continue;

                // Find consecutive SETITEM operations with consecutive indices
                List<(int index, int valueIndex, int setItemIndex)> items = new();
                int j = i + 1;
                
                while (j + 2 < instructions.Count && 
                      IsPushOpCode(instructions[j].OpCode) &&
                      instructions[j + 2].OpCode == OpCode.SETITEM)
                {
                    int index = GetPushOpCodeValue(instructions[j].OpCode);
                    items.Add((index, j + 1, j + 2));
                    j += 3;
                }
                
                // If we found at least 3 SETITEM operations, optimize
                if (items.Count >= 3)
                {
                    // Sort items by index
                    items.Sort((a, b) => a.index.CompareTo(b.index));
                    
                    // Check if indices are consecutive starting from 0
                    bool consecutive = true;
                    for (int k = 0; k < items.Count; k++)
                    {
                        if (items[k].index != k)
                        {
                            consecutive = false;
                            break;
                        }
                    }
                    
                    if (consecutive)
                    {
                        // Replace with PACK operation
                        // First, remove the NEWARRAY_T instruction
                        instructions.RemoveAt(i);
                        
                        // Add value instructions in reverse order
                        for (int k = items.Count - 1; k >= 0; k--)
                        {
                            // Copy the value instruction
                            instructions.Insert(i, instructions[items[k].valueIndex - (items.Count - k - 1) * 3]);
                        }
                        
                        // Add PACK instruction
                        instructions.Insert(i + items.Count, new Instruction { 
                            OpCode = GetPushOpCode(items.Count)
                        });
                        instructions.Insert(i + items.Count + 1, new Instruction { 
                            OpCode = OpCode.PACK
                        });
                        
                        // Remove the original SETITEM sequences
                        for (int k = 0; k < items.Count; k++)
                        {
                            instructions.RemoveAt(i + items.Count + 2);
                            instructions.RemoveAt(i + items.Count + 2);
                            instructions.RemoveAt(i + items.Count + 2);
                        }
                        
                        modified = true;
                        i = i + items.Count + 1; // Skip ahead
                    }
                }
            }

            return modified;
        }

        /// <summary>
        /// Checks if the instruction at the given index is an APPEND operation.
        /// </summary>
        /// <param name="instructions">The list of instructions.</param>
        /// <param name="index">The index to check.</param>
        /// <returns>True if the instruction is an APPEND operation, false otherwise.</returns>
        private static bool IsAppendOperation(List<Instruction> instructions, int index)
        {
            return index + 1 < instructions.Count && instructions[index + 1].OpCode == OpCode.APPEND;
        }

        /// <summary>
        /// Gets the PUSH OpCode for the given value.
        /// </summary>
        /// <param name="value">The value to push.</param>
        /// <returns>The corresponding PUSH OpCode.</returns>
        private static OpCode GetPushOpCode(int value)
        {
            return value switch
            {
                0 => OpCode.PUSH0,
                1 => OpCode.PUSH1,
                2 => OpCode.PUSH2,
                3 => OpCode.PUSH3,
                4 => OpCode.PUSH4,
                5 => OpCode.PUSH5,
                6 => OpCode.PUSH6,
                7 => OpCode.PUSH7,
                8 => OpCode.PUSH8,
                9 => OpCode.PUSH9,
                10 => OpCode.PUSH10,
                11 => OpCode.PUSH11,
                12 => OpCode.PUSH12,
                13 => OpCode.PUSH13,
                14 => OpCode.PUSH14,
                15 => OpCode.PUSH15,
                16 => OpCode.PUSH16,
                _ => OpCode.PUSHINT8
            };
        }

        /// <summary>
        /// Checks if the given OpCode is a PUSH OpCode.
        /// </summary>
        /// <param name="opCode">The OpCode to check.</param>
        /// <returns>True if the OpCode is a PUSH OpCode, false otherwise.</returns>
        private static bool IsPushOpCode(OpCode opCode)
        {
            return opCode >= OpCode.PUSH0 && opCode <= OpCode.PUSH16;
        }

        /// <summary>
        /// Gets the value of a PUSH OpCode.
        /// </summary>
        /// <param name="opCode">The PUSH OpCode.</param>
        /// <returns>The value of the PUSH OpCode.</returns>
        private static int GetPushOpCodeValue(OpCode opCode)
        {
            return (int)opCode - (int)OpCode.PUSH0;
        }
    }
}
