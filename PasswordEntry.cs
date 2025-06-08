namespace CulminatingCS;

public class PasswordEntry
{
    public string Username { get; set; }

    public PasswordEntry(string username, string encryptedPassword, string? timestamp = null)
    {
        Username = username;
        EncryptedPassword = encryptedPassword;
        Timestamp = DateTime.TryParse(timestamp, out DateTime result) ? result : DateTime.Now;
    }
    
    public PasswordEntry(string username, string encryptedPassword, DateTime timestamp)
    {
        Username = username;
        EncryptedPassword = encryptedPassword;
        Timestamp = timestamp;
    }

    public string EncryptedPassword { get; set; }
    public DateTime Timestamp { get; set; }
    public string GetDecryptedPassword()
    {
        return "";
    }
}