// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_InitOnlySetters.cs file belongs to the neo project and is free
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
    public class UnitTest_InitOnlySetters : DebugAndTestBase<Contract_InitOnlySetters>
    {
        [TestMethod]
        public void TestCreatePerson()
        {
            // Test that we can create an object with init-only properties
            var result = Contract.TestCreatePerson();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestGetName()
        {
            // Test that we can get the value of a regular property
            Assert.AreEqual("John", Contract.TestGetName());
        }

        [TestMethod]
        public void TestGetAge()
        {
            // Test that we can get the value of an init-only property
            Assert.AreEqual(new Integer(30), Contract.TestGetAge());
        }

        [TestMethod]
        public void TestGetAddress()
        {
            // Test that we can get the value of an init-only property with default value
            Assert.AreEqual("123 Main St", Contract.TestGetAddress());
        }

        [TestMethod]
        public void TestGetEmail()
        {
            // Test that we can get the value of an init-only property with backing field
            Assert.AreEqual("john@example.com", Contract.TestGetEmail());
        }

        [TestMethod]
        public void TestModifyName()
        {
            // Test that we can modify a regular property
            Assert.AreEqual("Jane", Contract.TestModifyName());
        }

        [TestMethod]
        public void TestCreatePersonWithConstructor()
        {
            // Test that we can create an object with constructor and init-only properties
            var result = Contract.TestCreatePersonWithConstructor();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestGetNameFromConstructor()
        {
            // Test that we can get the value of a property set in the constructor
            Assert.AreEqual("Alice", Contract.TestGetNameFromConstructor());
        }

        [TestMethod]
        public void TestGetAgeFromConstructor()
        {
            // Test that we can get the value of an init-only property set in the constructor
            Assert.AreEqual(new Integer(25), Contract.TestGetAgeFromConstructor());
        }
    }
}
