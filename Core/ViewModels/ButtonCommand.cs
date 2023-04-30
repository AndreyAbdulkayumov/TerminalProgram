using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Core.ViewModels
{
    public enum MessageType
    {
        Error,
        Warning,
        Information
    }

    public delegate void ViewMessage(string Message, MessageType Type);

    public class ButtonCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private Action<object?> ExecuteAction;

        private ViewMessage MessageBox;

        public ButtonCommand(Action<object?> ExecuteAction, ViewMessage MessageBox)
        {
            this.ExecuteAction = ExecuteAction;
            this.MessageBox = MessageBox;
        }

        public bool CanExecute(object? parameter)
        {
            // Кнопка всегда включена.
            return true;
        }

        public void Execute(object? parameter)
        {
            try
            {
                this.ExecuteAction(parameter);
            }
            
            catch (Exception error)
            {
                MessageBox?.Invoke(error.Message, MessageType.Error);
            }
        }
    }
}
