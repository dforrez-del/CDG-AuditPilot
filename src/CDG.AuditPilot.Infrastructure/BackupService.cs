using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
namespace CDG.AuditPilot.Infrastructure;
public static class BackupService {
 private static readonly byte[] Magic="CDGBAK01"u8.ToArray();
 public static void Create(string root,string destination,string password){
  if(password.Length<12)throw new ArgumentException("Mot de passe de sauvegarde : 12 caractères minimum.");
  var work=Path.Combine(Path.GetTempPath(),"cdg-backup-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(work);
  try{
   var files=Directory.GetFiles(root,"*",SearchOption.AllDirectories).Where(f=>!Path.GetFileName(f).EndsWith("-wal")&&!Path.GetFileName(f).EndsWith("-shm")&&!Path.GetFileName(f).EndsWith(".db")&&!Path.GetFileName(f).Equals("secrets.bin")&&!f.EndsWith(".tmp")).ToArray();
   foreach(var f in files){if((File.GetAttributes(f)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Lien symbolique interdit.");var dest=Path.Combine(work,Path.GetRelativePath(root,f));Directory.CreateDirectory(Path.GetDirectoryName(dest)!);File.Copy(f,dest);}
   using(var source=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=Path.Combine(root,"auditpilot.db")}.ToString()))using(var dest=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=Path.Combine(work,"auditpilot.db")}.ToString())){source.Open();dest.Open();source.BackupDatabase(dest);}
   var hashes=Directory.GetFiles(work,"*",SearchOption.AllDirectories).ToDictionary(f=>Path.GetRelativePath(work,f).Replace('\\','/'),f=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(f))));
   File.WriteAllText(Path.Combine(work,"manifest.json"),JsonSerializer.Serialize(hashes));
   using var zip=new MemoryStream();ZipFile.CreateFromDirectory(work,zip);if(zip.Length>512_000_000)throw new InvalidDataException("Sauvegarde supérieure à 512 Mo.");
   var salt=RandomNumberGenerator.GetBytes(16);var nonce=RandomNumberGenerator.GetBytes(12);var key=Rfc2898DeriveBytes.Pbkdf2(password,salt,210000,HashAlgorithmName.SHA256,32);var plain=zip.ToArray();var cipher=new byte[plain.Length];var tag=new byte[16];
   try{using var aes=new AesGcm(key,16);aes.Encrypt(nonce,plain,cipher,tag,Magic);using(var file=File.Create(destination+".tmp")){file.Write(Magic);file.Write(salt);file.Write(nonce);file.Write(tag);file.Write(cipher);}File.Move(destination+".tmp",destination,true);}finally{CryptographicOperations.ZeroMemory(key);CryptographicOperations.ZeroMemory(plain);}
  }finally{SqliteConnection.ClearAllPools();Directory.Delete(work,true);}
 }
 public static void Restore(string archive,string root,string password){
  if(new FileInfo(archive).Length>512_000_060)throw new InvalidDataException("Archive trop volumineuse.");
  var bytes=File.ReadAllBytes(archive);if(bytes.Length<52||!bytes.AsSpan(0,8).SequenceEqual(Magic))throw new InvalidDataException("Format de sauvegarde inconnu.");
  var key=Rfc2898DeriveBytes.Pbkdf2(password,bytes.AsSpan(8,16),210000,HashAlgorithmName.SHA256,32);var plain=new byte[bytes.Length-52];
  try{using var aes=new AesGcm(key,16);aes.Decrypt(bytes.AsSpan(24,12),bytes.AsSpan(52),bytes.AsSpan(36,16),plain,Magic);}finally{CryptographicOperations.ZeroMemory(key);}
  var stage=root+".restore-"+Guid.NewGuid().ToString("N");Directory.CreateDirectory(stage);
  try{
   using(var memory=new MemoryStream(plain))using(var zip=new ZipArchive(memory,ZipArchiveMode.Read)){
    if(zip.Entries.Sum(e=>e.Length)>1_000_000_000||zip.Entries.Count>10000)throw new InvalidDataException("Archive décompressée trop volumineuse.");
    var paths=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach(var entry in zip.Entries){var name=entry.FullName.Replace('\\','/');if(name.Split('/').Any(s=>s is ".." or ".")||Path.IsPathRooted(name)||name.Contains(':')||!paths.Add(name))throw new InvalidDataException("Chemin de sauvegarde invalide.");if(name.EndsWith('/'))continue;var path=new EvidenceStore(stage).Resolve(name);Directory.CreateDirectory(Path.GetDirectoryName(path)!);entry.ExtractToFile(path);}
   }
   var hashes=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(stage,"manifest.json")))??throw new InvalidDataException("Manifeste absent.");
   if(!hashes.ContainsKey("auditpilot.db"))throw new InvalidDataException("Base absente.");
   var actual=Directory.GetFiles(stage,"*",SearchOption.AllDirectories).Select(f=>Path.GetRelativePath(stage,f).Replace('\\','/')).Where(n=>n!="manifest.json").ToHashSet();if(!actual.SetEquals(hashes.Keys))throw new InvalidDataException("Fichiers inattendus.");
   foreach(var (name,hash) in hashes){var file=new EvidenceStore(stage).Resolve(name);if(!Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).Equals(hash,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Empreinte incorrecte.");}
   using(var db=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=Path.Combine(stage,"auditpilot.db"),Mode=SqliteOpenMode.ReadOnly}.ToString())){db.Open();using var cmd=db.CreateCommand();cmd.CommandText="PRAGMA integrity_check";if((string?)cmd.ExecuteScalar()!="ok")throw new InvalidDataException("Base corrompue.");}
   new CaseworkStore(Path.Combine(stage,"auditpilot.db")).Load().Validate();SqliteConnection.ClearAllPools();File.Delete(Path.Combine(stage,"manifest.json"));
   var before=root+".before-restore-"+Guid.NewGuid().ToString("N");bool moved=Directory.Exists(root);if(moved)Directory.Move(root,before);
   try{Directory.Move(stage,root);}catch{if(moved)Directory.Move(before,root);throw;}
  }finally{CryptographicOperations.ZeroMemory(plain);if(Directory.Exists(stage))Directory.Delete(stage,true);}
 }
}
