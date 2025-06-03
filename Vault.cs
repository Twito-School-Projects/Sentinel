using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using BCrypt.Net;
using Spectre.Console;

public enum SortingOrder
{
    Ascending,
    Descending
}

public class Vault
{
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public List<PasswordEntry> PasswordEntries { get; set; }
    public int TotalEntries => PasswordEntries.Count;
    public bool IsEmpty => PasswordEntries.Count == 0;

    public Vault(string name, string passwordHash)
    {
        Name = name;
        PasswordHash = passwordHash;
        PasswordEntries = new List<PasswordEntry>();
    }

    public void AddEntry(PasswordEntry entry)
    {
        if (PasswordEntries.Any(e => e.Username == entry.Username))
        {
            AnsiConsole.MarkupLine("[bold red]An entry with this username already exists.[/]");
            return;
        }

        PasswordEntries.Add(entry);
        FileHandler.SavePasswordsToCSV(this, false);
        AnsiConsole.MarkupLine($"[green]Entry for {entry.Username} added successfully.[/]");
    }

    public void DeleteEntry(string username)
    {
        //Find an entry in the existing list
        var entry = PasswordEntries.First(e => e.Username == username);

        if (entry != null)
        {
            PasswordEntries.Remove(entry);
            FileHandler.SavePasswordsToCSV(this, false);
            AnsiConsole.MarkupLine($"[green]Entry for {username} deleted successfully.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]No entry found with that username.[/]");
        }
    }

    public void EditEntry(string username, PasswordEntry newEntry)
    {
        // Find an entry in the existing list
        var entry = PasswordEntries.First(e => e.Username.Contains(username, StringComparison.OrdinalIgnoreCase));

        if (entry != null)
        {
            // Update the entry with new values
            entry.Username = newEntry.Username;
            entry.EncryptedPassword = newEntry.EncryptedPassword;

            Console.WriteLine(entry.Username);
            Console.WriteLine(entry.EncryptedPassword);
            FileHandler.SavePasswordsToCSV(this, false);
            AnsiConsole.MarkupLine($"[green]Entry for {username} updated successfully.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]No entry found with that username.[/]");
        }
    }

    public PasswordEntry? GetEntry(string username)
    {
        // Find an entry in the existing list
        var entry = PasswordEntries.FirstOrDefault(e => e.Username.Contains(username, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            AnsiConsole.MarkupLine("[bold red]No entry found with that username.[/]");
            return null;
        }

        return entry;
    }


    public List<PasswordEntry> GetAllEntries()
    {
        if (IsEmpty)
        {
            AnsiConsole.MarkupLine("[bold red]No entries found.[/]");
            return new List<PasswordEntry>();
        }

        return PasswordEntries;
    }

    public void DisplayAllEntries(SortingOrder order)
    {
        if (IsEmpty)
        {
            AnsiConsole.MarkupLine("[bold red]No entries found.[/]");
            return;
        }

        SortEntriesByDate(order);
        var menuTable = new Table();

        menuTable.AddColumn("Username");
        menuTable.AddColumn("Password");
        menuTable.AddColumn("Date Added");


        foreach (var entry in PasswordEntries)
        {
            menuTable.AddRow(entry.Username, entry.EncryptedPassword, entry.Timestamp);
        }

        AnsiConsole.Write(menuTable);
    }
    public List<PasswordEntry> FindEntries(string username)
    {
        // Find entries that match the username
        var entries = PasswordEntries.Where(e =>
        {
            //allows me to ignore case LINQ  is pretty nice, java should add it 
            return e.Username.Contains(username, StringComparison.OrdinalIgnoreCase);
        }).ToList();

        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[bold red]No entries found with that username.[/]");
            return new List<PasswordEntry>();
        }

        return entries;
    }
    public void SortEntriesByDate(SortingOrder order)
    {
        InsertionSort(PasswordEntries, order);
    }

    public void InsertionSort(List<PasswordEntry> entries, SortingOrder order)
    {
        for (int i = 1; i < entries.Count; i++)
        {
            var key = entries[i];
            int j = i - 1;

            //so it turns out that c# can check if dates are greater without turning them into numbers wow
            while (j >= 0 && (order == SortingOrder.Ascending ?
                DateTime.Parse(entries[j].Timestamp) > DateTime.Parse(key.Timestamp) :
                DateTime.Parse(entries[j].Timestamp) < DateTime.Parse(key.Timestamp)))
            {
                // Shift entries to the left
                entries[j + 1] = entries[j];
                j--;
            }
            entries[j + 1] = key;
        }
    }
}