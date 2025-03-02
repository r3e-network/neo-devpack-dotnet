// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_ExtensionMethods.cs file belongs to the neo project and is free
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
    public class UnitTest_ExtensionMethods : DebugAndTestBase<Contract_ExtensionMethods>
    {
        [TestMethod]
        public void TestStringAddPrefix()
        {
            Assert.AreEqual("Hello World", Contract.TestStringAddPrefix());
        }

        [TestMethod]
        public void TestStringRepeat()
        {
            Assert.AreEqual("abcabcabc", Contract.TestStringRepeat());
        }

        [TestMethod]
        public void TestStringGetLength()
        {
            Assert.AreEqual(new Integer(11), Contract.TestStringGetLength());
        }

        [TestMethod]
        public void TestIntDouble()
        {
            Assert.AreEqual(new Integer(10), Contract.TestIntDouble());
        }

        [TestMethod]
        public void TestIntAdd()
        {
            Assert.AreEqual(new Integer(15), Contract.TestIntAdd());
        }

        [TestMethod]
        public void TestChainedExtensionMethods()
        {
            Assert.AreEqual(new Integer(36), Contract.TestChainedExtensionMethods());
        }

        [TestMethod]
        public void TestMixedMethods()
        {
            Assert.AreEqual("Test4", Contract.TestMixedMethods());
        }

        [TestMethod]
        public void TestConditionalExtension()
        {
            Assert.AreEqual("Condition True: Test", Contract.TestConditionalExtension(true));
            Assert.AreEqual("Condition False: Test", Contract.TestConditionalExtension(false));
        }
    }
}
