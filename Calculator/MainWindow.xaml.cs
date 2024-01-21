using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text;

namespace Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Stack<string> _braces;

        private readonly Dictionary<string,string> _operations = new()
        {
            { "*", "*" },
            { "-", "-" },
            { "%", "%" },
            { "+", "+" },
            { "/", "/" },
            { "Mod", "mod" },
            { "xʸ", "^" },
            { "sqyX", "~" },
            { "(", "(" },
            { ")", ")" },
        };

        private static string _measuringUnit;
        private double _memory;
        private static double ConvertMeasuringUnits(double val)
        {
            if (_measuringUnit == "Градусы")
            {
                return val * (Math.PI / 180);
            }

            return val;
        }

        private readonly Dictionary<string, Func<double, double>> _unaryFunctions = new()
        {
            { "1/x", a => 1 / a },
            { "Inv", a => 1 / a },
            { "sqrt", a => Math.Sqrt(a) },
            { "+-", a => -a },
            { "log", a => Math.Log10(a) },
            { "ln", a => Math.Log(a) },
            { "10ˣ", a => Math.Pow(10, a) },
            { "tan", a => Math.Tan(ConvertMeasuringUnits(a)) },
            { "tanh", a => Math.Tanh(ConvertMeasuringUnits(a)) },
            { "cos", a => Math.Cos(ConvertMeasuringUnits(a)) },
            { "cosh", a => Math.Cosh(ConvertMeasuringUnits(a)) },
            { "sin", a => Math.Sin(ConvertMeasuringUnits(a)) },
            { "sinh", a => Math.Sinh(ConvertMeasuringUnits(a)) },
            { "Int", a => (int)a },
            { "sq3X", a => Math.Pow(a, 1 / (double)3) },
            { "n!", a => (int)a > 31 ? 0 : Enumerable.Range(1, (int)a).Aggregate(1, (p, item) => p * item) },
            { "x²", a => a * a },
            { "x³", a => a * a * a },
        };

        private readonly List<string> _expression;
        private readonly List<string> _currentNumber;

        public MainWindow()
        {
            InitializeComponent();
            _currentNumber = new List<string>();
            _expression = new List<string>();
            _braces = new Stack<string>();
            _measuringUnit = "Градусы";
        }


        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            _currentNumber.Add((string)button.Content);
            ScreenTb.Text = $"{double.Parse(string.Join("", _currentNumber)):0.####}";
        }

        private void OperationButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (_expression.Count > 0 && (_operations.ContainsKey(_expression.ElementAt(^1)) || _expression.ElementAt(^1) == ")"))
            {
                var str = string.Join("", _currentNumber);
                _expression.Add(string.IsNullOrEmpty(str) ? "0" : str);
                _expression.Add(_operations[(string)button.Content]);
                _currentNumber.Clear();
                ExpressionTb.Text = string.Join(" ", _expression);
                return;
            }

            if (!_currentNumber.Any())
                return;

            var res = double.Parse(string.Join("", _currentNumber));
            _expression.Add($"{res:0.####}");
            _currentNumber.Clear();

            _expression.Add(_operations[(string)button.Content]);
            ExpressionTb.Text = string.Join(" ", _expression);
            ScreenTb.Text = "0";
        }

        private void UnaryOperationButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentNumber.Count == 0)
                return;

            var button = (Button)sender;
            var currentNumber = string.Join("", _currentNumber);
            var res = double.Parse(currentNumber);
            var calculated = _unaryFunctions[(string)button.Content](res);
            if (double.IsInfinity(calculated) || double.IsNaN(calculated))
            {
                MessageBox.Show("Invalid calculation");
                return;
            }
            _currentNumber.Clear();
            foreach (var c in calculated.ToString(CultureInfo.InvariantCulture))
            {
                _currentNumber.Add(c.ToString());
            }

            ScreenTb.Text = $"{calculated:0.####}";
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_expression.Count <= 0 || !(_operations.ContainsKey(_expression.ElementAt(^1)) || _operations.ContainsValue(_expression.ElementAt(^1)) || _expression.ElementAt(^1).All(char.IsDigit)))
            {
                return;
            }

            var current = string.Join("", _currentNumber);
            if (_expression.ElementAt(^1) != ")")
                _expression.Add(string.IsNullOrEmpty(current) ? "0" : current);
            while (_braces.TryPop(out var brace))
                _expression.Add(")");

            var newExpr = _expression.Select(x => x == "%" ? "/ 100 * " : x).ToList();
            _expression.Clear();
            foreach (var n in newExpr)
            {
                _expression.Add(n);
            }

            for (int i = 0; i < newExpr.Count; i++)
            {
                if (newExpr[i] == "~")
                {
                    (newExpr[i - 1], newExpr[i + 1]) = (newExpr[i + 1], newExpr[i - 1]);
                }
            }
            var result = Dangl.Calculator.Calculator.Calculate(string.Join(" ", newExpr));
            if (result is null || double.IsNaN(result.Result) || double.IsInfinity(result.Result))
            {
                MessageBox.Show("Invalid calculation");
                return;
            }
            
            ScreenTb.Text = $"{result.Result:0.####}";
            var resultString = result.Result.ToString(CultureInfo.InvariantCulture);
            _currentNumber.Clear();
            _expression.Clear();
            ExpressionTb.Text = "";
            foreach (var c in resultString)
            {
                _currentNumber.Add(c.ToString());
            }
        }

        private void DeleteSymbolButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentNumber.Count == 0)
                return;
            _currentNumber.RemoveAt(_currentNumber.Count - 1);
            ScreenTb.Text = string.Join("", _currentNumber);
        }

        private void PiButton_Click(object sender, RoutedEventArgs e)
        {
            _currentNumber.Clear();
            var pi = $"{Math.PI:0.####}";
            foreach (var c in pi)
            {
                _currentNumber.Add(c.ToString());
            }

            ScreenTb.Text = string.Join("", _currentNumber);
        }

        private void BraceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var content = (string)button.Content;
            if (content == "(")
            {
                _braces.Push(content);
            }
            else
            {
                _expression.Add(string.Join("", _currentNumber));
                _currentNumber.Clear();
                _braces.Pop();
            }

            _expression.Add(content);
            ExpressionTb.Text = string.Join(" ", _expression);
        }

        private void DotButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentNumber.Contains("."))
                return;
            
            _currentNumber.Add(".");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _currentNumber.Clear();
            _braces.Clear();
            _expression.Clear();
            ExpressionTb.Text = "";
            ScreenTb.Text = "0";
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _measuringUnit = (string)((RadioButton)sender).Content;
        }

        private void MemoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                var button = (Button)sender;
                switch ((string)button.Content)
                {
                    case "MS":
                        _memory = double.Parse(string.Join("", _currentNumber));
                        break;
                    case "MR":
                        var memoryString = string.Join("", _memory);
                        _currentNumber.Clear();
                        foreach (var c in memoryString)
                        {
                            _currentNumber.Add(c.ToString());
                        }

                        ScreenTb.Text = string.Join("", _currentNumber);
                        break;
                    case "M+":
                        var current = double.Parse(string.Join("", _currentNumber));
                        _memory += current;
                        break;
                    case "M-":
                        var curr = double.Parse(string.Join("", _currentNumber));
                        _memory -= curr;
                        break;
                    case "MC":
                        _memory = 0;
                        break;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Memory empty");
            }
        }
    }
}