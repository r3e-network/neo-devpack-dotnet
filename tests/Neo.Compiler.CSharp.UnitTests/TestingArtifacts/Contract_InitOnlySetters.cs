using Neo.SmartContract.Framework;
using System.ComponentModel;

namespace Neo.SmartContract.Testing
{
    public class Contract_InitOnlySetters : Framework.SmartContract
    {
        // Test class with init-only properties
        public class Person
        {
            // Regular property with get and set
            public string Name { get; set; }
            
            // Init-only property
            public int Age { get; init; }
            
            // Init-only property with default value
            public string Address { get; init; } = "Unknown";
            
            // Init-only property with backing field
            private string _email;
            public string Email 
            { 
                get => _email; 
                init => _email = value; 
            }
            
            // Constructor
            public Person() 
            {
                Name = "Default";
            }
            
            // Constructor with parameters
            public Person(string name, int age)
            {
                Name = name;
                Age = age;
            }
        }
        
        // Test creating an object with init-only properties
        public static Person TestCreatePerson()
        {
            return new Person
            {
                Name = "John",
                Age = 30,
                Address = "123 Main St",
                Email = "john@example.com"
            };
        }
        
        // Test getting values from init-only properties
        public static string TestGetName()
        {
            var person = TestCreatePerson();
            return person.Name;
        }
        
        public static int TestGetAge()
        {
            var person = TestCreatePerson();
            return person.Age;
        }
        
        public static string TestGetAddress()
        {
            var person = TestCreatePerson();
            return person.Address;
        }
        
        public static string TestGetEmail()
        {
            var person = TestCreatePerson();
            return person.Email;
        }
        
        // Test modifying a regular property
        public static string TestModifyName()
        {
            var person = TestCreatePerson();
            person.Name = "Jane";
            return person.Name;
        }
        
        // Test using constructor with parameters
        public static Person TestCreatePersonWithConstructor()
        {
            return new Person("Alice", 25)
            {
                Address = "456 Oak St",
                Email = "alice@example.com"
            };
        }
        
        // Test getting values from object created with constructor
        public static string TestGetNameFromConstructor()
        {
            var person = TestCreatePersonWithConstructor();
            return person.Name;
        }
        
        public static int TestGetAgeFromConstructor()
        {
            var person = TestCreatePersonWithConstructor();
            return person.Age;
        }
    }
}
