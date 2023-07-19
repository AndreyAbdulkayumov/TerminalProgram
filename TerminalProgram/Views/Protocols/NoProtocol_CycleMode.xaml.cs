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
using TerminalProgram.ViewModels;
using TerminalProgram.ViewModels.MainWindow;

namespace TerminalProgram.Views.Protocols
{
    /// <summary>
    /// Логика взаимодействия для NoProtocol_CycleMode.xaml
    /// </summary>
    public partial class NoProtocol_CycleMode : Window
    {
        private readonly Action SendUI_Enable;

        public NoProtocol_CycleMode(Action SendUI_Enable_Handler)
        {
            InitializeComponent();

            SendUI_Enable = SendUI_Enable_Handler;

            DataContext = new ViewModel_NoProtocol_CycleMode(
                this,
                MessageView.Show,
                UI_State_Work,
                UI_State_Wait
                );
        }

        private void UI_State_Work()
        {
            Grid_Controls.Visibility = Visibility.Hidden;
            Grid_WorkInfo.Visibility = Visibility.Visible;

            TextBlock_Info.Text = "Каждые " + TextBox_Message_Period.Text + " " + TextBlock_Period_Unit.Text +
                " отправляется сообщение";

            TextBlock_SendMessage.Text = TextBox_Message_Content.Text;

            if (CheckBox_Message_CR.IsChecked == true)
            {
                TextBlock_SendMessage.Text += "\\r";
            }

            if (CheckBox_Message_LF.IsChecked == true)
            {
                TextBlock_SendMessage.Text += "\\n";
            }
        }

        private void UI_State_Wait()
        {
            Grid_Controls.Visibility = Visibility.Visible;
            Grid_WorkInfo.Visibility = Visibility.Hidden;

            TextBox_Message_Content.Focus();
        }

        private void SourceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
    }
}
