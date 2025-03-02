// Copyright (C) 2015-2024 The Neo Project.
//
// UnitTest_CrossContractReEntrancyAnalyzer.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Compiler.SecurityAnalyzer;
using Neo.SmartContract.Testing;
using System.Linq;

namespace Neo.Compiler.CSharp.UnitTests.SecurityAnalyzer
{
    [TestClass]
    public class UnitTest_CrossContractReEntrancyAnalyzer : TestBase
    {
        [TestMethod]
        public void TestCrossContractReEntrancyDetection()
        {
            var testString = @"
                using Neo.SmartContract.Framework;
                using Neo.SmartContract.Framework.Services;
                using System;
                using System.ComponentModel;

                namespace Neo.SmartContract
                {
                    public class Contract1 : Framework.SmartContract
                    {
                        public static void Main()
                        {
                            // Call contract B
                            ContractManagement.Call(UInt160.Zero, ""method"", CallFlags.All, new object[0]);
                            
                            // Call contract C - potential cross-contract re-entrancy
                            ContractManagement.Call(UInt160.Parse(""0x0000000000000000000000000000000000000001""), ""method"", CallFlags.All, new object[0]);
                        }
                    }
                }";

            var compilation = CompileScript(testString, "Contract1");
            var result = CrossContractReEntrancyAnalyzer.AnalyzeCrossContractReEntrancy(compilation.nef, compilation.manifest);
            
            Assert.IsTrue(result.vulnerabilityPairs.Count > 0, "Should detect potential cross-contract re-entrancy");
            Assert.IsTrue(result.GetWarningInfo().Contains("Potential Cross-Contract Re-entrancy"));
        }

        [TestMethod]
        public void TestNoCrossContractReEntrancy()
        {
            var testString = @"
                using Neo.SmartContract.Framework;
                using Neo.SmartContract.Framework.Services;
                using System;
                using System.ComponentModel;

                namespace Neo.SmartContract
                {
                    public class Contract2 : Framework.SmartContract
                    {
                        public static void Main()
                        {
                            // Call contract B
                            ContractManagement.Call(UInt160.Zero, ""method"", CallFlags.All, new object[0]);
                            
                            // No call to another contract
                        }
                    }
                }";

            var compilation = CompileScript(testString, "Contract2");
            var result = CrossContractReEntrancyAnalyzer.AnalyzeCrossContractReEntrancy(compilation.nef, compilation.manifest);
            
            Assert.IsTrue(result.vulnerabilityPairs.Count == 0, "Should not detect cross-contract re-entrancy");
            Assert.IsTrue(string.IsNullOrEmpty(result.GetWarningInfo()));
        }
    }
}
