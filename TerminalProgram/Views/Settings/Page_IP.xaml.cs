using System;
using System.Collections.Generic;
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
using TerminalProgram.ViewModels;
using TerminalProgram.ViewModels.Settings;

namespace TerminalProgram.Views.Settings
{
    /// <summary>
    /// Логика взаимодействия для Page_IP.xaml
    /// </summary>
    public partial class Page_IP : Page
    {
        public Page_IP(ViewModel_Settings_Ethernet ViewModel)
        {
            InitializeComponent();

            DataContext = ViewModel;
        }
    }
}
