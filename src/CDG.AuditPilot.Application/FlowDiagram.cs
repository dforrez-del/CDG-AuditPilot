using System.Text.Json;
namespace CDG.AuditPilot.Application;
public sealed class FlowNode
{
 public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
 public string Label { get; set; } = "Nouvelle étape";
 public string Service { get; set; } = "";
}
public sealed class FlowEdge
{
 public string From { get; set; } = "";
 public string To { get; set; } = "";
 public string Label { get; set; } = "";
}
public sealed record FlowDiagram(int SchemaVersion, string Title, FlowNode[] Nodes, FlowEdge[] Edges)
{
 public static FlowDiagram Parse(string json)
 {
  if (json.Length > 500000) throw new ArgumentException("Schéma trop volumineux.");
  FlowDiagram flow;
  try { flow = JsonSerializer.Deserialize<FlowDiagram>(json, QuestionnaireFormat.Json) ?? throw new JsonException(); }
  catch (JsonException) { throw new ArgumentException("Schéma JSON invalide. Utilisez le modèle de flux AuditPilot."); }
  Validate(flow); return flow;
 }
 public static void Validate(FlowDiagram flow)
 {
  if (flow.SchemaVersion != 1 || string.IsNullOrWhiteSpace(flow.Title) || flow.Title.Length > 200 || flow.Nodes.Length is < 1 or > 30 || flow.Edges.Length > 60)
   throw new ArgumentException("Schéma : titre obligatoire, 1 à 30 étapes et 60 liaisons maximum.");
  var ids = new HashSet<string>();
  foreach (var n in flow.Nodes)
   if (n is null || string.IsNullOrWhiteSpace(n.Id) || !ids.Add(n.Id) || n.Id.Length > 80 || string.IsNullOrWhiteSpace(n.Label) || n.Label.Length > 120 || n.Service is null || n.Service.Length > 80)
    throw new ArgumentException("Étape invalide : identifiant unique, libellé (120 caractères max) et service (80 max).");
  foreach (var e in flow.Edges)
   if (e is null || !ids.Contains(e.From) || !ids.Contains(e.To) || e.From == e.To || e.Label is null || e.Label.Length > 80)
    throw new ArgumentException("Chaque liaison doit relier deux étapes existantes différentes (libellé 80 caractères max).");
 }
 public Dictionary<string, (double X, double Y)> Layout()
 {
  Validate(this);
  var remaining = Nodes.Select(n=>n.Id).ToHashSet();
  var placed = new Dictionary<string, (double X, double Y)>();
  var row = 0;
  while (remaining.Count > 0)
  {
   var layer = Nodes.Where(n=>remaining.Contains(n.Id) && !Edges.Any(e=>e.To==n.Id && remaining.Contains(e.From))).Select(n=>n.Id).ToArray();
   if (layer.Length == 0) layer = [Nodes.First(n=>remaining.Contains(n.Id)).Id]; // Cycle : conserver toutes les étapes et liaisons.
   for (var i=0;i<layer.Length;i++) { placed[layer[i]] = (50 + (i % 3)*330, 60 + (row + i/3)*180); remaining.Remove(layer[i]); }
   row += (layer.Length+2)/3;
  }
  return placed;
 }
}
