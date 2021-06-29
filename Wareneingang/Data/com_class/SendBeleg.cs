using System.Collections.Generic;

namespace Wareneingang.Data.com_class
{
    public class BearbeiteterBelegEmpfangen
    {
        public int code { get; set; } = 1;

        // 1 = Beleg ist vollständig => 2 = Beleg nur teilweise gescannt
        public string error { get; set; } = "";

        public bool status { get; set; } = false;
    }

    public class ScanData
    {
        public int artikelid = 0;
        public int anzahl = 0;
        public bool override_count = false;
    }

    public class BearbeiteterBelegSenden
    {
        public string belegnr { get; set; } = "";
        public int reclamation { get; set; } = 0;
        public Dictionary<string, ScanData> scandata;
        public int userID { get; set; } = 0;
        //[4512589:5, 358312354:78, 358935:12...]
    }
}