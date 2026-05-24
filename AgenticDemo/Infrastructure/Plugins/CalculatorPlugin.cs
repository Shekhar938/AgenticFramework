using System.ComponentModel;
using System.Data;
using Microsoft.SemanticKernel;

namespace AgenticDemo.Infrastructure.Plugins;

public sealed class CalculatorPlugin
{
    [KernelFunction("calculate")]
    [Description("Performs a mathematical calculation (e.g. 100 * 1.5)")]
    public string Calculate(
        [Description("The math expression to solve")] string expression)
    {
        try
        {
            var result = new DataTable().Compute(expression, null);
            return $"The result of {expression} is {result}";
        }
        catch
        {
            return "Error: Invalid math expression.";
        }
    }
}
