using Neo.SmartContract.Framework;
using System.ComponentModel;

namespace Neo.SmartContract.Testing
{
    public class Contract_RecordTypes : Framework.SmartContract
    {
        // Simple positional record
        public record Person(string Name, int Age);
        
        // Record with properties and methods
        public record Employee
        {
            public string Name { get; init; }
            public int Age { get; init; }
            public string Department { get; init; }
            
            public Employee(string name, int age, string department)
            {
                Name = name;
                Age = age;
                Department = department;
            }
            
            public string GetInfo()
            {
                return $"{Name}, {Age}, {Department}";
            }
        }
        
        // Record with default values
        public record Product(string Name, int Price = 100);
        
        // Test creating a positional record
        public static Person TestCreatePositionalRecord()
        {
            return new Person("John", 30);
        }
        
        // Test accessing positional record properties
        public static string TestAccessPositionalRecordProperties()
        {
            var person = new Person("John", 30);
            return person.Name;
        }
        
        // Test creating a record with properties
        public static Employee TestCreateRecordWithProperties()
        {
            return new Employee("Alice", 25, "Engineering");
        }
        
        // Test accessing record properties
        public static string TestAccessRecordProperties()
        {
            var employee = new Employee("Alice", 25, "Engineering");
            return employee.Department;
        }
        
        // Test record methods
        public static string TestRecordMethods()
        {
            var employee = new Employee("Alice", 25, "Engineering");
            return employee.GetInfo();
        }
        
        // Test record with default values
        public static Product TestRecordWithDefaultValues()
        {
            return new Product("Laptop");
        }
        
        // Test record equality
        public static bool TestRecordEquality()
        {
            var person1 = new Person("John", 30);
            var person2 = new Person("John", 30);
            return person1.Equals(person2);
        }
        
        // Test record inequality
        public static bool TestRecordInequality()
        {
            var person1 = new Person("John", 30);
            var person2 = new Person("Jane", 25);
            return !person1.Equals(person2);
        }
        
        // Test with expression
        public static Person TestWithExpression()
        {
            var person = new Person("John", 30);
            return person with { Name = "Johnny" };
        }
    }
}
