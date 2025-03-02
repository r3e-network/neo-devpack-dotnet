// Copyright (C) 2015-2024 The Neo Project.
//
// CrossContractReEntrancyAnalyzer.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.Json;
using Neo.Optimizer;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo.Compiler.SecurityAnalyzer
{
    /// <summary>
    /// Cross-contract re-entrancy can happen when your contract A calls an untrusted contract B,
    /// then B calls another contract C, changing the storage of C,
    /// finally you call C.
    /// </summary>
    public static class CrossContractReEntrancyAnalyzer
    {
        public class CrossContractReEntrancyVulnerabilityPair
        {
            // key block calls another contract; value blocks call a third contract
            public readonly Dictionary<BasicBlock, HashSet<BasicBlock>> vulnerabilityPairs;
            // values are instruction addresses
            // where this basic block calls contract
            public readonly Dictionary<BasicBlock, HashSet<int>> callOtherContractInstructions;
            public JToken? DebugInfo { get; init; }

            public CrossContractReEntrancyVulnerabilityPair(
                Dictionary<BasicBlock, HashSet<BasicBlock>> vulnerabilityPairs,
                Dictionary<BasicBlock, HashSet<int>> callOtherContractInstructions,
                JToken? debugInfo = null)
            {
                this.vulnerabilityPairs = vulnerabilityPairs
                    .Where(v => v.Value.Count > 0).ToDictionary();
                this.callOtherContractInstructions = callOtherContractInstructions;
                DebugInfo = debugInfo;
            }

            public string GetWarningInfo(bool print = false)
            {
                if (vulnerabilityPairs.Count <= 0) return "";
                StringBuilder result = new();
                foreach ((BasicBlock callBlock, HashSet<BasicBlock> thirdContractBlocks) in vulnerabilityPairs)
                {
                    StringBuilder additional = new();
                    additional.AppendLine($"[SEC] Potential Cross-Contract Re-entrancy: Calling contracts at instruction address: " +
                        $"{string.Join(", ", callOtherContractInstructions[callBlock])} before calling another contract at");
                    foreach (BasicBlock thirdContractBlock in thirdContractBlocks)
                        additional.AppendLine($"\t{string.Join(", ", callOtherContractInstructions[thirdContractBlock])}");
                    if (print)
                        Console.Write(additional.ToString());
                    result.Append(additional);
                }
                return result.ToString();
            }
        }

        /// <summary>
        /// This method finds all cases where your contract A first calls another contract B,
        /// and then you call a third contract C.
        /// 
        /// This helps prevent cross-contract re-entrancy, where
        /// your contract A calls an untrusted contract B,
        /// then B calls another contract C, changing the storage of C,
        /// finally you call C.
        /// </summary>
        /// <param name="nef">Nef file</param>
        /// <param name="manifest">Manifest</param>
        /// <param name="debugInfo">Debug information</param>
        public static CrossContractReEntrancyVulnerabilityPair AnalyzeCrossContractReEntrancy
            (NefFile nef, ContractManifest manifest, JToken? debugInfo = null)
        {
            ContractInBasicBlocks contractInBasicBlocks = new(nef, manifest, debugInfo);
            List<BasicBlock> basicBlocks = contractInBasicBlocks.sortedBasicBlocks;
            // key block calls another contract; value blocks call a third contract
            Dictionary<BasicBlock, HashSet<BasicBlock>> vulnerabilityPairs =
                basicBlocks.ToDictionary(b => b, b => new HashSet<BasicBlock>());
            // Whether each basic block may call other contract
            Dictionary<BasicBlock, HashSet<int>> callOtherContractInstructions =
                basicBlocks.ToDictionary(b => b, b => new HashSet<int>());
            // Detect basic blocks that call other contracts
            foreach (BasicBlock b in basicBlocks)
            {
                int addr = b.startAddr;
                foreach (VM.Instruction instruction in b.instructions)
                {
                    if (instruction.OpCode == VM.OpCode.SYSCALL && instruction.TokenU32 == ApplicationEngine.System_Contract_Call.Hash)
                        callOtherContractInstructions[b].Add(addr);
                    addr += instruction.Size;
                }
            }

            // For each basic block that calls a contract,
            // find downstream blocks that call another contract
            foreach (BasicBlock block in basicBlocks.Where(b => callOtherContractInstructions[b].Count > 0))
            {
                HashSet<BasicBlock> visited = new();
                Queue<BasicBlock> q = new();
                if (block.nextBlock != null)
                    q.Enqueue(block.nextBlock);
                foreach (BasicBlock jumpTarget in block.jumpTargetBlocks)
                    q.Enqueue(jumpTarget);
                while (q.Count > 0)
                {
                    BasicBlock current = q.Dequeue();
                    visited.Add(current);
                    if (callOtherContractInstructions[current].Count > 0)
                        vulnerabilityPairs[block].Add(current);
                    if (current.nextBlock != null && !visited.Contains(current.nextBlock))
                        q.Enqueue(current.nextBlock);
                    foreach (BasicBlock jumpTarget in current.jumpTargetBlocks)
                        if (!visited.Contains(jumpTarget))
                            q.Enqueue(jumpTarget);
                }
            }
            return new(vulnerabilityPairs, callOtherContractInstructions, debugInfo);
        }
    }
}
