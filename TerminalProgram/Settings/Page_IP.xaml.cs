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
using SystemOfSaving;

namespace TerminalProgram.Settings
{
    /// <summary>
    /// Логика взаимодействия для Page_IP.xaml
    /// </summary>
    public partial class Page_IP : Page
    {
        private DeviceData Settings;
        private SettingsMediator SettingsManager;

        public Page_IP(DeviceData Data, SettingsMediator SettingsManager)
        {
            InitializeComponent();

            Settings = Data;
            this.SettingsManager = SettingsManager;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetValue(TextBox_IP, Settings.IP);
            SetValue(TextBox_Port, Settings.Port);
        }

        private void SetValue(TextBox Box, string Value)
        {
            if (Value == SettingsManager.DefaultNodeValue)
            {
                Box.Text = String.Empty;
                return;
            }

            Box.Text = Value;
        }

        private void TextBox_IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.IP = TextBox_IP.Text;
        }

        private void TextBox_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Port = TextBox_Port.Text;
        }
    }
}
