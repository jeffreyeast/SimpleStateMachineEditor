using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace LexicalAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public IEnumerable<Lexeme> Lexemes 
        {
            get => _lexemes;
            private set
            {
                if (_lexemes != value)
                {
                    _lexemes = value;
                    OnPropertyChanged("Lexemes");
                }
            }
        }
        IEnumerable<Lexeme> _lexemes;

        public ScannerStateMachineImplementation Scanner
        {
            get => _scanner;
            set
            {
                if (_scanner != value)
                {
                    _scanner = value;
                    OnPropertyChanged("Scanner");
                }
            }
        }
        ScannerStateMachineImplementation _scanner;

        public string Trace
        {
            get => _trace;
            set
            {
                if (_trace != value)
                {
                    _trace = value;
                    OnPropertyChanged("Trace");
                }
            }
        }
        string _trace;

        public event PropertyChangedEventHandler PropertyChanged;




        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            List<Lexeme> lexemes = new List<Lexeme>();

            ExceptionTextBox.Text = "";

            using (TextReader reader = new StringReader(InputTextBox.Text))
            {
                Scanner = new ScannerStateMachineImplementation(reader);

                Lexeme lexeme;

                try
                {
                    do
                    {
                        lexeme = Scanner.Next;
                        lexemes.Add(lexeme);
                    } while (lexeme.LexemeType != Lexeme.LexemeTypes.EOF && lexeme.LexemeType != Lexeme.LexemeTypes.Error);
                }
                catch (Exception exc)
                {
                    ExceptionTextBox.Text = exc.Message;
                }
            }

            Lexemes = lexemes;
            Trace = Scanner.Trace;
        }
    }
}
