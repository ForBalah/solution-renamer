using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SolutionRenamer.Win.Logic.Logging
{
    internal class TextBoxLogger : ILogger
    {
        private static Image _image;
        private static Label _label;
        private static TextBox _textbox;
        private BitmapImage _infoBitmap = new BitmapImage(new Uri(@"/Images/info-128.png", UriKind.Relative));
        private string _lastMessage;
        private DateTime _lastUpdateDate;
        private BitmapImage _warnBitmap = new BitmapImage(new Uri(@"/Images/warn-128.png", UriKind.Relative));

        private TextBoxLogger()
        {
        }

        public static ILogger Get()
        {
            if (_label == null || _textbox == null)
            {
                throw new InvalidOperationException("First initialize the logger with a textbox and label to log to");
            }

            return new TextBoxLogger();
        }

        public static void Initialize(TextBox textbox, Label label, Image image)
        {
            if (textbox == null)
            {
                throw new ArgumentNullException(nameof(textbox));
            }

            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            _textbox = textbox;
            _label = label;
            _image = image;
        }

        public void WriteInfo(string message)
        {
            var printMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";

            ShowMessage(message, printMessage, _infoBitmap);
        }

        public void WriteWarning(string message)
        {
            var printMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";

            ShowMessage(message, printMessage, _warnBitmap);
        }

        public void WriteWarning(string message, Exception ex)
        {
            var printMessage = $"{DateTime.Now}: {message}{Environment.NewLine}{ex.ToString()}";

            ShowMessage(message, printMessage, _warnBitmap);
        }

        private void ShowMessage(string message, string printMessage, BitmapImage image)
        {
            _textbox.Text += printMessage;
            if (_lastUpdateDate < DateTime.Now.AddSeconds(-1) && _textbox.LineCount > 0)
            {
                _textbox.ScrollToLine(_textbox.LineCount - 1);
                _lastUpdateDate = DateTime.Now;
            }

            _label.Content = message;
            _image.Source = image;
            _lastMessage = printMessage;
        }
    }
}