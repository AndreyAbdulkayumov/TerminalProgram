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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace View_WPF.Protocols
{
    /// <summary>
    /// Логика взаимодействия для Http.xaml
    /// </summary>
    public partial class Http : Page
    {
        private readonly string MainWindowTitle;

        public Http(MainWindow window)
        {
            InitializeComponent();

            MainWindowTitle = window.Title;
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    //Button_Send_Click(Button_Send, new RoutedEventArgs());
                    break;
            }
        }


        private void Button_SaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_ClearFieldRX_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
