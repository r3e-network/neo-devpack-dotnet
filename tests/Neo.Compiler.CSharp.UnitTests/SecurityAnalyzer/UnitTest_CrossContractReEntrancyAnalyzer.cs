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
using Neo.Compiler.CSharp.SecurityAnalyzer;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.VM;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Compiler.CSharp.UnitTests.SecurityAnalyzer
{
    [TestClass]
    public class UnitTest_CrossContractReEntrancyAnalyzer : TestBase
    {
        [TestMethod]
        public void TestCrossContractReEntrancyDetection()
        {
            // Create a test contract with potential cross-contract re-entrancy vulnerability
            var testString = @"
                using Neo.SmartContract.Framework;
                using Neo.SmartContract.Framework.Services;
                using System;
                using System.Numerics;

                public class Contract1 : Framework.SmartContract
                {
                    public static void VulnerableMethod(UInt160 contractHash, string method)
                    {
                        // First write to storage
                        Storage.Put(""key"", ""value"");
                        
                        // Then call another contract
                        Contract.Call(contractHash, method, CallFlags.All, new object[] { });
                        
                        // Write to storage again
                        Storage.Put(""key2"", ""value2"");
                    }
                    
                    public static void SafeMethod(UInt160 contractHash, string method)
                    {
                        // First call another contract
                        Contract.Call(contractHash, method, CallFlags.All, new object[] { });
                        
                        // Then write to storage
                        Storage.Put(""key"", ""value"");
                    }
                }";

            // Compile the contract
            var compilation = CompileScript(testString, "Contract1");
            
            // Run the cross-contract re-entrancy analyzer
            var result = CrossContractReEntrancyAnalyzer.AnalyzeCrossContractReEntrancy(
                compilation.nef, compilation.manifest, compilation.debugInfo);
            
            // Get the warning information
            string warnings = result.GetWarningInfo();
            
            // Verify that the analyzer detected the vulnerability
            Assert.IsTrue(warnings.Contains("Potential cross-contract re-entrancy vulnerability"), 
                "The analyzer should detect the potential vulnerability");
            
            // Verify that the warning contains the correct method name
            Assert.IsTrue(warnings.Contains("System.Storage.Put"), 
                "The warning should mention the storage operation");
            Assert.IsTrue(warnings.Contains("System.Contract.Call"), 
                "The warning should mention the contract call");
        }
        
        [TestMethod]
        public void TestNoVulnerabilityDetection()
        {
            // Create a test contract without cross-contract re-entrancy vulnerability
            var testString = @"
                using Neo.SmartContract.Framework;
                using Neo.SmartContract.Framework.Services;
                using System;
                using System.Numerics;

                public class Contract2 : Framework.SmartContract
                {
                    public static void SafeMethod1(UInt160 contractHash, string method)
                    {
                        // First call another contract
                        Contract.Call(contractHash, method, CallFlags.All, new object[] { });
                        
                        // Then write to storage
                        Storage.Put(""key"", ""value"");
                    }
                    
                    public static void SafeMethod2()
                    {
                        // Only write to storage, no contract calls
                        Storage.Put(""key"", ""value"");
                        Storage.Put(""key2"", ""value2"");
                    }
                    
                    public static void SafeMethod3(UInt160 contractHash, string method)
                    {
                        // Only contract calls, no storage writes
                        Contract.Call(contractHash, method, CallFlags.All, new object[] { });
                        Contract.Call(contractHash, method, CallFlags.All, new object[] { });
                    }
                }";

            // Compile the contract
            var compilation = CompileScript(testString, "Contract2");
            
            // Run the cross-contract re-entrancy analyzer
            var result = CrossContractReEntrancyAnalyzer.AnalyzeCrossContractReEntrancy(
                compilation.nef, compilation.manifest, compilation.debugInfo);
            
            // Get the warning information
            string warnings = result.GetWarningInfo();
            
            // Verify that the analyzer did not detect any vulnerabilities
            Assert.IsTrue(warnings.Contains("No cross-contract re-entrancy vulnerabilities detected"), 
                "The analyzer should not detect any vulnerabilities");
        }
    }
}
