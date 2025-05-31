public class PasswordEntry
{
    public string Username { get; set; }

    public PasswordEntry(string username, string encryptedPassword, string? timestamp = null)
    {
        Username = username;
        EncryptedPassword = encryptedPassword;

        if (timestamp == null)
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        Timestamp = timestamp;
    }

    public string EncryptedPassword { get; set; }
    public string Timestamp { get; set; }
    public string GetDecryptedPassword()
    {
        return "";
    }
}