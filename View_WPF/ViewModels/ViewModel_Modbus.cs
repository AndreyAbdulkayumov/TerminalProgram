using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Models.Modbus;

namespace View_WPF.ViewModels
{
    public class ViewModel_Modbus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        #region Properties



        #endregion

        #region Commands

        public ICommand Write_Command { get; }
        public ICommand Read_Command { get; }

        #endregion

        private Model_Modbus Model = new Model_Modbus();

        private ViewMessage Message;

        public ViewModel_Modbus(ViewMessage MessageBox)
        {
            Message = MessageBox;
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
