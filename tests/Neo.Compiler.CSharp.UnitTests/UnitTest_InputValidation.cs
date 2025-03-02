// Copyright (C) 2015-2024 The Neo Project.
//
// UnitTest_InputValidation.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_InputValidation : TestBase
    {
        [TestMethod]
        public void TestValidateInput()
        {
            // Use reflection to access the private ValidateInput method
            MethodInfo? method = typeof(CompilationContext).GetMethod("ValidateInput", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ValidateInput method should exist");

            // Create a CompilationContext instance using a mock compilation engine and symbol
            var mockEngine = new CompilationEngine(new CompilationOptions());
            var mockSymbol = Substitute.For<INamedTypeSymbol>();
            var context = new CompilationContext(mockEngine, mockSymbol);

            // Test null contract name
            Assert.ThrowsException<CompilationException>(() => 
                method.Invoke(context, new object[] { null, new[] { "NEP-17" } }));

            // Test empty contract name
            Assert.ThrowsException<CompilationException>(() => 
                method.Invoke(context, new object[] { "", new[] { "NEP-17" } }));

            // Test contract name too long
            Assert.ThrowsException<CompilationException>(() => 
                method.Invoke(context, new object[] { new string('a', 129), new[] { "NEP-17" } }));

            // Test null supported standards
            Assert.ThrowsException<CompilationException>(() => 
                method.Invoke(context, new object[] { "TestContract", null }));

            // Test empty standard name
            Assert.ThrowsException<CompilationException>(() => 
                method.Invoke(context, new object[] { "TestContract", new[] { "" } }));

            // Test standard name too long
            Assert.ThrowsException<CompilationException>(() => 
                method.Invoke(context, new object[] { "TestContract", new[] { new string('a', 33) } }));

            // Test valid input
            method.Invoke(context, new object[] { "TestContract", new[] { "NEP-17" } });
        }

        [TestMethod]
        public void TestCreateManifestValidation()
        {
            // Test that CreateManifest calls ValidateInput
            // This is an integration test that verifies the validation is actually used

            // Create a test contract with a valid name and standards
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;
                using System.ComponentModel;

                namespace Neo.SmartContract
                {
                    [DisplayName(""TestContract"")]
                    [SupportedStandards(""NEP-17"")]
                    public class Contract1 : Framework.SmartContract
                    {
                        public static void Main() { }
                    }
                }";

            // This should compile successfully
            var compilation = CompileScript(testString, "Contract1");
            Assert.IsNotNull(compilation.manifest, "Manifest creation should succeed with valid input");

            // Test with an extremely long contract name
            var longNameString = @"
                using Neo.SmartContract.Framework;
                using System;
                using System.ComponentModel;

                namespace Neo.SmartContract
                {
                    [DisplayName(""" + new string('a', 129) + @""")]
                    public class Contract2 : Framework.SmartContract
                    {
                        public static void Main() { }
                    }
                }";

            // This should throw a CompilationException
            Assert.ThrowsException<CompilationException>(() => CompileScript(longNameString, "Contract2"));

            // Test with an extremely long standard name
            var longStandardString = @"
                using Neo.SmartContract.Framework;
                using System;
                using System.ComponentModel;

                namespace Neo.SmartContract
                {
                    [SupportedStandards(""" + new string('a', 33) + @""")]
                    public class Contract3 : Framework.SmartContract
                    {
                        public static void Main() { }
                    }
                }";

            // This should throw a CompilationException
            Assert.ThrowsException<CompilationException>(() => CompileScript(longStandardString, "Contract3"));
        }
    }
}
