using Spectre.Console;

public static class FileHandler
{
    private static string VAULT_PATH = "Resources/";

    /// <summary>
    /// Loads vaults from a CSV file.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static List<Vault> LoadVaultsFromCsv()
    {
        string fileName = VAULT_PATH + "vaults.csv";

        if (!File.Exists(fileName))
        {
            AnsiConsole.MarkupLine($"[orange1]Vault does not have a file, creating one[/]\n");
        }

        var vaults = new List<Vault>();

        using (var reader = new StreamReader(VAULT_PATH + "vaults.csv"))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var attributes = line.Split(',');

                var vaultName = attributes[0];
                var passwordHash = attributes[1];

                if (attributes.Length < 2)
                {
                    continue; // Skip invalid entries
                }

                //creates a new vault
                var vault = new Vault(vaultName, passwordHash);

                //adds the vault to the list of vaults
                vaults.Add(vault);
            }
        }
        return vaults;
    }

    public static List<PasswordEntry> LoadPasswordsFromCsv(string vaultName)
    {
        var entries = new List<PasswordEntry>();
        string fileName = VAULT_PATH + vaultName + ".csv";

        if (!File.Exists(fileName))
        {
            AnsiConsole.MarkupLine($"[orange1]Vault does not have a file, creating one[/]\n");
        }
        using (var reader = new StreamReader(fileName))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var attributes = line.Split(',');

                var username = attributes[0];
                var encryptedPassword = attributes[1];
                var timestamp = attributes[2];

                if (attributes.Length < 3)
                {
                    continue; // Skip invalid entries
                }

                //creates a new vault
                var vault = new PasswordEntry(username, encryptedPassword, timestamp);

                //adds the vault to the list of vaults
                entries.Add(vault);
            }
        }
        return entries;
    }

    /// <summary>
    /// Saves the vaults to a CSV file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="vaults"></param>
    /// <param name="append"></param>
    public static void SaveVaultsToCSV(List<Vault> vaults, bool append)
    {
        string fileName = VAULT_PATH + "vaults.csv";

        if (!File.Exists(fileName))
        {
            AnsiConsole.MarkupLine($"[orange1]Vault does not have a file, creating one[/]\n");
        }

        using (var writer = new StreamWriter(fileName, append))
        {
            foreach (var vault in vaults)
            {
                writer.WriteLine($"{vault.Name},{vault.PasswordHash}");
                SavePasswordsToCSV(vault, false);
            }
        }
    }

    /// <summary>
    /// Saves the passwords of a vault to a CSV file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="vaults"></param>
    /// <param name="append"></param>
    public static void SavePasswordsToCSV(Vault vault, bool append)
    {
        string fileName = VAULT_PATH + vault.Name + ".csv";

        if (!File.Exists(fileName))
        {
            AnsiConsole.MarkupLine($"[orange1]Vault does not have a file, creating one[/]\n");
        }

        using (var writer = new StreamWriter(fileName, append))
        {
            foreach (var entry in vault.PasswordEntries)
            {
                writer.WriteLine($"{entry.Username},{entry.EncryptedPassword},{entry.Timestamp}");
            }
        }
    }
}