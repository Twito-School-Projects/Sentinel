using Spectre.Console;

namespace CulminatingCS;

public static class FileHandler
{
    private static string ROOT_FOLDER = "Resources/";
    
    private static void EnsureDirectoryExists()
    {
        if (Directory.Exists(ROOT_FOLDER)) return;
        
        Directory.CreateDirectory(ROOT_FOLDER);
        AnsiConsole.MarkupLine($"[yellow]Resources directory created at: {ROOT_FOLDER}[/]\n");
    }

    /// <summary>
    /// Loads vaults from a CSV file.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static List<Vault> LoadVaultsFromCsv()
    {
        try
        {
            EnsureDirectoryExists();
            string fileName = Path.Combine(ROOT_FOLDER, "vaults.csv");
            if (!File.Exists(fileName))
            {
                AnsiConsole.MarkupLine($"[orange1]Vault does not have a file, creating one[/]\n");
                File.Create(fileName).Close(); 
                return [];
            }


            var vaults = new List<Vault>();

            using var reader = new StreamReader(ROOT_FOLDER + "vaults.csv");
            while (reader.ReadLine() is { } line)
            {
                var attributes = line.Split(',');
                if (attributes.Length < 2) continue; // Skip invalid entries

                var vaultName = attributes[0];
                var passwordHash = attributes[1];

                //creates a new vault
                var vault = new Vault(vaultName, passwordHash);

                //adds the vault to the list of vaults
                vaults.Add(vault);
            }

            return vaults;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return [];
        }
    }

    public static List<PasswordEntry> LoadPasswordsFromCsv(string vaultName)
    {
        try
        {
            EnsureDirectoryExists();

            var entries = new List<PasswordEntry>();
            string fileName = Path.Combine(ROOT_FOLDER, vaultName + ".csv");

            if (!File.Exists(fileName))
            {
                AnsiConsole.MarkupLine($"[orange1]Vault does not have a file, creating one[/]\n");          
                File.Create(fileName).Close(); 
                File.Create(fileName).Close(); 
                return [];
            }

            using var reader = new StreamReader(fileName);
            while (reader.ReadLine() is { } line)
            {
                var attributes = line.Split(',');
                if (attributes.Length < 3) continue; // Skip invalid entries

                var username = attributes[0];
                var encryptedPassword = attributes[1];
                var timestamp = attributes[2];

                //creates a new password entry
                var entry = new PasswordEntry(username, encryptedPassword, timestamp);

                //adds the entry to the list of entries
                entries.Add(entry);
            }

            return entries;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return [];
        }
    }

    /// <summary>
    /// Saves the vaults to a CSV file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="vaults"></param>
    /// <param name="append"></param>
    public static void SaveVaultsToCsv(List<Vault> vaults, bool append)
    {
        try
        {
            EnsureDirectoryExists();
            string fileName = Path.Combine(ROOT_FOLDER, "vaults.csv");

            if (!File.Exists(fileName))
            {
                AnsiConsole.MarkupLine($"[orange1]Vault does not have a file, creating one[/]\n");
                File.Create(fileName).Close(); 
            }

            using var writer = new StreamWriter(fileName, append);
            foreach (var vault in vaults)
            {
                writer.WriteLine($"{vault.Name},{vault.PasswordHash}");
                SavePasswordsToCsv(vault, false);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    /// <summary>
    /// Saves the passwords of a vault to a CSV file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="vaults"></param>
    /// <param name="append"></param>
    public static void SavePasswordsToCsv(Vault vault, bool append)
    {
        try
        {
            EnsureDirectoryExists();

            string fileName = Path.Combine(ROOT_FOLDER, vault.Name + ".csv");

            if (!File.Exists(fileName))
            {
                AnsiConsole.MarkupLine($"[orange1]Vault does not have a file, creating one[/]\n");
                File.Create(fileName).Close(); 
            }

            using var writer = new StreamWriter(fileName, append);
            foreach (var entry in vault.PasswordEntries)
            {
                writer.WriteLine($"{entry.Username},{entry.EncryptedPassword},{entry.Timestamp}");
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void DeleteVault(Vault vault)
    {
        try
        {
            EnsureDirectoryExists();
            string fileName = Path.Combine(ROOT_FOLDER, $"{vault.Name}.csv");

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            else
            {
                AnsiConsole.MarkupLine($"[bold orange]Vault does not have a file[/]\n");
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }
}