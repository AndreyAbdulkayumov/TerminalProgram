﻿using MessageBox_Core;
using MessageBox_WPF;
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
using System.Windows.Shapes;
using ViewModels.MainWindow;

namespace TerminalProgram.Views.Protocols
{
    /// <summary>
    /// Логика взаимодействия для Modbus_CycleMode.xaml
    /// </summary>
    public partial class Modbus_CycleMode : Window
    {
        private readonly Action SendUI_Enable;

        private readonly ViewModel_Modbus_CycleMode ViewModel;

        private readonly WPF_MessageView MessageView;


        public Modbus_CycleMode(WPF_MessageView MessageView, Action SendUI_Enable_Handler)
        {
            InitializeComponent();

            SendUI_Enable = SendUI_Enable_Handler;

            ViewModel = new ViewModel_Modbus_CycleMode(
                MessageView.Show,
                UI_State_Work,
                UI_State_Wait
                );

            ViewModel.DeviceIsDisconnected += ViewModel_DeviceIsDisconnected;

            DataContext = ViewModel;

            this.MessageView = MessageView;
        }

        private void ViewModel_DeviceIsDisconnected(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void UI_State_Work()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Grid_Controls.IsEnabled = false;
            }));            
        }

        private void UI_State_Wait()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Grid_Controls.IsEnabled = true;
            }));            
        }

        private void SourceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.SourceWindowClosingAction();

            SendUI_Enable.Invoke();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_MinimizeApplication_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SourceWindow_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        ViewModel.Start_Stop_Handler();
                        break;

                    case Key.Escape:
                        this.Close();
                        break;
                }
            }

            catch (Exception error)
            {
                MessageView.Show(error.Message, MessageType.Error);
            }
        }
    }
}
