using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

[Serializable] public sealed class AccountRequest { public string username,password,refreshToken,recovery,id,secret; }
[Serializable] public sealed class AccountResponse { public string id,refreshToken,ticket,recovery,error;public bool created;public InventoryState profile; }
public static class AccountClient
{
    public static bool Enabled=>Array.IndexOf(Environment.GetCommandLineArgs(),"-dedicatedServer")<0&&(Array.IndexOf(Environment.GetCommandLineArgs(),"-cookieSmoke")<0||Array.IndexOf(Environment.GetCommandLineArgs(),"-accountFlowTest")>=0);
    private static readonly HttpClient http=new HttpClient{Timeout=TimeSpan.FromSeconds(15)};
    public static string Token;
    public static string CachePath=>Path.Combine(Application.persistentDataPath,Array.IndexOf(Environment.GetCommandLineArgs(),"-accountFlowTest")>=0?"test-account-session.bin":"account-session.bin");
    public static void Load(){try{Token=File.Exists(CachePath)?WindowsSessionProtection.Unprotect(File.ReadAllBytes(CachePath)):null;}catch{Token=null;}}
    public static bool Save(string token)
    {
        Token=token;
        try{var bytes=WindowsSessionProtection.Protect(token);string temporary=CachePath+".tmp";File.WriteAllBytes(temporary,bytes);if(File.Exists(CachePath))File.Replace(temporary,CachePath,null);else File.Move(temporary,CachePath);return true;}catch{return false;}
    }
    public static void Forget(){Token=null;if(File.Exists(CachePath))File.Delete(CachePath);}
    [Serializable] private sealed class Health { public bool ready; }
    public static IEnumerator CheckServer(Action<bool> done)
    {
        string url=null;try{url=ServerEndpoint.AccountUrl+"/health";}catch{done(false);yield break;}
        using(var cancellation=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5)))
        {
            var task=http.GetAsync(url,cancellation.Token);while(!task.IsCompleted)yield return null;
            if(task.IsCanceled||task.IsFaulted){done(false);yield break;}
            using(var response=task.Result){var read=response.Content.ReadAsStringAsync();while(!read.IsCompleted)yield return null;bool ready=false;if(read.IsCompletedSuccessfully&&response.IsSuccessStatusCode)try{ready=JsonUtility.FromJson<Health>(read.Result).ready;}catch{}done(ready);}
        }
    }
    public static IEnumerator Request(string address,string route,AccountRequest request,Action<AccountResponse,string> done)
    {
        if(address!="127.0.0.1"&&address!="localhost"){done(null,"Remote accounts require HTTPS and encrypted game transport. This build supports local accounts only.");yield break;}
        using(var content=new StringContent(JsonUtility.ToJson(request),Encoding.UTF8,"application/json"))
        {
            var task=http.PostAsync(ServerEndpoint.AccountUrl+"/"+route,content);while(!task.IsCompleted)yield return null;
            if(task.IsCanceled||task.IsFaulted){done(null,"The server is unavailable. Please try again later.");yield break;}
            using(var response=task.Result)
            {
                var read=response.Content.ReadAsStringAsync();while(!read.IsCompleted)yield return null;
                if(read.IsFaulted||read.IsCanceled){done(null,"Could not read account response. Retry sign-in.");yield break;}
                AccountResponse data=null;try{data=JsonUtility.FromJson<AccountResponse>(read.Result);}catch{}
                if(!response.IsSuccessStatusCode||data==null){done(null,data?.error??"Account request rejected.");yield break;}
                done(data,null);
            }
        }
    }
}

// Windows current-user DPAPI. No plaintext fallback on other platforms.
public static class WindowsSessionProtection
{
    [StructLayout(LayoutKind.Sequential)] private struct Blob { public int size;public IntPtr data; }
    [DllImport("crypt32.dll",SetLastError=true,CharSet=CharSet.Unicode)] private static extern bool CryptProtectData(ref Blob input,string description,IntPtr entropy,IntPtr reserved,IntPtr prompt,int flags,out Blob output);
    [DllImport("crypt32.dll",SetLastError=true)] private static extern bool CryptUnprotectData(ref Blob input,IntPtr description,IntPtr entropy,IntPtr reserved,IntPtr prompt,int flags,out Blob output);
    [DllImport("kernel32.dll")] private static extern IntPtr LocalFree(IntPtr memory);
    private static byte[] Transform(byte[] bytes,bool protect)
    {
        if(Application.platform!=RuntimePlatform.WindowsPlayer&&Application.platform!=RuntimePlatform.WindowsEditor)throw new PlatformNotSupportedException();
        var input=new Blob{size=bytes.Length,data=Marshal.AllocHGlobal(bytes.Length)};Blob output=default;
        try{Marshal.Copy(bytes,0,input.data,bytes.Length);bool ok=protect?CryptProtectData(ref input,"Echoes of the Rift session",IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,1,out output):CryptUnprotectData(ref input,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,1,out output);if(!ok)throw new InvalidOperationException("Device session protection failed.");var result=new byte[output.size];Marshal.Copy(output.data,result,0,result.Length);return result;}
        finally{Marshal.FreeHGlobal(input.data);if(output.data!=IntPtr.Zero)LocalFree(output.data);}
    }
    public static byte[] Protect(string value)=>Transform(Encoding.UTF8.GetBytes(value),true);
    public static string Unprotect(byte[] value)=>Encoding.UTF8.GetString(Transform(value,false));
}
