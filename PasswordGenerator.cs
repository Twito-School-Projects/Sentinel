public static class PasswordGenerator
{
    private static Random random = new Random();
    private static string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+[{]}|;:',<.>/?`~]";

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