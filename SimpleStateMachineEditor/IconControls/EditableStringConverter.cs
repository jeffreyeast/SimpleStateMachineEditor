using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SimpleStateMachineEditor.IconControls
{
    public class EditableStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else if (targetType == typeof(ObjectModel.EditableString))
            {
                if (value is string s)
                {
                    return new ObjectModel.EditableString(s);
                }
            }
            else if (targetType == typeof(string))
            {
                if (value is ObjectModel.EditableString es)
                {
                    return es.Value;
                }
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
