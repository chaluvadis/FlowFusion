using FlowFusion.Core.Interfaces;
using FlowFusion.Core.Models;
using FlowFusion.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Setup DI with singleton ExpressionEvaluator
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Default evaluator (auto-variables mode enabled by default)
        services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
        services.AddSingleton<ExpressionEvaluator>();
    })
    .Build();

var evaluator = host.Services.GetRequiredService<FlowFusion.Core.Interfaces.IExpressionEvaluator>();
var traditionalEvaluator = new ExpressionEvaluator(autoVariablesMode: false);


Console.WriteLine("FlowFusion Expression Evaluator Examples");
Console.WriteLine("=======================================");

// Example 1: Basic variable access
var variableExpression = $"""
    Variables["order"].Total > 100
""";
await RunExample("Basic Variable Access", variableExpression, new FlowExecutionContext(new Dictionary<string, object?>
{
    ["order"] = new { Total = 150.0 }
}));

// Example 2: Direct variable name
var directVariableName = "order.Total > 100";
await RunExample("Direct Variable Name", directVariableName, new FlowExecutionContext(new Dictionary<string, object?>
{
    ["order"] = new { Total = 150.0 }
}));

// Example 3: Complex business logic - Order approval
var businessLogicExpression = """
Variables["order"].Total <= Variables["customer"].CreditLimit && Variables["order"].Items.Count() > 0 && (Variables["customer"].Status == "Gold" || Variables["order"].Total < 500)
""";
await RunExample("Order Approval Logic",
    businessLogicExpression,
    new FlowExecutionContext(new Dictionary<string, object?>
    {
        ["order"] = new { Total = 450.0, Items = new List<string> { "item1", "item2" } },
        ["customer"] = new { CreditLimit = 1000.0, Status = "Silver" }
    }));

// Example 4: Real-time fraud detection
await RunExample("Fraud Detection",
    "Variables[\"transaction\"].Amount > Variables[\"user\"].AvgTransaction * 3 && Variables[\"transaction\"].Location != Variables[\"user\"].HomeLocation && Variables[\"user\"].FailedAttempts > 2",
    new FlowExecutionContext(new Dictionary<string, object?>
    {
        ["transaction"] = new { Amount = 5000.0, Location = "New York" },
        ["user"] = new { AvgTransaction = 1000.0, HomeLocation = "California", FailedAttempts = "3" }
    }));

// Example 5: Inventory management
await RunExample("Inventory Alert",
    "Variables[\"product\"].StockLevel <= Variables[\"product\"].ReorderPoint || (Variables[\"product\"].StockLevel <= Variables[\"product\"].ReorderPoint * 1.5 && Variables[\"product\"].DemandRate > Variables[\"product\"].SupplyRate)",
    new FlowExecutionContext(new Dictionary<string, object?>
    {
        ["product"] = new { StockLevel = 50, ReorderPoint = "100", DemandRate = 200, SupplyRate = 150 }
    }));

// Example 6: User access control
await RunExample("Access Control",
    "Variables[\"user\"].Role == \"Admin\" || (Variables[\"user\"].Role == \"Manager\" && Variables[\"resource\"].Department == Variables[\"user\"].Department) || Variables[\"resource\"].IsPublic",
    new FlowExecutionContext(new Dictionary<string, object?>
    {
        ["user"] = new { Role = "Manager", Department = "Sales" },
        ["resource"] = new { Department = "Sales", IsPublic = false }
    }));

// Example 7: Complex mathematical calculation
await RunExample("Mathematical Calculation",
    "(Variables[\"data\"].Value1 * Variables[\"data\"].Value2 + Variables[\"data\"].Value3) / Variables[\"data\"].Value4 >= Variables[\"threshold\"]",
    new FlowExecutionContext(new Dictionary<string, object?>
    {
        ["data"] = new { Value1 = 10.0, Value2 = 5.0, Value3 = 20.0, Value4 = 2.0 },
        ["threshold"] = 35.0
    }));

    // Example 8: Workflow routing with multiple conditions
    await RunExample("Workflow Routing",
        "Variables[\"application\"].Score >= 700 && Variables[\"application\"].Income >= Variables[\"requirements\"].MinIncome && !Variables[\"application\"].HasDefaults",
        new FlowExecutionContext(new Dictionary<string, object?>
        {
            ["application"] = new { Score = "750", Income = 80000.0, HasDefaults = false },
            ["requirements"] = new { MinIncome = 50000.0 }
        }));

    // Example 9: String literal comparison
    await RunExample("String Literal Comparison",
        "Variables[\"user\"].Name == \"John Doe\" && Variables[\"user\"].Age > 18",
        new FlowExecutionContext(new Dictionary<string, object?>
        {
            ["user"] = new { Name = "John Doe", Age = 25 }
        }));

    // Example 10: Escaped string
    await RunExample("Escaped String",
        "Variables[\"message\"].Text == \"Hello \\\"World\\\"\"",
        new FlowExecutionContext(new Dictionary<string, object?>
        {
            ["message"] = new { Text = "Hello \"World\"" }
        }));

    // Example 11: Error handling - Invalid syntax
    await RunExample("Invalid Syntax Error",
        "Variables[\"x\"] @ invalid",
        new FlowExecutionContext(new Dictionary<string, object?> { ["x"] = 10 }));

    // Example 12: Auto-variables mode - Default simplified syntax
    await RunExample("Auto-Variables Mode (Default)",
        "order.Total > 100 && customer.Status == \"Gold\"",
        new FlowExecutionContext(new Dictionary<string, object?>
        {
            ["order"] = new { Total = 150.0 },
            ["customer"] = new { Status = "Gold" }
        }));

    // Example 13: Traditional Variables syntax (explicitly disabled auto-variables)
    await RunExampleTraditional("Traditional Variables Syntax",
        "Variables[\"order\"].Total > 100 && Variables[\"customer\"].Status == \"Gold\"",
        new FlowExecutionContext(new Dictionary<string, object?>
        {
            ["order"] = new { Total = 150.0 },
            ["customer"] = new { Status = "Gold" }
        }));

    async Task RunExample(string title, string expression, FlowExecutionContext context)
    {
        Console.WriteLine($"\n{title}:");
        Console.WriteLine($"Expression: {expression}");

        try
        {
            // Warmup (compile) the expression
            await evaluator.WarmupAsync(expression);

            // Evaluate
            var result = await evaluator.EvaluateAsync(expression, context, CancellationToken.None);
            Console.WriteLine($"Result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    async Task RunExampleTraditional(string title, string expression, FlowExecutionContext context)
    {
        Console.WriteLine($"\n{title}:");
        Console.WriteLine($"Expression: {expression}");

        try
        {
            // Warmup (compile) the expression
            await traditionalEvaluator.WarmupAsync(expression);

            // Evaluate
            var result = await traditionalEvaluator.EvaluateAsync(expression, context, CancellationToken.None);
            Console.WriteLine($"Result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
