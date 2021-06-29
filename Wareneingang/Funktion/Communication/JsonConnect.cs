using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

using Newtonsoft.Json;

namespace Wareneingang.Funktion.Communication
{
    internal class JsonConnect
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly string _apiDomain = "https://";
        private readonly string _apiKey = "";
        private readonly string _apiPath = "/api/";
        private readonly string _testPath = "test/";
        private string _apiEndpoint;
        private bool TestModus = true; //Set Testmodus on or Off
        private JsonSerializerSettings _jsonSerializerOptions;

        /// <summary>
        /// Set Json Settings
        /// </summary>
        /// <returns></returns>
        public JsonSerializerSettings Api_com()
        {
            this._jsonSerializerOptions = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };

            return _jsonSerializerOptions;
        }

        /// <summary>
        /// Communication Function to get data from MySql
        /// </summary>
        /// <param name="url"></param>
        /// <param name="apikey"></param>
        /// <param name="parameter"></param>
        /// <param name="companyId"></param>
        /// <returns>string</returns>
        public async Task<string> ComToApi(string url, Boolean apikey, Dictionary<string, string> parameter, int companyId)
        {
            _ = new Dictionary<string, string>();

            if (companyId == 0)
            {
                this._apiEndpoint = "custom_scripts_new";
            }
            else
            {
                this._apiEndpoint = "custom_scripts_disapo";
            }

            Dictionary<string, string> values;
            if (apikey)
            {
                values = new Dictionary<string, string>
                {
                    {"api_key", this._apiKey },
                    {"program", "true" }
                };
            }
            else
            {
                values = new Dictionary<string, string>();
            }

            foreach (string key in parameter.Keys)
            {
                if (key == "api_key" || key == "program")
                {
                    continue; //Do not allow custom modified/manipulated parameters that should not be changed.
                }
                values[key] = parameter[key];
            }
            try
            {
                var content = new FormUrlEncodedContent(values);

                string WorkUrl = this._apiDomain + this._apiEndpoint + this._apiPath;

                if (this.TestModus is true)
                {
                    WorkUrl = WorkUrl + this._testPath;
                }

                var response = await Client.PostAsync(WorkUrl + url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                Console.WriteLine(responseString);

                return responseString;
            }
            catch (Exception e)
            {
                MessageBox.Show("Es ist ein fehler Aufgetreten. \n Bitte wiederholen Sie den vorgang nochmal \n" + e, "Fehler Bei übertragung", MessageBoxButton.OK, MessageBoxImage.Error);
                Mailer.send("JsonConnect", e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Methode to get User Data
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<string> ComToApiLogin(string username, string password, int companyId)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            this._apiEndpoint = companyId == 0 ? "custom_scripts_new" : "custom_scripts_disapo";

            values = new Dictionary<string, string>
                {
                {"api_key", this._apiKey },
                {"username", username },
                {"password", password },
                {"authmode_hash" , "" + true }
                };

            try
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                var response = await Client.PostAsync("https://test.ba.de/" + this._apiEndpoint + "/api/endpoint/auth", content);
                string responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (Exception e)
            {
                MessageBox.Show("Es ist ein fehler Aufgetreten. \n Bitte wiederholen Sie den vorgang nochmal \n" + e, "Fehler Bei übertragung", MessageBoxButton.OK, MessageBoxImage.Error);
                Mailer.send("JsonConnect", e.ToString());
                return null;
            }
        }
    }
}