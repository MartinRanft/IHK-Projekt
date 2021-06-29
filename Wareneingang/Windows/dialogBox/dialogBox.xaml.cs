using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Wareneingang.Windows.dialogBox
{
    /// <summary>
    /// Interaktionslogik für DialogBox.xaml
    /// </summary>
    public partial class DialogBox : Window
    {
        internal DispatcherTimer timer = new DispatcherTimer();
        private bool canClose = false;

        public DialogBox(string text, string title, bool numbersonly = false, string preSelect = "")
        {
            InitializeComponent();
            this.Title = title;
            this.label_text.Content = text;
            this.numbersonly = numbersonly;
            if (preSelect.Length > 0)
            {
                this.textbox_userinput.Text = preSelect;
            }
            this.textbox_userinput.Focus();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        internal bool numbersonly { get; set; }
        internal string result { get; set; }

        private void button_abbruch_Click(object sender, RoutedEventArgs e)
        {
            this.result = "";
            this.Close();
        }

        private void button_eingabe_Click(object sender, RoutedEventArgs e)
        {
            if (this.numbersonly == false || Regex.Match(this.textbox_userinput.Text, "^[0-9]*$").Success)
            {
                this.result = this.textbox_userinput.Text;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ToolTip toolTip = new ToolTip
                {
                    Content = "Bitte nur Zahlen angeben",
                    HasDropShadow = true,
                    StaysOpen = true
                };

                button_eingabe.ToolTip = toolTip;
            }
        }

        private void textbox_userinput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && textbox_userinput.Text.Length <= 8 && this.canClose == true)
            {
                button_eingabe.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.canClose = true;
        }
    }
}