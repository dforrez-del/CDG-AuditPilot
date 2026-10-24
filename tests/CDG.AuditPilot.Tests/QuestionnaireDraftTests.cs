using CDG.AuditPilot.Application;
using Xunit;
namespace CDG.AuditPilot.Tests;
public class QuestionnaireDraftTests
{
    [Fact] public void EditingPreservesIdentifiersAndOriginalSnapshot()
    {
        var source=new AuditQuestionnaire(1,"Audit",[new("Section",[new("Q1","Question","singleChoice",true,["Oui","Non"])])]);
        var draft=QuestionnaireDraft.From(source);draft.Sections[0].Questions[0].Label="Question révisée";
        var result=draft.Build();Assert.Equal("Q1",result.Sections[0].Questions[0].Id);Assert.Equal("Question",source.Sections[0].Questions[0].Label);Assert.Equal(new[]{"Oui","Non"},result.Sections[0].Questions[0].Options);
    }
    [Fact] public void InvalidChoiceAndEmptySectionsCannotBePublished()
    {
        var draft=new QuestionnaireDraft();var section=new SectionDraft();draft.Sections.Add(section);
        Assert.Throws<ArgumentException>(()=>draft.Build());section.Questions.Add(new(){Type="singleChoice"});Assert.Throws<ArgumentException>(()=>draft.Build());
        section.Questions[0].OptionsText="Un\nUn";Assert.Throws<ArgumentException>(()=>draft.Build());
    }
    [Fact] public void ReorderingKeepsDistinctSectionsAndChangingTypeRemovesOptions()
    {
        var draft=QuestionnaireDraft.From(new(1,"Audit",[new("Même titre",[new("A","A","singleChoice",false,["Un"]) ]),new("Même titre",[new("B","B","text",false,[])])]));
        draft.Sections.Move(0,1);draft.Sections[1].Questions[0].Type="text";
        var q=draft.Build();Assert.Equal(2,q.Sections.Length);Assert.Equal("B",q.Sections[0].Questions[0].Id);Assert.Empty(q.Sections[1].Questions[0].Options);
    }
}
