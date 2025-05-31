using BCrypt.Net;
using Spectre.Console;

public static class VaultManager
{
    public static List<Vault> Vaults { get; set; } = new List<Vault>();
    public static bool IsEmpty => Vaults.Count == 0;
    public static string PasswordHash;
    public static Vault CurrentVault { get; private set; }

    public static void LoadVaults()
    {
        Vaults = FileHandler.LoadVaultsFromCsv();
        foreach (var vault in Vaults)
        {
            vault.PasswordEntries = FileHandler.LoadPasswordsFromCsv(vault.Name);
        }
    }
    public static void SaveVaults()
    {
        //FileHandler.SaveVaultsToCSV(Vaults, false);
    }

    public static void DisplayVaults()
    {
        var vaultTable = new Table();

        vaultTable.AddColumn("Name");
        vaultTable.AddColumn("Total Entries");

        var rule2 = new Rule("[blue]Vaults[/]");
        rule2.RuleStyle("white");
        rule2.LeftJustified();
        AnsiConsole.Write(rule2);

        Vaults.ForEach(vault =>
        {
            vaultTable.AddRow(vault.Name, vault.TotalEntries.ToString());
        });
        AnsiConsole.Write(vaultTable);
    }
    public static void CreateVault(string name, string password)
    {
        var entries = Vaults.Where(e =>
        {
            //allows me to ignore case LINQ  is pretty nice, java should add it 
            return e.Name.Contains(name, StringComparison.OrdinalIgnoreCase);
        }).ToList();

        if (entries.Count > 0)
        {
            throw new Exception("[bold red]A vault with this name already exists.[/]");
        }
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        Vaults.Add(new Vault(name, passwordHash));
        FileHandler.SaveVaultsToCSV(Vaults, false);
        AnsiConsole.MarkupLine($"[green]Vault '{name}' saved successfully.[/]\n");
    }
    public static void DeleteVault(string name)
    {
        var vaultToDelete = Vaults.FirstOrDefault(x => x.Name == name);

        if (vaultToDelete is null)
        {
            throw new Exception("[bold red]Vault does not exist.[/]");
        }

        Vaults.Remove(vaultToDelete);
        FileHandler.SaveVaultsToCSV(Vaults, false);
        AnsiConsole.MarkupLine($"[green]Vault '{name}' removed successfully.[/]\n");
    }
    public static Vault? AuthenticateVault(string name, string password)
    {
        try
        {
            var vault = Vaults.FirstOrDefault(x => x.Name == name);
            if (vault is null)
            {
                throw new Exception("[bold red]Vault does not exist.[/]");
            }

            else if (!BCrypt.Net.BCrypt.Verify(password, vault.PasswordHash))
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
}