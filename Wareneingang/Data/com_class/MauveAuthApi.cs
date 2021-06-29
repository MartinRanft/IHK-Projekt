using System.Diagnostics.CodeAnalysis;

// ReSharper disable All

namespace Wareneingang.Data.com_class
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class LoginData
    {
        public string first_name { get; set; } = "";
        public bool is_admin { get; set; }
        public string last_name { get; set; } = "";
        public int user_id { get; set; } = 0;
        public int usergroup_id { get; set; } = 0;
    }

    public class MauveAuthApi
    {
        public LoginData data;
        public string Error { get; set; }
        public bool Success { get; set; }
    }
}