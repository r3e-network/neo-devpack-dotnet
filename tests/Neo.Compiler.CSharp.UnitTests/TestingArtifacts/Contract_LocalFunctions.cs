using Neo.SmartContract.Framework;
using System.ComponentModel;

namespace Neo.SmartContract.Testing
{
    public class Contract_LocalFunctions : Framework.SmartContract
    {
        // Test simple local function
        public static int TestSimpleLocalFunction()
        {
            int Add(int a, int b)
            {
                return a + b;
            }
            
            return Add(5, 7);
        }
        
        // Test local function with parameters
        public static int TestLocalFunctionWithParameters(int x, int y)
        {
            int Multiply(int a, int b)
            {
                return a * b;
            }
            
            return Multiply(x, y);
        }
        
        // Test expression-bodied local function
        public static int TestExpressionBodiedLocalFunction()
        {
            int Square(int x) => x * x;
            
            return Square(4);
        }
        
        // Test local function that uses variables from outer scope
        public static int TestLocalFunctionWithCapturedVariables()
        {
            int factor = 10;
            
            int MultiplyByFactor(int x)
            {
                return x * factor;
            }
            
            return MultiplyByFactor(5);
        }
        
        // Test local function that calls another local function
        public static int TestNestedLocalFunctions()
        {
            int Add(int a, int b)
            {
                return a + b;
            }
            
            int AddAndMultiply(int a, int b, int c)
            {
                return Add(a, b) * c;
            }
            
            return AddAndMultiply(3, 4, 2);
        }
        
        // Test local function with conditional logic
        public static int TestLocalFunctionWithConditional(bool condition)
        {
            int GetValue(bool cond)
            {
                if (cond)
                {
                    return 10;
                }
                else
                {
                    return 20;
                }
            }
            
            return GetValue(condition);
        }
        
        // Test local function with loop
        public static int TestLocalFunctionWithLoop()
        {
            int Sum(int n)
            {
                int result = 0;
                for (int i = 1; i <= n; i++)
                {
                    result += i;
                }
                return result;
            }
            
            return Sum(5);
        }
        
        // Test local function with default parameter value
        public static int TestLocalFunctionWithDefaultParameter()
        {
            int GetValue(int x = 10)
            {
                return x * 2;
            }
            
            return GetValue() + GetValue(5);
        }
    }
}
