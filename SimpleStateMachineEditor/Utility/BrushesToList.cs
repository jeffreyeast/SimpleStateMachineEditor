using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.Utility
{
    public class DisplayColor : IEquatable<DisplayColor>, INotifyPropertyChanged
    {
        public SolidColorBrush Brush
        {
            get
            {
                return BrushesToList.ColorNameToBrushTable[ColorName] as SolidColorBrush;
            }
        }

        public string ColorName
        {
            get { return _colorName; }
            set
            {
                if ((_colorName == null ^ value == null) || !_colorName.Equals(value))
                {
                    _colorName = value;
                    OnPropertyChanged("ColorName");
                    OnPropertyChanged("Brush");
                }
            }
        }
        string _colorName;

        public event PropertyChangedEventHandler PropertyChanged;


        public DisplayColor()
        {
            _colorName = "Black";
        }

        public DisplayColor(string colorName)
        {
            _colorName = colorName;
        }

        public bool Equals(DisplayColor other)
        {
            return ColorName == other.ColorName;
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return ColorName;
        }
    }

    public class DisplayColors : IEquatable<DisplayColors>, INotifyPropertyChanged
    {
        public DisplayColor Foreground
        {
            get { return _foreground; }
            set
            {
                if ((_foreground == null ^ value == null) || !_foreground.Equals(value))
                {
                    _foreground = value;
                    OnPropertyChanged("Foreground");
                }
            }
        }
        DisplayColor _foreground;

        public DisplayColor Background
        {
            get { return _background; }
            set
            {
                if ((_background == null ^ value == null) || !_background.Equals(value))
                {
                    _background = value;
                    OnPropertyChanged("Background");
                }
            }
        }
        DisplayColor _background;

        public event PropertyChangedEventHandler PropertyChanged;


        public DisplayColors()
        {
            Foreground = new DisplayColor("Black");
            Background = new DisplayColor("White");
        }

        public DisplayColors (string foregroundColorName, string backgroundColorName)
        {
            Foreground = new DisplayColor(foregroundColorName);
            Background = new DisplayColor(backgroundColorName);
        }

        public bool Equals(DisplayColors other)
        {
            return Foreground.Equals(other.Foreground) && Background.Equals(other.Background);
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static DisplayColors Parse(string s)
        {
            int p = s.IndexOf('{');
            if (p >= 0)
            {
                int delimiter = s.IndexOf(';', p + 1);
                if (delimiter > 0)
                {
                    return new DisplayColors(s.Substring(p + 1, delimiter - p - 1), s.Substring(delimiter + 1, s.Length - delimiter - 2));
                }
            }

            throw new ArgumentException("s");
        }

        public override string ToString()
        {
            return $@"{Foreground};{Background}";
        }
    }

    public static class BrushesToList
    {
        public static DisplayColors[] DisplayColors;

        public static Dictionary<string,SolidColorBrush> ColorNameToBrushTable;
        public static Dictionary<SolidColorBrush,string> BrushToColorNameTable;

        static BrushesToList()
        {
            ColorNameToBrushTable = new Dictionary<string, SolidColorBrush>();
            BrushToColorNameTable = new Dictionary<SolidColorBrush, string>();

            var values = typeof(Brushes).GetProperties().Select(p => new { Name = p.Name, Brush = p.GetValue(null) as SolidColorBrush }).ToArray();

            foreach (var entry in values)
            {
                ColorNameToBrushTable.Add(entry.Name, entry.Brush);

                //  Turns out there's synonym names for some of the color values

                if (!BrushToColorNameTable.ContainsKey(entry.Brush))
                {
                    BrushToColorNameTable.Add(entry.Brush, entry.Name);
                }
            }

            DisplayColors = new DisplayColors[]
            {
                new DisplayColors("White", "Black"),
                new DisplayColors("White", "DarkGray"),
                new DisplayColors("Black",  "LightGray"),
                new DisplayColors("Black",  "LightBlue"),
                new DisplayColors("White",  "CornflowerBlue"),
                new DisplayColors("Black",  "Turquoise"),
                new DisplayColors("Black",  "Cyan"),
                new DisplayColors("White",  "Blue"),
                new DisplayColors("White",  "Green"),
                new DisplayColors("White",  "YellowGreen"),
                new DisplayColors("Black",  "Lime"),
                new DisplayColors("Black",  "Chartreuse"),
                new DisplayColors("Black",  "LightGreen"),
                new DisplayColors("Black",  "GreenYellow"),
                new DisplayColors("Black",  "Yellow"),
                new DisplayColors("Black",  "Orange"),
                new DisplayColors("White",  "OrangeRed"),
                new DisplayColors("White",  "Chocolate"),
                new DisplayColors("White",  "Brown"),
                new DisplayColors("White",  "Red"),
                new DisplayColors("White",  "DarkRed"),
                new DisplayColors("White",  "Purple"),
                new DisplayColors("White",  "Fuchsia"),
                new DisplayColors("White",  "MediumPurple"),
                new DisplayColors("Black",  "LightPink"),
            };
        }
    }
}
