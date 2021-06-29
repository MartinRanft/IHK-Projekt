using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Wareneingang.Data.com_class;
using Wareneingang.Funktion.Filter;
using Wareneingang.Windows.Belegsearch;

namespace Wareneingang.Windows.Beleg
{
    /// <summary>
    /// Interaktionslogik für belegWorksheet.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class BelegWorksheet : Window
    {
        #region needed variable

        public string errorMess;
        public int maxScanCount = 5;
        internal int _company;
        internal int _userId;
        internal BelegEinzelData belegEinzelData;
        internal int getIndex = 0;
        private readonly Funktion.SettingBelegdata.Beleg scan = new Funktion.SettingBelegdata.Beleg();
        private SortAdorner _listViewSortAdorner;
        private GridViewColumnHeader _listViewSortCol;

        #endregion needed variable

        #region Dictionary´s and Collection

        internal Dictionary<string, int> ArticleNrToArticleIdMap { get; set; } = null;
        internal ObservableCollection<ListViewItem> BestellteWaren { get; set; } = null;
        internal Dictionary<string, int> InitSessionData { get; set; } = null;
        internal Dictionary<string, ScanData> ScanSessionData { get; set; } = null;

        #endregion Dictionary´s and Collection

        #region init belegWorksheet

        public BelegWorksheet(string BelegNumer, int UserID, bool IsAdmin, int company)
        {
            InitializeComponent();
            this._company = company;
            this._userId = UserID;
            this.Title = BelegNumer;
            textbox_sku.Focus();
            InitSessionData = new Dictionary<string, int>();
            ScanSessionData = new Dictionary<string, ScanData>();
            ArticleNrToArticleIdMap = new Dictionary<string, int>();
            BestellteWaren = new ObservableCollection<ListViewItem>();

            if (IsAdmin)
            {
                button_admin_beleg_complete.Visibility = Visibility.Visible;
            }

            Funktion.SettingBelegdata.Beleg setBeleg = new Funktion.SettingBelegdata.Beleg();

            setBeleg.SetBelegData(this);
        }

        #endregion init belegWorksheet

        #region Button Click event

        private void Button_admin_beleg_complete_OnClick(object sender, RoutedEventArgs e)
        {
            if (button_admin_beleg_complete.Visibility == Visibility.Visible)
            {
                foreach (var objListViewItem in this.BestellteWaren)
                {
                    var listenItem = objListViewItem.Content as PostenView;
                    scan.SkuScan(this, listenItem.artikelnr, listenItem.anzahl);
                }
            }
        }

        private void button_close_save_beleg_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult boxResult = MessageBoxResult.Yes;
            int set_order_to_reclamation = 0;

            if (checkbox_beleg_abschliessen.IsChecked ?? true)
            {
                boxResult = MessageBox.Show("Achtung! Soll der Beleg wirklich geschlossen werden? Dies berechnet unterlieferung/überlieferung,falschlieferung und Bruch und legt entsprechende Belege zusätzlich zum Lagerzugang an.\nDer Beleg kann nicht länger bearbeitet werden.", "Achtung Unterlieferung!", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                set_order_to_reclamation = 1;
            }

            if (boxResult == MessageBoxResult.Yes)
            {
                scan.SendBelegData(this, set_order_to_reclamation);
            }

            foreach (Window currentWindow in Application.Current.Windows)
            {
                if (currentWindow.Name == "window_belegselection")
                {
                    (currentWindow as Belegselection).IsEnabled = true;
                    (currentWindow as Belegselection).textbox_beleg_userinput.Text = "";
                    (currentWindow as Belegselection).textbox_beleg_userinput.Focus();
                    (currentWindow as Belegselection)._timer.Start();
                    (currentWindow as Belegselection).SetListView();
                    break;
                }
            }

            worksheet.InitSessionData.Clear();

            worksheet.Close();
        }

        private void button_close_without_saving_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Beleg Wirklich schliessen?", "Beleg Schliesen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                foreach (Window currentWindow in Application.Current.Windows)
                {
                    if (currentWindow.Name == "window_belegselection")
                    {
                        (currentWindow as Belegselection).IsEnabled = true;
                        (currentWindow as Belegselection).textbox_beleg_userinput.Text = "";
                        (currentWindow as Belegselection).textbox_beleg_userinput.Focus();
                        (currentWindow as Belegselection)._timer.Start();
                        (currentWindow as Belegselection).SetListView();
                        break;
                    }
                }

                if (worksheet.InitSessionData.Count > 0)
                {
                    worksheet.InitSessionData.Clear();
                }

                worksheet.Close();
            }
        }

        private void button_set_lagerplatz_Click(object sender, RoutedEventArgs e)
        {
            scan.ChangeLagerPlatz(this, 1);
        }

        private void button_set_reservelagerplatz_Click(object sender, RoutedEventArgs e)
        {
            scan.ChangeLagerPlatz(this, 2);
        }

        private void button_UpdateBeleg_Click(object sender, RoutedEventArgs e)
        {
            scan.SkuScan(this);
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            if (column != null)
            {
                string sortBy = column.Tag.ToString(); // string is befüllt und stimmt
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol)?.Remove(_listViewSortAdorner);
                    listview_BelegDaten.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (_listViewSortCol == column && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol)?.Add(_listViewSortAdorner);
                listview_BelegDaten.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        #endregion Button Click event

        #region event trigger

        private void BelegDaten_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_BelegDaten.SelectedItems.Count == 1)
            {
                button_set_lagerplatz.IsEnabled = true;
                button_set_reservelagerplatz.IsEnabled = true;
            }
            else
            {
                button_set_lagerplatz.IsEnabled = false;
                button_set_reservelagerplatz.IsEnabled = false;
            }
        }

        private async void ContextListview(object sender, RoutedEventArgs e)
        {
            await scan.SetCountGescannt(this);
        }

        private void textbox_sku_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                button_scan_sku.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void worksheet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #endregion event trigger

        #region INotifyPropertyChanged overite

        /// <summary>
        /// Change Listview BG of a item
        /// </summary>
        internal class PostenView : INotifyPropertyChanged
        {
            private int _erfasst;
            private string _lagerplatz;
            private string _reservelagerplatz;

            public event PropertyChangedEventHandler PropertyChanged;

            public int anzahl { get; set; }
            public int artikelid { get; set; }
            public string artikelname { get; set; }
            public string artikelnr { get; set; }
            public string ean { get; set; }
            public string ean_second { get; set; }

            public int erfasst
            {
                get { return _erfasst; }
                set
                {
                    _erfasst = value;
                    if (this._erfasst <= 0)
                    {
                        item.Background = Brushes.DarkOrange;
                        item.Foreground = Brushes.Black;
                    }
                    else if (this._erfasst == this.anzahl)
                    {
                        item.Background = Brushes.Green;
                        item.Foreground = Brushes.AntiqueWhite;
                    }
                    else if (this._erfasst > this.anzahl)
                    {
                        item.Background = Brushes.Blue;
                        item.Foreground = Brushes.White;
                    }
                    else
                    {
                        item.Background = Brushes.Yellow;
                        item.Foreground = Brushes.Black;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(erfasst)));
                }
            }

            public ListViewItem item { get; set; } = null;
            public int lagerbestand { get; set; }

            public string lagerplatz
            {
                get { return _lagerplatz; }
                set
                {
                    _lagerplatz = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(lagerplatz)));
                }
            }

            public string reservelagerplatz
            {
                get { return _reservelagerplatz; }
                set
                {
                    _reservelagerplatz = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(reservelagerplatz)));
                }
            }
        }

        #endregion INotifyPropertyChanged overite
    }
}