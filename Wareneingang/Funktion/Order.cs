using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Wareneingang.Data.com_class;
using Wareneingang.Funktion.Communication;

namespace Wareneingang.Funktion
{
    internal class Order : JsonConnect
    {
        private readonly string _orderList = "order-list";

        public async Task<OrderList> GetOrderList(int company)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();

            OrderList obj = new OrderList();

            try
            {
                obj =
                   JsonConvert.DeserializeObject<OrderList>(await this.ComToApi(_orderList, true, parameter, company));
            }
            catch (Exception e)
            {
                Mailer.send("Beleg", e.ToString());
            }
            return obj;
        }
    }
}