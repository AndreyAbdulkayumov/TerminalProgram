using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TerminalProgram.ViewModels
{
    public enum ThemeType
    {
        Dark,
        Light
    }

    public static class ThemesManager
    {
        public const string ThemeTypeName_Dark = "Dark";
        public const string ThemeTypeName_Light = "Light";

        public static void Select(ThemeType Type)
        {
            Application.Current.Resources.Clear();

            string ThemePath = "Views/Themes/";

            switch(Type)
            {
                case ThemeType.Dark:
                    ThemePath += "Dark.xaml";
                    break;

                case ThemeType.Light:
                    ThemePath += "Light.xaml";
                    break;

                default:
                    ThemePath += "Light.xaml";
                    break;
            }

            Application.Current.Resources.MergedDictionaries.Add(
                (ResourceDictionary)Application.LoadComponent(new Uri(ThemePath, UriKind.Relative)));
        }

        public static ThemeType GetType(string TypeName)
        {
            switch(TypeName)
            {
                case ThemeTypeName_Dark:
                    return ThemeType.Dark;

                case ThemeTypeName_Light:
                    return ThemeType.Light;

                default:
                    throw new Exception("Задано неизвестное имя темы оформления: " + TypeName);
            }
        }
    }
}
