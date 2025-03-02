// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_RecordTypes.cs file belongs to the neo project and is free
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
    public class UnitTest_RecordTypes : DebugAndTestBase<Contract_RecordTypes>
    {
        [TestMethod]
        public void TestCreatePositionalRecord()
        {
            var result = Contract.TestCreatePositionalRecord();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestAccessPositionalRecordProperties()
        {
            Assert.AreEqual("John", Contract.TestAccessPositionalRecordProperties());
        }

        [TestMethod]
        public void TestCreateRecordWithProperties()
        {
            var result = Contract.TestCreateRecordWithProperties();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestAccessRecordProperties()
        {
            Assert.AreEqual("Engineering", Contract.TestAccessRecordProperties());
        }

        [TestMethod]
        public void TestRecordMethods()
        {
            Assert.AreEqual("Alice, 25, Engineering", Contract.TestRecordMethods());
        }

        [TestMethod]
        public void TestRecordWithDefaultValues()
        {
            var result = Contract.TestRecordWithDefaultValues();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestRecordEquality()
        {
            Assert.IsTrue(Contract.TestRecordEquality());
        }

        [TestMethod]
        public void TestRecordInequality()
        {
            Assert.IsTrue(Contract.TestRecordInequality());
        }

        [TestMethod]
        public void TestWithExpression()
        {
            var result = Contract.TestWithExpression();
            Assert.IsNotNull(result);
        }
    }
}
