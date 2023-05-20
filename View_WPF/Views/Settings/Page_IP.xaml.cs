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

namespace View_WPF.Views.Settings
{
    /// <summary>
    /// Логика взаимодействия для Page_IP.xaml
    /// </summary>
    public partial class Page_IP : Page
    {
        public Page_IP()
        {
            InitializeComponent();
        }

        public void UpdateUI()
        {
            
        }

        private void SetValue(TextBox Box, string? Value)
        {
            if (Value == null)
            {
                Box.Text = String.Empty;
                return;
            }

            Box.Text = Value;
        }

        private void TextBox_IP_Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void TextBox_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
