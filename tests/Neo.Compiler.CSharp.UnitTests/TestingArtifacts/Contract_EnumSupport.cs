using Neo.SmartContract.Framework;
using System;
using System.ComponentModel;

namespace Neo.SmartContract.Testing
{
    public enum TestEnumSupport
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }

    public class Contract_EnumSupport : Framework.SmartContract
    {
        // Test enum declaration and member access
        public static TestEnumSupport GetEnumValue(int value)
        {
            return (TestEnumSupport)value;
        }

        // Test enum to int conversion
        public static int ConvertEnumToInt(TestEnumSupport value)
        {
            return (int)value;
        }

        // Test int to enum conversion
        public static TestEnumSupport ConvertIntToEnum(int value)
        {
            return (TestEnumSupport)value;
        }

        // Test enum comparison
        public static bool CompareEnumValues(TestEnumSupport value1, TestEnumSupport value2)
        {
            return value1 == value2;
        }

        // Test enum member access
        public static TestEnumSupport GetEnumMember()
        {
            return TestEnumSupport.Value2;
        }

        // Test enum as method parameter
        public static string GetEnumName(TestEnumSupport value)
        {
            switch (value)
            {
                case TestEnumSupport.Value1:
                    return "Value1";
                case TestEnumSupport.Value2:
                    return "Value2";
                case TestEnumSupport.Value3:
                    return "Value3";
                default:
                    return "Unknown";
            }
        }

        // Test enum in switch statement
        public static int SwitchOnEnum(TestEnumSupport value)
        {
            switch (value)
            {
                case TestEnumSupport.Value1:
                    return 10;
                case TestEnumSupport.Value2:
                    return 20;
                case TestEnumSupport.Value3:
                    return 30;
                default:
                    return 0;
            }
        }
    }
}
