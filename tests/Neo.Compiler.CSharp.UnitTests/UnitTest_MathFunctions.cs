// Copyright (C) 2015-2024 The Neo Project.
//
// UnitTest_MathFunctions.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.VM;
using System;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_MathFunctions : TestBase
    {
        [TestMethod]
        public void TestTrigonometricFunctions()
        {
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;

                public class Contract1 : Framework.SmartContract
                {
                    public static double TestSin(double x)
                    {
                        return Math.Sin(x);
                    }

                    public static double TestCos(double x)
                    {
                        return Math.Cos(x);
                    }

                    public static double TestTan(double x)
                    {
                        return Math.Tan(x);
                    }
                }";

            // This should compile successfully
            var compilation = CompileScript(testString, "Contract1");
            
            // Verify that the methods exist in the compiled contract
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestSin"));
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestCos"));
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestTan"));
        }

        [TestMethod]
        public void TestTrigonometricFunctionsWithDifferentTypes()
        {
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;

                public class Contract1 : Framework.SmartContract
                {
                    public static double TestSinInt(int x)
                    {
                        return Math.Sin(x);
                    }

                    public static double TestCosLong(long x)
                    {
                        return Math.Cos(x);
                    }

                    public static double TestTanFloat(float x)
                    {
                        return Math.Tan(x);
                    }
                }";

            // This should compile successfully
            var compilation = CompileScript(testString, "Contract1");
            
            // Verify that the methods exist in the compiled contract
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestSinInt"));
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestCosLong"));
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestTanFloat"));
        }

        [TestMethod]
        public void TestTrigonometricFunctionsWithConstants()
        {
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;

                public class Contract1 : Framework.SmartContract
                {
                    public static double TestSinZero()
                    {
                        return Math.Sin(0);
                    }

                    public static double TestCosPiOver2()
                    {
                        return Math.Cos(Math.PI / 2);
                    }

                    public static double TestTanPiOver4()
                    {
                        return Math.Tan(Math.PI / 4);
                    }
                }";

            // This should compile successfully
            var compilation = CompileScript(testString, "Contract1");
            
            // Verify that the methods exist in the compiled contract
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestSinZero"));
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestCosPiOver2"));
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestTanPiOver4"));
        }

        [TestMethod]
        public void TestTrigonometricFunctionsInExpressions()
        {
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;

                public class Contract1 : Framework.SmartContract
                {
                    public static double TestSinCosExpression(double x)
                    {
                        return Math.Sin(x) * Math.Sin(x) + Math.Cos(x) * Math.Cos(x);
                    }

                    public static double TestTanExpression(double x)
                    {
                        return Math.Sin(x) / Math.Cos(x) - Math.Tan(x);
                    }
                }";

            // This should compile successfully
            var compilation = CompileScript(testString, "Contract1");
            
            // Verify that the methods exist in the compiled contract
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestSinCosExpression"));
            Assert.IsTrue(compilation.manifest.Abi.Methods.Exists(m => m.Name == "TestTanExpression"));
        }
    }
}
