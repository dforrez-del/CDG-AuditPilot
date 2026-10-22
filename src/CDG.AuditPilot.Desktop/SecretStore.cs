using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace CDG.AuditPilot.Desktop;
public static class SecretStore {
 private static string PathFor(string name)=>Path.Combine(App.DataDirectory,name+".bin");
 public static void Save(string name,string value){var raw=Encoding.UTF8.GetBytes(value);try{var encrypted=ProtectedData.Protect(raw,null,DataProtectionScope.CurrentUser);File.WriteAllBytes(PathFor(name)+".tmp",encrypted);File.Move(PathFor(name)+".tmp",PathFor(name),true);}finally{CryptographicOperations.ZeroMemory(raw);}}
 public static string Load(string name){if(!File.Exists(PathFor(name)))return "";var raw=ProtectedData.Unprotect(File.ReadAllBytes(PathFor(name)),null,DataProtectionScope.CurrentUser);try{return Encoding.UTF8.GetString(raw);}finally{CryptographicOperations.ZeroMemory(raw);}}
}
