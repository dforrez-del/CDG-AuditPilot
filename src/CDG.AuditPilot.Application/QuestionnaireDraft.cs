using System.Collections.ObjectModel;
using System.Text.Json;
namespace CDG.AuditPilot.Application;

public sealed class QuestionDraft
{
    public string Id { get; set; } = "Q-" + Guid.NewGuid().ToString("N");
    public string Label { get; set; } = "Nouvelle question";
    public string Type { get; set; } = "text";
    public bool Required { get; set; }
    public string OptionsText { get; set; } = "";
}
public sealed class SectionDraft
{
    public string Title { get; set; } = "Nouvelle section";
    public ObservableCollection<QuestionDraft> Questions { get; } = [];
}
public sealed class QuestionnaireDraft
{
    public string Title { get; set; } = "Nouveau questionnaire";
    public ObservableCollection<SectionDraft> Sections { get; } = [];
    public static QuestionnaireDraft From(AuditQuestionnaire source)
    {
        var draft = new QuestionnaireDraft { Title = source.Title };
        foreach (var section in source.Sections)
        {
            var target = new SectionDraft { Title = section.Title };
            foreach (var q in section.Questions)
                target.Questions.Add(new() { Id = q.Id, Label = q.Label, Type = q.Type, Required = q.Required, OptionsText = string.Join("\n", q.Options) });
            draft.Sections.Add(target);
        }
        return draft;
    }
    public AuditQuestionnaire Build()
    {
        var result = new AuditQuestionnaire(1, Title.Trim(), Sections.Select(s => new AuditSection(s.Title.Trim(), s.Questions.Select(q => new AuditQuestion(q.Id, q.Label.Trim(), q.Type, q.Required,
            q.Type is "singleChoice" or "multipleChoice" ? q.OptionsText.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) : [])).ToArray())).ToArray());
        return QuestionnaireFormat.Parse(JsonSerializer.Serialize(result, QuestionnaireFormat.Json));
    }
}
