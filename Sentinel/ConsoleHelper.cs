using Spectre.Console;

namespace Sentinel;

public static class ConsoleHelper
{
    public static void WriteRule(string colour, string message)
    {
        var rule = new Rule($"[{colour}]{message}[/]");
        rule.RuleStyle("white");
        rule.LeftJustified();
        AnsiConsole.Write(rule);
    }
}