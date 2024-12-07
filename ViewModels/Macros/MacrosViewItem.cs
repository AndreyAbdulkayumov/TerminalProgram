using ReactiveUI;
using System.Reactive;

namespace ViewModels.Macros
{
    public class MacrosViewItem : ReactiveObject
    {
        public string Title { get; set; }
        public ReactiveCommand<Unit, Unit> Command_ItemAction { get; set; }

        public MacrosViewItem(string title, Action clickAction)
        {
            Title = title;

            Command_ItemAction = ReactiveCommand.Create(clickAction);
        }
    }
}
