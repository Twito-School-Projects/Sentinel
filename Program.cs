/*
 * Toheeb Eji
 * Culminating Assignment
 * June 8/25
 * ICS4U1 - Mr. DiPietro
 * 
 * This program is a password manager that allows users to create and manage multiple vaults.
 * Each vault can contain multiple password entries, which can be added, edited, or deleted.
 * 
 */
using Spectre.Console;
using Spectre.Console.Extensions;

namespace CulminatingCS;

public class Program
{
    // Main menu options for vault authentication
    private static readonly string[][] vaultAuthMenu =
    [
        [ "1", "Login"],
        [ "2", "Create Vault"],
        [ "3", "Delete Vault"],
        [ "4", "Exit" ]
    ];

    // Menu options for password management once logged into a vault
    private static readonly string[][] vaultMenu =
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

    /// <summary>
    /// Main application loop that handles the program flow.
    /// </summary>
    /// <param name="ranBefore">Indicates whether this is a subsequent run after logout.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    private static async Task Run(bool ranBefore = false)
    {
        // Display title only on first run
        if (!ranBefore) AnsiConsole.MarkupLine("[underline red]Sentinel Password Manager[/]\n");

        VaultManager.LoadVaults();

        if (VaultManager.IsEmpty)
        {
            AnsiConsole.MarkupLine("[bold red]No vaults found.[/]");
            AnsiConsole.MarkupLine("Please create a vault to get started.\n");
            await Task.Delay(500).Spinner(); // Short delay with spinner for better UX

            CreateVault();
        }

        // The main authentication menu loop - continues until a vault is successfully logged into
        int option;
        do
        {
            VaultManager.DisplayVaults();

            // Shows an authentication menu and get user selection
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
                    DeleteVault();
                    break;
                case 4:
                    AnsiConsole.MarkupLine("[bold green]Exiting...[/]");
                    Environment.Exit(0);
                    return;
                default: // Invalid menu option
                    AnsiConsole.MarkupLine("[bold red]Invalid option selected.[/]");
                    break;
            }
        } while (VaultManager.CurrentVault == null); // Continue until successfully logged in

        // Password management menu loop - continues until user logs out or exits
        int option2;
        do
        {
            // Display a password management menu and get user selection
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
                    Environment.Exit(0);
                    break;
                default:
                    AnsiConsole.MarkupLine("[bold red]Invalid option selected.[/]");
                    break;
            }
        } while (option2 != vaultMenu.GetLength(0));
    }

    /// <summary>
    /// Logs out of the current vault and returns to the vault selection menu.
    /// </summary>
    private static void LogoutOfVault()
    {
        VaultManager.Logout();
        AnsiConsole.MarkupLine("[bold green]Logged out of the current vault.[/]");

        // Return to the main menu with a flag indicating the program already ran
        Run(true).Wait();
    }

    /// <summary>
    /// Searches for password entries by name and displays matching results.
    /// </summary>
    private static void FindVaultPasswordEntryByName()
    {
        // Prompt user for search term
        string name = AnsiConsole.Prompt(new TextPrompt<string>("[blue] Entry Name:[/] "));

        // Find entries matching the search term
        List<PasswordEntry> entries = VaultManager.CurrentVault?.FindEntries(name);

        var menuTable = new Table();

        menuTable.AddColumn("Username");
        menuTable.AddColumn("Password");

        foreach (var entry in entries)
        {
            menuTable.AddRow(entry.Username, entry.Password);
        }

        AnsiConsole.Write(menuTable);
    }

    /// <summary>
    /// Deletes a password entry from the current vault.
    /// </summary>
    private static void DeleteVaultPasswordEntry()
    {
        // Display a selection prompt with all available entries
        string name = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] Choose entry (press 'enter' to choose one) [/]")
                .MoreChoicesText("[grey](Move up and down to reveal more entries)[/]")
                .AddChoices(VaultManager.CurrentVault!.GetAllEntries().Select(x => x.Username)));

        VaultManager.CurrentVault.DeleteEntry(name);
    }
    
    /// <summary>
    /// Deletes a vault from the system.
    /// </summary>
    private static void DeleteVault()
    {
        // Display a selection prompt with all available vaults
        string name = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] Choose a vault to delete (press 'enter' to choose one) [/]")
                .MoreChoicesText("[grey](Move up and down to reveal more entries)[/]")
                .AddChoices(VaultManager.Vaults.Select(x => x.Name)));

        VaultManager.DeleteVault(name);
    }

    /// <summary>
    /// Edits an existing password entry in the current vault.
    /// </summary>
    private static void EditVaultPasswordEntry()
    {
        // Get all entries and prompt user to select one to edit
        var entries = VaultManager.CurrentVault!.GetAllEntries();
        string entryName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] Choose an entry to edit (press 'enter' to choose one) [/]")
                .MoreChoicesText("[grey](Move up and down to reveal more vaults)[/]")
                .AddChoices(entries.Select(x => x.Username)));

        var entry = entries.First(x => x.Username.Contains(entryName, StringComparison.OrdinalIgnoreCase));

        // Create a copy of the entry for updating
        var updatedEntry = new PasswordEntry(entry.Username, entry.Password, entry.Timestamp);
        
        int option = 0;

        // Edit loop - continue until user chooses to exit
        while (option != 3)
        {
            // Prompt user for what they want to edit
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[orange1] What would you like to edit? (press 'enter' to choose one) [/]")
                    .MoreChoicesText("[grey](Move up and down to reveal more vaults)[/]")
                    .AddChoices("Username", "Password", "Exit"));

            switch (choice)
            {
                case "Username":
                    string newUsername = AnsiConsole.Prompt(new TextPrompt<string>("[blue] New Username: [/] "));
                    updatedEntry.Username = newUsername;
                    break;
                case "Password":
                    string newPassword = AnsiConsole.Prompt(new TextPrompt<string>("[blue] New Password (enter 'random' for a randomly generated password): [/] "));
                    if (newPassword.ToLower() == "random")
                        newPassword = GeneratePassword();
                    
                    updatedEntry.Password = newPassword; // Assuming encryption is handled elsewhere
                    break;
                case "Exit":
                    option = 3;
                    break;
                default:
                    AnsiConsole.MarkupLine("[bold red]Invalid option selected.[/]");
                    break;
            }
        }

        // Save the updated entry
        VaultManager.CurrentVault.EditEntry(entry.Username, updatedEntry);
    }

    /// <summary>
    /// Generates a random password based on a user-specified length.
    /// </summary>
    /// <returns>A randomly generated password string.</returns>
    private static string GeneratePassword()
    {
        int length = AnsiConsole.Prompt(
            new TextPrompt<int>("[blue] Enter a password character length: [/] ") 
                .Validate((n) => n switch
                {
                    <= 0 => ValidationResult.Error("[bold red] Length must be greater than 0 [/]"),
                    < 50 => ValidationResult.Success(),
                    > 50 => ValidationResult.Error("[bold red] Length must be less than 50 characters [/]"),
                }));
            
        var newPassword = PasswordGenerator.Generate(length);
        return newPassword;
    }

    /// <summary>
    /// Displays all password entries in the current vault, sorted by date.
    /// </summary>
    /// <param name="sortingOrder">The order to sort entries (ascending or descending).</param>
    private static void DisplayVaultEntries(SortingOrder sortingOrder)
    {
        // Display all entries with the specified sort order
        VaultManager.CurrentVault?.DisplayAllEntries(sortingOrder);
    }

    /// <summary>
    /// Adds a new password entry to the current vault.
    /// </summary>
    private static void AddVaultPasswordEntry()
    {
        ConsoleHelper.WriteRule("green", "Password Entry Creation");;

        string username = AnsiConsole.Prompt(new TextPrompt<string>("[blue] Username: [/] "));
        string password = AnsiConsole.Prompt(new TextPrompt<string>("[blue] Password (enter 'random' for a randomly generated password): [/] "));

        if (password.ToLower() == "random")
            password = GeneratePassword();
        var entry = new PasswordEntry(username, password);
        VaultManager.CurrentVault?.AddEntry(entry);
    }

    /// <summary>
    /// Displays a menu with options and prompts the user to select one.
    /// </summary>
    /// <param name="menu">The menu array containing options and descriptions.</param>
    /// <param name="name">The title of the menu.</param>
    /// <returns>The selected menu option number.</returns>
    private static int DisplayMenuOption(string[][] menu, string name)
    {
        ConsoleHelper.WriteRule("green", name);
        var menuTable = new Table().HideHeaders();

        menuTable.AddColumn("Option");
        menuTable.AddColumn("Description");

        // Add each menu option to the table
        for (int i = 0; i < menu.GetLength(0); i++)
        {
            menuTable.AddRow(menu[i][0], menu[i][1]);
        }

        AnsiConsole.Write(menuTable);

        int option;

        // Get user input, ensuring it's within valid range
        do
        {
            option = AnsiConsole.Prompt(new TextPrompt<int>("Select an option:"));
            if (option >= 1 && option <= menu.Length) continue;
            
            AnsiConsole.MarkupLine("[bold red]Invalid option.[/]");
        } while (option < 1 || option > menu.Length);

        return option;
    }

    /// <summary>
    /// Handles the vault login process.
    /// </summary>
    private static void LoginToVault()
    {
        ConsoleHelper.WriteRule("green", "Vault Authentication");

        // Prompt user to select a vault
        var name = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] Choose the vault to authenticate into (press 'enter' to choose one) [/]")
                .MoreChoicesText("[grey](Move up and down to reveal more vaults)[/]")
                .AddChoices(VaultManager.Vaults.Select(x => x.Name)));

        do
        {
            var password = AnsiConsole.Prompt(new TextPrompt<string>("[blue] Enter the password of the Vault (type 'exit' to choose a different vault):[/] "));

            // Allow user to cancel and choose a different vault
            if (password.ToLower() == "exit")
            {
                LoginToVault();
            }

            // Validate password is not empty
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

    /// <summary>
    /// Handles the creation of a new vault.
    /// </summary>
    private static void CreateVault()
    {
        ConsoleHelper.WriteRule("green", "Vault Creation");

        string vaultName;

        do
        {
            vaultName = AnsiConsole.Prompt(new TextPrompt<string>("Enter the vault's name:"));

            // Check if a vault with this name already exists
            if (VaultManager.Vaults.Any(v => v.Name.Equals(vaultName, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[bold red]A vault with that name already exists![/]");
                continue;
            }

            break;
        } while (true);

        do
        {
            // Prompt for password and confirmation
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