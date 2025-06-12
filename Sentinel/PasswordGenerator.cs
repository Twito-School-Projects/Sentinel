namespace Sentinel;

/// <summary>
/// Provides functionality for generating secure random passwords.
/// </summary>
public static class PasswordGenerator
{
    private static Random random = new Random();

    /// <summary>The set of characters used in generated passwords.</summary>
    private static string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+{}|;:',<.>/?`~";

    /// <summary>
    /// Generates a random password of the specified length.
    /// </summary>
    /// <param name="length">The desired length of the password.</param>
    /// <returns>A randomly generated password string.</returns>
    public static string Generate(int length)
    {
        string password = "";

        for (int i = 0; i < length; i++)
        {
            int index = random.Next(characters.Length);
            password += characters[index];
        }
        return password;
    }
}