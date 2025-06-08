# ✅ Testing Report – Password Manager

**Course:** ICS4U1<br>
**Project Name:** Sentinel Password Manager<br>
**Tested By:** Toheeb Eji<br>
**Date:** June 08/25<br>
**Files Involved:**

*   `Program.cs`
*   `VaultManager.cs`
*   `Vault.cs`
*   `PasswordEntry.cs`
*   `FileHandler.cs`

---

## 🔧 Environment Details

*   **OS:** Windows 10
*   **IDE:** Jetbrains Rider
*   **.NET Version:** .NET 8.0
*   **Libraries:** BCrypt.Net-Next, Spectre.Console, Spectre.Console.Cli
*   **Data File:** `Resources/vaults.csv` and `Resources/test-vault.csv` (used for persistent vault storage)

---

## 📋 Test Cases Summary

| #   | **Feature Tested**         | **Test Case Description**                                  | **Expected Result**                                         | **Actual Result**          | **Pass/Fail** | **Notes**                                                  |
| :-- | :------------------------- | :--------------------------------------------------------- | :---------------------------------------------------------- | :------------------------- | :------------ | :--------------------------------------------------------- |
| 1   | Create Vault               | Create a new vault with a unique name and strong password   | Vault is created, and vault list is updated                 | Vault created successfully | ✅           | Check `vaults.csv` to verify                               |
| 2   | Login to Vault             | Login to an existing vault with the correct password         | Vault is unlocked, and the vault menu is displayed          | Vault login successful     | ✅           | Verify access to password entries                          |
| 3   | Login to Vault (Incorrect) | Attempt to login to an existing vault with the wrong password | Error message is displayed, and login fails                  | Login failed as expected   | ✅           | Check for proper error handling                             |
| 4   | Add Password Entry         | Add a new password entry to a vault                         | Entry is added to the vault, and total entries count increases | Entry added successfully   | ✅           | Check if encrypted password is saved correctly              |
| 5   | Edit Password Entry        | Edit an existing password entry in a vault                   | Entry is updated in the vault with the new details           | Entry edited successfully  | ✅           | Ensure timestamp is updated                                 |
| 6   | Delete Password Entry      | Delete an existing password entry from a vault               | Entry is removed from the vault, and total entries decreases | Entry deleted successfully | ✅           | Check if deletion is handled correctly                    |
| 7   | Search Password Entry      | Search for a password entry by username                     | Matching entries are displayed                               | Search returns match       | ✅           | Verify case-insensitive search                              |
| 8   | Search Password Entry (No Match) | Search for a password entry by username when no match exists | Message indicating no matching entries is displayed          | No match message shown     | ✅           | Ensure proper message is displayed                          |
| 9   | Display All Entries        | Display all password entries in a vault                     | All entries are displayed in a readable format                 | Entries displayed correctly| ✅           | Check formatting and ensure no data is missed               |
| 10  | Sort Entries (Newest/Oldest) | Sort the entries by the timestamp (newest/oldest)           | Entries are displayed in the correct sorted order            | Entries sorted correctly   | ✅           | Test both newest and oldest sorting orders                |
| 11  | Delete Vault               | Delete an existing vault                                      | Vault is deleted, and is removed from the list of vaults    | Vault deleted successfully | ✅           | Check `vaults.csv` to verify                               |

---

## ⚠️ Edge Case Tests

| **Scenario**                     | **Result**                     | **Notes**                                                                       |
| :------------------------------- | :----------------------------- | :------------------------------------------------------------------------------ |
| Empty `vaults.csv` file        | No vaults loaded; prompt to create | Application should gracefully handle the empty file                                |
| Invalid vault name during creation | Error message displayed          | Check if proper validation is in place for vault names                            |
| Empty password during entry creation | Entry not allowed, prompt for password | Ensure password is not empty                                                          |
| Vault with many entries (e.g., 1000) | Application remains responsive | Verify performance when dealing with a large number of entries                     |
