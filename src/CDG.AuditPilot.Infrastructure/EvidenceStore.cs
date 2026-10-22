using System.Security.Cryptography;
using CDG.AuditPilot.Application;
namespace CDG.AuditPilot.Infrastructure;
public sealed class EvidenceStore(string root) {
 public Evidence Import(Guid missionId,string sourceFile,string origin) {
  if(missionId==Guid.Empty||string.IsNullOrWhiteSpace(origin))throw new ArgumentException("Mission et origine obligatoires.");
  var info=new FileInfo(sourceFile);if(info.Length>100_000_000)throw new ArgumentException("100 Mo maximum par preuve.");
  var id=Guid.NewGuid();var relative=Path.Combine("Missions",missionId.ToString("N"),"Evidence",id.ToString("N")+".bin");var target=Resolve(relative);Directory.CreateDirectory(Path.GetDirectoryName(target)!);
  try{using(var source=File.OpenRead(sourceFile))using(var dest=new FileStream(target,FileMode.CreateNew,FileAccess.Write,FileShare.None))source.CopyTo(dest);
   using var stream=File.OpenRead(target);return new(id,missionId,Path.GetFileName(sourceFile),Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(),stream.Length,DateTimeOffset.UtcNow,origin,relative);
  }catch{if(File.Exists(target))File.Delete(target);throw;}
 }
 public string Resolve(string relative) {
  if(Path.IsPathRooted(relative))throw new InvalidDataException("Chemin absolu interdit.");
  var prefix=Path.GetFullPath(root)+Path.DirectorySeparatorChar;var path=Path.GetFullPath(Path.Combine(root,relative));
  if(!path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Chemin hors du dossier.");
  for(var dir=Path.GetDirectoryName(path);dir is not null&&dir.StartsWith(prefix,StringComparison.OrdinalIgnoreCase);dir=Path.GetDirectoryName(dir))if(Directory.Exists(dir)&&(File.GetAttributes(dir)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Lien symbolique interdit.");
  return path;
 }
 public bool Verify(Evidence evidence){using var stream=File.OpenRead(Resolve(evidence.StoredRelativePath));return stream.Length==evidence.SizeBytes&&Convert.ToHexString(SHA256.HashData(stream)).Equals(evidence.Sha256,StringComparison.OrdinalIgnoreCase);}
 public void Export(Evidence evidence,string target){if(!Verify(evidence))throw new InvalidDataException("L’empreinte de la preuve a changé.");File.Copy(Resolve(evidence.StoredRelativePath),target,false);}
}
