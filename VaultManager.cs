using BCrypt.Net;
using Spectre.Console;

namespace CulminatingCS;

/// <summary>
/// Static manager class for handling vault operations.
/// Provides functionality for vault creation, authentication, and management.
/// </summary>
public static class VaultManager
{
    public static List<Vault> Vaults { get; set; } = new List<Vault>();
    public static bool IsEmpty => Vaults.Count == 0;
    public static string PasswordHash = "";

    /// <summary>Gets the currently authenticated vault, or null if no vault is authenticated.</summary>
    public static Vault? CurrentVault { get; private set; }
    
    /// <summary>
    /// Loads all vaults and their password entries from csv.
    /// </summary>
    public static void LoadVaults()
    {
        Vaults = FileHandler.LoadVaultsFromCsv();

        // Load password entries for each vault
        foreach (var vault in Vaults)
        {
            vault.PasswordEntries = FileHandler.LoadPasswordsFromCsv(vault.Name);
        }
    }

    /// <summary>
    /// Displays all vaults in a formatted table, showing vault names and entry counts.
    /// </summary>
    public static void DisplayVaults()
    {
        var vaultTable = new Table();

        vaultTable.AddColumn("Name");
        vaultTable.AddColumn("Total Entries");

        ConsoleHelper.WriteRule("blue", "Vaults");

        // Add each vault to the table
        Vaults.ForEach(vault =>
        {
            vaultTable.AddRow(vault.Name, vault.TotalEntries.ToString());
        });
        AnsiConsole.Write(vaultTable);
    }
    /// <summary>
    /// Creates a new vault with the specified name and password.
    /// </summary>
    /// <param name="name">The name for the new vault.</param>
    /// <param name="password">The password for accessing the vault.</param>
    public static void CreateVault(string name, string password)
    {
        // Check if a vault with this name already exists
        var entries = Vaults.Where(e => e.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();

        if (entries.Count > 0)
        {
            AnsiConsole.WriteLine("[bold red]A vault with this name already exists.[/]");
            return;
        }

        // Hash the password for secure storage
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        Vaults.Add(new Vault(name, passwordHash));
        FileHandler.SaveVaultsToCsv(Vaults, false);
        AnsiConsole.MarkupLine($"[green]Vault '{name}' saved successfully.[/]\n");
    }
    /// <summary>
    /// Deletes a vault with the specified name.
    /// </summary>
    /// <param name="name">The name of the vault to delete.</param>
    public static void DeleteVault(string name)
    {
        var vaultToDelete = Vaults.FirstOrDefault(x => x.Name == name);

        if (vaultToDelete is null)
        {
            AnsiConsole.WriteLine("[bold red]Vault does not exist.[/]");
            return;
        }

        Vaults.Remove(vaultToDelete);
        FileHandler.DeleteVault(vaultToDelete);
        FileHandler.SaveVaultsToCsv(Vaults, false);
        AnsiConsole.MarkupLine($"[green]Vault '{name}' removed successfully.[/]\n");
    }
    /// <summary>
    /// Authenticates and sets the current vault using the provided credentials.
    /// </summary>
    /// <param name="name">The name of the vault to authenticate.</param>
    /// <param name="password">The password for accessing the vault.</param>
    /// <returns>The authenticated vault if successful, or null if authentication failed.</returns>
    public static Vault? AuthenticateVault(string name, string password)
    {
        try
        {
            var vault = Vaults.FirstOrDefault(x => x.Name == name);
            if (vault is null)
            {
                AnsiConsole.WriteLine("[bold red]Vault does not exist.[/]");
                return null;
            }

            // Verify the provided password against stored hash
            if (!BCrypt.Net.BCrypt.Verify(password, vault.PasswordHash))
            {
                AnsiConsole.MarkupLine("[bold red]Invalid vault password provided.[/]");
                return null;
            }

            AnsiConsole.MarkupLine($"[green]Successfully logged into Vault '{name}'.[/]\n");
            CurrentVault = vault;
            return vault;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return null;
        }
    }

    /// <summary>
    /// Logs out of the current vault, saving changes and clearing the current vault reference.
    /// </summary>
    public static void Logout()
    {
        FileHandler.SaveVaultsToCsv(Vaults, false);
        CurrentVault = null!;
    }
}