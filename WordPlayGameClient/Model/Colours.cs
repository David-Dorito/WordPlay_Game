using System;
using System.Windows.Media;

namespace WordPlayGameClient.Model
{
    internal static class Colours
    {
        public static readonly Brush DefaultDarkMode = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1F22"));
        public static readonly Brush LightDarkMode = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B2D31"));
        public static readonly Brush TextLightGray = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDDDE"));
    }
}