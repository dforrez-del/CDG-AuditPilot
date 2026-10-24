using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace CDG.AuditPilot.Desktop;

// Adapts the existing string answer format; opening a control never rewrites data.
public sealed class AnswerEditor : ContentControl
{
    private QuestionAnswerViewModel? answer;
    private bool updating;
    private Action? synchronize;
    public AnswerEditor()
    {
        DataContextChanged += (_,_) => Attach();
        Loaded += (_,_) => Attach();
        Unloaded += (_,_) => Detach();
    }
    private void Detach(){if(answer != null)answer.PropertyChanged -= Changed;answer=null;}
    private void Attach()
    {
        Detach();answer=DataContext as QuestionAnswerViewModel;synchronize=null;Content=null;
        if(answer==null)return;
        answer.PropertyChanged+=Changed;
        var panel=new StackPanel();Content=panel;
        var fields=new StackPanel();panel.Children.Add(fields);
        fields.SetBinding(IsEnabledProperty,new Binding(nameof(QuestionAnswerViewModel.IsApplicable)));
        var type=answer.Question.Type;
        if(type is "yesNo" or "singleChoice" or "scale0to5" or "scale0to10")
        {
            var values=type switch {"yesNo"=>new[]{"Oui","Non"},"scale0to5"=>Enumerable.Range(0,6).Select(x=>x.ToString(CultureInfo.InvariantCulture)).ToArray(),"scale0to10"=>Enumerable.Range(0,11).Select(x=>x.ToString(CultureInfo.InvariantCulture)).ToArray(),_=>answer.Question.Options};
            var combo=new ComboBox{ItemsSource=values,MinHeight=34,Margin=new Thickness(0,6,0,6)};fields.Children.Add(combo);
            synchronize=()=>combo.SelectedItem=values.Contains(answer!.Value)?answer.Value:null;
            combo.SelectionChanged+=(_,_)=>{if(!updating&&answer!=null)answer.Value=combo.SelectedItem as string??"";};
        }
        else if(type=="multipleChoice")
        {
            var boxes=new List<CheckBox>();
            foreach(var option in answer.Question.Options)
            {
                var box=new CheckBox{Content=option,Margin=new Thickness(0,5,0,5)};boxes.Add(box);fields.Children.Add(box);
                void Update(object sender,RoutedEventArgs e)
                {
                    if(updating||answer==null)return;
                    var unknown=answer.Value.Split(';',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries).Where(x=>!answer.Question.Options.Contains(x));
                    answer.Value=string.Join(";",boxes.Where(x=>x.IsChecked==true).Select(x=>(string)x.Content).Concat(unknown));
                }
                box.Checked+=Update;box.Unchecked+=Update;
            }
            synchronize=()=>{var selected=answer!.Value.Split(';',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);foreach(var box in boxes)box.IsChecked=selected.Contains((string)box.Content);};
        }
        else
        {
            var input=new TextBox{TextWrapping=TextWrapping.Wrap,AcceptsReturn=type=="longText",MinHeight=type=="longText"?90:34,MaxLength=10000};
            input.SetBinding(TextBox.TextProperty,new Binding(nameof(QuestionAnswerViewModel.Value)){Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});fields.Children.Add(input);
            if(type=="date")
            {
                var calendar=new System.Windows.Controls.Calendar();var expander=new Expander{Header="Choisir dans le calendrier",Content=calendar,Margin=new Thickness(0,5,0,5)};fields.Children.Add(expander);
                synchronize=()=>{calendar.SelectedDate=DateTime.TryParseExact(answer!.Value,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date)?date:null;};
                calendar.SelectedDatesChanged+=(_,_)=>{if(!updating&&answer!=null&&calendar.SelectedDate is DateTime date){answer.Value=date.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);expander.IsExpanded=false;}};
            }
        }
        var clear=new Button{Content="Effacer la réponse",HorizontalAlignment=HorizontalAlignment.Left,FontSize=11,Padding=new Thickness(8,4,8,4)};
        clear.Click+=(_,_)=>{if(answer!=null)answer.Value="";};fields.Children.Add(clear);
        Sync();
    }
    private void Changed(object? sender,PropertyChangedEventArgs e){if(e.PropertyName==nameof(QuestionAnswerViewModel.Value))Sync();}
    private void Sync(){updating=true;try{synchronize?.Invoke();}finally{updating=false;}}
}
