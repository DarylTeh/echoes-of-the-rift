using System.IO;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Prevent double-decoded UTF-8 punctuation from reaching another shipped UI.
public sealed class TextEncodingValidation : IPreprocessBuildWithReport
{
    public int callbackOrder=>0;
    public void OnPreprocessBuild(BuildReport report)
    {
        var strict=new UTF8Encoding(false,true);int count=0;
        foreach(string path in Directory.EnumerateFiles("Assets/_Project","*",SearchOption.AllDirectories))
        {
            string extension=Path.GetExtension(path);if(extension!=".cs"&&extension!=".json"&&extension!=".asset"&&extension!=".unity"&&extension!=".prefab")continue;
            string text=File.ReadAllText(path,strict);
            foreach(string bad in new[]{"\u00c2\u00b7","\u00c3\u0082","\u00e2\u20ac","\ufffd"})
                if(text.Contains(bad))throw new BuildFailedException("Corrupted text encoding in "+path);
            count++;
        }
        Debug.Log("TEXT_ENCODING_OK: "+count+" authored files checked.");
    }
}
