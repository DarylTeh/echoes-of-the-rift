using System;
using System.IO;
using UnityEngine;

[Serializable] public sealed class ServerEndpoint
{
    public string address="127.0.0.1";
    public string accountUrl="http://127.0.0.1:8082";
    public static ServerEndpoint Load()
    {
        string path=Path.Combine(Application.streamingAssetsPath,"server.json");
        return File.Exists(path)?JsonUtility.FromJson<ServerEndpoint>(File.ReadAllText(path)):new ServerEndpoint();
    }
    public static string AccountUrl
    {
        get
        {
            var config=Load();
            if(Debug.isDebugBuild&&Array.IndexOf(Environment.GetCommandLineArgs(),"-cookieSmoke")>=0&&int.TryParse(Environment.GetEnvironmentVariable("RIFT_TEST_ACCOUNT_PORT"),out int testPort))config.accountUrl="http://127.0.0.1:"+testPort;
            if(!Uri.TryCreate(config.accountUrl,UriKind.Absolute,out var uri)||(!uri.IsLoopback&&uri.Scheme!="https")||(uri.Scheme!="https"&&uri.Scheme!="http"))throw new InvalidOperationException("The account server needs a valid HTTPS address (HTTP is only allowed on this PC).");
            return config.accountUrl.TrimEnd('/');
        }
    }
}
