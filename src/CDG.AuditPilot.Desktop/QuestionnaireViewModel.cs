using System.Collections.ObjectModel;
using CDG.AuditPilot.Application;
using CommunityToolkit.Mvvm.ComponentModel;
namespace CDG.AuditPilot.Desktop;
public partial class QuestionAnswerViewModel(AuditQuestion question, string section, string answer) : ObservableObject
{
 public AuditQuestion Question { get; } = question;
 public string Label => $"{section} • {Question.Id} — {Question.Label}" + (Question.Required ? " *" : "");
 public string Hint => Question.Type + (Question.Options.Length > 0 ? " : " + string.Join(" ; ", Question.Options) : Question.Type == "yesNo" ? " : Oui / Non" : Question.Type == "date" ? " : AAAA-MM-JJ" : "") + (Question.Type == "multipleChoice" ? " (séparer par ;)" : "");
 [ObservableProperty] private string value = answer;
 [ObservableProperty] private string comment = "";
 [ObservableProperty] private bool notApplicable;
}
public partial class QuestionnaireViewModel : ObservableObject
{
 public ObservableCollection<InterviewSession> Sessions { get; } = [];
 public ObservableCollection<QuestionAnswerViewModel> Answers { get; } = [];
 [ObservableProperty] private string status = "Importez un questionnaire JSON ou reprenez une session.";
 [ObservableProperty] private string endpoint = "https://api.openai.com/v1/chat/completions";
 [ObservableProperty] private string model = "";
 [ObservableProperty] private string result = "";
 [ObservableProperty] private bool busy;
 public bool IsIdle => !Busy;
 partial void OnBusyChanged(bool value) => OnPropertyChanged(nameof(IsIdle));
}
