using UnityEditor;
using System.IO;
using UnityEngine;

public class PackageUpdater : Editor
{
    private static string sourcePath = "Assets/Navigation";
    private static string destinationPath = "../../../../My Unity Packages/Navigation Grid System";

    [MenuItem("Tools/Update Package Files")]
    public static void UpdatePackage()
    {
        // Ensure source exists
        if (!Directory.Exists(sourcePath))
        {
            Debug.LogError("Source path does not exist!");
            return;
        }

        // Preserve .git folder: Delete all except .git
        if (Directory.Exists(destinationPath))
        {
            foreach (var directory in Directory.GetDirectories(destinationPath))
            {
                if (!directory.EndsWith(".git"))
                {
                    Directory.Delete(directory, true);
                }
            }

            foreach (var file in Directory.GetFiles(destinationPath))
            {
                File.Delete(file);
            }

            Debug.Log("Cleaned destination folder, preserving .git folder.");
        }

        // Copy files from source to destination
        CopyFilesRecursively(sourcePath, destinationPath);
        AssetDatabase.Refresh();

        Debug.Log("Package updated successfully!");
    }

    // Custom method to copy files recursively
    private static void CopyFilesRecursively(string source, string destination)
    {
        // Create destination folder if it doesn't exist
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }

        // Copy files
        foreach (var file in Directory.GetFiles(source))
        {
            var destFile = Path.Combine(destination, Path.GetFileName(file));
            File.Copy(file, destFile, true); // Overwrite existing files
        }

        // Copy directories recursively
        foreach (var directory in Directory.GetDirectories(source))
        {
            var destDirectory = Path.Combine(destination, Path.GetFileName(directory));
            CopyFilesRecursively(directory, destDirectory);
        }
    }
}
