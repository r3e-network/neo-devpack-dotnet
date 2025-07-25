// Copyright (C) 2015-2025 The Neo Project.
//
// DumpNef.cs file belongs to the neo project and is free
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
using Neo.SmartContract.Native;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Neo.Optimizer
{
    public static class DumpNef
    {
        internal static readonly Regex DocumentRegex = new(@"\[(\d+)\](\d+)\:(\d+)\-(\d+)\:(\d+)", RegexOptions.Compiled);
        internal static readonly Regex RangeRegex = new(@"(\d+)\-(\d+)", RegexOptions.Compiled);
        internal static readonly Regex SequencePointRegex = new(@"(\d+)(\[\d+\]\d+\:\d+\-\d+\:\d+)", RegexOptions.Compiled);

        static readonly Lazy<IReadOnlyDictionary<uint, string>> sysCallNames = new(
            () => ApplicationEngine.Services.ToImmutableDictionary(kvp => kvp.Value.Hash, kvp => kvp.Value.Name));

        public static byte[] ZipDebugInfo(byte[] content, string innerFilename)
        {
            using var compressedFileStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, false))
            {
                var zipEntry = zipArchive.CreateEntry(innerFilename);
                using var originalFileStream = new MemoryStream(content);
                using var zipEntryStream = zipEntry.Open();
                originalFileStream.CopyTo(zipEntryStream);
            }
            return compressedFileStream.ToArray();
        }

        public static string UnzipDebugInfo(byte[] zippedBuffer)
        {
            using var zippedStream = new MemoryStream(zippedBuffer);
            using var archive = new ZipArchive(zippedStream, ZipArchiveMode.Read, false, Encoding.UTF8);
            var entry = archive.Entries.FirstOrDefault();
            if (entry != null)
            {
                using var unzippedEntryStream = entry.Open();
                using var ms = new MemoryStream();
                unzippedEntryStream.CopyTo(ms);
                var unzippedArray = ms.ToArray();
                return Encoding.UTF8.GetString(unzippedArray);
            }
            throw new ArgumentException("No file found in zip archive");
        }

        public static string GetInstructionAddressPadding(this Script script)
        {
            var digitCount = EnumerateInstructions(script).Last().address switch
            {
                var x when x < 10 => 1,
                var x when x < 100 => 2,
                var x when x < 1000 => 3,
                var x when x < 10000 => 4,
                var x when x <= ushort.MaxValue => 5,
                _ => throw new Exception($"Max script length is {ushort.MaxValue} bytes"),
            };
            return new string('0', digitCount);
        }

        public static string WriteInstruction(int address, Instruction instruction, string padString, MethodToken[] tokens)
        {
            string result = "";
            try
            {
                result += $"{address.ToString(padString)}";
                result += $" {instruction.OpCode}";
                if (!instruction.Operand.IsEmpty)
                    result += $" {instruction.GetOperandString()}";

                var comment = instruction.GetComment(address, tokens);
                if (comment.Length > 0)
                    result += $" # {comment}";
            }
            finally { }
            return result;
        }

        /// <summary>
        /// DO NOT use this very often. It builds new instruction objects,
        /// while the optimizer compares instruction objects using ReferenceEquals
        /// </summary>
        /// <param name="script"></param>
        /// <param name="print">Console.WriteLine all instructions for debugging</param>
        /// <returns></returns>
        public static IEnumerable<(int address, Instruction instruction)> EnumerateInstructions(this Script script, bool print = false)
        {
            int address = 0;
            OpCode opcode = OpCode.PUSH0;
            Instruction instruction;
            for (; address < script.Length; address += instruction.Size)
            {
                instruction = script.GetInstruction(address);
                opcode = instruction.OpCode;
                if (print)
                    Console.WriteLine(WriteInstruction(address, instruction, "0000", []));
                yield return (address, instruction);
            }
            if (opcode != OpCode.RET)
                yield return (address, Instruction.RET);
        }

        public static string GetOperandString(this Instruction instruction) => BitConverter.ToString(instruction.Operand.Span.ToArray());

        public static string GetComment(this Instruction instruction, int ip, MethodToken[]? tokens = null)
        {
            tokens ??= [];

            switch (instruction.OpCode)
            {
                case OpCode.PUSHINT8:
                case OpCode.PUSHINT16:
                case OpCode.PUSHINT32:
                case OpCode.PUSHINT64:
                case OpCode.PUSHINT128:
                case OpCode.PUSHINT256:
                    return $"{new BigInteger(instruction.Operand.Span)}";
                case OpCode.PUSHA:
                    return $"{checked(ip + instruction.TokenI32)}";
                case OpCode.PUSHDATA1:
                case OpCode.PUSHDATA2:
                case OpCode.PUSHDATA4:
                    {
                        var text = System.Text.Encoding.UTF8.GetString(instruction.Operand.Span)
                            .Replace("\r", "\"\\r\"").Replace("\n", "\"\\n\"");
                        if (instruction.Operand.Length == 20)
                        {
                            return $"as script hash: {new UInt160(instruction.Operand.Span)}, as text: \"{text}\"";
                        }
                        return $"as text: \"{text}\"";
                    }
                case OpCode.JMP:
                case OpCode.JMPIF:
                case OpCode.JMPIFNOT:
                case OpCode.JMPEQ:
                case OpCode.JMPNE:
                case OpCode.JMPGT:
                case OpCode.JMPGE:
                case OpCode.JMPLT:
                case OpCode.JMPLE:
                case OpCode.CALL:
                    return OffsetComment(instruction.TokenI8);
                case OpCode.JMP_L:
                case OpCode.JMPIF_L:
                case OpCode.JMPIFNOT_L:
                case OpCode.JMPEQ_L:
                case OpCode.JMPNE_L:
                case OpCode.JMPGT_L:
                case OpCode.JMPGE_L:
                case OpCode.JMPLT_L:
                case OpCode.JMPLE_L:
                case OpCode.CALL_L:
                    return OffsetComment(instruction.TokenI32);
                case OpCode.CALLT:
                    {
                        int index = instruction.TokenU16;
                        if (index >= tokens.Length)
                            return $"Unknown token {instruction.TokenU16}";
                        var token = tokens[index];
                        var contract = NativeContract.Contracts.SingleOrDefault(c => c.Hash == token.Hash);
                        var tokenName = contract is null ? $"{token.Hash}" : contract.Name;
                        return $"{tokenName}.{token.Method} token call";
                    }
                case OpCode.TRY:
                    return TryComment(instruction.TokenI8, instruction.TokenI8_1);
                case OpCode.TRY_L:
                    return TryComment(instruction.TokenI32, instruction.TokenI32_1);
                case OpCode.ENDTRY:
                    return OffsetComment(instruction.TokenI8);
                case OpCode.ENDTRY_L:
                    return OffsetComment(instruction.TokenI32);
                case OpCode.SYSCALL:
                    return sysCallNames.Value.TryGetValue(instruction.TokenU32, out var name)
                        ? $"{name} SysCall"
                        : $"Unknown SysCall {instruction.TokenU32}";
                case OpCode.INITSSLOT:
                    return $"{instruction.TokenU8} static variables";
                case OpCode.INITSLOT:
                    return $"{instruction.TokenU8} local variables, {instruction.TokenU8_1} arguments";
                case OpCode.LDSFLD:
                case OpCode.STSFLD:
                case OpCode.LDLOC:
                case OpCode.STLOC:
                case OpCode.LDARG:
                case OpCode.STARG:
                    return $"Slot index {instruction.TokenU8}";
                case OpCode.NEWARRAY_T:
                case OpCode.ISTYPE:
                case OpCode.CONVERT:
                    return $"{(VM.Types.StackItemType)instruction.TokenU8} type";
                default:
                    return string.Empty;
            }

            string OffsetComment(int offset) => $"pos: {checked(ip + offset)} (offset: {offset})";
            string TryComment(int catchOffset, int finallyOffset)
            {
                var builder = new System.Text.StringBuilder();
                builder.Append(catchOffset == 0 ? "no catch block, " : $"catch {OffsetComment(catchOffset)}, ");
                builder.Append(finallyOffset == 0 ? "no finally block" : $"finally {OffsetComment(finallyOffset)}");
                return builder.ToString();
            }
        }

        public static (int start, int end) GetMethodStartEndAddress(string name, JToken debugInfo)
        {
            name = name.Length == 0 ? string.Empty : name[0].ToString().ToUpper() + name[1..];  // first letter uppercase
            int start = -1, end = -1;
            foreach (JToken? method in (JArray)debugInfo["methods"]!)
            {
                string methodName = method!["name"]!.AsString().Split(",")[1];
                if (methodName == name)
                {
                    GroupCollection rangeGroups = RangeRegex.Match(method["range"]!.AsString()).Groups;
                    (start, end) = (int.Parse(rangeGroups[1].ToString()), int.Parse(rangeGroups[2].ToString()));
                }
            }
            return (start, end);
        }

        public static List<int> OpCodeAddressesInMethod(NefFile nef, JToken DebugInfo, string method, OpCode opcode)
        {
            (int start, int end) = GetMethodStartEndAddress(method, DebugInfo);
            List<(int a, Instruction i)> instructions = EnumerateInstructions(nef.Script).ToList();
            return instructions.Where(
                ai => ai.i.OpCode == opcode &&
                ai.a >= start && ai.a <= end
                ).Select(ai => ai.a).ToList();
        }

        /// <summary>
        /// Dumps .nef assembly to a text file
        /// </summary>
        /// <param name="nef">The NEF file.</param>
        /// <param name="debugInfo">The debug information. If available, provides source code comments in the dumped assembly.</param>
        /// <param name="manifest">The contract manifest. Provides method start comments. Not necessary if debugInfo is available</param>
        /// <returns></returns>
        public static string GenerateDumpNef(NefFile nef, JToken? debugInfo, ContractManifest? manifest = null)
        {
            Script script = nef.Script;
            string addressPadding = script.GetInstructionAddressPadding();
            List<(int, Instruction)> addressAndInstructionsList = script.EnumerateInstructions().ToList();
            Dictionary<int, Instruction> addressToInstruction = [];
            foreach ((int a, Instruction i) in addressAndInstructionsList)
                addressToInstruction.Add(a, i);
            Dictionary<int, string> methodStartAddrToName = [];
            Dictionary<int, string> methodEndAddrToName = [];
            Dictionary<int, List<(int docId, int startLine, int startCol, int endLine, int endCol)>> newAddrToSequencePoint = [];

            if (debugInfo != null)
            {
                foreach (JToken? method in (JArray)debugInfo["methods"]!)
                {
                    GroupCollection rangeGroups = RangeRegex.Match(method!["range"]!.AsString()).Groups;
                    (int methodStartAddr, int methodEndAddr) = (int.Parse(rangeGroups[1].ToString()), int.Parse(rangeGroups[2].ToString()));
                    methodStartAddrToName.Add(methodStartAddr, method!["id"]!.AsString());  // TODO: same format of method name as dumpnef
                    methodEndAddrToName.Add(methodEndAddr, method["id"]!.AsString());

                    foreach (JToken? sequencePoint in (JArray)method["sequence-points"]!)
                    {
                        GroupCollection sequencePointGroups = SequencePointRegex.Match(sequencePoint!.AsString()).Groups;
                        GroupCollection documentGroups = DocumentRegex.Match(sequencePointGroups[2].ToString()).Groups;
                        int addr = int.Parse(sequencePointGroups[1].Value);
                        if (!newAddrToSequencePoint.ContainsKey(addr))
                            newAddrToSequencePoint.Add(addr, []);
                        newAddrToSequencePoint[addr].Add((
                            int.Parse(documentGroups[1].ToString()),
                            int.Parse(documentGroups[2].ToString()),
                            int.Parse(documentGroups[3].ToString()),
                            int.Parse(documentGroups[4].ToString()),
                            int.Parse(documentGroups[5].ToString())
                        ));
                    }
                }
            }

            if (manifest != null)
                foreach (ContractMethodDescriptor method in manifest.Abi.Methods)
                    methodStartAddrToName.TryAdd(method.Offset, method.Name);

            Dictionary<string, string[]> docPathToContent = [];
            StringBuilder dumpnef = new();
            foreach ((int a, _) in script.EnumerateInstructions(/*print: true*/).ToList())
            {
                if (methodStartAddrToName.TryGetValue(a, out var value))
                    dumpnef.AppendLine($"# Method Start {value}");
                if (methodEndAddrToName.TryGetValue(a, out value))
                    dumpnef.AppendLine($"# Method End {value}");
                if (newAddrToSequencePoint.TryGetValue(a, out var sequence))
                {
                    foreach ((int docId, int startLine, int startCol, int endLine, int endCol) in sequence)
                    {
                        string docPath = debugInfo!["documents"]![docId]!.AsString();
                        if (debugInfo["document-root"] != null)
                            docPath = Path.Combine(debugInfo["document-root"]!.AsString(), docPath);
                        if (!docPathToContent.ContainsKey(docPath))
                            docPathToContent.Add(docPath, [.. File.ReadAllLines(docPath)]);
                        if (startLine == endLine)
                            dumpnef.AppendLine($"# Code {Path.GetFileName(docPath)} line {startLine}: \"{docPathToContent[docPath][startLine - 1][(startCol - 1)..(endCol - 1)]}\"");
                        else
                            for (int lineIndex = startLine; lineIndex <= endLine; lineIndex++)
                            {
                                string src;
                                if (lineIndex == startLine)
                                    src = docPathToContent[docPath][lineIndex - 1][(startCol - 1)..].Trim();
                                else if (lineIndex == endLine)
                                    src = docPathToContent[docPath][lineIndex - 1][..(endCol - 1)].Trim();
                                else
                                    src = docPathToContent[docPath][lineIndex - 1].Trim();
                                dumpnef.AppendLine($"# Code {Path.GetFileName(docPath)} line {startLine}: \"{src}\"");
                            }
                    }
                }
                if (a < script.Length)
                    dumpnef.AppendLine($"{WriteInstruction(a, script.GetInstruction(a), addressPadding, nef.Tokens)}");
            }
            return dumpnef.ToString();
        }
    }
}
