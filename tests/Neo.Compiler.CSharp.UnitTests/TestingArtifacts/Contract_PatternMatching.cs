using Neo.SmartContract.Framework;
using System.ComponentModel;

namespace Neo.SmartContract.Testing
{
    public class Contract_PatternMatching : Framework.SmartContract
    {
        // Test constant pattern
        public static bool TestConstantPattern(object value)
        {
            return value is 42;
        }
        
        // Test type pattern
        public static bool TestTypePattern(object value)
        {
            return value is string;
        }
        
        // Test declaration pattern
        public static string TestDeclarationPattern(object value)
        {
            if (value is string message)
            {
                return message;
            }
            return "Not a string";
        }
        
        // Test relational pattern
        public static bool TestRelationalPattern(int value)
        {
            return value is > 10 and < 100;
        }
        
        // Test logical patterns (and, or, not)
        public static bool TestLogicalPatterns(int value)
        {
            return value is >= 0 and <= 10 or >= 90 and <= 100;
        }
        
        // Test parenthesized pattern
        public static bool TestParenthesizedPattern(int value)
        {
            return value is (> 0 and < 10) or (> 90 and < 100);
        }
        
        // Test recursive pattern with property pattern
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
        
        public static bool TestRecursivePattern(Person person)
        {
            return person is { Name: "John", Age: > 18 };
        }
        
        // Test list pattern
        public static bool TestListPattern(int[] array)
        {
            return array is [1, 2, 3];
        }
        
        // Test list pattern with discard
        public static bool TestListPatternWithDiscard(int[] array)
        {
            return array is [_, 2, _];
        }
        
        // Test list pattern with slice
        public static bool TestListPatternWithSlice(int[] array)
        {
            return array is [1, 2, ..];
        }
        
        // Test switch expression
        public static string TestSwitchExpression(int value)
        {
            return value switch
            {
                1 => "One",
                2 => "Two",
                3 => "Three",
                _ => "Other"
            };
        }
        
        // Test switch expression with patterns
        public static string TestSwitchExpressionWithPatterns(object value)
        {
            return value switch
            {
                string s => $"String: {s}",
                int i when i > 0 => "Positive integer",
                int i when i < 0 => "Negative integer",
                int i => "Zero",
                _ => "Other"
            };
        }
        
        // Test switch expression with property patterns
        public static string TestSwitchExpressionWithPropertyPatterns(Person person)
        {
            return person switch
            {
                { Name: "John", Age: > 18 } => "Adult John",
                { Name: "John", Age: <= 18 } => "Young John",
                { Name: "Jane" } => "Jane",
                _ => "Other"
            };
        }
        
        // Test switch expression with list patterns
        public static string TestSwitchExpressionWithListPatterns(int[] array)
        {
            return array switch
            {
                [1, 2, 3] => "One, Two, Three",
                [1, var x, 3] => $"One, {x}, Three",
                [.., 9, 10] => "Ends with Nine, Ten",
                [1, ..] => "Starts with One",
                _ => "Other"
            };
        }
    }
}
