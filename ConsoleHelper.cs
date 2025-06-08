using Spectre.Console;

namespace CulminatingCS;

public static class ConsoleHelper
{
    public static void WriteRule(string message, string colour)
    {
        var rule = new Rule($"[{colour}]{message}[/]");
        rule.RuleStyle("white");
        rule.LeftJustified();
        AnsiConsole.Write(rule);
    } 
}