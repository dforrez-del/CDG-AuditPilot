using System.Globalization;
using System.Xml.Linq;
namespace CDG.AuditPilot.Application;
public static class FlowSvg {
 public static string Render(FlowDiagram flow){var positions=flow.Layout();XNamespace ns="http://www.w3.org/2000/svg";string N(double n)=>n.ToString(CultureInfo.InvariantCulture);var svg=new XElement(ns+"svg",new XAttribute("viewBox",$"0 0 1100 {N(positions.Values.Max(p=>p.Y)+160)}"));svg.Add(new XElement(ns+"title",flow.Title));
  svg.Add(new XElement(ns+"defs",new XElement(ns+"marker",new XAttribute("id","arrow"),new XAttribute("viewBox","0 0 10 10"),new XAttribute("refX",9),new XAttribute("refY",5),new XAttribute("markerWidth",7),new XAttribute("markerHeight",7),new XAttribute("orient","auto-start-reverse"),new XElement(ns+"path",new XAttribute("d","M 0 0 L 10 5 L 0 10 z"),new XAttribute("fill","#355a73")))));
  foreach(var e in flow.Edges){var a=positions[e.From];var b=positions[e.To];svg.Add(new XElement(ns+"path",new XAttribute("d",$"M {N(a.X+125)} {N(a.Y+100)} L {N(b.X+125)} {N(b.Y)}"),new XAttribute("stroke","#355a73"),new XAttribute("fill","none"),new XAttribute("marker-end","url(#arrow)")),new XElement(ns+"text",new XAttribute("x",N((a.X+b.X)/2+130)),new XAttribute("y",N((a.Y+b.Y)/2+50)),e.Label));}
  foreach(var n in flow.Nodes){var a=positions[n.Id];svg.Add(new XElement(ns+"rect",new XAttribute("x",N(a.X)),new XAttribute("y",N(a.Y)),new XAttribute("width",250),new XAttribute("height",100),new XAttribute("fill","#e8f1f8"),new XAttribute("stroke","#355a73")));var t=new XElement(ns+"text",new XAttribute("font-size",12));var words=(n.Label+" / "+n.Service).Chunk(32).Select(c=>new string(c)).ToArray();for(int i=0;i<words.Length;i++)t.Add(new XElement(ns+"tspan",new XAttribute("x",N(a.X+10)),new XAttribute("y",N(a.Y+20+i*15)),words[i]));svg.Add(t);}return svg.ToString();
 }
}
