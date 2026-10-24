using System.Collections.ObjectModel;
using CDG.AuditPilot.Application;
using CommunityToolkit.Mvvm.ComponentModel;
namespace CDG.AuditPilot.Desktop;
public partial class QuestionAnswerViewModel(AuditQuestion question, string section, string answer) : ObservableObject
{
 public AuditQuestion Question { get; } = question;
 public string Label => $"{section} • {Question.Id} — {Question.Label}" + (Question.Required ? " *" : "");
 public string Hint => Question.Type switch { "yesNo"=>"Choisissez Oui ou Non.","singleChoice"=>"Choisissez une réponse.","multipleChoice"=>"Cochez les réponses pertinentes.","scale0to5"=>"Note de 0 à 5.","scale0to10"=>"Note de 0 à 10.","number"=>"Nombre au format français, par exemple 12,5.","date"=>"Date au format AAAA-MM-JJ ou sélection dans le calendrier.","longText"=>"Réponse détaillée.",_=>"Réponse courte."};
 public bool IsApplicable => !NotApplicable;
 public string ValidationMessage => NotApplicable ? "" : QuestionnaireFormat.ValidateAnswer(Question,Value) is string error ? error+" Valeur conservée : "+Value : "";
 partial void OnValueChanged(string value)=>OnPropertyChanged(nameof(ValidationMessage));
 partial void OnNotApplicableChanged(bool value){OnPropertyChanged(nameof(IsApplicable));OnPropertyChanged(nameof(ValidationMessage));}
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
