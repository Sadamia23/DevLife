using DevLife.Database;
using DevLife.Enums;
using DevLife.Models.CodeRoast;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DevLife.Extensions;

public static class CodeRoastSeedingExtensions
{
    public static async Task SeedCodeRoastTasksAsync(this ApplicationDbContext context)
    {
        // Check if we already have code roast tasks
        if (await context.CodeRoastTasks.AnyAsync())
        {
            return; // Already seeded
        }

        var tasks = new List<CodeRoastTask>();

        // C# / .NET Tasks
        tasks.AddRange(CreateDotNetTasks());

        // JavaScript Tasks
        tasks.AddRange(CreateJavaScriptTasks());

        // Python Tasks
        tasks.AddRange(CreatePythonTasks());

        // React Tasks
        tasks.AddRange(CreateReactTasks());

        // Add more tech stacks as needed
        tasks.AddRange(CreateGeneralTasks());

        context.CodeRoastTasks.AddRange(tasks);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded {tasks.Count} code roast tasks into the database.");
    }

    private static List<CodeRoastTask> CreateDotNetTasks()
    {
        return new List<CodeRoastTask>
        {
            new CodeRoastTask
            {
                Title = "LINQ Master Challenge",
                Description = "You have a list of employees with their salaries and departments. Find all employees who earn more than the average salary in their department.",
                Requirements = "Use LINQ to filter employees based on department average salary. Return a list of employee names and their salaries.",
                TechStack = TechnologyStack.DotNet,
                DifficultyLevel = ExperienceLevel.Junior,
                StarterCode = @"public class Employee
{
    public string Name { get; set; }
    public string Department { get; set; }
    public decimal Salary { get; set; }
}

// TODO: Implement the solution
public List<Employee> FindHighEarners(List<Employee> employees)
{
    // Your code here
}",
                TestCasesJson = JsonSerializer.Serialize(new[]
                {
                    "Input: [{Name: 'John', Department: 'IT', Salary: 50000}, {Name: 'Jane', Department: 'IT', Salary: 60000}]",
                    "Expected: High earners above department average",
                    "Edge case: Empty list should return empty list"
                }),
                ExamplesJson = JsonSerializer.Serialize(new[]
                {
                    "If IT department average is 55000, only Jane (60000) should be returned",
                    "Each department should be calculated separately"
                }),
                EstimatedMinutes = 25,
                Topic = "LINQ and Data Processing"
            },

            new CodeRoastTask
            {
                Title = "Async/Await Nightmare",
                Description = "Create a method that fetches data from multiple APIs concurrently and combines the results. Handle errors gracefully and implement proper cancellation.",
                Requirements = "Use HttpClient to fetch from 3 different endpoints simultaneously. Combine results into a single response object. Handle timeouts and cancellation.",
                TechStack = TechnologyStack.DotNet,
                DifficultyLevel = ExperienceLevel.Middle,
                StarterCode = @"public class ApiResponse
{
    public string Source { get; set; }
    public string Data { get; set; }
    public bool Success { get; set; }
}

public async Task<List<ApiResponse>> FetchMultipleApisAsync(
    string[] urls, 
    CancellationToken cancellationToken = default)
{
    // Your async magic here
}",
                TestCasesJson = JsonSerializer.Serialize(new[]
                {
                    "Should handle concurrent requests to multiple endpoints",
                    "Should handle partial failures gracefully",
                    "Should respect cancellation token"
                }),
                ExamplesJson = JsonSerializer.Serialize(new[]
                {
                    "3 URLs provided, should make 3 concurrent HTTP requests",
                    "If one fails, others should still complete"
                }),
                EstimatedMinutes = 45,
                Topic = "Asynchronous Programming"
            },

            new CodeRoastTask
            {
                Title = "Design Pattern Disaster",
                Description = "Implement a notification system using the Observer pattern. Multiple notification channels (email, SMS, push) should be notified when events occur.",
                Requirements = "Create a proper Observer pattern implementation. Support adding/removing observers. Demonstrate with a user registration event.",
                TechStack = TechnologyStack.DotNet,
                DifficultyLevel = ExperienceLevel.Senior,
                StarterCode = @"// Design your Observer pattern interfaces and classes
// Implement for notification system

public class UserRegistrationEvent
{
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
}

// Your implementation here",
                TestCasesJson = JsonSerializer.Serialize(new[]
                {
                    "Should notify all registered observers",
                    "Should support adding/removing observers dynamically",
                    "Should handle observer exceptions gracefully"
                }),
                ExamplesJson = JsonSerializer.Serialize(new[]
                {
                    "Register EmailNotifier and SMSNotifier as observers",
                    "When user registers, both should receive notification"
                }),
                EstimatedMinutes = 60,
                Topic = "Design Patterns"
            }
        };
    }

    private static List<CodeRoastTask> CreateJavaScriptTasks()
    {
        return new List<CodeRoastTask>
        {
            new CodeRoastTask
            {
                Title = "Array Method Madness",
                Description = "Given an array of shopping cart items, calculate the total price with discounts, tax, and group by category.",
                Requirements = "Use modern JavaScript array methods (map, filter, reduce). Apply discounts, calculate tax, and group items by category.",
                TechStack = TechnologyStack.JavaScript,
                DifficultyLevel = ExperienceLevel.Junior,
                StarterCode = @"const cartItems = [
  { name: 'Laptop', price: 1000, category: 'Electronics', discount: 0.1 },
  { name: 'Book', price: 20, category: 'Education', discount: 0.05 },
  // ... more items
];

function processCart(items, taxRate = 0.08) {
  // Your solution here
}",
                TestCasesJson = JsonSerializer.Serialize(new[]
                {
                    "Should apply discounts correctly",
                    "Should calculate tax on discounted prices",
                    "Should group items by category"
                }),
                ExamplesJson = JsonSerializer.Serialize(new[]
                {
                    "Laptop: $1000 - 10% discount = $900 + 8% tax = $972",
                    "Group: {Electronics: [...], Education: [...]}"
                }),
                EstimatedMinutes = 30,
                Topic = "Array Methods and Functional Programming"
            },

            new CodeRoastTask
            {
                Title = "Promise Chain Hell",
                Description = "Convert callback-based code to use Promises and async/await. Handle errors at each step and implement retry logic.",
                Requirements = "Transform nested callbacks into clean async/await code. Add error handling and retry mechanism for failed operations.",
                TechStack = TechnologyStack.JavaScript,
                DifficultyLevel = ExperienceLevel.Middle,
                StarterCode = @"// Convert this callback hell to async/await
function processUserData(userId, callback) {
  getUser(userId, (err, user) => {
    if (err) return callback(err);
    
    getProfile(user.id, (err, profile) => {
      if (err) return callback(err);
      
      updateLastLogin(user.id, (err, result) => {
        if (err) return callback(err);
        callback(null, { user, profile, lastLogin: result });
      });
    });
  });
}

// Your async/await solution here",
                TestCasesJson = JsonSerializer.Serialize(new[]
                {
                    "Should handle all errors gracefully",
                    "Should maintain the same functionality",
                    "Should be much more readable"
                }),
                ExamplesJson = JsonSerializer.Serialize(new[]
                {
                    "Convert nested callbacks to clean async/await",
                    "Use try-catch for error handling"
                }),
                EstimatedMinutes = 40,
                Topic = "Asynchronous JavaScript"
            }
        };
    }

    private static List<CodeRoastTask> CreatePythonTasks()
    {
        return new List<CodeRoastTask>
        {
            new CodeRoastTask
            {
                Title = "Pythonic Data Processing",
                Description = "Process a CSV-like dataset using Python's built-in data structures and comprehensions. Clean, filter, and aggregate the data.",
                Requirements = "Use list comprehensions, dictionary comprehensions, and built-in functions. Handle missing data and provide summary statistics.",
                TechStack = TechnologyStack.Python,
                DifficultyLevel = ExperienceLevel.Junior,
                StarterCode = @"# Sample data structure
sales_data = [
    {'date': '2024-01-01', 'product': 'Laptop', 'amount': 1200, 'region': 'North'},
    {'date': '2024-01-02', 'product': '', 'amount': None, 'region': 'South'},
    # ... more data
]

def process_sales_data(data):
    # Clean, filter, and aggregate the data
    # Return a summary with total sales by region and product
    pass",
                TestCasesJson = JsonSerializer.Serialize(new[]
                {
                    "Should handle missing/None values",
                    "Should aggregate by region and product",
                    "Should use Pythonic constructs"
                }),
                ExamplesJson = JsonSerializer.Serialize(new[]
                {
                    "Filter out records with missing data",
                    "Group sales by region: {'North': 1200, 'South': 800}"
                }),
                EstimatedMinutes = 35,
                Topic = "Data Processing and Comprehensions"
            },

            new CodeRoastTask
            {
                Title = "Class Design Challenge",
                Description = "Design a library system with books, members, and borrowing functionality. Implement proper inheritance and encapsulation.",
                Requirements = "Create classes for Book, Member, and Library. Implement borrowing logic with due dates, late fees, and member restrictions.",
                TechStack = TechnologyStack.Python,
                DifficultyLevel = ExperienceLevel.Middle,
                StarterCode = @"from datetime import datetime, timedelta
from abc import ABC, abstractmethod

class Book:
    def __init__(self, isbn, title, author):
        # Your implementation
        pass

class Member:
    def __init__(self, member_id, name, member_type='regular'):
        # Your implementation
        pass

class Library:
    def __init__(self):
        # Your implementation
        pass
    
    def borrow_book(self, member_id, isbn):
        # Your implementation
        pass",
                TestCasesJson = JsonSerializer.Serialize(new[]
                {
                    "Should enforce borrowing limits per member type",
                    "Should calculate due dates correctly",
                    "Should handle book availability"
                }),
                ExamplesJson = JsonSerializer.Serialize(new[]
                {
                    "Regular member: 3 books max, 14 days",
                    "Premium member: 5 books max, 30 days"
                }),
                EstimatedMinutes = 50,
                Topic = "Object-Oriented Programming"
            }
        };
    }

    private static List<CodeRoastTask> CreateReactTasks()
    {
        return new List<CodeRoastTask>
        {
            new CodeRoastTask
            {
                Title = "Hook Horror Story",
                Description = "Create a custom React hook for managing form state with validation. Handle multiple input types and provide error messages.",
                Requirements = "Build a reusable useForm hook that handles input changes, validation, and form submission. Support text, email, and number inputs.",
                TechStack = TechnologyStack.React,
                DifficultyLevel = ExperienceLevel.Middle,
                StarterCode = @"import { useState, useCallback } from 'react';

// Create a custom hook for form management
function useForm(initialValues, validationRules) {
  // Your hook implementation here
  
  return {
    values,
    errors,
    handleChange,
    handleSubmit,
    reset
  };
}

// Example usage:
function ContactForm() {
  const { values, errors, handleChange, handleSubmit } = useForm(
    { name: '', email: '', age: 0 },
    {
      name: (value) => value.length < 2 ? 'Name too short' : null,
      email: (value) => !value.includes('@') ? 'Invalid email' : null,
      age: (value) => value < 18 ? 'Must be 18+' : null
    }
  );
  
  // Your component JSX here
}",
                TestCasesJson = JsonSerializer.Serialize(new[]
                {
                    "Should validate inputs on change",
                    "Should prevent submission with errors",
                    "Should reset form state"
                }),
                ExamplesJson = JsonSerializer.Serialize(new[]
                {
                    "Email validation: must contain @",
                    "Age validation: must be 18 or older"
                }),
                EstimatedMinutes = 45,
                Topic = "Custom React Hooks"
            }
        };
    }

    private static List<CodeRoastTask> CreateGeneralTasks()
    {
        return new List<CodeRoastTask>
        {
            new CodeRoastTask
            {
                Title = "Algorithm Apocalypse",
                Description = "Implement a simple caching system with LRU (Least Recently Used) eviction policy. Support get, put, and size limit operations.",
                Requirements = "Create a cache that maintains insertion order and evicts least recently used items when capacity is exceeded. O(1) operations preferred.",
                TechStack = TechnologyStack.JavaScript, // Can be adapted to any language
                DifficultyLevel = ExperienceLevel.Senior,
                StarterCode = @"class LRUCache {
  constructor(capacity) {
    this.capacity = capacity;
    // Your implementation
  }
  
  get(key) {
    // Retrieve value and mark as recently used
  }
  
  put(key, value) {
    // Add/update value and handle capacity
  }
  
  size() {
    // Return current cache size
  }
}",
                TestCasesJson = JsonSerializer.Serialize(new[]
                {
                    "Should evict LRU item when capacity exceeded",
                    "Should update access order on get operations",
                    "Should handle duplicate keys"
                }),
                ExamplesJson = JsonSerializer.Serialize(new[]
                {
                    "Capacity 2: put(1,'a'), put(2,'b'), put(3,'c') -> evicts key 1",
                    "get(1) makes key 1 most recently used"
                }),
                EstimatedMinutes = 60,
                Topic = "Data Structures and Algorithms"
            }
        };
    }
}