using Spectre.Console;
using Spectre.Console.Extensions;

public class Program
{
    private static string[][] menuOptions =
    [
        [ "1", "Create Vault"],
        [ "2", "Load Vaults" ],
        [ "3", "Save Vaults" ],
        [ "4", "Display Vaults" ],
        [ "5", "Exit" ]
    ];

    public static void Main(string[] args)
    {
        Run();
    }

    public static async Task Run()
    {
        AnsiConsole.MarkupLine("[underline red]Sentinel Password Manager[/]\n");

        VaultManager.LoadVaults();

        var vaultTable = new Table();

        vaultTable.AddColumn("Name");
        vaultTable.AddColumn("Total Entries");

        if (VaultManager.IsEmpty)
        {
            AnsiConsole.MarkupLine("[bold red]No vaults found.[/]");
            AnsiConsole.MarkupLine("Please create a vault to get started.\n");
            await Task.Delay(200).Spinner();

            CreateVault();
        }

        var rule2 = new Rule("[blue]Vaults[/]");
        rule2.RuleStyle("white");
        rule2.LeftJustified();
        AnsiConsole.Write(rule2);

        VaultManager.Vaults.ForEach(vault =>
        {
            vaultTable.AddRow(vault.Name, vault.TotalEntries.ToString());
        });

        AnsiConsole.Write(vaultTable);

        var menuTable = new Table().HideHeaders();
        menuTable.AddColumn("Option");
        menuTable.AddColumn("Description");

        menuTable.AddRow(menuOptions[0][0], menuOptions[0][1]);
        menuTable.AddRow(menuOptions[1][0], menuOptions[1][1]);
        menuTable.AddRow(menuOptions[2][0], menuOptions[2][1]);
        menuTable.AddRow(menuOptions[3][0], menuOptions[3][1]);
        menuTable.AddRow(menuOptions[4][0], menuOptions[4][1]);
        AnsiConsole.Write(menuTable);

        int option = 0;

        do
        {
            option = AnsiConsole.Prompt(
                    new TextPrompt<int>("Select an option:"));
            if (option < 1 || option > menuOptions.Length)
            {
                AnsiConsole.MarkupLine("[bold red]Invalid option.[/]");
                continue;
            }
        } while (option < 1 || option > menuOptions.Length);

        do
        {
            switch (option)
            {
                case 1:
                    CreateVault();
                    break;
                case 2:
                    VaultManager.LoadVaults();
                    break;
                case 3:
                    VaultManager.SaveVaults();
                    break;
                case 4:
                    VaultManager.DisplayVaults();
                    break;
                case 5:
                    AnsiConsole.MarkupLine("[bold green]Exiting...[/]");
                    return;
                    break;
                default:
                    AnsiConsole.MarkupLine("[bold red]Invalid option selected.[/]");
                    break;
            }
       } while (option != menuOptions.GetLength(1) - 1); // number of columns
    }

    public static void CreateVault()
    {
        var rule = new Rule("[green]Vault Creation[/]");
        rule.RuleStyle("white");
        rule.LeftJustified();
        AnsiConsole.Write(rule);

        string vaultPassword = "";
        string confirmPassword = "";
        string vaultName = "";

        do
        {
            vaultName = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the vault's name:"));

            if (VaultManager.Vaults.Any(v => v.Name.Equals(vaultName, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[bold red]A vault with that name already exists![/]");
                continue;
            }

            break;
        } while (true);
        
        do
        {
            vaultPassword = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the vault's password:").Secret());

            confirmPassword = AnsiConsole.Prompt(
                new TextPrompt<string>("Confirm the vault's password:").Secret());

            if (vaultPassword != confirmPassword)
            {
                AnsiConsole.MarkupLine("[bold red]Passwords do not match![/]\n");
                continue;
            }

            VaultManager.CreateVault(vaultName, vaultPassword);
        } while (vaultPassword != confirmPassword);
    }
}