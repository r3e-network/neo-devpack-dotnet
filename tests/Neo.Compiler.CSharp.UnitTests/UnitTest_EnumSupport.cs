// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_EnumSupport.cs file belongs to the neo project and is free
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
    public class UnitTest_EnumSupport : DebugAndTestBase<Contract_EnumSupport>
    {
        [TestMethod]
        public void TestGetEnumValue()
        {
            Assert.AreEqual(new Integer(1), Contract.GetEnumValue(1));
            Assert.AreEqual(new Integer(2), Contract.GetEnumValue(2));
            Assert.AreEqual(new Integer(3), Contract.GetEnumValue(3));
        }

        [TestMethod]
        public void TestConvertEnumToInt()
        {
            Assert.AreEqual(new Integer(1), Contract.ConvertEnumToInt(1));
            Assert.AreEqual(new Integer(2), Contract.ConvertEnumToInt(2));
            Assert.AreEqual(new Integer(3), Contract.ConvertEnumToInt(3));
        }

        [TestMethod]
        public void TestConvertIntToEnum()
        {
            Assert.AreEqual(new Integer(1), Contract.ConvertIntToEnum(1));
            Assert.AreEqual(new Integer(2), Contract.ConvertIntToEnum(2));
            Assert.AreEqual(new Integer(3), Contract.ConvertIntToEnum(3));
        }

        [TestMethod]
        public void TestCompareEnumValues()
        {
            Assert.IsTrue(Contract.CompareEnumValues(1, 1));
            Assert.IsFalse(Contract.CompareEnumValues(1, 2));
            Assert.IsFalse(Contract.CompareEnumValues(2, 3));
            Assert.IsTrue(Contract.CompareEnumValues(3, 3));
        }

        [TestMethod]
        public void TestGetEnumMember()
        {
            Assert.AreEqual(new Integer(2), Contract.GetEnumMember());
        }

        [TestMethod]
        public void TestGetEnumName()
        {
            Assert.AreEqual("Value1", Contract.GetEnumName(1));
            Assert.AreEqual("Value2", Contract.GetEnumName(2));
            Assert.AreEqual("Value3", Contract.GetEnumName(3));
            Assert.AreEqual("Unknown", Contract.GetEnumName(4));
        }

        [TestMethod]
        public void TestSwitchOnEnum()
        {
            Assert.AreEqual(new Integer(10), Contract.SwitchOnEnum(1));
            Assert.AreEqual(new Integer(20), Contract.SwitchOnEnum(2));
            Assert.AreEqual(new Integer(30), Contract.SwitchOnEnum(3));
            Assert.AreEqual(new Integer(0), Contract.SwitchOnEnum(4));
        }
    }
}
