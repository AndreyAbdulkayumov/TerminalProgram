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

namespace View_WPF.Views.Settings
{
    /// <summary>
    /// Логика взаимодействия для Page_SerialPort.xaml
    /// </summary>
    public partial class Page_SerialPort : Page
    {
        private readonly string[] ArrayBaudRate = { "4800", "9600", "19200", "38400", "57600", "115200" };
        private readonly string[] ArrayParity = { "None", "Even", "Odd" };
        private readonly string[] ArrayDataBits = { "5", "6", "7", "8" };
        private readonly string[] ArrayStopBits = { "0", "1", "1.5", "2" };


        public Page_SerialPort()
        {
            InitializeComponent();


            ComboBoxFilling(ComboBox_BaudRate, ref ArrayBaudRate);
            ComboBoxFilling(ComboBox_Parity, ref ArrayParity);
            ComboBoxFilling(ComboBox_DataBits, ref ArrayDataBits);
            ComboBoxFilling(ComboBox_StopBits, ref ArrayStopBits);
        }

        public void UpdateUI()
        {
            
        }

        private void SetValue(ComboBox Box, string? Value)
        {
            if (Value == null)
            {
                Box.SelectedIndex = -1;
                return;
            }

            int index = Box.Items.IndexOf(Value);

            if (index < 0)
            {
                return;
            }

            Box.SelectedIndex = index;
        }

        private void ComboBoxFilling(ComboBox Box, ref string[] Items)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                Box.Items.Add(Items[i]);
            }
        }

        private void ComboBox_COMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                        
        }

        private void ComboBox_BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
        
        private void CheckBox_BaudRate_Custom_Enable_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void TextBox_BaudRate_Custom_SelectionChanged(object sender, RoutedEventArgs e)
        {
            
        }

        private void ComboBox_Parity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                       
        }

        private void ComboBox_DataBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                        
        }

        private void ComboBox_StopBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void Button_ReScan_COMPorts_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
