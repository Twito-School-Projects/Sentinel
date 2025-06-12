namespace Sentinel;

/// <summary>
/// Represents a password entry stored in a vault.
/// Contains the username, encrypted password, and timestamp information.
/// </summary>
public class PasswordEntry
{
    public string Username { get; set; }
    public string Password { get; set; }

    /// <summary>Gets or sets the timestamp when this entry was created or last modified.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Initializes a new instance of the PasswordEntry class with string timestamp.
    /// </summary>
    /// <param name="username">The username for this entry.</param>
    /// <param name="password">The encrypted password for this entry.</param>
    /// <param name="timestamp">Optional timestamp string. If null, current time is used.</param>
    public PasswordEntry(string username, string password, string? timestamp = null)
    {
        Username = username;
        Password = password;
        Timestamp = DateTime.TryParse(timestamp, out DateTime result) ? result : DateTime.Now;
    }

    /// <summary>
    /// Initializes a new instance of the PasswordEntry class with DateTime timestamp.
    /// </summary>
    /// <param name="username">The username for this entry.</param>
    /// <param name="password">The encrypted password for this entry.</param>
    /// <param name="timestamp">The timestamp for this entry.</param>
    public PasswordEntry(string username, string password, DateTime timestamp)
    {
        Username = username;
        Password = password;
        Timestamp = timestamp;
    }
}