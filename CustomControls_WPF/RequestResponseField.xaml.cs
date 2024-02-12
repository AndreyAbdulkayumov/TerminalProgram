using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomControls_WPF
{
    /// <summary>
    /// Логика взаимодействия для RequestResponseField.xaml
    /// </summary>
    public partial class RequestResponseField : UserControl
    {
        public static readonly DependencyProperty Property_ByteNumber_Background =
            DependencyProperty.Register(
                nameof(ByteNumber_Background),
                typeof(SolidColorBrush),
                typeof(RequestResponseField)
                );

        public SolidColorBrush ByteNumber_Background
        {
            get => (SolidColorBrush)GetValue(Property_ByteNumber_Background);
            set => SetValue(Property_ByteNumber_Background, value);
        }

        public IEnumerable FieldItems
        {
            get => (IEnumerable)GetValue(FieldItems_Property);
            set => SetValue(FieldItems_Property, value);
        }

        public static readonly DependencyProperty FieldItems_Property =
            DependencyProperty.Register(
                nameof(FieldItems),
                typeof(IEnumerable),
                typeof(RequestResponseField));

        public static readonly DependencyProperty Property_FieldTitle =
            DependencyProperty.Register(
                nameof(FieldTitle),
                typeof(string),
                typeof(RequestResponseField)
                );

        public string FieldTitle
        {
            get => (string)GetValue(Property_FieldTitle);
            set => SetValue(Property_FieldTitle, value);
        }

        public static readonly DependencyProperty Property_FieldTitle_Foreground =
            DependencyProperty.Register(
                nameof(FieldTitle_Foreground),
                typeof(SolidColorBrush),
                typeof(RequestResponseField)
                );

        public SolidColorBrush FieldTitle_Foreground
        {
            get => (SolidColorBrush)GetValue(Property_FieldTitle_Foreground);
            set => SetValue(Property_FieldTitle_Foreground, value);
        }

        public static readonly DependencyProperty Property_Field_Background =
            DependencyProperty.Register(
                nameof(Field_Background),
                typeof(SolidColorBrush),
                typeof(RequestResponseField)
                );

        public SolidColorBrush Field_Background
        {
            get => (SolidColorBrush)GetValue(Property_Field_Background);
            set => SetValue(Property_Field_Background, value);
        }

        public static readonly DependencyProperty Property_Field_BorderBrush =
            DependencyProperty.Register(
                nameof(Field_BorderBrush),
                typeof(SolidColorBrush),
                typeof(RequestResponseField)
                );

        public SolidColorBrush Field_BorderBrush
        {
            get => (SolidColorBrush)GetValue(Property_Field_BorderBrush);
            set => SetValue(Property_Field_BorderBrush, value);
        }

        public static readonly DependencyProperty Property_Data_Background =
            DependencyProperty.Register(
                nameof(Data_Background),
                typeof(SolidColorBrush),
                typeof(RequestResponseField)
                );

        public SolidColorBrush Data_Background
        {
            get => (SolidColorBrush)GetValue(Property_Data_Background);
            set => SetValue(Property_Data_Background, value);
        }

        public static readonly DependencyProperty Property_Data_Foreground =
            DependencyProperty.Register(
                nameof(Data_Foreground),
                typeof(SolidColorBrush),
                typeof(RequestResponseField)
                );

        public SolidColorBrush Data_Foreground
        {
            get => (SolidColorBrush)GetValue(Property_Data_Foreground);
            set => SetValue(Property_Data_Foreground, value);
        }

        public static readonly DependencyProperty Property_Data =
            DependencyProperty.Register(
                nameof(Data),
                typeof(string),
                typeof(RequestResponseField)
                );

        public string Data
        {
            get => (string)GetValue(Property_Data);
            set => SetValue(Property_Data, value);
        }

        public ICommand Command_CopyRequest
        {
            get => (ICommand)GetValue(Command_CopyRequest_Property);
            set => SetValue(Command_CopyRequest_Property, value);
        }

        public static readonly DependencyProperty Command_CopyRequest_Property =
            DependencyProperty.Register(
                nameof(Command_CopyRequest),
                typeof(ICommand),
                typeof(RequestResponseField));

        public ICommand Command_CopyResponse
        {
            get => (ICommand)GetValue(Command_CopyResponse_Property);
            set => SetValue(Command_CopyResponse_Property, value);
        }

        public static readonly DependencyProperty Command_CopyResponse_Property =
            DependencyProperty.Register(
                nameof(Command_CopyResponse),
                typeof(ICommand),
                typeof(RequestResponseField));

        public RequestResponseField()
        {
            InitializeComponent();

            // Значения по умолчанию

            FieldTitle_Foreground = Brushes.Black;
                        
            Field_Background = Brushes.White;
            Field_BorderBrush = Brushes.Black;

            Data_Foreground = Brushes.Black;
        }
    }
}
