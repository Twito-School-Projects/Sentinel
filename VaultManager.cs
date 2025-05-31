using Spectre.Console;

public  static class VaultManager
{
    public static List<Vault> Vaults { get; set; } = new List<Vault>();
    public static bool IsEmpty => Vaults.Count == 0;

    public static void LoadVaults()
    {
        Vaults = FileHandler.LoadFromCsv("Resources/vaults.csv");
    }
    public static void SaveVaults()
    {

    }
    public static void DisplayVaults()
    {

    }
    public static void CreateVault(string name, string password)
    {
        Vaults.Add(new Vault(name, password));
        FileHandler.SaveToCsv("Resources/vaults.csv", Vaults, false);
        AnsiConsole.MarkupLine($"[green]Vault '{name}' saved successfully.[/]\n");
    }
    public static void DeleteVault(string name)
    {

    }
    public static Vault AuthenticateVault(string name, string Password)
    {
        return null;
    }
}