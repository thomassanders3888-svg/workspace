using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TerraForge.Tests;

/// <summary>
/// Minimal test framework runner
/// </summary>
public static class TestRunner
{
    private static int _passed = 0;
    private static int _failed = 0;
    private static readonly List<string> _failures = new();

    public static async Task<int> RunAllTestsAsync()
    {
        _passed = 0;
        _failed = 0;
        _failures.Clear();
        
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              TerraForge Test Suite                       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Discover and run all test classes
        var testTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Any(m => m.GetCustomAttributes(typeof(TestAttribute), false).Any()));

        foreach (var type in testTypes)
        {
            await RunTestClassAsync(type);
        }

        // Print summary
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                      Test Summary                          ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Total:  {_passed + _failed,-45} ║");
        Console.WriteLine($"║  Passed: {_passed,-45} ║");
        Console.WriteLine($"║  Failed: {_failed,-45} ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        if (_failed > 0)
        {
            Console.WriteLine("Failed tests:");
            foreach (var failure in _failures)
            {
                Console.WriteLine($"  - {failure}");
            }
            Console.WriteLine();
            return 1;
        }

        return 0;
    }

    private static async Task RunTestClassAsync(Type type)
    {
        Console.WriteLine($"\n▶ {type.Name}");
        Console.WriteLine(new string('─', 50));

        var instance = Activator.CreateInstance(type);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Any())
            .OrderBy(m => ((TestAttribute)m.GetCustomAttributes(typeof(TestAttribute), false)[0]).Order);

        foreach (var method in methods)
        {
            await RunTestMethodAsync(instance!, method);
        }
    }

    private static async Task RunTestMethodAsync(object instance, MethodInfo method)
    {
        var attr = (TestAttribute)method.GetCustomAttributes(typeof(TestAttribute), false)[0];
        var testName = attr.Name ?? method.Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = method.Invoke(instance, null);
            if (result is Task task)
            {
                await task;
            }

            stopwatch.Stop();
            _passed++;
            Console.WriteLine($"  ✓ {testName,-40} ({stopwatch.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _failed++;
            var error = ex.InnerException?.Message ?? ex.Message;
            Console.WriteLine($"  ✗ {testName,-40} ({stopwatch.ElapsedMilliseconds}ms)");
            Console.WriteLine($"    → {error}");
            _failures.Add($"{method.DeclaringType?.Name}.{method.Name}: {error}");
        }
    }
}

/// <summary>
/// Test attribute - marks a method as a test
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute : Attribute
{
    public string? Name { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// Skip attribute - conditionally skip test
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SkipAttribute : Attribute
{
    public string Reason { get; }
    public SkipAttribute(string reason) => Reason = reason;
}

/// <summary>
/// Assertion helpers
/// </summary>
public static class Assert
{
    public static void True(bool condition, string message = "Assertion failed")
    {
        if (!condition) throw new AssertionException(message);
    }

    public static void False(bool condition, string message = "Expected false")
    {
        if (condition) throw new AssertionException(message);
    }

    public static void Equal<T>(T expected, T actual, string message = "Values not equal")
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new AssertionException($"{message}: expected {expected}, got {actual}");
    }

    public static void NotNull(object? obj, string message = "Object is null")
    {
        if (obj == null) throw new AssertionException(message);
    }

    public static void Null(object? obj, string message = "Expected null")
    {
        if (obj != null) throw new AssertionException(message);
    }

    public static void Throws<T>(Action action) where T : Exception
    {
        try
        {
            action();
            throw new AssertionException($"Expected exception {typeof(T).Name} was not thrown");
        }
        catch (T)
        {
            // Expected
        }
    }

    public static void GreaterThan<T>(T value, T threshold) where T : IComparable<T>
    {
        if (value.CompareTo(threshold) <= 0)
            throw new AssertionException($"Expected {value} to be greater than {threshold}");
    }

    public static void LessThan<T>(T value, T threshold) where T : IComparable<T>
    {
        if (value.CompareTo(threshold) >= 0)
            throw new AssertionException($"Expected {value} to be less than {threshold}");
    }

    public static void Empty<T>(IEnumerable<T> collection, string message = "Expected empty collection")
    {
        if (collection.Any()) throw new AssertionException(message);
    }

    public static void NotEmpty<T>(IEnumerable<T> collection, string message = "Expected non-empty collection")
    {
        if (!collection.Any()) throw new AssertionException(message);
    }
}

public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}
