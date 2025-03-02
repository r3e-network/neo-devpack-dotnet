// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_RefLocalsReturns.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.SmartContract.Testing;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_RefLocalsReturns : DebugAndTestBase<Contract_RefLocalsReturns>
    {
        [TestMethod]
        public void TestRefLocal()
        {
            Assert.AreEqual(20, Contract.TestRefLocal());
        }

        [TestMethod]
        public void TestRefReturn()
        {
            Assert.AreEqual(42, Contract.TestRefReturn());
        }

        [TestMethod]
        public void TestRefLocalWithAssignment()
        {
            Assert.AreEqual(70, Contract.TestRefLocalWithAssignment());
        }

        [TestMethod]
        public void TestRefReadonlyReturn()
        {
            Assert.AreEqual(2, Contract.TestRefReadonlyReturn());
        }

        [TestMethod]
        public void TestRefStructField()
        {
            Assert.AreEqual(30, Contract.TestRefStructField());
        }

        [TestMethod]
        public void TestRefParameter()
        {
            Assert.AreEqual(42, Contract.TestRefParameter());
        }
    }
}
