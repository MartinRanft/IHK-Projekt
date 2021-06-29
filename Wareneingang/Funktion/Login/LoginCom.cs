using Newtonsoft.Json;

using System;
using System.Threading.Tasks;
using System.Windows;

using Wareneingang.Data.com_class;
using Wareneingang.Funktion.Communication;
using Wareneingang.Windows.Belegsearch;

namespace Wareneingang.Funktion.Login
{
    public class LoginCom
    {
        private int _failsToLogin;
        private double _timeStampLoginBan;

        public async void Login(MainWindow window)
        {
            double unixTime = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            MauveAuthApi user = await this.Authcheck(
                window.textbox_benutzername.Text,
                window.passwordbox_passwort.Password,
                window.combobox_firma.SelectedIndex
                );

            if (this._timeStampLoginBan == 0 || unixTime > this._timeStampLoginBan)
            {
                if (user.data != null && user.data.user_id > 0)
                {
                    if (window.combobox_firma.SelectedItem is null)
                    {
                        MessageBox.Show("Bitte eine Firma auswählen", "Firma auswahl", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        window.textbox_benutzername.Text = "";
                        window.passwordbox_passwort.Password = "";
                        window.textbox_benutzername.Focus();
                        this._failsToLogin = 0;
                        this._timeStampLoginBan = 0;
                        Belegselection belegselection = new Belegselection(user, window.combobox_firma.SelectedIndex);
                        belegselection.Show();
                        window.Close();
                    }
                }
                else
                {
                    if (window.combobox_firma.SelectedItem != null)
                    {
                        MessageBox.Show("Der Benutzername und/oder das Passwort ist falsch.");
                        _failsToLogin++;
                        if (_failsToLogin >= 3)
                        {
                            this._timeStampLoginBan = unixTime + 5;
                            this._failsToLogin = 0;
                        }
                    }
                }
            }
            else
            {
                window.label_error.Visibility = Visibility.Visible;
            }
        }

        private async Task<MauveAuthApi> Authcheck(string username, string password, int company)
        {
            JsonConnect jsonConnect = new JsonConnect();
            try
            {
                return JsonConvert.DeserializeObject<MauveAuthApi>(await jsonConnect.ComToApiLogin(username, password, company));
            }
            catch (Exception e)
            {
                Mailer.send("LoginCom", e.ToString());
                return null;
            }
        }
    }
}