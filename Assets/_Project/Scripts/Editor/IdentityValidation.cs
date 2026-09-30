using System;
using System.IO;
using UnityEngine;

public static class IdentityValidation
{
    public static void Validate()
    {
        if (ProjectValidation.DetectPipeline() != "BuiltIn") throw new Exception("Unexpected render pipeline.");
        string root = Path.Combine(Path.GetTempPath(), "rift-migration-" + Guid.NewGuid().ToString("N"));
        string old = Path.Combine(root, "old"), current = Path.Combine(root, "new");
        Directory.CreateDirectory(old); Directory.CreateDirectory(current);
        File.WriteAllText(Path.Combine(old, "profile-v1.json"), "legacy");
        File.WriteAllText(Path.Combine(current, "profile-v1.json"), "current");
        File.WriteAllText(Path.Combine(old, "account-session.bin"), "protected-session-fixture");
        File.WriteAllText(Path.Combine(old, "connection-identity.json"), "identity-fixture");
        File.WriteAllText(Path.Combine(old, "test-account-session.bin"), "must-not-copy");
        LegacyStorageMigration.Migrate(old, current);
        if (File.ReadAllText(Path.Combine(current, "profile-v1.json")) != "current"
            || !File.Exists(Path.Combine(current, "connection-identity.json"))
            || !File.Exists(Path.Combine(current, "account-session.bin"))
            || File.Exists(Path.Combine(current, "test-account-session.bin")))
            throw new Exception("Storage migration did not preserve account isolation.");
        File.Delete(Path.Combine(current, "account-session.bin"));
        LegacyStorageMigration.Migrate(old, current);
        if (File.Exists(Path.Combine(current, "account-session.bin"))) throw new Exception("Sign-out token was resurrected.");
        foreach (string folder in new[] { old, current }) { foreach (string file in Directory.GetFiles(folder)) File.Delete(file); Directory.Delete(folder); }
        Directory.Delete(root);
        Debug.Log("IDENTITY_OK: migration preserves current saves, copies protected identity, excludes test sessions and respects sign-out.");
    }
}
