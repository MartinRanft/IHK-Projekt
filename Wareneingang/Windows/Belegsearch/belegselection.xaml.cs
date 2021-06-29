using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

using Wareneingang.Data.com_class;
using Wareneingang.Funktion;
using Wareneingang.Funktion.Filter;
using Wareneingang.Windows.Beleg;

namespace Wareneingang.Windows.Belegsearch
{
    /// <summary>
    /// Interaktionslogik für belegselection.xaml
    /// </summary>
    public partial class Belegselection : Window
    {
        public DispatcherTimer _timer;
        private int _company;
        private SortAdorner _listViewSortAdorner;
        private GridViewColumnHeader _listViewSortCol;

        private bool _programStartet = false;
        private MauveAuthApi _user;

        public Belegselection(MauveAuthApi user, int company)
        {
            InitializeComponent();
            this._company = company;
            this._user = user;
            label_username.Content = this._user.data.first_name + " " + this._user.data.last_name;
            SetListView();
            textbox_beleg_userinput.Focus();

            //start for the auto logout mechanics
            _timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 30, 0)
            };
            _timer.Tick += LogoutTimerEvent;
            _timer.Start();
        }

        /// <summary>
        /// init/update the data of listview
        /// </summary>
        internal async void SetListView()
        {
            Order getList = new Order();

            OrderList orderList = await getList.GetOrderList(_company);

            listview_beleg_auswahl.ItemsSource = orderList.data;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listview_beleg_auswahl.ItemsSource);

            view.Filter = this.BelegFilter;
            _programStartet = true;

            view.SortDescriptions.Add(new SortDescription("belegnr", ListSortDirection.Descending));
        }

        private void ausgabenColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol)?.Remove(_listViewSortAdorner);
                    listview_beleg_auswahl.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (_listViewSortCol == column && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol)?.Add(_listViewSortAdorner);
                listview_beleg_auswahl.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        /// <summary>
        /// will be searching for beleg numbers and auftragsnummer
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool BelegFilter(object item)
        {
            if (String.IsNullOrEmpty(textbox_beleg_userinput.Text))
                return true;
            else
                return (((BelegData)item).belegnr.IndexOf(textbox_beleg_userinput.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Will be open the belegWorksheet with the needed data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_belegauswahl_Click(object sender, RoutedEventArgs e)
        {
            if (listview_beleg_auswahl.SelectedItems.Count == 1)
            {
                BelegData objData = listview_beleg_auswahl.SelectedItem as BelegData;
                BelegWorksheet belegWorksheet = new BelegWorksheet(objData.belegnr.ToString(), _user.data.user_id, _user.data.is_admin, _company);
                if (String.IsNullOrEmpty(belegWorksheet.errorMess))
                {
                    _timer.Stop();
                    belegWorksheet.Show();
                    this.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show(belegWorksheet.errorMess, "Fehler mit Beleg " + objData.belegnr, MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void icon_aktualisieren_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetListView();
        }

        private void image_exit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Logout();
        }

        private void listview_beleg_auswahl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            button_belegauswahl.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void listview_beleg_auswahl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ = listview_beleg_auswahl.SelectedItems.Count == 1 ? button_belegauswahl.IsEnabled = true : button_belegauswahl.IsEnabled = false;
        }

        private void LogoutTimerEvent(object sender, EventArgs e)
        {
            if (this.IsEnabled)
            {
                foreach (Window currentWindow in Application.Current.Windows)
                {
                    if (currentWindow.Name == "window_belegselection" && currentWindow.IsActive)
                    {
                        this.Logout();
                    }
                }
            }
        }

        /// <summary>
        /// triggers the Click event on Belegsearch button in Enter Key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textbox_beleg_userinput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.listview_beleg_auswahl.Items.Count == 1)
                {
                    this.listview_beleg_auswahl.SelectAll();
                    button_belegauswahl.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                }
            }
        }

        private void textbox_beleg_userinput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_programStartet)
            {
                CollectionViewSource.GetDefaultView(listview_beleg_auswahl.ItemsSource).Refresh();
            }
        }

        private void Logout()
        {
            MainWindow logMainWindow = new MainWindow();
            logMainWindow.Show();

            this._timer.Stop();
            this.Close();
            GC.Collect();
        }
    }
}