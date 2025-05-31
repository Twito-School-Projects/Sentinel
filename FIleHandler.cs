public static class FileHandler
{
    /// <summary>
    /// Loads vaults from a CSV file.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static List<Vault> LoadFromCsv(string file)
    {
        var vaults = new List<Vault>();

        using (var reader = new StreamReader(file))
        {
            string line;
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

                using (var reader = new StreamReader(file))
                {
                    // Skip the first line which contains vault name and password hash
                    reader.ReadLine();
                }
                // Read password entries of the vault
                string pLine;
                while ((pLine = reader.ReadLine()) != null)
                {
                    var entryAttributes = pLine.Split(',');
                    if (entryAttributes.Length < 3)
                    {
                        continue; // Skip invalid entries
                    }

                    var username = entryAttributes[0];
                    var encryptedPassword = entryAttributes[1];
                    var timestamp = entryAttributes[2];

                    //Adds the password entry to the vault
                    vault.AddEntry(new PasswordEntry(username, encryptedPassword, timestamp));
                }

                //adds the vault to the list of vaults
                vaults.Add(vault);
            }
        }
        return vaults;
    }

    /// <summary>
    /// Saves the vaults to a CSV file.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="vaults"></param>
    /// <param name="overwite"></param>
    public static void SaveToCsv(string file, List<Vault> vaults, bool overwite)
    {
        using (var writer = new StreamWriter(file, overwite))
        {
            foreach (var vault in vaults)
            {
                writer.WriteLine($"{vault.Name},{vault.PasswordHash}");
                foreach (var entry in vault.PasswordEntries)
                {
                    writer.WriteLine($"{entry.Username},{entry.EncryptedPassword},{entry.Timestamp}");
                }
            }
        }
    }
}