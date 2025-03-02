using Neo.SmartContract.Framework;
using System.ComponentModel;

namespace Neo.SmartContract.Testing
{
    public class Contract_NameOf : Framework.SmartContract
    {
        // Test nameof with a simple identifier
        public static string TestNameOfSimple()
        {
            return nameof(TestNameOfSimple);
        }

        // Test nameof with a parameter
        public static string TestNameOfParameter(string parameter)
        {
            return nameof(parameter);
        }

        // Test nameof with a member access
        public static string TestNameOfMemberAccess()
        {
            return nameof(System.String.Length);
        }

        // Test nameof with a type
        public static string TestNameOfType()
        {
            return nameof(System.String);
        }

        // Test nameof with a generic type
        public static string TestNameOfGenericType()
        {
            return nameof(System.Collections.Generic.List<int>);
        }

        // Test nameof with a predefined type
        public static string TestNameOfPredefinedType()
        {
            return nameof(int);
        }

        // Test nameof in a string interpolation
        public static string TestNameOfInInterpolation()
        {
            return $"The name is {nameof(TestNameOfInInterpolation)}";
        }

        // Test nameof in a conditional expression
        public static string TestNameOfInConditional(bool condition)
        {
            return condition ? nameof(condition) : nameof(TestNameOfInConditional);
        }
    }
}
