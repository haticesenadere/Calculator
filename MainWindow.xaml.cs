using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace WpfCalculator
{
    public partial class MainWindow : Window
    {
        private string currentInput = "0";
        private string currentOperation = "";
        private double firstNumber = 0;
        private bool isNewCalculation = true;
        private bool isOperationJustPressed = false;

        // Yerel ondalık ayırıcı
        private readonly string decSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public MainWindow()
        {
            InitializeComponent();
            ResultTextBlock.Text = currentInput;
        }

        // Güvenli parse (mevcut kültüre göre)
        private double ParseCurrent(string s)
        {
            double v;
            if (!double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out v))
            {
                // nadiren noktalı girişlerde yardımcı olsun diye
                double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
            }
            return v;
        }

        // Operatör metnini normalize et (XAML'de ×, −, ÷ gelse bile tek tipe çevir)
        private string NormalizeOp(string op)
        {
            if (op == "×" || op == "x" || op == "*") return "*";
            if (op == "−" || op == "-") return "-";
            if (op == "÷" || op == "/") return "/";
            if (op == "+") return "+";
            return op;
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var digit = button.Content != null ? button.Content.ToString() : "";

            if (currentInput.Length >= 12 && !isNewCalculation && !isOperationJustPressed)
                return;

            if (isNewCalculation || currentInput == "0" || isOperationJustPressed)
            {
                currentInput = digit;
                isNewCalculation = false;
                isOperationJustPressed = false;
            }
            else
            {
                currentInput += digit;
            }
            ResultTextBlock.Text = currentInput;
        }

        private void OperationButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var opRaw = button.Content != null ? button.Content.ToString() : "";
            var op = NormalizeOp(opRaw);

            if (!isOperationJustPressed)
            {
                if (!string.IsNullOrEmpty(currentOperation))
                {
                    CalculateResult();
                }
                else
                {
                    firstNumber = ParseCurrent(currentInput);
                }
            }

            currentOperation = op;
            isOperationJustPressed = true;
        }

        private void EqualButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentOperation) && !isOperationJustPressed)
            {
                CalculateResult();
                currentOperation = "";
                isNewCalculation = true;   // yeni sayı girişi başlasın
            }
        }

        private void CalculateResult()
        {
            double secondNumber = ParseCurrent(currentInput);
            double result = 0;

            switch (currentOperation)
            {
                case "+":
                    result = firstNumber + secondNumber;
                    break;
                case "-":
                    result = firstNumber - secondNumber;
                    break;
                case "*":
                    result = firstNumber * secondNumber;
                    break;
                case "/":
                    if (secondNumber != 0)
                        result = firstNumber / secondNumber;
                    else
                    {
                        MessageBox.Show("Sıfıra bölünemez.", "Hata",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        // Hata sonrası state reset
                        currentInput = "0";
                        ResultTextBlock.Text = currentInput;
                        firstNumber = 0;
                        isNewCalculation = true;
                        isOperationJustPressed = false;
                        return;
                    }
                    break;
                default:
                    // Tanınmayan operatör gelirse mevcut değeri koru
                    result = secondNumber;
                    break;
            }

            // Gösterim biçimi: akıllı (uzunsa/bilimselse kısalt)
            string formatted;
            if ((Math.Abs(result) > 999999999 || (Math.Abs(result) < 0.000001 && result != 0)))
                formatted = result.ToString("E6", CultureInfo.CurrentCulture);
            else
                formatted = result.ToString("G10", CultureInfo.CurrentCulture);

            currentInput = formatted;
            ResultTextBlock.Text = currentInput;
            firstNumber = result;          // ardışık işlemler için sonucu sol tarafa yaz
            isOperationJustPressed = false;
            isNewCalculation = false;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            currentInput = "0";
            currentOperation = "";
            firstNumber = 0;
            isNewCalculation = true;
            isOperationJustPressed = false;
            ResultTextBlock.Text = currentInput;
        }

        private void DecimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentInput.Length >= 12 && !isNewCalculation && !isOperationJustPressed)
                return;

            if (isOperationJustPressed)
            {
                currentInput = "0" + decSep;
                isOperationJustPressed = false;
                isNewCalculation = false;
            }
            else if (currentInput.IndexOf(decSep, StringComparison.Ordinal) < 0)
            {
                isNewCalculation = false;
                currentInput += decSep;
            }

            ResultTextBlock.Text = currentInput;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isOperationJustPressed && currentInput.Length > 0 && !isNewCalculation)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                if (currentInput.Length == 0 || currentInput == "-")
                    currentInput = "0";

                ResultTextBlock.Text = currentInput;
            }
        }

        private void PercentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isOperationJustPressed && !isNewCalculation)
            {
                double number = ParseCurrent(currentInput);
                number /= 100.0;
                currentInput = number.ToString("G10", CultureInfo.CurrentCulture);
                ResultTextBlock.Text = currentInput;
            }
        }

        private void PlusMinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isOperationJustPressed && !isNewCalculation && currentInput != "0")
            {
                if (currentInput.StartsWith("-", StringComparison.Ordinal))
                    currentInput = currentInput.Substring(1);
                else
                    currentInput = "-" + currentInput;

                ResultTextBlock.Text = currentInput;
            }
        }
    }
}
