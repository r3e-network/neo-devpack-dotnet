// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_LocalFunctions.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.SmartContract.Testing;
using Neo.VM.Types;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_LocalFunctions : DebugAndTestBase<Contract_LocalFunctions>
    {
        [TestMethod]
        public void TestSimpleLocalFunction()
        {
            Assert.AreEqual(new Integer(12), Contract.TestSimpleLocalFunction());
        }

        [TestMethod]
        public void TestLocalFunctionWithParameters()
        {
            Assert.AreEqual(new Integer(15), Contract.TestLocalFunctionWithParameters(3, 5));
            Assert.AreEqual(new Integer(24), Contract.TestLocalFunctionWithParameters(6, 4));
        }

        [TestMethod]
        public void TestExpressionBodiedLocalFunction()
        {
            Assert.AreEqual(new Integer(16), Contract.TestExpressionBodiedLocalFunction());
        }

        [TestMethod]
        public void TestLocalFunctionWithCapturedVariables()
        {
            Assert.AreEqual(new Integer(50), Contract.TestLocalFunctionWithCapturedVariables());
        }

        [TestMethod]
        public void TestNestedLocalFunctions()
        {
            Assert.AreEqual(new Integer(14), Contract.TestNestedLocalFunctions());
        }

        [TestMethod]
        public void TestLocalFunctionWithConditional()
        {
            Assert.AreEqual(new Integer(10), Contract.TestLocalFunctionWithConditional(true));
            Assert.AreEqual(new Integer(20), Contract.TestLocalFunctionWithConditional(false));
        }

        [TestMethod]
        public void TestLocalFunctionWithLoop()
        {
            Assert.AreEqual(new Integer(15), Contract.TestLocalFunctionWithLoop());
        }

        [TestMethod]
        public void TestLocalFunctionWithDefaultParameter()
        {
            Assert.AreEqual(new Integer(30), Contract.TestLocalFunctionWithDefaultParameter());
        }
    }
}
