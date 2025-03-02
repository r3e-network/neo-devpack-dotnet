using Neo.SmartContract.Framework;
using System.ComponentModel;

namespace Neo.SmartContract.Testing
{
    public static class StringExtensions
    {
        // Simple extension method for strings
        public static string AddPrefix(this string str, string prefix)
        {
            return prefix + str;
        }
        
        // Extension method with multiple parameters
        public static string Repeat(this string str, int count)
        {
            string result = "";
            for (int i = 0; i < count; i++)
            {
                result += str;
            }
            return result;
        }
        
        // Extension method that returns a different type
        public static int GetLength(this string str)
        {
            return str.Length;
        }
    }
    
    public static class IntExtensions
    {
        // Extension method for integers
        public static int Double(this int value)
        {
            return value * 2;
        }
        
        // Extension method with multiple parameters
        public static int Add(this int value, int amount)
        {
            return value + amount;
        }
        
        // Extension method chain
        public static int Square(this int value)
        {
            return value * value;
        }
    }
    
    public class Contract_ExtensionMethods : Framework.SmartContract
    {
        // Test simple string extension method
        public static string TestStringAddPrefix()
        {
            string str = "World";
            return str.AddPrefix("Hello ");
        }
        
        // Test string extension method with parameter
        public static string TestStringRepeat()
        {
            string str = "abc";
            return str.Repeat(3);
        }
        
        // Test extension method that returns a different type
        public static int TestStringGetLength()
        {
            string str = "Hello World";
            return str.GetLength();
        }
        
        // Test integer extension method
        public static int TestIntDouble()
        {
            int value = 5;
            return value.Double();
        }
        
        // Test integer extension method with parameter
        public static int TestIntAdd()
        {
            int value = 10;
            return value.Add(5);
        }
        
        // Test chaining extension methods
        public static int TestChainedExtensionMethods()
        {
            int value = 3;
            return value.Double().Square();
        }
        
        // Test mixing extension methods with regular methods
        public static string TestMixedMethods()
        {
            string str = "Test";
            int length = str.GetLength();
            return str + length.ToString();
        }
        
        // Test extension method with conditional
        public static string TestConditionalExtension(bool condition)
        {
            string str = "Test";
            if (condition)
            {
                return str.AddPrefix("Condition True: ");
            }
            else
            {
                return str.AddPrefix("Condition False: ");
            }
        }
    }
}
