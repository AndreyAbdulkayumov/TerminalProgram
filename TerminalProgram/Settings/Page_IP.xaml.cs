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

namespace TerminalProgram.Settings
{
    /// <summary>
    /// Логика взаимодействия для Page_IP.xaml
    /// </summary>
    public partial class Page_IP : Page
    {
        private DeviceData Settings;

        public Page_IP(ref DeviceData Settings)
        {
            InitializeComponent();

            this.Settings = Settings;
        }

        public void UpdateUI(DeviceData UpdateSettings)
        {
            if (UpdateSettings.Connection_IP == null)
            {
                throw new Exception("Нет информации о настройках подключения по Ethernet.");
            }

            Settings = UpdateSettings;

            SetValue(TextBox_IP_Address, UpdateSettings.Connection_IP.IP_Address);
            SetValue(TextBox_Port, UpdateSettings.Connection_IP.Port);
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
            if (Settings.Connection_IP != null)
            {
                Settings.Connection_IP.IP_Address = TextBox_IP_Address.Text;
            }
        }

        private void TextBox_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Settings.Connection_IP != null)
            {
                Settings.Connection_IP.Port = TextBox_Port.Text;
            }
        }
    }
}
