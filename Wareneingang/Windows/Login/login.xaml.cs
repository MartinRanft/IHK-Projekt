using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Wareneingang.Funktion.Login;
using Wareneingang.Funktion.Communication;

namespace Wareneingang
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            System.Console.WriteLine("geht");
#endif
            this.textbox_benutzername.Focus();
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void button_login_Click(object sender, RoutedEventArgs e)
        {
            LoginCom login = new LoginCom();
            login.Login(this);
        }

        private void combobox_firma_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (combobox_firma.SelectedIndex == 1)
            {
                bodyguard_grid.Visibility = Visibility.Hidden;
                disapo_grid.Visibility = Visibility.Visible;
                button_login.Background = new SolidColorBrush(Color.FromArgb(50, 236, 190, 190));
            }
            else if (combobox_firma.SelectedIndex == 0)
            {
                disapo_grid.Visibility = Visibility.Hidden;
                bodyguard_grid.Visibility = Visibility.Visible;
                button_login.Background = new SolidColorBrush(Color.FromArgb(100, 200, 224, 234));
            }
        }

        private void passwordbox_passwort_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                button_login.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}