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

    public string EncryptedPassword { get; set; }
    public DateTime Timestamp { get; set; }
    public string GetDecryptedPassword()
    {
        return "";
    }
}