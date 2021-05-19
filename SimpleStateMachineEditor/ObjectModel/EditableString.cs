using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    public class EditableString : IEditableObject, IComparable<EditableString>, IComparable<string>
    {
        public string Value;
        string SavedValue;

        public EditableString() { }

        public EditableString(string s)
        {
            Value = s;
        }

        public static implicit operator string(EditableString s)
        {
            return s.Value;
        }

        public static implicit operator EditableString(string s)
        {
            return new EditableString() { Value = s, };
        }

        public static bool operator ==(EditableString s1, EditableString s2)
        {
            return s1?.Value == s2?.Value;
        }

        public static bool operator ==(EditableString s1, String s2)
        {
            return s1?.Value == s2;
        }

        public static bool operator ==(String s1, EditableString s2)
        {
            return s1 == s2?.Value;
        }

        public static bool operator !=(EditableString s1, EditableString s2)
        {
            return !(s1 == s2);
        }

        public static bool operator !=(EditableString s1, string s2)
        {
            return !(s1 == s2);
        }

        public static bool operator !=(String s1, EditableString s2)
        {
            return !(s1 == s2);
        }

        public void BeginEdit()
        {
            SavedValue = Value;
        }

        public void CancelEdit()
        {
            Value = SavedValue;
        }

        public bool Contains(string other)
        {
            return this?.Value.Contains(other) ?? false;
        }

        public void EndEdit()
        {
        }

        public override string ToString()
        {
            return Value;
        }

        public int CompareTo(EditableString other)
        {
            return (this?.Value == null && other?.Value == null) || Value == other ? 0 : (Value?.CompareTo(other?.Value) ?? (other == (ObjectModel.EditableString)null ? 0 : 1));
        }

        public int CompareTo(string other)
        {
            return (this?.Value == null && other == null) || Value == other ? 0 : (Value?.CompareTo(other) ?? (other == null ? 0 : 1));
        }
    }
}
