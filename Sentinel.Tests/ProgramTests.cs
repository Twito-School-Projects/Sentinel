using Xunit;
using Sentinel;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace Sentinel.Tests;

public class PasswordManagerTests : IDisposable
{
    private readonly string _testDirectory;

    public PasswordManagerTests()
    {
        // Create a unique temporary directory for test files
        _testDirectory = Path.Combine(Path.GetTempPath(), "SentinelTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        // Use reflection to change the private static ROOT_FOLDER in FileHandler
        // This redirects all file operations to our temporary test directory.
        var fileHandlerType = typeof(FileHandler);
        var rootFolderField = fileHandlerType.GetField("ROOT_FOLDER", BindingFlags.NonPublic | BindingFlags.Static);
        rootFolderField.SetValue(null, _testDirectory + Path.DirectorySeparatorChar);

        // Ensure static state is clean before each test
        CleanupStaticState();
    }

    // This method runs after each test to clean up files and static variables
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

    // =========================================================================
    // == TEST CASES SUMMARY
    // =========================================================================

}