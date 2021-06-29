using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json;

using Wareneingang.Data.com_class;
using Wareneingang.Funktion.Communication;
using Wareneingang.Windows.Beleg;
using Wareneingang.Windows.dialogBox;

using static Wareneingang.Windows.Beleg.BelegWorksheet;

namespace Wareneingang.Funktion.SettingBelegdata
{
    internal class Beleg : JsonConnect
    {
        /// <summary>
        /// Changing the Lagerplatz of a Item. lagerplatztype == 1 Lagerplatz , 2 Reserverlagerplatz
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="lagerplatztyp"></param>
        public async void ChangeLagerPlatz(BelegWorksheet worksheet, int lagerplatztyp)
        {
            string lagerplatzname = lagerplatztyp == 1 ? "Lagerplatz" : "Reservelagerplatz";
            ListViewItem listviewItem = (worksheet.listview_BelegDaten.SelectedItem as ListViewItem);
            DataResultContent apiResult = new DataResultContent();

            if (listviewItem is not null)
            {
                PostenView item = (listviewItem.Content as PostenView);

                if (item is not null)
                {
                    Windows.dialogBox.DialogBox dialogBox = new Windows.dialogBox.DialogBox(string.Format("Bitte {0} für Artikel: '{1}' eingeben.", lagerplatzname, item.artikelnr), "Lagerplatz Eingabe", false);

                    if (lagerplatztyp == 1)
                    {
                        dialogBox.textbox_userinput.Text = item.lagerplatz;
                    }
                    else
                    {
                        dialogBox.textbox_userinput.Text = item.reservelagerplatz;
                        //test commit
                    }

                    dialogBox.ShowDialog();

                    if (dialogBox.DialogResult == true)
                    {
                        Dictionary<string, string> parameter = new Dictionary<string, string>();

                        parameter["lagerplatz_typ"] = lagerplatztyp.ToString();
                        parameter["artikelid"] = item.artikelid.ToString();
                        parameter["lagerplatz"] = dialogBox.result;

                        try
                        {
                            apiResult =
                                JsonConvert.DeserializeObject<DataResultContent>(await ComToApi("URL-SHORTCUT", true,
                                    parameter, worksheet._company));
                        }
                        catch (Exception e)
                        {
                            Mailer.send("Beleg", e.ToString());
                            MessageBox.Show(
                                "Ein fehler ist aufgetreten, die IT wurde Informiert. \n das Programm wird geschlossen",
                                "Schwerwiegender fehler", MessageBoxButton.OK, MessageBoxImage.Error);

                            worksheet.Close();
                        }

                        if (apiResult.status == true)
                        {
                            foreach (var ware in worksheet.BestellteWaren)
                            {
                                PostenView postenView = ware.Content as PostenView;

                                if (postenView.artikelnr == item.artikelnr)
                                {
                                    if (lagerplatztyp == 1)
                                    {
                                        postenView.lagerplatz = dialogBox.result;
                                    }
                                    else
                                    {
                                        postenView.reservelagerplatz = dialogBox.result;
                                    }
                                }
                            }
                            worksheet.listview_BelegDaten.ItemsSource = null;
                            worksheet.listview_BelegDaten.ItemsSource = worksheet.BestellteWaren;
                            worksheet.listview_BelegDaten.Items.Refresh();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Send the changes from the worksheet to the API to change the DB
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="set_order_to_reclamation"></param>
        public async void SendBelegData(BelegWorksheet worksheet, int set_order_to_reclamation)
        {
            BearbeiteterBelegEmpfangen result = new BearbeiteterBelegEmpfangen();

            BearbeiteterBelegSenden belegSenden = new BearbeiteterBelegSenden
            {
                belegnr = worksheet.textbox_belegnr.Text,
                userID = worksheet._userId,
                reclamation = set_order_to_reclamation,

                scandata = new Dictionary<string, ScanData>()
            };

            foreach (var keyValue in worksheet.ScanSessionData)
            {
                belegSenden.scandata.Add(worksheet.ArticleNrToArticleIdMap[keyValue.Key].ToString(), keyValue.Value);
            }

            worksheet.IsEnabled = false;

            string json = JsonConvert.SerializeObject(belegSenden);

            Dictionary<string, string> parameter = new Dictionary<string, string>
            {
                {"data", json }
            };

            try
            {
                result =
                    JsonConvert.DeserializeObject<BearbeiteterBelegEmpfangen>(await this.ComToApi("URL-SHORTCUT", true,
                        parameter, worksheet._company));
            }
            catch (Exception e)
            {
                Mailer.send("Beleg", e.ToString());
            }

            if (result.status == true)
            {
                MessageBoxResult val = MessageBox.Show("Der Beleg wurde Erfolgreich Gespeichert", "Speichern Erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                if (val == MessageBoxResult.OK)
                {
                    worksheet.Close();
                }
                else
                {
                    MessageBox.Show("Beleg konnte nicht Gespeichert werden.\n Bitte Versuchen Sie es erneut", "Fehler Beleg erfassung", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Set the Data for the worksheet textboxes
        /// </summary>
        /// <param name="worksheet"></param>
        public async void SetBelegData(BelegWorksheet worksheet)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>
            {
                {"belegnr", worksheet.Title}
            };

            try
            {
                worksheet.belegEinzelData =
                    JsonConvert.DeserializeObject<BelegEinzelData>(await ComToApi("URL-SHORTCUT", true, parameter,
                        worksheet._company));
            }
            catch (Exception e)
            {
                Mailer.send("Beleg", e.ToString());
            }

            worksheet.errorMess = worksheet.belegEinzelData.data.stats.error;

            if (string.IsNullOrEmpty(worksheet.belegEinzelData.data.stats.error))
            {
                worksheet.textbox_Auftragsnumer.Text = worksheet.belegEinzelData.data.stats.auftragsnr;
                worksheet.textbox_externerefnr.Text = worksheet.belegEinzelData.data.stats.externereferenznr;
                worksheet.textbox_auftragsdatum.Text = worksheet.belegEinzelData.data.stats.auftragsdatum;
                worksheet.textbox_belegdatum.Text = worksheet.belegEinzelData.data.stats.belegdatum;
                worksheet.textbox_Kundennr.Text = worksheet.belegEinzelData.data.stats.kundennr;
                worksheet.textbox_Lieferant.Text = worksheet.belegEinzelData.data.stats.firma;
                worksheet.textbox_erstellt_von.Text = worksheet.belegEinzelData.data.stats.erstelltvon;
                worksheet.textbox_belegnr.Text = worksheet.belegEinzelData.data.stats.belegnr;
                worksheet.textbox_kommentar.Text = worksheet.belegEinzelData.data.stats.bemerkung;
            }

            //worksheet.BestellteWaren = new ObservableCollection<ListViewItem>();

            foreach (var ware in worksheet.belegEinzelData.data.items)
            {
                worksheet.InitSessionData[ware.artikelnr] = ware.scancount;
                worksheet.ArticleNrToArticleIdMap[ware.artikelnr] = ware.artikelid;

                ListViewItem item = new ListViewItem();

                item.Content = new BelegWorksheet.PostenView
                {
                    item = item,
                    artikelname = ware.name,
                    artikelnr = ware.artikelnr,
                    artikelid = ware.artikelid,
                    lagerbestand = ware.lagerbestand,
                    ean = ware.barcode,
                    ean_second = ware.barcodeSecond,
                    anzahl = ware.anzahl,
                    lagerplatz = ware.lagerplatz,
                    reservelagerplatz = ware.reservelagerplatz,
                    erfasst = ware.scancount
                };

                worksheet.BestellteWaren.Add(item);
                worksheet.listview_BelegDaten.ItemsSource = worksheet.BestellteWaren;
            }
            worksheet.listview_BelegDaten.DataContext = worksheet.BestellteWaren;
        }

        /// <summary>
        /// Gescannt Counter is here changed
        /// </summary>
        /// <param name="worksheet"></param>
        public async Task<bool> SetCountGescannt(BelegWorksheet worksheet)
        {
            int scannet = 0;
            string currentSKU = "";

            if (worksheet.listview_BelegDaten.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in worksheet.listview_BelegDaten.Items)
                {
                    if (item.IsSelected == true)
                    {
                        currentSKU = (item.Content as PostenView).artikelnr;
                        scannet = (item.Content as PostenView).erfasst;
                    }
                }
            }

            Windows.dialogBox.DialogBox dialogBox = new DialogBox("Bitte den neuen Wert Angeben", "Gescannt Ändern", true, scannet.ToString());

            dialogBox.ShowDialog();

            int change = int.Parse(dialogBox.result);

            this.SkuScan(worksheet, currentSKU, change);

            return true;
        }

        /// <summary>
        /// Search the Artikel in the grid and change the Gescannt counter. But will not Save it to DB
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="forceSKU"></param>
        public async void SkuScan(BelegWorksheet worksheet, string forceSKU = "", int forceCount = -1)
        {
            string SKU = worksheet.textbox_sku.Text;

            if (SKU == "_FMP_")
            {
                PromptBuilder promptBuilder = new PromptBuilder();
                PromptStyle promptStyle = new PromptStyle();
                promptStyle.Volume = PromptVolume.ExtraLoud;
                promptStyle.Rate = PromptRate.Fast;
                promptBuilder.StartStyle(promptStyle);
                promptBuilder.AppendText("FULL METAL PEIKERT");
                promptBuilder.EndStyle();
                SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.Speak(promptBuilder);
            }

            //Set forceSKU as SKU if given
            if (forceSKU.Length > 0)
            {
                SKU = forceSKU;
            }
            else
            {
                //Check string for - and clean it from
                SKU = SKU.Substring(1);
            }

            worksheet.grid_window.IsEnabled = false;

            Items itemData = this.getDataByUserinput(SKU, worksheet);
            int amountToAdd = 1;
            bool dialogError = false;

            if (itemData is not null)
            {
                string foundSKU = itemData.artikelnr;

                //Der Dialog erscheint nicht wenn wir ForceSKU gesetzt haben
                if (forceSKU.Length == 0)
                {
                    //Dialog JA/Nein? Wenn ja -> userinput der Anzahl, sonst ist die Anzahl 1
                    if (itemData.anzahl > worksheet.maxScanCount)
                    {
                        dialogError = true;
                        Windows.dialogBox.DialogBox dialogBox = new Windows.dialogBox.DialogBox("Bitte geben Sie die Waren anzahl an", "Waren größe", true);
                        if (dialogBox.ShowDialog() == true)
                        {
                            dialogError = false;
                            if (dialogBox.result.Length > 0)
                            {
                                amountToAdd = int.Parse(dialogBox.result);
                            }
                        }
                    }
                }

                //Add hack to force "amount"
                if (forceCount >= 0) //!= -1
                {
                    amountToAdd = forceCount;
                }

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////

                bool success = false;
                bool articleIsOverrideInGui = false;
                if (dialogError == false)
                {
                    ScanData scanDataObj = null;
                    if (worksheet.ScanSessionData.ContainsKey(foundSKU) == true)
                    {
                        scanDataObj = worksheet.ScanSessionData[foundSKU];
                        articleIsOverrideInGui = worksheet.ScanSessionData[foundSKU].override_count;
                    }
                    else
                    {
                        scanDataObj = new ScanData();
                        scanDataObj.artikelid = worksheet.ArticleNrToArticleIdMap[foundSKU];
                        worksheet.ScanSessionData[foundSKU] = scanDataObj;
                    }

                    if (forceCount >= 0)
                    {
                        scanDataObj.anzahl = amountToAdd;
                        scanDataObj.override_count = true;
                        articleIsOverrideInGui = true;
                    }
                    else
                    {
                        scanDataObj.anzahl += amountToAdd;
                    }

                    success = true;

                    if (
                        (articleIsOverrideInGui == true && amountToAdd > itemData.anzahl)
                        ||
                        (articleIsOverrideInGui == false && worksheet.InitSessionData[foundSKU] + worksheet.ScanSessionData[foundSKU].anzahl + amountToAdd > itemData.anzahl)
                    )
                    {
                        MessageBox.Show("Es wurde mehr Ware vom Atikel: \n" + itemData.artikelnr + "\n geliefert"
                            , "Überlieferung", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                //Update ListView in GUI
                if (success)
                {
                    foreach (ListViewItem obj in worksheet.BestellteWaren)
                    {
                        BelegWorksheet.PostenView postenView;
                        postenView = obj.Content as BelegWorksheet.PostenView;

                        if (postenView.artikelnr == foundSKU)
                        {
                            if (articleIsOverrideInGui == true)
                            {
                                postenView.erfasst = worksheet.ScanSessionData[foundSKU].anzahl;
                            }
                            else
                            {
                                postenView.erfasst = worksheet.InitSessionData[foundSKU] + worksheet.ScanSessionData[foundSKU].anzahl;
                            }
                        }
                    }

                    worksheet.listview_BelegDaten.ItemsSource = null;
                    worksheet.listview_BelegDaten.ItemsSource = worksheet.BestellteWaren;
                    worksheet.listview_BelegDaten.Items.Refresh();

                    if (worksheet.getIndex > 0)
                    {
                        worksheet.listview_BelegDaten.SelectedIndex = worksheet.getIndex;
                        worksheet.listview_BelegDaten.ScrollIntoView(worksheet.listview_BelegDaten.SelectedItem);
                    }
                    worksheet.button_close_save_beleg.IsEnabled = true;
                    worksheet.textbox_sku.Text = "";
                    worksheet.textbox_sku.Focus();
                }
            }
            else
            {
                MessageBox.Show("Gescannter Artikel wurde nicht gefunden \n Falsche Eingabe? \n Bei Falschlieferung Bitte im Büro melden.", "Fehlerhafte SKU/EAN", MessageBoxButton.OK, MessageBoxImage.Warning);
                worksheet.textbox_sku.Text = "";
                worksheet.textbox_sku.Focus();
            }
            worksheet.grid_window.IsEnabled = true;
        }

        private Items getDataByUserinput(string sKU, BelegWorksheet worksheet)
        {
            Items retVal = null;

            string shortUserInput = sKU;
            long trash = 0;
            bool isNumeric = long.TryParse(sKU, out trash);

            if (sKU.Length > 10 && isNumeric == true)
            {
                shortUserInput = sKU.Substring(3);
            }

            ///match against (contains: "11329654" OR "03522930001881" OR "188")

            worksheet.getIndex = 0;

            foreach (Items dataItem in worksheet.belegEinzelData.data.items)
            {
                if (dataItem.artikelnr.Contains(shortUserInput) == true || dataItem.barcode.Contains(shortUserInput) == true || dataItem.barcodeSecond.Contains(shortUserInput) == true)
                {
                    retVal = dataItem;
                    break;
                }

                worksheet.getIndex++;
            }

            return retVal;
        }
    }
}