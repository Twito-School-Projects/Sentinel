namespace Sentinel.Tests
{
    public class VaultManagerAndVaultTests
    {
        public VaultManagerAndVaultTests()
        {
            // Reset static state before each test
            VaultManager.Vaults = new List<Vault>();
            typeof(VaultManager).GetProperty("CurrentVault")?.SetValue(null, null);
        }

        [Fact]
        public void CreateVault_WithUniqueNameAndStrongPassword_CreatesVaultAndUpdatesList()
        {
            VaultManager.CreateVault("TestVault", "StrongPassword123!");
            Assert.Single(VaultManager.Vaults);
            Assert.Equal("TestVault", VaultManager.Vaults[0].Name);
        }

        [Fact]
        public void CreateVault_WithDuplicateName_DoesNotAddVault()
        {
            VaultManager.CreateVault("TestVault", "StrongPassword123!");
            VaultManager.CreateVault("TestVault", "AnotherPassword!");
            Assert.Single(VaultManager.Vaults);
        }

        [Fact]
        public void AuthenticateVault_WithCorrectPassword_ReturnsVaultAndSetsCurrentVault()
        {
            VaultManager.CreateVault("LoginVault", "CorrectPassword!");
            var vault = VaultManager.AuthenticateVault("LoginVault", "CorrectPassword!");
            Assert.NotNull(vault);
            Assert.Equal("LoginVault", vault.Name);
            Assert.Equal(vault, VaultManager.CurrentVault);
        }

        [Fact]
        public void AuthenticateVault_WithIncorrectPassword_ReturnsNull()
        {
            VaultManager.CreateVault("LoginVault", "CorrectPassword!");
            var vault = VaultManager.AuthenticateVault("LoginVault", "WrongPassword");
            Assert.Null(vault);
            Assert.Null(VaultManager.CurrentVault);
        }

        [Fact]
        public void DeleteVault_RemovesVaultFromList()
        {
            VaultManager.CreateVault("DeleteVault", "DeletePass!");
            Assert.Single(VaultManager.Vaults);
            VaultManager.DeleteVault("DeleteVault");
            Assert.Empty(VaultManager.Vaults);
        }

        [Fact]
        public void AddPasswordEntry_ToVault_IncreasesEntryCount()
        {
            VaultManager.CreateVault("EntryVault", "EntryPassword!");
            var vault = VaultManager.AuthenticateVault("EntryVault", "EntryPassword!");
            var entry = new PasswordEntry("user", "pass123");
            vault.AddEntry(entry);
            Assert.Single(vault.PasswordEntries);
            Assert.Equal("user", vault.PasswordEntries[0].Username);
        }

        [Fact]
        public void EditPasswordEntry_UpdatesEntryAndTimestamp()
        {
            VaultManager.CreateVault("EditVault", "EditPass!");
            var vault = VaultManager.AuthenticateVault("EditVault", "EditPass!");
            var entry = new PasswordEntry("user", "oldpass");
            vault.AddEntry(entry);
            var oldTimestamp = vault.PasswordEntries[0].Timestamp;
            var updatedEntry = new PasswordEntry("user", "newpass");
            vault.EditEntry("user", updatedEntry);
            Assert.Equal("newpass", vault.PasswordEntries[0].Password);
            Assert.True(vault.PasswordEntries[0].Timestamp >= oldTimestamp);
        }

        [Fact]
        public void DeletePasswordEntry_RemovesEntry()
        {
            VaultManager.CreateVault("DeleteVault", "DeletePass!");
            var vault = VaultManager.AuthenticateVault("DeleteVault", "DeletePass!");
            var entry = new PasswordEntry("user", "pass");
            vault.AddEntry(entry);
            Assert.Single(vault.PasswordEntries);
            vault.DeleteEntry("user");
            Assert.Empty(vault.PasswordEntries);
        }

        [Fact]
        public void FindEntries_ByUsername_ReturnsMatchingEntries()
        {
            VaultManager.CreateVault("SearchVault", "SearchPass!");
            var vault = VaultManager.AuthenticateVault("SearchVault", "SearchPass!");
            vault.AddEntry(new PasswordEntry("user1", "pass1"));
            vault.AddEntry(new PasswordEntry("user2", "pass2"));
            var results = vault.FindEntries("user1");
            Assert.Single(results);
            Assert.Equal("user1", results[0].Username);
        }

        [Fact]
        public void FindEntries_ByUsername_NoMatch_ReturnsEmptyList()
        {
            VaultManager.CreateVault("SearchVault", "SearchPass!");
            var vault = VaultManager.AuthenticateVault("SearchVault", "SearchPass!");
            vault.AddEntry(new PasswordEntry("user1", "pass1"));
            var results = vault.FindEntries("nonexistent");
            Assert.Empty(results);
        }

        [Fact]
        public void GetAllEntries_ReturnsAllEntries()
        {
            VaultManager.CreateVault("DisplayVault", "DisplayPass!");
            var vault = VaultManager.AuthenticateVault("DisplayVault", "DisplayPass!");
            vault.AddEntry(new PasswordEntry("user1", "pass1"));
            vault.AddEntry(new PasswordEntry("user2", "pass2"));
            var allEntries = vault.GetAllEntries();
            Assert.Equal(2, allEntries.Count);
        }

        [Fact]
        public void SortEntriesByDate_Descending_SortsCorrectly()
        {
            VaultManager.CreateVault("SortVault", "SortPass!");
            var vault = VaultManager.AuthenticateVault("SortVault", "SortPass!");
            var entry1 = new PasswordEntry("user1", "pass1", DateTime.UtcNow.AddMinutes(-10));
            var entry2 = new PasswordEntry("user2", "pass2", DateTime.UtcNow);
            vault.AddEntry(entry1);
            vault.AddEntry(entry2);
            vault.DisplayAllEntries(SortingOrder.Descending);
            Assert.Equal("user2", vault.PasswordEntries[0].Username);
            Assert.Equal("user1", vault.PasswordEntries[1].Username);
        }

        [Fact]
        public void SortEntriesByDate_Ascending_SortsCorrectly()
        {
            VaultManager.CreateVault("SortVault", "SortPass!");
            var vault = VaultManager.AuthenticateVault("SortVault", "SortPass!");
            var entry1 = new PasswordEntry("user1", "pass1", DateTime.UtcNow.AddMinutes(-10));
            var entry2 = new PasswordEntry("user2", "pass2", DateTime.UtcNow);
            vault.AddEntry(entry1);
            vault.AddEntry(entry2);
            vault.DisplayAllEntries(SortingOrder.Ascending);
            Assert.Equal("user1", vault.PasswordEntries[0].Username);
            Assert.Equal("user2", vault.PasswordEntries[1].Username);
        }
    }
}