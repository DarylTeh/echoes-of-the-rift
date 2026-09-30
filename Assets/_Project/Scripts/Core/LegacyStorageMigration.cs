using System;
using System.IO;
using UnityEngine;

public static class LegacyStorageMigration
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Application.platform != RuntimePlatform.WindowsPlayer) return;
        // Only the previous Windows product directory is eligible. Never import server data.
        try
        {
            string destination=Application.persistentDataPath;
            if(string.IsNullOrWhiteSpace(destination)) return;
            var parent=Directory.GetParent(destination);var previous=parent?.Parent;
            if(previous==null) return;
            string legacy=Path.Combine(previous.FullName,"CookieRaid","CookieRaid");
            Migrate(legacy,destination);
        }
        catch (Exception) { Debug.LogWarning("Previous device storage could not be migrated. Sign in to recover server progress."); }
    }

    public static void Migrate(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        string marker = Path.Combine(destination, "legacy-storage-migrated-v1");
        if (File.Exists(marker)) return;
        foreach (string name in new[] { "profile-v1.json", "connection-identity.json", "account-session.bin" })
        {
            string old = Path.Combine(source, name), current = Path.Combine(destination, name);
            if (File.Exists(old) && !File.Exists(current)) File.Copy(old, current, false);
        }
        // Prevent an old cached token from being resurrected after sign-out.
        File.WriteAllText(marker, "Completed");
    }
}
