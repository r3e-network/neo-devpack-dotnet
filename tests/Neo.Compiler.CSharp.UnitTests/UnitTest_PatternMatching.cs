// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_PatternMatching.cs file belongs to the neo project and is free
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
    public class UnitTest_PatternMatching : DebugAndTestBase<Contract_PatternMatching>
    {
        [TestMethod]
        public void TestConstantPattern()
        {
            Assert.IsTrue(Contract.TestConstantPattern(42));
            Assert.IsFalse(Contract.TestConstantPattern(43));
        }

        [TestMethod]
        public void TestTypePattern()
        {
            Assert.IsTrue(Contract.TestTypePattern("hello"));
            Assert.IsFalse(Contract.TestTypePattern(42));
        }

        [TestMethod]
        public void TestDeclarationPattern()
        {
            Assert.AreEqual("hello", Contract.TestDeclarationPattern("hello"));
            Assert.AreEqual("Not a string", Contract.TestDeclarationPattern(42));
        }

        [TestMethod]
        public void TestRelationalPattern()
        {
            Assert.IsTrue(Contract.TestRelationalPattern(50));
            Assert.IsFalse(Contract.TestRelationalPattern(5));
            Assert.IsFalse(Contract.TestRelationalPattern(150));
        }

        [TestMethod]
        public void TestLogicalPatterns()
        {
            Assert.IsTrue(Contract.TestLogicalPatterns(5));
            Assert.IsTrue(Contract.TestLogicalPatterns(95));
            Assert.IsFalse(Contract.TestLogicalPatterns(50));
        }

        [TestMethod]
        public void TestParenthesizedPattern()
        {
            Assert.IsTrue(Contract.TestParenthesizedPattern(5));
            Assert.IsTrue(Contract.TestParenthesizedPattern(95));
            Assert.IsFalse(Contract.TestParenthesizedPattern(50));
        }

        [TestMethod]
        public void TestRecursivePattern()
        {
            var person1 = new Contract_PatternMatching.Person { Name = "John", Age = 25 };
            var person2 = new Contract_PatternMatching.Person { Name = "Jane", Age = 30 };
            var person3 = new Contract_PatternMatching.Person { Name = "John", Age = 15 };

            Assert.IsTrue(Contract.TestRecursivePattern(person1));
            Assert.IsFalse(Contract.TestRecursivePattern(person2));
            Assert.IsFalse(Contract.TestRecursivePattern(person3));
        }

        [TestMethod]
        public void TestListPattern()
        {
            Assert.IsTrue(Contract.TestListPattern(new[] { 1, 2, 3 }));
            Assert.IsFalse(Contract.TestListPattern(new[] { 1, 2, 4 }));
            Assert.IsFalse(Contract.TestListPattern(new[] { 1, 2, 3, 4 }));
        }

        [TestMethod]
        public void TestListPatternWithDiscard()
        {
            Assert.IsTrue(Contract.TestListPatternWithDiscard(new[] { 1, 2, 3 }));
            Assert.IsTrue(Contract.TestListPatternWithDiscard(new[] { 5, 2, 7 }));
            Assert.IsFalse(Contract.TestListPatternWithDiscard(new[] { 1, 3, 3 }));
        }

        [TestMethod]
        public void TestListPatternWithSlice()
        {
            Assert.IsTrue(Contract.TestListPatternWithSlice(new[] { 1, 2, 3 }));
            Assert.IsTrue(Contract.TestListPatternWithSlice(new[] { 1, 2, 3, 4, 5 }));
            Assert.IsFalse(Contract.TestListPatternWithSlice(new[] { 2, 2, 3 }));
        }

        [TestMethod]
        public void TestSwitchExpression()
        {
            Assert.AreEqual("One", Contract.TestSwitchExpression(1));
            Assert.AreEqual("Two", Contract.TestSwitchExpression(2));
            Assert.AreEqual("Three", Contract.TestSwitchExpression(3));
            Assert.AreEqual("Other", Contract.TestSwitchExpression(4));
        }

        [TestMethod]
        public void TestSwitchExpressionWithPatterns()
        {
            Assert.AreEqual("String: hello", Contract.TestSwitchExpressionWithPatterns("hello"));
            Assert.AreEqual("Positive integer", Contract.TestSwitchExpressionWithPatterns(42));
            Assert.AreEqual("Negative integer", Contract.TestSwitchExpressionWithPatterns(-42));
            Assert.AreEqual("Zero", Contract.TestSwitchExpressionWithPatterns(0));
            Assert.AreEqual("Other", Contract.TestSwitchExpressionWithPatterns(true));
        }

        [TestMethod]
        public void TestSwitchExpressionWithPropertyPatterns()
        {
            var person1 = new Contract_PatternMatching.Person { Name = "John", Age = 25 };
            var person2 = new Contract_PatternMatching.Person { Name = "John", Age = 15 };
            var person3 = new Contract_PatternMatching.Person { Name = "Jane", Age = 30 };
            var person4 = new Contract_PatternMatching.Person { Name = "Bob", Age = 40 };

            Assert.AreEqual("Adult John", Contract.TestSwitchExpressionWithPropertyPatterns(person1));
            Assert.AreEqual("Young John", Contract.TestSwitchExpressionWithPropertyPatterns(person2));
            Assert.AreEqual("Jane", Contract.TestSwitchExpressionWithPropertyPatterns(person3));
            Assert.AreEqual("Other", Contract.TestSwitchExpressionWithPropertyPatterns(person4));
        }

        [TestMethod]
        public void TestSwitchExpressionWithListPatterns()
        {
            Assert.AreEqual("One, Two, Three", Contract.TestSwitchExpressionWithListPatterns(new[] { 1, 2, 3 }));
            Assert.AreEqual("One, 5, Three", Contract.TestSwitchExpressionWithListPatterns(new[] { 1, 5, 3 }));
            Assert.AreEqual("Ends with Nine, Ten", Contract.TestSwitchExpressionWithListPatterns(new[] { 1, 2, 3, 9, 10 }));
            Assert.AreEqual("Starts with One", Contract.TestSwitchExpressionWithListPatterns(new[] { 1, 5, 7 }));
            Assert.AreEqual("Other", Contract.TestSwitchExpressionWithListPatterns(new[] { 2, 3, 4 }));
        }
    }
}
