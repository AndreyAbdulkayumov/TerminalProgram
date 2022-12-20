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
        private readonly string DefaultValue;

        public Page_IP(ref DeviceData Settings, string DefaultValue)
        {
            InitializeComponent();

            this.Settings = Settings;
            this.DefaultValue = DefaultValue;
        }

        public void UpdateUI(DeviceData UpdateSettings)
        {
            SetValue(TextBox_IP, UpdateSettings.IP);
            SetValue(TextBox_Port, UpdateSettings.Port);

            Settings = UpdateSettings;
        }

        private void SetValue(TextBox Box, string Value)
        {
            if (Value == DefaultValue)
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
