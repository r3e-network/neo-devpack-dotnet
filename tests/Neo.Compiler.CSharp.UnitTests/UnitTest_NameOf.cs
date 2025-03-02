// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_NameOf.cs file belongs to the neo project and is free
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
    public class UnitTest_NameOf : DebugAndTestBase<Contract_NameOf>
    {
        [TestMethod]
        public void TestNameOfSimple()
        {
            Assert.AreEqual("TestNameOfSimple", Contract.TestNameOfSimple());
        }

        [TestMethod]
        public void TestNameOfParameter()
        {
            Assert.AreEqual("parameter", Contract.TestNameOfParameter("test"));
        }

        [TestMethod]
        public void TestNameOfMemberAccess()
        {
            Assert.AreEqual("Length", Contract.TestNameOfMemberAccess());
        }

        [TestMethod]
        public void TestNameOfType()
        {
            Assert.AreEqual("String", Contract.TestNameOfType());
        }

        [TestMethod]
        public void TestNameOfGenericType()
        {
            Assert.AreEqual("List", Contract.TestNameOfGenericType());
        }

        [TestMethod]
        public void TestNameOfPredefinedType()
        {
            Assert.AreEqual("int", Contract.TestNameOfPredefinedType());
        }

        [TestMethod]
        public void TestNameOfInInterpolation()
        {
            Assert.AreEqual("The name is TestNameOfInInterpolation", Contract.TestNameOfInInterpolation());
        }

        [TestMethod]
        public void TestNameOfInConditional()
        {
            Assert.AreEqual("condition", Contract.TestNameOfInConditional(true));
            Assert.AreEqual("TestNameOfInConditional", Contract.TestNameOfInConditional(false));
        }
    }
}
