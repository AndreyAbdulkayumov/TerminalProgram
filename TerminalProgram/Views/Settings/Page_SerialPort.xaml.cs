using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModels.Settings;

namespace TerminalProgram.Views.Settings
{
    /// <summary>
    /// Логика взаимодействия для Page_SerialPort.xaml
    /// </summary>
    public partial class Page_SerialPort : Page
    {
        public Page_SerialPort(ViewModel_Settings_SerialPort ViewModel)
        {
            InitializeComponent();

            DataContext = ViewModel;
        }
    }
}
