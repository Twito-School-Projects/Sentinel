using Spectre.Console;

namespace CulminatingCS;

public static class FileHandler
{
    /// <summary>The root folder path where vault data is stored.</summary>
    private static string ROOT_FOLDER = "Resources/";

    /// <summary>
    /// Ensures the resources directory exists, creating it if necessary.
    /// </summary>
    private static void EnsureDirectoryExists()
    {
        if (Directory.Exists(ROOT_FOLDER)) return;
        
        Directory.CreateDirectory(ROOT_FOLDER);
        AnsiConsole.MarkupLine($"[yellow]Resources directory created at: {ROOT_FOLDER}[/]\n");
    }

    /// <summary>
    /// Loads vaults from a CSV file.
    /// </summary>
    /// <returns>A list of loaded vaults, or an empty list if none found or an error occurred.</returns>
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

            // Read and parse the vaults CSV file
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

    /// <summary>
    /// Loads password entries for a specific vault from a CSV file.
    /// </summary>
    /// <param name="vaultName">The name of the vault to load entries for.</param>
    /// <returns>A list of password entries, or an empty list if none found or an error occurred.</returns>
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
                return [];
            }

            // Read and parse the password entries CSV file
            using var reader = new StreamReader(fileName);
            while (reader.ReadLine() is { } line)
            {
                var attributes = line.Split(',');
                if (attributes.Length < 3) continue; // Skip invalid entries

                var username = attributes[0];
                var encryptedPassword = attributes[1];
                var timestamp = attributes[2];

                // Create a new password entry from the data
                var entry = new PasswordEntry(username, encryptedPassword, timestamp);

                // Add the entry to the list
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
    /// <param name="vaults">The list of vaults to save.</param>
    /// <param name="append">Whether to append to existing file or overwrite it.</param>
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

                // Also save the password entries for this vault
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
    /// <param name="vault">The vault containing password entries to save.</param>
    /// <param name="append">Whether to append to existing file or overwrite it.</param>
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

            // Write each password entry to the CSV file
            using var writer = new StreamWriter(fileName, append);
            foreach (var entry in vault.PasswordEntries)
            {
                writer.WriteLine($"{entry.Username},{entry.Password},{entry.Timestamp}");
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    /// <summary>
    /// Deletes a vault's password entries file.
    /// </summary>
    /// <param name="vault">The vault to delete.</param>
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