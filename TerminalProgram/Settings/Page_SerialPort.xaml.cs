﻿using System;
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
using SystemOfSaving;

namespace TerminalProgram.Settings
{
    /// <summary>
    /// Логика взаимодействия для Page_SerialPort.xaml
    /// </summary>
    public partial class Page_SerialPort : Page
    {
        private readonly string[] ArrayBaudRate = { "4800", "9600", "19200", "38400", "57600", "115200" };
        private readonly string[] ArrayParity = { "None", "Even", "Odd" };
        private readonly string[] ArrayDataBits = { "8", "9" };
        private readonly string[] ArrayStopBits = { "0", "1", "1.5", "2" };

        private DeviceData Settings;
        private SettingsMediator SettingsManager;

        public Page_SerialPort(DeviceData Data, SettingsMediator SettingsManager)
        {
            InitializeComponent();

            Settings = Data;
            this.SettingsManager = SettingsManager;

            ComboBoxFilling(ComboBox_BaudRate, ref ArrayBaudRate);
            ComboBoxFilling(ComboBox_Parity, ref ArrayParity);
            ComboBoxFilling(ComboBox_DataBits, ref ArrayDataBits);
            ComboBoxFilling(ComboBox_StopBits, ref ArrayStopBits);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox_COMPort.AddHandler(ComboBox.MouseLeftButtonUpEvent,
                    new MouseButtonEventHandler(ComboBox_MouseLeftButtonDown), true);

            SetValue(ComboBox_COMPort, Settings.COMPort);
            SetValue(ComboBox_BaudRate, Settings.BaudRate);
            SetValue(ComboBox_Parity, Settings.Parity);
            SetValue(ComboBox_DataBits, Settings.DataBits);
            SetValue(ComboBox_StopBits, Settings.StopBits);
        }

        private void SetValue(ComboBox Box, string Value)
        {
            if (Value == SettingsManager.DefaultNodeValue)
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

        private void ComboBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SearchSerialPorts(ComboBox_COMPort);
        }

        private void SearchSerialPorts(ComboBox Box)
        {
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length != Box.Items.Count)
            {
                int CountItems = Box.Items.Count;

                for (int i = 0; i < CountItems; i++)
                {
                    Box.Items.RemoveAt(0);
                }

                foreach (string port in ports)
                {
                    Box.Items.Add(port);
                }
            }
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
            Settings.COMPort = ComboBox_COMPort.SelectedItem?.ToString();
        }

        private void ComboBox_BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.BaudRate = ComboBox_BaudRate.SelectedItem?.ToString();
        }

        private void ComboBox_Parity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Parity = ComboBox_Parity.SelectedItem?.ToString();
        }

        private void ComboBox_DataBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.DataBits = ComboBox_DataBits.SelectedItem?.ToString();
        }

        private void ComboBox_StopBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.StopBits = ComboBox_StopBits.SelectedItem?.ToString();
        }
    }
}
