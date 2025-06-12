using System.Reflection;

namespace Sentinel.Tests
{
    public class VaultManagerAndVaultTests
    {
        private readonly string _testDirectory;

        public VaultManagerAndVaultTests()
        {
            // Reset static state before each test
            VaultManager.Vaults = new List<Vault>();
            typeof(VaultManager).GetProperty("CurrentVault")?.SetValue(null, null);

            // Create a unique temporary directory for test files
            _testDirectory = Path.Combine(Path.GetTempPath(), "SentinelTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);

            // Use reflection to change the private static ROOT_FOLDER in FileHandler
            // This redirects all file operations to our temporary test directory.
            var fileHandlerType = typeof(FileHandler);
            var rootFolderField = fileHandlerType.GetField("ROOT_FOLDER", BindingFlags.NonPublic | BindingFlags.Static);
            rootFolderField.SetValue(null, _testDirectory + Path.DirectorySeparatorChar);

            // Reset static state before each test
            CleanupStaticState();
        }


        public void Dispose()
        {
            CleanupStaticState();
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        private void CleanupStaticState()
        {
            // Clear the static list of vaults and reset the current vault
            VaultManager.Vaults.Clear();
            var currentVaultProperty = typeof(VaultManager).GetProperty("CurrentVault", BindingFlags.Public | BindingFlags.Static);
            currentVaultProperty.SetValue(null, null);
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


        [Fact]
        [Trait("TestReport", "Case 1: Create Vault")]
        public void CreateVault_ShouldSucceedAndPersistToFile()
        {
            // Arrange
            string vaultName = "TestVault";
            string password = "strong_password_123";
            string expectedFilePath = Path.Combine(_testDirectory, "vaults.csv");

            // Act
            VaultManager.CreateVault(vaultName, password);

            // Assert
            Assert.Single(VaultManager.Vaults); // Check in-memory list
            Assert.True(File.Exists(expectedFilePath)); // Verify file was created
            string fileContent = File.ReadAllText(expectedFilePath);
            Assert.Contains(vaultName, fileContent); // Verify vault name is in the file
        }

        [Fact]
        [Trait("TestReport", "Case 2 & 3: Login to Vault")]
        public void LoginToVault_ShouldSucceedForCorrectPasswordAndFailForIncorrect()
        {
            // Arrange
            string vaultName = "LoginTestVault";
            string correctPassword = "correct_password";
            string incorrectPassword = "wrong_password";
            VaultManager.CreateVault(vaultName, correctPassword);
            VaultManager.Logout(); // Ensure we are not logged in

            // Act & Assert (Correct Password)
            var successResult = VaultManager.AuthenticateVault(vaultName, correctPassword);
            Assert.NotNull(successResult);
            Assert.NotNull(VaultManager.CurrentVault);
            Assert.Equal(vaultName, VaultManager.CurrentVault.Name);

            // Act & Assert (Incorrect Password)
            VaultManager.Logout(); // Reset before next attempt
            var failureResult = VaultManager.AuthenticateVault(vaultName, incorrectPassword);
            Assert.Null(failureResult);
            Assert.Null(VaultManager.CurrentVault);
        }

        [Fact]
        [Trait("TestReport", "Case 4, 5, 6: Add, Edit, and Delete Entry")]
        public void PasswordEntry_ShouldBeAddedEditedAndDeletedCorrectly()
        {
            // --- ARRANGE ---
            VaultManager.CreateVault("MyVault", "pw");
            var vault = VaultManager.CurrentVault;
            var vaultFilePath = Path.Combine(_testDirectory, "MyVault.csv");

            // --- ACT & ASSERT (Add) ---
            var entry = new PasswordEntry("NewUser", "EncryptedPass");
            vault.AddEntry(entry);
            Assert.Single(vault.PasswordEntries);
            Assert.Contains("NewUser,EncryptedPass", File.ReadAllText(vaultFilePath));

            // --- ACT & ASSERT (Edit) ---
            var updatedEntry = new PasswordEntry("UpdatedUser", "NewEncryptedPass");
            vault.EditEntry("NewUser", updatedEntry);
            Assert.Single(vault.PasswordEntries); // Count should be the same
            Assert.Equal("UpdatedUser", vault.PasswordEntries.First().Username);
            Assert.True(vault.PasswordEntries.First().Timestamp.Date == DateTime.Today); // Timestamp is updated
            Assert.Contains("UpdatedUser,NewEncryptedPass", File.ReadAllText(vaultFilePath));
            Assert.DoesNotContain("NewUser", File.ReadAllText(vaultFilePath));

            // --- ACT & ASSERT (Delete) ---
            vault.DeleteEntry("UpdatedUser");
            Assert.Empty(vault.PasswordEntries);
            Assert.Equal("", File.ReadAllText(vaultFilePath).Trim());
        }

        [Fact]
        [Trait("TestReport", "Case 7 & 8: Search Password Entry")]
        public void SearchPasswordEntry_ShouldReturnMatchAndHandleNoMatch()
        {
            // Arrange
            VaultManager.CreateVault("SearchVault", "pw");
            var vault = VaultManager.CurrentVault;
            vault.AddEntry(new PasswordEntry("TestUser123", "p1"));

            // Act (Matching search, case-insensitive)
            var foundEntries = vault.FindEntries("user123");

            // Assert
            Assert.Single(foundEntries);
            Assert.Equal("TestUser123", foundEntries.First().Username);

            // Act (No match)
            var notFoundEntries = vault.FindEntries("nonexistent");

            // Assert
            Assert.Empty(notFoundEntries);
        }

        [Fact]
        [Trait("TestReport", "Case 10: Sort Entries")]
        public void SortEntries_ShouldOrderByTimestampCorrectly()
        {
            // Arrange
            VaultManager.CreateVault("SortVault", "pw");
            var vault = VaultManager.CurrentVault;
            var now = DateTime.Now;
            var entryOldest = new PasswordEntry("oldest", "p", now.AddDays(-2));
            var entryNewest = new PasswordEntry("newest", "p", now);
            vault.AddEntry(entryNewest); // Add out of order
            vault.AddEntry(entryOldest);

            // Act (Ascending) & Assert
            vault.DisplayAllEntries(SortingOrder.Ascending);
            Assert.Equal("oldest", vault.PasswordEntries[0].Username);
            Assert.Equal("newest", vault.PasswordEntries[1].Username);

            // Act (Descending) & Assert
            vault.DisplayAllEntries(SortingOrder.Descending);
            Assert.Equal("newest", vault.PasswordEntries[0].Username);
            Assert.Equal("oldest", vault.PasswordEntries[1].Username);
        }

        [Fact]
        [Trait("TestReport", "Case 11: Delete Vault")]
        public void DeleteVault_ShouldRemoveVaultAndAssociatedFiles()
        {
            // Arrange
            string vaultName = "ToDelete";
            VaultManager.CreateVault(vaultName, "pw");
            var vaultFilePath = Path.Combine(_testDirectory, $"{vaultName}.csv");
            Assert.True(File.Exists(vaultFilePath), "Pre-condition: Vault file should exist.");
            Assert.Single(VaultManager.Vaults);

            // Act
            VaultManager.DeleteVault(vaultName);

            // Assert
            Assert.Empty(VaultManager.Vaults);
            Assert.False(File.Exists(vaultFilePath), "Vault's specific .csv file should be deleted.");
            string vaultsCsvContent = File.ReadAllText(Path.Combine(_testDirectory, "vaults.csv"));
            Assert.DoesNotContain(vaultName, vaultsCsvContent);
        }

        // =========================================================================
        // == EDGE CASE TESTS
        // =========================================================================

        [Fact]
        [Trait("TestReport", "Edge Case: Empty vaults.csv")]
        public void LoadVaults_WhenVaultsCsvIsEmpty_ShouldHandleGracefully()
        {
            // Arrange
            // The setup already creates an empty directory, so we just need to call Load.

            // Act
            VaultManager.LoadVaults();

            // Assert
            Assert.Empty(VaultManager.Vaults);
        }

        [Fact]
        [Trait("TestReport", "Edge Case: Invalid (Duplicate) Vault Name")]
        public void CreateVault_WithDuplicateName_ShouldNotCreateNewVault()
        {
            // Arrange
            string vaultName = "DuplicateVault";
            VaultManager.CreateVault(vaultName, "pw1");

            // Act
            VaultManager.CreateVault(vaultName, "pw2"); // Attempt to create again

            // Assert
            Assert.Single(VaultManager.Vaults); // Should only be one vault with that name
        }
    }
}