using Spectre.Console;
using Spectre.Console.Extensions;

namespace CulminatingCS;

public class Program
{
    private static string[][] vaultAuthMenu =
    [
        [ "1", "Login"],
        [ "2", "Create Vault"],
        [ "3", "Exit" ]
    ];

    private static string[][] vaultMenu =
    [
        [ "1", "Add Entry"],
        [ "2", "Edit Entry"],
        [ "3", "Delete Entry"],
        [ "4", "Find Entry by Name"],
        [ "5", "Display All Entries (Ascending Date)"],
        [ "6", "Display all Entries (Descending Date)"],
        [ "7", "Logout"],
        [ "8", "Exit" ]
    ];

    public static void Main(string[] args)
    {
        Run().Wait();
    }

    public static async Task Run(bool ranBefore = false)
    {
        if (!ranBefore) AnsiConsole.MarkupLine("[underline red]Sentinel Password Manager[/]\n");

        VaultManager.LoadVaults();



        if (VaultManager.IsEmpty)
        {
            AnsiConsole.MarkupLine("[bold red]No vaults found.[/]");
            AnsiConsole.MarkupLine("Please create a vault to get started.\n");
            await Task.Delay(500).Spinner();

            CreateVault();
        }
        else
        {
            VaultManager.DisplayVaults();
        }

        int option;
        do
        {
            option = DisplayMenuOption(vaultAuthMenu, "Vault Authentication");
            switch (option)
            {
                case 1:
                    LoginToVault();
                    break;
                case 2:
                    CreateVault();
                    break;
                case 3:
                    AnsiConsole.MarkupLine("[bold green]Exiting...[/]");
                    Environment.Exit(0);
                    return;
                default:
                    AnsiConsole.MarkupLine("[bold red]Invalid option selected.[/]");
                    break;
            }
        } while (option < 0 || option >= vaultAuthMenu.GetLength(0));

        int option2;
        do
        {
            option2 = DisplayMenuOption(vaultMenu, "Password Management");
            switch (option2)
            {
                case 1:
                    AddVaultPasswordEntry();
                    break;
                case 2:
                    EditVaultPasswordEntry();
                    break;
                case 3:
                    DeleteVaultPasswordEntry();
                    break;
                case 4:
                    FindVaultPasswordEntryByName();
                    break;
                case 5:
                    DisplayVaultEntries(SortingOrder.Ascending);
                    break;
                case 6:
                    DisplayVaultEntries(SortingOrder.Descending);
                    break;
                case 7:
                    LogoutOfVault();
                    break;
                case 8:
                    AnsiConsole.MarkupLine("[bold green]Exiting...[/]");
                    return;
                default:
                    AnsiConsole.MarkupLine("[bold red]Invalid option selected.[/]");
                    break;
            }
        } while (option2 != vaultMenu.GetLength(0));
    }

    private static void LogoutOfVault()
    {
        VaultManager.Logout();
        AnsiConsole.MarkupLine("[bold green]Logged out of the current vault.[/]");
        Run(true).Wait();
    }

    private static void FindVaultPasswordEntryByName()
    {
        string name = AnsiConsole.Prompt(new TextPrompt<string>("[blue] Entry Name:[/] "));
        VaultManager.CurrentVault?.FindEntries(name);

        var menuTable = new Table();

        menuTable.AddColumn("Username");
        menuTable.AddColumn("Password");

        foreach (var entry in VaultManager.CurrentVault!.PasswordEntries)
        {
            menuTable.AddRow(entry.Username, entry.EncryptedPassword);
        }

        AnsiConsole.Write(menuTable);
    }

    private static void DeleteVaultPasswordEntry()
    {
        string name = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] Choose entry (press 'enter' to choose one) [/]")
                .MoreChoicesText("[grey](Move up and down to reveal more entries)[/]")
                .AddChoices(VaultManager.CurrentVault!.GetAllEntries().Select(x => x.Username)));

        VaultManager.CurrentVault.DeleteEntry(name);
    }

    private static void EditVaultPasswordEntry()
    {
        string entryName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] CHoose an entry to edit (press 'enter' to choose one) [/]")
                .MoreChoicesText("[grey](Move up and down to reveal more vaults)[/]")
                .AddChoices(VaultManager.CurrentVault!.GetAllEntries().Select(x => x.Username)));

        var entry = VaultManager.CurrentVault.GetEntry(entryName);
        if (entry == null)
        {
            AnsiConsole.MarkupLine("[bold red]No entry found with that username.[/]");
            return;
        }

        var updatedEntry = new PasswordEntry(entry!.Username, entry.EncryptedPassword, entry.Timestamp.ToShortDateString());
        int option = 0;

        while (option != 3)
        {

            string a = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[orange1] What would you like to edit? (press 'enter' to choose one) [/]")
                    .MoreChoicesText("[grey](Move up and down to reveal more vaults)[/]")
                    .AddChoices(new[] { "Username", "Password", "Exit" }));

            switch (a)
            {
                case "Username":
                    string newUsername = AnsiConsole.Prompt(new TextPrompt<string>("[blue] New Username: [/] "));
                    updatedEntry.Username = newUsername;
                    break;
                case "Password":
                    string newPassword = AnsiConsole.Prompt(new TextPrompt<string>("[blue] New Password: [/] ").Secret());
                    updatedEntry.EncryptedPassword = newPassword; // Assuming encryption is handled elsewhere
                    break;
                case "Exit":
                    option = 3;
                    break;
                default:
                    AnsiConsole.MarkupLine("[bold red]Invalid option selected.[/]");
                    break;
            }
        }

        VaultManager.CurrentVault.EditEntry(entry.Username, updatedEntry);
    }

    private static void DisplayVaultEntries(SortingOrder sortingOrder)
    {
        VaultManager.CurrentVault?.DisplayAllEntries(sortingOrder);
    }

    private static void AddVaultPasswordEntry()
    {
        ConsoleHelper.WriteRule("green", "Password Entry Creation");;

        string username = AnsiConsole.Prompt(new TextPrompt<string>("[blue] Username: [/] "));
        string password = AnsiConsole.Prompt(new TextPrompt<string>("[blue] Password: [/] ").Secret());

        var entry = new PasswordEntry(username, password);
        VaultManager.CurrentVault?.AddEntry(entry);
    }

    private static int DisplayMenuOption(string[][] menu, string name)
    {
        ConsoleHelper.WriteRule("green", name);
        var menuTable = new Table().HideHeaders();

        menuTable.AddColumn("Option");
        menuTable.AddColumn("Description");

        for (int i = 0; i < menu.GetLength(0); i++)
        {
            menuTable.AddRow(menu[i][0], menu[i][1]);
        }

        AnsiConsole.Write(menuTable);

        int option;

        do
        {
            option = AnsiConsole.Prompt(new TextPrompt<int>("Select an option:"));
            if (option >= 1 && option <= menu.Length) continue;
            
            AnsiConsole.MarkupLine("[bold red]Invalid option.[/]");
        } while (option < 1 || option > menu.Length);

        return option;
    }

    private static void LoginToVault()
    {
        ConsoleHelper.WriteRule("green", "Vault Authentication");

        var name = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] Choose the vault to authenticate into (press 'enter' to choose one) [/]")
                .MoreChoicesText("[grey](Move up and down to reveal more vaults)[/]")
                .AddChoices(VaultManager.Vaults.Select(x => x.Name)));

        do
        {
            var password = AnsiConsole.Prompt(new TextPrompt<string>("[blue] Enter the password of the Vault (type 'exit' to choose a different vault):[/] "));

            if (password.ToLower() == "exit")
            {
                LoginToVault();
            }

            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
            {
                AnsiConsole.MarkupLine("[bold red]Please type in a password.[/]");
                continue;
            }

            try
            {
                var result = VaultManager.AuthenticateVault(name, password);

                if (result == null)
                {
                    continue;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ex.Message);
            }
            break;
        } while (true);
    }

    private static void CreateVault()
    {
        ConsoleHelper.WriteRule("green", "Vault Creation");

        string vaultName;

        do
        {
            vaultName = AnsiConsole.Prompt(new TextPrompt<string>("Enter the vault's name:"));
            if (VaultManager.Vaults.Any(v => v.Name.Equals(vaultName, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[bold red]A vault with that name already exists![/]");
                continue;
            }

            break;
        } while (true);

        do
        {
            var vaultPassword = AnsiConsole.Prompt(new TextPrompt<string>("Enter the vault's password:").Secret());
            var confirmPassword = AnsiConsole.Prompt(new TextPrompt<string>("Confirm the vault's password:").Secret());

            if (vaultPassword != confirmPassword)
            {
                AnsiConsole.MarkupLine("[bold red]Passwords do not match![/]\n");
                continue;
            }

            try
            {
                VaultManager.CreateVault(vaultName, vaultPassword);
                break;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ex.Message);
                CreateVault();
            }
        } while (true);
    }
}