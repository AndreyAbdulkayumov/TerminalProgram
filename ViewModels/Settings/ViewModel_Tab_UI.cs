using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Settings
{
    public class ViewModel_Tab_UI : ReactiveObject
    {
        private ReactiveCommand<Unit, Unit> Select_Dark_Theme { get; }
        private ReactiveCommand<Unit, Unit> Select_Light_Theme { get; }


        public ViewModel_Tab_UI(
            Action Set_Dark_Theme_Handler,
            Action Set_Light_Theme_Handler)
        {
            Select_Dark_Theme = ReactiveCommand.Create(Set_Dark_Theme_Handler);
            Select_Light_Theme = ReactiveCommand.Create(Set_Light_Theme_Handler);
        }
    }
}
