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

namespace TerminalProgram.ServiceWindows
{
    /// <summary>
    /// Логика взаимодействия для EnterTextWindow.xaml
    /// </summary>
    public partial class EnterTextWindow : Window
    {
        public string FileName { get; private set; } = String.Empty;

        public EnterTextWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (FileName == String.Empty)
            {
                if (MessageBox.Show("Вы не ввели имя файла. Операция будет отменена. Выйти?", "Предупреждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Button_Select_Click(Button_Select, new RoutedEventArgs());
                    break;

                case Key.Escape:
                    Close();
                    break;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBox_FileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            FileName = TextBox_FileName.Text;
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
