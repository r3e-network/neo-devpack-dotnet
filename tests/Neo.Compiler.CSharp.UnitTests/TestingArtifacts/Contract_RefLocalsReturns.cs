using Neo.SmartContract.Framework;
using System.ComponentModel;

namespace Neo.SmartContract.Testing
{
    public class Contract_RefLocalsReturns : Framework.SmartContract
    {
        // Test ref local variables
        public static int TestRefLocal()
        {
            int x = 10;
            ref int refX = ref x;
            refX = 20;
            return x; // Should return 20
        }
        
        // Test ref return
        public static ref int GetRef(int[] array, int index)
        {
            return ref array[index];
        }
        
        public static int TestRefReturn()
        {
            int[] array = new int[] { 1, 2, 3 };
            ref int refValue = ref GetRef(array, 1);
            refValue = 42;
            return array[1]; // Should return 42
        }
        
        // Test ref local with assignment
        public static int TestRefLocalWithAssignment()
        {
            int x = 10;
            int y = 20;
            ref int refVar = ref x;
            refVar = 30;
            refVar = ref y;
            refVar = 40;
            return x + y; // Should return 30 + 40 = 70
        }
        
        // Test ref readonly return
        public static ref readonly int GetReadonlyRef(int[] array, int index)
        {
            return ref array[index];
        }
        
        public static int TestRefReadonlyReturn()
        {
            int[] array = new int[] { 1, 2, 3 };
            ref readonly int refValue = ref GetReadonlyRef(array, 1);
            return refValue; // Should return 2
        }
        
        // Test ref struct field
        public struct Point
        {
            public int X;
            public int Y;
        }
        
        public static int TestRefStructField()
        {
            Point point = new Point { X = 10, Y = 20 };
            ref int refX = ref point.X;
            refX = 30;
            return point.X; // Should return 30
        }
        
        // Test ref parameter
        public static void ModifyRef(ref int value)
        {
            value = 42;
        }
        
        public static int TestRefParameter()
        {
            int x = 10;
            ModifyRef(ref x);
            return x; // Should return 42
        }
    }
}
