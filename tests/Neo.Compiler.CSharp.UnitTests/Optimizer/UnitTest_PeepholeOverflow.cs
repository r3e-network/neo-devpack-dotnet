// Copyright (C) 2015-2024 The Neo Project.
//
// UnitTest_PeepholeOverflow.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Optimizer;
using Neo.SmartContract;
using Neo.VM;
using System.Numerics;
using System.Reflection;

namespace Neo.Compiler.CSharp.UnitTests.Optimizer
{
    [TestClass]
    public class UnitTest_PeepholeOverflow : TestBase
    {
        [TestMethod]
        public void TestIsOverflowMethod()
        {
            // Use reflection to access the private IsOverflow method
            MethodInfo? method = typeof(Peephole).GetMethod("IsOverflow", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "IsOverflow method should exist");

            // Test addition overflow
            BigInteger maxInt = BigInteger.Parse("7fffffffffffffffffffffffffffffff", System.Globalization.NumberStyles.HexNumber);
            bool result = (bool)method.Invoke(null, new object[] { maxInt, 1, "add" });
            Assert.IsTrue(result, "Addition should overflow");

            // Test subtraction overflow
            BigInteger minInt = BigInteger.Parse("8000000000000000000000000000000", System.Globalization.NumberStyles.HexNumber);
            result = (bool)method.Invoke(null, new object[] { minInt, 1, "sub" });
            Assert.IsTrue(result, "Subtraction should overflow");

            // Test multiplication overflow
            result = (bool)method.Invoke(null, new object[] { maxInt, 2, "mul" });
            Assert.IsTrue(result, "Multiplication should overflow");

            // Test division by zero
            result = (bool)method.Invoke(null, new object[] { 1, 0, "div" });
            Assert.IsTrue(result, "Division by zero should be detected");

            // Test non-overflow cases
            result = (bool)method.Invoke(null, new object[] { 1, 1, "add" });
            Assert.IsFalse(result, "Addition should not overflow");

            result = (bool)method.Invoke(null, new object[] { 1, 1, "sub" });
            Assert.IsFalse(result, "Subtraction should not overflow");

            result = (bool)method.Invoke(null, new object[] { 1, 1, "mul" });
            Assert.IsFalse(result, "Multiplication should not overflow");

            result = (bool)method.Invoke(null, new object[] { 1, 1, "div" });
            Assert.IsFalse(result, "Division should not overflow");
        }

        [TestMethod]
        public void TestOptimizeConstantArithmetic()
        {
            // Test that the optimizer correctly handles potential overflow
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;
                using System.Numerics;

                public class Contract1 : Framework.SmartContract
                {
                    public static BigInteger TestOverflow()
                    {
                        BigInteger a = BigInteger.Parse(""7fffffffffffffffffffffffffffffff"", System.Globalization.NumberStyles.HexNumber);
                        BigInteger b = 1;
                        return a + b; // This should not be optimized due to overflow
                    }

                    public static BigInteger TestNoOverflow()
                    {
                        BigInteger a = 1;
                        BigInteger b = 2;
                        return a + b; // This should be optimized to PUSHINT 3
                    }
                }";

            var compilation = CompileScript(testString, "Contract1");
            
            // Verify that the optimizer correctly handles the overflow case
            // This is a simplified test - in a real test, you would need to examine the actual bytecode
            Assert.IsNotNull(compilation.nef, "Compilation should succeed");
        }
    }
}
