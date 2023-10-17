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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TerminalProgram.Views.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для ModbusDataField.xaml
    /// </summary>
    public partial class ModbusDataField : UserControl
    {
        public static readonly DependencyProperty Property_FieldTitle =
            DependencyProperty.Register(
                nameof(FieldTitle),
                typeof(string),
                typeof(ModbusDataField)
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
                typeof(ModbusDataField)
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
                typeof(ModbusDataField)
                );

        public SolidColorBrush Field_Background
        {
            get => (SolidColorBrush)GetValue(Property_Field_Background);
            set => SetValue(Property_Field_Background, value);
        }

        public static readonly DependencyProperty Property_Data_Foreground =
            DependencyProperty.Register(
                nameof(Data_Foreground),
                typeof(SolidColorBrush),
                typeof(ModbusDataField)
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
                typeof(ModbusDataField)
                );

        public string Data
        {
            get => (string)GetValue(Property_Data);
            set => SetValue(Property_Data, value);
        }


        public ModbusDataField()
        {
            InitializeComponent();

            // Значения по умолчанию

            FieldTitle_Foreground = Brushes.Black;

            Field_Background = Brushes.White;
            Data_Foreground = Brushes.Black;
        }
    }
}
