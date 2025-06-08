using System.IO.Pipes;
using BCrypt.Net;
using Spectre.Console;

namespace CulminatingCS;

/// <summary>
/// Specifies the order for sorting entries by date.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the Vault class.
    /// </summary>
    /// <param name="name">The name of the vault.</param>
    /// <param name="passwordHash">The hashed password for accessing the vault.</param>
    public Vault(string name, string passwordHash)
    {
        Name = name;
        PasswordHash = passwordHash;
        PasswordEntries = new List<PasswordEntry>();
    }

    /// <summary>
    /// Adds a new password entry to the vault.
    /// </summary>
    /// <param name="entry">The password entry to add.</param>
    public void AddEntry(PasswordEntry entry)
    {
        // Check for duplicate username
        if (PasswordEntries.Any(e => e.Username == entry.Username))
        {
            AnsiConsole.MarkupLine("[bold red]An entry with this username already exists.[/]");
            return;
        }

        PasswordEntries.Add(entry);
        FileHandler.SavePasswordsToCsv(this, false);
        AnsiConsole.MarkupLine($"[green]Entry for {entry.Username} added successfully.[/]");
    }

    /// <summary>
    /// Deletes a password entry from the vault by username.
    /// </summary>
    /// <param name="username">The username of the entry to delete.</param>
    public void DeleteEntry(string username)
    {
        // Find the entry to delete
        var entry = PasswordEntries.FirstOrDefault(e => e.Username == username);

        if (entry != null)
        {
            PasswordEntries.Remove(entry);
            FileHandler.SavePasswordsToCsv(this, false);
            AnsiConsole.MarkupLine($"[green]Entry for {username} deleted successfully.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]No entry found with that username.[/]");
        }
    }

    /// <summary>
    /// Updates an existing password entry with new information.
    /// </summary>
    /// <param name="username">The username of the entry to edit.</param>
    /// <param name="newEntry">The updated entry information.</param>
    public void EditEntry(string username, PasswordEntry newEntry)
    {
        // Find the entry to edit
        var entry = PasswordEntries.FirstOrDefault(e => e.Username.Contains(username, StringComparison.OrdinalIgnoreCase));

        if (entry != null)
        {
            // Update entry with new values
            entry.Username = newEntry.Username;
            entry.Password = newEntry.Password;
            entry.Timestamp = DateTime.Now; // Update timestamp to current time

            FileHandler.SavePasswordsToCsv(this, false);
            AnsiConsole.MarkupLine($"[green]Entry for {username} updated successfully.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]No entry found with that username.[/]");
        }
    }

    /// <summary>
    /// Retrieves a password entry by username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The matching password entry or null if not found.</returns>
    public PasswordEntry? GetEntry(string username)
    {
        // Find an entry by username (case-insensitive)
        var entry = PasswordEntries.FirstOrDefault(e => e.Username.Contains(username, StringComparison.OrdinalIgnoreCase));

        if (entry == null) return entry;
        AnsiConsole.MarkupLine("[bold red]No entry found with that username.[/]");
        return null;
    }


    /// <summary>
    /// Gets all password entries in the vault.
    /// </summary>
    /// <returns>A list of all password entries, or an empty list if the vault is empty.</returns>
    public List<PasswordEntry> GetAllEntries()
    {
        if (!IsEmpty) return PasswordEntries;
        AnsiConsole.MarkupLine("[bold red]No entries found.[/]");
        return [];
    }

    /// <summary>
    /// Displays all password entries in a formatted table, sorted by date.
    /// </summary>
    /// <param name="order">The order to sort the entries (ascending or descending).</param>
    public void DisplayAllEntries(SortingOrder order)
    {
        if (IsEmpty)
        {
            AnsiConsole.MarkupLine("[bold red]No entries found.[/]");
            return;
        }

        // Sort entries by date according to specified order
        SortEntriesByDate(order);
        var menuTable = new Table();

        menuTable.AddColumn("Username");
        menuTable.AddColumn("Password");
        menuTable.AddColumn("Date Added");


        foreach (var entry in PasswordEntries)
        {
            menuTable.AddRow(entry.Username, entry.Password, entry.Timestamp.ToShortDateString());
        }

        AnsiConsole.Write(menuTable);
    }
    /// <summary>
    /// Finds password entries that match a username search term.
    /// </summary>
    /// <param name="username">The username search term.</param>
    /// <returns>A list of matching password entries, or an empty list if none found.</returns>
    public List<PasswordEntry> FindEntries(string username)
    {
        // Find entries that contain the search term (case-insensitive)
        var entries = PasswordEntries.Where(e => e.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();

        if (entries.Count != 0) return entries;
        AnsiConsole.MarkupLine("[bold red]No entries found with that username.[/]");
        return [];
    }

    /// <summary>
    /// Sorts the password entries by date.
    /// </summary>
    /// <param name="order">The order to sort (ascending or descending).</param>
    private void SortEntriesByDate(SortingOrder order)
    {
        InsertionSort(PasswordEntries, order);
    }

    /// <summary>
    /// Performs an insertion sort on the password entries by timestamp.
    /// </summary>
    /// <param name="entries">The list of entries to sort.</param>
    /// <param name="order">The order to sort (ascending or descending).</param>
    private void InsertionSort(List<PasswordEntry> entries, SortingOrder order)
    {
        for (int i = 1; i < entries.Count; i++)
        {
            var key = entries[i];
            int j = i - 1;

            // Compare timestamps based on the requested sort order
            // For ascending: oldest first (earlier dates before later dates)
            // For descending: newest first (later dates before earlier dates)
            while (j >= 0 && (order != SortingOrder.Ascending ?
                       entries[j].Timestamp < key.Timestamp : // For descending order
                       entries[j].Timestamp > key.Timestamp)) // For ascending order
            {
                // Shift entries to make room for insertion
                entries[j + 1] = entries[j];
                j--;
            }
            // Insert the current entry in its sorted position
            entries[j + 1] = key;
        }
    }
}