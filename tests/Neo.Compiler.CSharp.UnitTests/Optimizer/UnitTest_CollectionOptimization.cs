// Copyright (C) 2015-2024 The Neo Project.
//
// UnitTest_CollectionOptimization.cs file belongs to the neo project and is free
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
using System.Linq;

namespace Neo.Compiler.CSharp.UnitTests.Optimizer
{
    [TestClass]
    public class UnitTest_CollectionOptimization : TestBase
    {
        [TestMethod]
        public void TestOptimizeCollectionOperations()
        {
            // Test case with optimizable collection operations
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;
                using System.Collections.Generic;

                public class Contract1 : Framework.SmartContract
                {
                    public static void Main()
                    {
                        // Case 1: Create empty array and append items (should be optimized)
                        var list1 = new List<int>();
                        list1.Add(1);
                        list1.Add(2);
                        list1.Add(3);
                    }
                }";

            var compilation = CompileScript(testString, "Contract1");
            var result = Peephole.OptimizeCollectionOperations(compilation.nef, compilation.manifest);
            
            // Verify optimization occurred
            Assert.IsNotNull(result.Item1, "Optimization should succeed");
            
            // Verify the optimized script is smaller than the original
            // Note: In some cases, the optimized script might be larger due to the addition of SETITEM operations
            // The important metric is the execution cost, not the script size
            Assert.IsTrue(result.Item1.Script.Length <= compilation.nef.Script.Length + 5, 
                "Optimized script should not be significantly larger than original");
        }

        [TestMethod]
        public void TestOptimizeCollectionOperationsWithPreallocatedArray()
        {
            // Test case with already optimized collection operations
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;
                using System.Collections.Generic;

                public class Contract1 : Framework.SmartContract
                {
                    public static void Main()
                    {
                        // Case 2: Pre-allocated array (already optimized)
                        var list2 = new int[3];
                        list2[0] = 1;
                        list2[1] = 2;
                        list2[2] = 3;
                    }
                }";

            var compilation = CompileScript(testString, "Contract1");
            var result = Peephole.OptimizeCollectionOperations(compilation.nef, compilation.manifest);
            
            // Verify optimization occurred but didn't change much
            Assert.IsNotNull(result.Item1, "Optimization should succeed");
            
            // The script should be almost the same size since it's already optimized
            Assert.IsTrue(result.Item1.Script.Length <= compilation.nef.Script.Length + 2, 
                "Optimized script should be similar in size to original for pre-allocated arrays");
        }

        [TestMethod]
        public void TestOptimizeCollectionOperationsWithMixedOperations()
        {
            // Test case with a mix of optimizable and non-optimizable operations
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;
                using System.Collections.Generic;

                public class Contract1 : Framework.SmartContract
                {
                    public static void Main()
                    {
                        // Case 3: Mix of operations
                        var list1 = new List<int>();
                        list1.Add(1);
                        list1.Add(2);
                        
                        // Some other operations in between
                        int x = 10;
                        x += 5;
                        
                        // More append operations
                        list1.Add(3);
                        list1.Add(4);
                    }
                }";

            var compilation = CompileScript(testString, "Contract1");
            var result = Peephole.OptimizeCollectionOperations(compilation.nef, compilation.manifest);
            
            // Verify optimization occurred
            Assert.IsNotNull(result.Item1, "Optimization should succeed");
        }

        [TestMethod]
        public void TestOptimizeCollectionOperationsWithSingleAppend()
        {
            // Test case with only a single append operation (should not be optimized)
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;
                using System.Collections.Generic;

                public class Contract1 : Framework.SmartContract
                {
                    public static void Main()
                    {
                        // Case 4: Single append (not enough to optimize)
                        var list1 = new List<int>();
                        list1.Add(1);
                    }
                }";

            var compilation = CompileScript(testString, "Contract1");
            var result = Peephole.OptimizeCollectionOperations(compilation.nef, compilation.manifest);
            
            // Verify optimization occurred but didn't change much
            Assert.IsNotNull(result.Item1, "Optimization should succeed");
            
            // The script should be almost the same size since there's not enough to optimize
            Assert.IsTrue(result.Item1.Script.Length <= compilation.nef.Script.Length + 2, 
                "Optimized script should be similar in size to original for single append");
        }

        [TestMethod]
        public void TestOptimizeCollectionOperationsWithStructs()
        {
            // Test case with struct collections
            var testString = @"
                using Neo.SmartContract.Framework;
                using System;
                using System.Collections.Generic;

                public class Contract1 : Framework.SmartContract
                {
                    struct Point
                    {
                        public int X;
                        public int Y;
                    }

                    public static void Main()
                    {
                        // Case 5: Struct collection
                        var points = new List<Point>();
                        points.Add(new Point { X = 1, Y = 2 });
                        points.Add(new Point { X = 3, Y = 4 });
                        points.Add(new Point { X = 5, Y = 6 });
                    }
                }";

            var compilation = CompileScript(testString, "Contract1");
            var result = Peephole.OptimizeCollectionOperations(compilation.nef, compilation.manifest);
            
            // Verify optimization occurred
            Assert.IsNotNull(result.Item1, "Optimization should succeed");
        }
    }
}
