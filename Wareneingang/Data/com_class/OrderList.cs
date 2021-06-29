using System;
using System.Collections.Generic;

// ReSharper disable All

namespace Wareneingang.Data.com_class
{
    public class BelegData
    {
        public string artikel { get; set; } = "";
        public DateTime auftragsdatum { get; set; }
        public string auftragsnr { get; set; } = "";
        public DateTime belegdatum { get; set; }
        public string belegid { get; set; } = "";
        public string belegnr { get; set; } = "";
        public double betrag { get; set; } = 0.00;
        public string erstelltvon { get; set; } = "";
        public string externereferenznr { get; set; } = "";
        public string firma { get; set; } = "";
        public string kundennr { get; set; } = "";
    }

    public class OrderList
    {
        public IList<BelegData> data;
    }
}