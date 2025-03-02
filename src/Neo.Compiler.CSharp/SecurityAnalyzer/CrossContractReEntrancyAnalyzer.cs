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
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Compiler.CSharp.SecurityAnalyzer
{
    /// <summary>
    /// Analyzer for detecting cross-contract re-entrancy vulnerabilities.
    /// This analyzer identifies patterns where a contract calls another contract that could potentially
    /// call back into the original contract or manipulate state in unexpected ways.
    /// </summary>
    public class CrossContractReEntrancyAnalyzer
    {
        private readonly NefFile _nef;
        private readonly ContractManifest _manifest;
        private readonly JToken? _debugInfo;
        private readonly List<string> _warnings = new();

        private CrossContractReEntrancyAnalyzer(NefFile nef, ContractManifest manifest, JToken? debugInfo)
        {
            _nef = nef;
            _manifest = manifest;
            _debugInfo = debugInfo;
        }

        /// <summary>
        /// Analyzes a contract for potential cross-contract re-entrancy vulnerabilities.
        /// </summary>
        /// <param name="nef">The NEF file of the contract.</param>
        /// <param name="manifest">The manifest of the contract.</param>
        /// <param name="debugInfo">Debug information for the contract.</param>
        /// <returns>An instance of CrossContractReEntrancyAnalyzer containing analysis results.</returns>
        public static CrossContractReEntrancyAnalyzer AnalyzeCrossContractReEntrancy(NefFile nef, ContractManifest manifest, JToken? debugInfo = null)
        {
            var analyzer = new CrossContractReEntrancyAnalyzer(nef, manifest, debugInfo);
            analyzer.Analyze();
            return analyzer;
        }

        /// <summary>
        /// Gets warning information from the analysis.
        /// </summary>
        /// <param name="print">Whether to print the warnings to the console.</param>
        /// <returns>A string containing the warning information.</returns>
        public string GetWarningInfo(bool print = false)
        {
            if (_warnings.Count == 0)
                return "[SEC] No cross-contract re-entrancy vulnerabilities detected.";

            string result = $"[SEC] Found {_warnings.Count} potential cross-contract re-entrancy vulnerabilities:";
            foreach (var warning in _warnings)
            {
                result += Environment.NewLine + warning;
                if (print) Console.WriteLine(warning);
            }
            return result;
        }

        private void Analyze()
        {
            // Get the script from the NEF file
            byte[] script = _nef.Script;
            
            // Create a script object to analyze
            Script scriptObj = new(script);
            
            // Track contract calls and storage operations
            Dictionary<int, (string method, bool isContractCall)> operations = new();
            Dictionary<int, int> storageWrites = new();
            Dictionary<int, List<int>> contractCalls = new();
            
            // Analyze the script for contract calls and storage operations
            for (int ip = 0; ip < script.Length;)
            {
                Instruction instruction;
                try
                {
                    instruction = scriptObj.GetInstruction(ip);
                }
                catch
                {
                    // Skip invalid instructions
                    ip++;
                    continue;
                }
                
                // Check for SYSCALL instructions
                if (instruction.OpCode == OpCode.SYSCALL)
                {
                    // Get the system call hash
                    uint syscallHash = BitConverter.ToUInt32(instruction.Operand.Span);
                    string? syscallName = InteropService.GetMethodName(syscallHash);
                    
                    if (syscallName != null)
                    {
                        // Track contract calls
                        if (syscallName.StartsWith("System.Contract.Call"))
                        {
                            operations[ip] = (syscallName, true);
                            
                            // Find the most recent storage write
                            foreach (var write in storageWrites)
                            {
                                if (write.Key < ip)
                                {
                                    if (!contractCalls.ContainsKey(write.Key))
                                        contractCalls[write.Key] = new List<int>();
                                    
                                    contractCalls[write.Key].Add(ip);
                                }
                            }
                        }
                        // Track storage operations
                        else if (syscallName.StartsWith("System.Storage.Put") || 
                                 syscallName.StartsWith("System.Storage.Delete"))
                        {
                            operations[ip] = (syscallName, false);
                            storageWrites[ip] = ip;
                        }
                    }
                }
                
                ip += instruction.Size;
            }
            
            // Analyze for potential cross-contract re-entrancy vulnerabilities
            foreach (var entry in contractCalls)
            {
                int storageWriteIp = entry.Key;
                List<int> callIps = entry.Value;
                
                foreach (var callIp in callIps)
                {
                    // Get method names for better reporting
                    string storageMethod = operations[storageWriteIp].method;
                    string callMethod = operations[callIp].method;
                    
                    // Get source code information if available
                    string sourceInfo = GetSourceInfo(storageWriteIp, callIp);
                    
                    // Add warning
                    _warnings.Add($"Potential cross-contract re-entrancy vulnerability: Storage operation '{storageMethod}' at 0x{storageWriteIp:X} " +
                                 $"followed by contract call '{callMethod}' at 0x{callIp:X}. {sourceInfo}");
                }
            }
        }

        private string GetSourceInfo(int storageWriteIp, int callIp)
        {
            if (_debugInfo == null)
                return "No debug information available.";
            
            try
            {
                // Try to get method names and line numbers from debug info
                var methods = _debugInfo["methods"];
                if (methods == null)
                    return "No method information in debug data.";
                
                string storageMethod = "unknown";
                string callMethod = "unknown";
                int storageLine = -1;
                int callLine = -1;
                
                foreach (JObject method in methods)
                {
                    var sequencePoints = method["sequence-points"];
                    if (sequencePoints == null) continue;
                    
                    foreach (JObject sp in sequencePoints)
                    {
                        int address = (int)sp["address"];
                        int line = (int)sp["line"];
                        
                        if (address <= storageWriteIp && (storageLine == -1 || address > storageLine))
                        {
                            storageMethod = (string)method["name"];
                            storageLine = line;
                        }
                        
                        if (address <= callIp && (callLine == -1 || address > callLine))
                        {
                            callMethod = (string)method["name"];
                            callLine = line;
                        }
                    }
                }
                
                return $"Storage operation in method '{storageMethod}' at line {storageLine}, " +
                       $"contract call in method '{callMethod}' at line {callLine}.";
            }
            catch
            {
                return "Error parsing debug information.";
            }
        }
    }
}
