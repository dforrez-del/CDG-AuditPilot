using CDG.AuditPilot.Application;
using Xunit;
namespace CDG.AuditPilot.Tests;
public sealed class FlowTests
{
 [Fact] public void ForkIsPlacedBelowItsParent()
 {
  var flow=new FlowDiagram(1,"Validation",[new(){Id="a"},new(){Id="b"},new(){Id="c"}],[new(){From="a",To="b",Label="Oui"},new(){From="a",To="c",Label="Non"}]);
  var positions=flow.Layout();Assert.True(positions["b"].Y>positions["a"].Y);Assert.Equal(positions["b"].Y,positions["c"].Y);Assert.NotEqual(positions["b"].X,positions["c"].X);
 }
 [Fact] public void CyclesDoNotLoseStepsOrHang()
 {
  var flow=new FlowDiagram(1,"Boucle",[new(){Id="a"},new(){Id="b"}],[new(){From="a",To="b"},new(){From="b",To="a"}]);Assert.Equal(2,flow.Layout().Count);
 }
 [Fact] public void DanglingEdgesAreRejected()
 {
  var flow=new FlowDiagram(1,"Invalide",[new(){Id="a"}],[new(){From="a",To="absent"}]);Assert.Throws<ArgumentException>(()=>FlowDiagram.Validate(flow));
 }
}
