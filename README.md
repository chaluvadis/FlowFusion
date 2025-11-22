# FlowFusion
<p style="display:flex;text-align:center;">
<img src="./resources/logo-transparent.png" width="120" height="80">
</p>
A high-performance expression evaluation engine for .NET applications.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com)

## Features

- **High-Performance Expression Evaluation**: Uses compiled LINQ expressions for fast execution
- **C# Syntax Support**: Full C# expression syntax with operators, method calls, and property access
- **Async Execution**: Fully asynchronous with cancellation support
- **Context-Aware**: Rich evaluation with access to variables and complex objects
- **Clean Architecture**: Modular design with clear separation of concerns

## Quick Start - Expression Evaluation

```csharp
using FlowFusion.Core.Interfaces;
using FlowFusion.Core.Models;
using Microsoft.Extensions.DependencyInjection;

// Setup DI
var services = new ServiceCollection();
services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
var provider = services.BuildServiceProvider();

var evaluator = provider.GetRequiredService<IExpressionEvaluator>();

// Create execution context with variables
var context = new FlowExecutionContext(new Dictionary<string, object?> {
    ["order"] = new { Total = 150.0, Items = new List<string> { "item1" } },
    ["customer"] = new { CreditLimit = 200.0, Status = "Gold" },
    ["user"] = new { Name = "John Doe", Age = 25 }
});

// Traditional syntax with Variables access
string expression = """
    Variables["order"].Total > 100 && Variables["customer"].Status == "Gold"
""";
bool result = await evaluator.EvaluateAsync(expression, context);
Console.WriteLine($"Order approved: {result}"); // Output: Order approved: True

// Simplified syntax with auto-variables mode
var simpleEvaluator = new ExpressionEvaluator(autoVariablesMode: true);
string simpleExpression = "order.Total > 100 && customer.Status == \"Gold\"";
bool simpleResult = await simpleEvaluator.EvaluateAsync(simpleExpression, context);
Console.WriteLine($"Simple approval: {simpleResult}"); // Output: Simple approval: True

// String literals and complex logic
string userCheck = """
    Variables["user"].Name == "John Doe" && Variables["user"].Age >= 18
""";
bool isValidUser = await evaluator.EvaluateAsync(userCheck, context);
Console.WriteLine($"Valid user: {isValidUser}"); // Output: Valid user: True
```

## Architecture

FlowFusion follows clean architecture with modern .NET practices:

- **FlowFusion.Core**: Domain models and interfaces (IExpressionEvaluator, etc.)
- **FlowFusion.Expression**: High-performance expression evaluation engine
- **FlowFusion.RunTime**: Workflow execution engine with conditional branching
- **Example**: Usage demonstrations

## Expression Syntax

Supports comprehensive C# expression syntax:

```csharp
// Variable access patterns
Variables["order"].Total > 100  // Traditional explicit access
order.Total > 100               // Auto-variables mode (simplified)
context.Variables["order"].Total > 100  // Explicit context access

// Operators and comparisons
Variables["user"].Age >= 18 && Variables["user"].Balance > 100 || Variables["user"].IsVip
Variables["a"] == Variables["b"] || Variables["a"] != null

// Arithmetic and mathematical
(Variables["x"] + Variables["y"]) * 2 > Variables["threshold"]
Math.Max(Variables["a"], Variables["b"]) / 2

// String literals and operations
Variables["user"].Name == "John Doe"
Variables["message"].Text == "Hello \"World\""
Variables["text"].Length > 0

// Method calls and properties
DateTime.Now.Hour > 9
Variables["list"].Count() > 0
Variables["obj"].GetType().Name == "MyClass"

// Complex nested access
Variables["user"].Profile.Address.City == "New York"

// Array and collection operations
Variables["list"][0] == "first"
Variables["array"].Length > 5

// Null checks and coalescing
Variables["optional"]?.Value ?? 0 > 10
Variables["data"] != null && Variables["data"].IsActive

// Type checking and casting
Variables["obj"].GetType().Name == "MyType"
(Variables["value"] as int?) ?? 0 > 5

// Custom expressions with different property names
Data["item"].Status == "Active"  // When using custom "Data" property
ctx.Variables["item"].Count > 0  // When using custom "ctx" identifier
```

## Building from Source

```bash
# Clone the repository
git clone https://github.com/your-repo/FlowFusion.git
cd FlowFusion

# Build all projects
dotnet build

# Run tests
dotnet test

# Run the example application
cd example/FlowFusion.Example
dotnet run
```

## Project Structure

```
src/
├── FlowFusion.Core/          # Domain models and interfaces
├── FlowFusion.Expression/    # Expression evaluation
└── FlowFusion.RunTime/       # Workflow execution engine

test/
├── FlowFusion.Tests/         # Unit tests for all components
│   ├── Expression/           # Evaluator and tokenizer tests
│   └── Workflow/             # Workflow engine tests

example/
└── FlowFusion.Example/       # Comprehensive usage examples
```

## API Reference

### IExpressionEvaluator
```csharp
ValueTask<bool> EvaluateAsync(string expression, FlowExecutionContext context, CancellationToken cancellation = default);
ValueTask<Func<FlowExecutionContext, bool>> WarmupAsync(string expression, CancellationToken cancellation = default);
```

### FlowExecutionContext
```csharp
public FlowExecutionContext(IDictionary<string, object?> variables);
public IDictionary<string, object?> Variables { get; }
```

## Contributing

1. Follow clean architecture and modern .NET practices
2. Add comprehensive tests for new features
3. Update documentation and examples
4. Ensure all tests pass and performance is maintained
5. Use `ValueTask` for async methods where appropriate

## Performance Benchmarks

- **Tokenization**: ~3x faster with span-based parsing
- **Evaluation**: Compiled expressions provide near-native performance
- **Memory**: Reduced allocations through span usage and `ValueTask`

## License

MIT License - see LICENSE file for details.
