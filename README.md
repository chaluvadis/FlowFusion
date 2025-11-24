# FlowFusion

<p style="display:flex;text-align:center;">
<img src="./resources/logo-transparent.png" width="120" height="80">
</p>

A high-performance, modern expression evaluation engine for .NET applications with natural syntax support (e.g. `order.Total > 100`) and full C# expression support. Supports async execution, cancellation, and can be customized to fit workflow scenarios.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com)

Table of Contents
- Features
- Quick Start
  - Install (local)
  - DI example
  - Direct instantiation example
- Expression Syntax (and Workflow Expressions)
- Building from Source
- Project Structure
- API Reference
- Supported Platforms
- Contributing
- Performance & Benchmarks
- Troubleshooting
- License
- Maintainers

## Features

- 🚀 Auto-Variables Mode: write natural expressions like `order.Total > 100`
- High-performance evaluation: compiled LINQ expressions with `ValueTask` for async performance
- Rich C# syntax support: operators, method calls, property access, string literals, etc.
- Async & Cancellation support (`CancellationToken`)
- Context-aware: access variables, complex objects, and customized property names/identifiers
- Clean modular architecture with dependency injection
- Expression-based Workflows: use the same expression language to define conditional branching and guards in workflows

## Quick Start

### DI (recommended)

Example using Microsoft.Extensions.DependencyInjection in an async Main:

```csharp
using FlowFusion.Core.Interfaces;
using FlowFusion.Core.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
// Register the concrete evaluator implementation (adjust if you provide an AddFlowFusion extension)
services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();

var provider = services.BuildServiceProvider();
var evaluatorFromDI = provider.GetRequiredService<IExpressionEvaluator>();

var context = new FlowExecutionContext(new Dictionary<string, object?> {
    ["order"] = new { Total = 150.0, Items = new List<string> { "item1" } },
    ["customer"] = new { CreditLimit = 200.0, Status = "Gold" },
    ["user"] = new { Name = "John Doe", Age = 25 }
});

// Use expressions for evaluation (these same expressions can also be used as workflow conditions)
string expression = "order.Total > 100 && customer.Status == \"Gold\"";
bool result = await evaluatorFromDI.EvaluateAsync(expression, context);
Console.WriteLine($"Order approved: {result}"); // Output: Order approved: True
```

### Direct instantiation

If you prefer not to use DI:

```csharp
// Auto-variables enabled by default
var directEvaluator = new ExpressionEvaluator();

var context = new FlowExecutionContext(new Dictionary<string, object?> {
    ["user"] = new { Name = "John Doe", Age = 25 }
});

string userCheck = "user.Name == \"John Doe\" && user.Age >= 18";
bool isValidUser = await directEvaluator.EvaluateAsync(userCheck, context);
Console.WriteLine($"Valid user: {isValidUser}"); // Output: Valid user: True
```

Notes:
- The project supports an "auto-variables" mode (simpler, natural syntax) and a traditional explicit Variables[] access mode. You can choose the mode via constructor or configuration (e.g., `new ExpressionEvaluator(autoVariablesMode: false)`).
- Expressions used with the evaluator are the same expression language you can use to drive workflow conditions and branching in FlowFusion runtime.

## Expression Syntax (and Workflow Expressions)

FlowFusion supports a comprehensive subset of C# expression syntax and these expressions can be used both for direct evaluation and to define workflow step conditions/branching. Examples:

```csharp
// Traditional explicit access
Variables["order"].Total > 100

// Auto-variables mode (simplified)
order.Total > 100

// Operators & comparisons
Variables["user"].Age >= 18 && Variables["user"].Balance > 100 || Variables["user"].IsVip

// Math and method calls
(Math.Max(Variables["a"], Variables["b"]) / 2) > Variables["threshold"]

// Strings
Variables["user"].Name == "John Doe"
Variables["message"].Text == "Hello \"World\""

// Null-coalescing and null-conditional
Variables["optional"]?.Value ?? 0 > 10

// Collections
Variables["list"][0] == "first"
Variables["array"].Length > 5
Variables["list"].Count() > 0

// Custom identifiers
Data["item"].Status == "Active"  // When using a custom "Data" key
ctx.Variables["item"].Count > 0  // When using a custom "ctx" identifier

// Example: use an expression as a workflow condition
// If a workflow step accepts a condition string, you can evaluate it with the same evaluator to determine branching.
```

Add more examples in the Example project for edge-cases (async method calls, custom functions, and type casting).

## Project Structure

```
src/
├── FlowFusion.Core/          # Domain models and interfaces
├── FlowFusion.Expression/    # Expression evaluation engine
└── FlowFusion.RunTime/       # Workflow execution engine

test/
├── FlowFusion.Tests/         # Unit tests for all components
│   ├── Expression/           # Evaluator and tokenizer tests
│   └── Workflow/             # Workflow engine tests

example/
└── FlowFusion.Example/       # Usage examples
```

## API Reference

Relevant public types and signatures (surface-level).

IExpressionEvaluator
```csharp
ValueTask<bool> EvaluateAsync(string expression, FlowExecutionContext context, CancellationToken cancellation = default);
ValueTask<Func<FlowExecutionContext, bool>> WarmupAsync(string expression, CancellationToken cancellation = default);
```

FlowExecutionContext
```csharp
public FlowExecutionContext(IDictionary<string, object?> variables);
public IDictionary<string, object?> Variables { get; }
```

(If any signatures differ in code, keep this readme in sync — consider generating part of the API docs or adding XML doc links.)

## Contributing

Thanks for considering contributing! Please follow these guidelines:

1. Fork the repository and create a topic branch.
2. Follow the existing architecture and coding conventions.
3. Add tests for new features or bug fixes.
4. Ensure all tests pass locally: dotnet test
5. Open a PR against main (or the documented mainline branch) with a descriptive title and summary.
6. If applicable, update the README and examples.

## License
MIT License — see LICENSE file for details.For support or questions, open an issue or reach out via GitHub discussions (if enabled).
