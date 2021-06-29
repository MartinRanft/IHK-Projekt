using System.Collections.Generic;

// ReSharper disable All

namespace Wareneingang.Data.com_class
{
    public class BelegEinzelData
    {
        public EinzelData data;
    }

    public class EinzelData
    {
        public IList<Items> items;
        public Stats stats;
    }

    public class Items
    {
        public int anzahl { get; set; } = 0;
        public int artikelid { get; set; } = 0;
        public string artikelnr { get; set; } = "";
        public string barcode { get; set; } = "";
        public string barcodeSecond { get; set; } = "";
        public int lagerbestand { get; set; } = 0;
        public string lagerplatz { get; set; } = "";
        public string name { get; set; } = "";
        public string reservelagerplatz { get; set; } = "";
        public int scancount { get; set; } = 0;
    }

    public class Stats
    {
        public string artikel { get; set; } = "";
        public string auftragsdatum { get; set; }
        public string auftragsnr { get; set; } = "";
        public string belegdatum { get; set; }
        public string belegnr { get; set; } = "";
        public string bemerkung { get; set; } = "";
        public double betrag { get; set; } = 0.00;
        public string error { get; set; } = "";
        public string erstelltvon { get; set; } = "";
        public string externereferenznr { get; set; } = "";
        public string firma { get; set; } = "";
        public string kundennr { get; set; } = "";
    }

    public class TestData
    {
        public IList<test> items;
    }

    public class test
    {
        public int anzahl { get; set; } = 0;
        public int artikelid { get; set; } = 0;
        public string artikelnr { get; set; } = "";
        public string ean { get; set; } = "";
        public string ean_second { get; set; } = "";
        public int lagerbestand { get; set; } = 0;
        public string lagerplatz { get; set; } = "";
        public string artikelname { get; set; } = "";
        public string reservelagerplatz { get; set; } = "";
        public int erfasst { get; set; } = 0;
    }
}