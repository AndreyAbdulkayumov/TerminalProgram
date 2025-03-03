using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using MessageBox_Core;

namespace TerminalProgramBase.Views
{
    public partial class ServiceWindow : Window
    {
        public string? SelectedFilePath { get; private set; }

        private enum ControlForSelect
        {
            TextBox,
            ComboBox
        }

        private readonly ControlForSelect _controlForSelect;

        private readonly IMessageBox _messageBox;


        public ServiceWindow()
        {
            InitializeComponent();

            _messageBox = new MessageBox(this);

            _controlForSelect = ControlForSelect.TextBox;

            TextBlock_Description.Text = "Введите имя файла";

            ComboBox_SelectFileName.IsVisible = false;
            TextBox_SelectFileName.IsVisible = true;
        }

        public ServiceWindow(string[] ArrayOfDocuments)
        {
            InitializeComponent();

            _messageBox = new MessageBox(this);

            _controlForSelect = ControlForSelect.ComboBox;

            TextBlock_Description.Text = "Выберите файл настроек";

            ComboBox_SelectFileName.IsVisible = true;
            TextBox_SelectFileName.IsVisible = false;
        }

        private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        private void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Select_Click(object? sender, RoutedEventArgs e)
        {
            switch (_controlForSelect)
            {
                case ControlForSelect.TextBox:
                    TextBox_ResultHandler();
                    break;

                case ControlForSelect.ComboBox:
                    ComboBox_ResultHandler();
                    break;
            }

            Close();
        }

        private void TextBox_ResultHandler()
        {
            SelectedFilePath = TextBox_SelectFileName.Text;
        }

        private void ComboBox_ResultHandler()
        {
            string? selectedFile = ComboBox_SelectFileName.SelectedItem?.ToString();

            if (selectedFile == null)
            {
                _messageBox.Show("Не удалось выбрать документ.", MessageType.Error);
            }

            else
            {
                SelectedFilePath = selectedFile;
            }
        }
    }
}
