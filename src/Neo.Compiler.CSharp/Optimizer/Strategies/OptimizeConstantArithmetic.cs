// Copyright (C) 2015-2024 The Neo Project.
//
// OptimizeConstantArithmetic.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.Json;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Neo.Optimizer
{
    public static partial class Peephole
    {
        /// <summary>
        /// Optimizes constant arithmetic operations by pre-computing them at compile time.
        /// This optimization checks for potential integer overflow to prevent security vulnerabilities.
        /// </summary>
        /// <param name="nef">Nef file</param>
        /// <param name="manifest">Manifest</param>
        /// <param name="debugInfo">Debug information</param>
        /// <returns></returns>
        [Strategy(Priority = 1 << 8)]
        public static (NefFile, ContractManifest, JObject?) OptimizeConstantArithmetic(NefFile nef, ContractManifest manifest, JObject? debugInfo = null)
        {
            ContractInBasicBlocks contractInBasicBlocks = new(nef, manifest, debugInfo);
            InstructionCoverage oldContractCoverage = contractInBasicBlocks.coverage;
            Dictionary<int, Instruction> oldAddressToInstruction = oldContractCoverage.addressToInstructions;
            (Dictionary<Instruction, Instruction> jumpSourceToTargets,
                Dictionary<Instruction, (Instruction, Instruction)> trySourceToTargets,
                Dictionary<Instruction, HashSet<Instruction>> jumpTargetToSources) =
                (oldContractCoverage.jumpInstructionSourceToTargets,
                oldContractCoverage.tryInstructionSourceToTargets,
                oldContractCoverage.jumpTargetToSources);
            Dictionary<int, int> oldSequencePointAddressToNew = new();
            System.Collections.Specialized.OrderedDictionary simplifiedInstructionsToAddress = new();
            int currentAddress = 0;
            
            foreach ((int oldStartAddr, List<Instruction> basicBlock) in contractInBasicBlocks.sortedListInstructions)
            {
                int oldAddr = oldStartAddr;
                for (int index = 0; index < basicBlock.Count; index++)
                {
                    // Check if we have at least 3 instructions (2 operands and an operator)
                    if (index + 2 < basicBlock.Count)
                    {
                        Instruction first = basicBlock[index];
                        Instruction second = basicBlock[index + 1];
                        Instruction operation = basicBlock[index + 2];
                        
                        // Check if both operands are constants
                        if (OpCodeTypes.pushInt.Contains(first.OpCode) && OpCodeTypes.pushInt.Contains(second.OpCode))
                        {
                            BigInteger a = new BigInteger(first.Operand.Span);
                            BigInteger b = new BigInteger(second.Operand.Span);
                            BigInteger? result = null;
                            string opType = "";
                            
                            // Determine the operation and check for overflow
                            switch (operation.OpCode)
                            {
                                case OpCode.ADD:
                                    opType = "add";
                                    if (!IsOverflow(a, b, opType))
                                        result = a + b;
                                    break;
                                case OpCode.SUB:
                                    opType = "sub";
                                    if (!IsOverflow(a, b, opType))
                                        result = a - b;
                                    break;
                                case OpCode.MUL:
                                    opType = "mul";
                                    if (!IsOverflow(a, b, opType))
                                        result = a * b;
                                    break;
                                case OpCode.DIV:
                                    opType = "div";
                                    if (!IsOverflow(a, b, opType))
                                        result = a / b;
                                    break;
                                case OpCode.MOD:
                                    if (b == 0) break; // Avoid division by zero
                                    result = a % b;
                                    break;
                                case OpCode.SHL:
                                    if (b < 0 || b > int.MaxValue) break; // Invalid shift
                                    try
                                    {
                                        result = a << (int)b;
                                    }
                                    catch (OverflowException)
                                    {
                                        // Skip optimization if overflow occurs
                                    }
                                    break;
                                case OpCode.SHR:
                                    if (b < 0 || b > int.MaxValue) break; // Invalid shift
                                    result = a >> (int)b;
                                    break;
                                case OpCode.AND:
                                    result = a & b;
                                    break;
                                case OpCode.OR:
                                    result = a | b;
                                    break;
                                case OpCode.XOR:
                                    result = a ^ b;
                                    break;
                                default:
                                    // Not an arithmetic operation we can optimize
                                    break;
                            }
                            
                            // If we have a valid result, replace the three instructions with a single PUSHINT
                            if (result.HasValue)
                            {
                                byte[] resultBytes = result.Value.ToByteArray();
                                Instruction pushResult;
                                
                                // Optimize for common small integers
                                if (result == 0)
                                    pushResult = new Script(new byte[] { (byte)OpCode.PUSH0 }).GetInstruction(0);
                                else if (result == 1)
                                    pushResult = new Script(new byte[] { (byte)OpCode.PUSH1 }).GetInstruction(0);
                                else if (result == -1)
                                    pushResult = new Script(new byte[] { (byte)OpCode.PUSHM1 }).GetInstruction(0);
                                else if (result >= 2 && result <= 16)
                                    pushResult = new Script(new byte[] { (byte)((byte)OpCode.PUSH2 + (byte)result - 2) }).GetInstruction(0);
                                else
                                {
                                    // Create a PUSHINT instruction with the result
                                    byte[] pushIntBytes = new byte[1 + resultBytes.Length];
                                    pushIntBytes[0] = (byte)OpCode.PUSHINT;
                                    Array.Copy(resultBytes, 0, pushIntBytes, 1, resultBytes.Length);
                                    pushResult = new Script(pushIntBytes).GetInstruction(0);
                                }
                                
                                // Add the optimized instruction
                                simplifiedInstructionsToAddress.Add(pushResult, currentAddress);
                                oldSequencePointAddressToNew.Add(oldAddr, currentAddress);
                                currentAddress += pushResult.Size;
                                
                                // Skip the three instructions we just replaced
                                oldAddr += first.Size + second.Size + operation.Size;
                                index += 2;
                                
                                // Retarget any jumps to the first instruction
                                OptimizedScriptBuilder.RetargetJump(first, pushResult,
                                    jumpSourceToTargets, trySourceToTargets, jumpTargetToSources);
                                
                                continue;
                            }
                        }
                    }
                    
                    // If we couldn't optimize, keep the original instruction
                    Instruction currentInstruction = basicBlock[index];
                    simplifiedInstructionsToAddress.Add(currentInstruction, currentAddress);
                    currentAddress += currentInstruction.Size;
                    oldAddr += currentInstruction.Size;
                }
            }
            
            return AssetBuilder.BuildOptimizedAssets(nef, manifest, debugInfo,
                simplifiedInstructionsToAddress,
                jumpSourceToTargets, trySourceToTargets,
                oldAddressToInstruction, oldSequencePointAddressToNew: oldSequencePointAddressToNew);
        }
    }
}
