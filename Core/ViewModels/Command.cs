using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Core.ViewModels
{
    public class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private Action<object?> ExecuteAction;

        public Command(Action<object?> ExecuteAction)
        {
            this.ExecuteAction = ExecuteAction;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            this.ExecuteAction(parameter);
        }
    }
}
