using System;
using System.Data;

namespace AlfaDirectAutomation
{
    public class AlfaDirectProvider : AlfaDirectBase
    {
        private string ItemNumber { get; set; }
        private string ItemCode { get; set; }
        private string AccountCode { get; set; }
        private string StoreCode { get; set; }

        public AlfaDirectProvider(string itemNumber, string itemCode, string accountCode, string storeCode) : base()
        {
            ItemNumber = itemNumber;
            ItemCode = itemCode;
            AccountCode = accountCode;
            StoreCode = storeCode;

            AlfaDirectConnection.adObj.GlobalFilter["FI"] = ItemNumber;
            AlfaDirectConnection.adObj.GlobalFilter["Q"] = ItemNumber;
            AlfaDirectConnection.adObj.GlobalFilter["AT"] = ItemNumber; 
        }

        /// <summary>
        /// Создает заказ
        /// </summary>
        /// <param name="buyOrSell">B или S</param>
        /// <returns></returns>
        public CallResult CreateOrder(int quantity, double price, string buyOrSell, DateTime activateIfDateReached)
        {
            var res = new CallResult();

            try
            { 
                AlfaDirectConnection.Connect();

                int orderNumber = AlfaDirectConnection.adObj.CreateLimitOrder(
                 AccountCode, StoreCode, ItemCode, DateTime.Today.AddDays(1),
                    "PaidUpA", "RUR", buyOrSell, quantity, price,
                    null, null, null, null, null, null, null, null,
                    null, null, null, null, null, null, null, null, 10);

                res.Message = "Заявка №" + orderNumber + "\r\n" + AlfaDirectConnection.adObj.LastResultMsg;

                if (orderNumber == 0)
                    throw new Exception(res.Message);

                res.Data = orderNumber;
                res.Success = true;

                return res;


            }
            catch (Exception ex)
            {

                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }

        /// <summary>
        /// Удаляет заказ
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public CallResult DropOrder(int orderNumber)
        {
            var res = new CallResult();

            try
            {
                
                AlfaDirectConnection.Connect();

                AlfaDirectConnection.adObj.DropOrder(orderNumber, null, null, null, null, null, 10);

                res.Message = AlfaDirectConnection.adObj.LastResultMsg; 
                res.Success = AlfaDirectConnection.adObj.LastResult == ADLite.StateCodes.stcSuccess;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }

        /// <summary>
        /// Возвращает рыночную цену инструмента
        /// </summary>
        /// <returns></returns>
        public CallResult GetStockItemPrice()
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                string fields = "last_price";
                var data = AlfaDirectConnection.adObj.GetLocalDBData("fin_info", fields, string.Format("paper_no = {0}",ItemNumber));
                res.Data = CutPart(ref data, "|");
                 
                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }
          
        /// <summary>
        /// Вернуть всю информацию о рынках
        /// </summary>
        /// <returns></returns>
        public CallResult GetTradePlacesInfo()
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                string fields = "place_code, place_name, curr_code, ex_code, quote_place_code";
                var data = AlfaDirectConnection.adObj.GetLocalDBData("TRADE_PLACES", fields, null);
                res.Data = ConvertToTable(data, fields);

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }

        /// <summary>
        /// Вернуть информацию о всех ценных бумагах рынка
        /// </summary>
        /// <returns></returns>
        public CallResult GetPapers()
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                string fields = "place_name, paper_no, p_code, place_code, ANSI_name";
                var data = AlfaDirectConnection.adObj.GetLocalDBData("PAPERS", fields, "place_code = " + StoreCode);
                res.Data = ConvertToTable(data, fields);

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Success = false;

                return res;
            }
        }


        /// <summary>
        /// Вернуть информацию о структуре ДБ
        /// </summary>
        /// <returns></returns>
        public CallResult GetDbStruct()
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();
                 
                var data = AlfaDirectConnection.adObj.GetLocalDBStruct("orders");
                res.Data = data;

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Success = false;

                return res;
            }
        }

        /// <summary>
        /// Вернуть информацию о статусах для зяавок
        /// </summary>
        /// <returns></returns>
        public CallResult GetOrdersStatusesCallResult()
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                var data = AlfaDirectConnection.adObj.GetLocalDBData("order_statuses", "*", "");
                res.Data = data;

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Success = false;

                return res;
            }
        }


        /// <summary>
        /// Вся информация обо всех позициях в портфеле
        /// </summary>
        /// <returns></returns>
        public CallResult GetBalanceAllItemsInfo()
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                string fields = "p_code,place_code,paper_no";
                var data = AlfaDirectConnection.adObj.GetLocalDBData("balance", fields, string.Format("acc_code = {0}", AccountCode));
                res.Data = ConvertToTable(data, fields);

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }

        public CallResult GetBalanceItemInfo()
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                string fields = "forword_rest,p_code,place_code,income_rest";
                var data = AlfaDirectConnection.adObj.GetLocalDBData("balance", fields, string.Format("acc_code = {0} and place_code = {1} and paper_no = {2}", AccountCode, StoreCode, ItemNumber));
                res.Data = ConvertToTable(data, fields);

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }

        ///// <summary>
        ///// Возвращает сумму денег на балансе
        ///// </summary>
        ///// <returns></returns>
        //public CallResult GetBalanceRest()
        //{
        //    var res = new CallResult();

        //    try
        //    {

        //        AlfaDirectConnection.Connect();

        //        string fields = "forword_rest";
        //        var data = AlfaDirectConnection.adObj.GetLocalDBData("balance", fields, string.Format("acc_code = {0} and place_code = {1} and paper_no = {2}", AccountCode, BalanceStore, BalanceNumber));
        //        res.Data = CutPart(ref data, "|");

        //        res.Message = AlfaDirectConnection.adObj.LastResultMsg;
        //        res.Success = true;

        //        return res;
        //    }
        //    catch (Exception ex)
        //    {
        //        res.Message = ex.Message;
        //        res.Exception = ex;
        //        res.Success = false;

        //        return res;
        //    }
        //}

        /// <summary>
        /// Вернуть количество фьючерсов инструмента в портфеле
        /// </summary>
        /// <returns></returns>
        public CallResult GetBalanceItemQuantity()
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                string fields = "forword_rest";
                var data = AlfaDirectConnection.adObj.GetLocalDBData("balance", fields, string.Format("acc_code = {0} and place_code = {1} and paper_no = {2}", AccountCode, StoreCode, ItemNumber));
                res.Data = CutPart(ref data, "|");

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }

        /// <summary>
        /// Вернуть информацию о ценных бумагах
        /// </summary>
        /// <returns></returns>
        public CallResult GetPaperInfo()
        {
            var res = new CallResult();
       
            try
            {

                AlfaDirectConnection.Connect();
                
                string fields = "ts_p_code, p_code, place_code, ANSI_name";
       
                var data = AlfaDirectConnection.adObj.GetLocalDBData("papers", fields, string.Format("paper_no = " + ItemNumber));
                res.Data = ConvertToTable(data, fields);

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;
 
                return res;
            }
        }

        public CallResult GetOrderByNumber(string number)
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                string fields = "ord_no, b_s, price, qty, status, place_name, p_code, comments";
                var data = AlfaDirectConnection.adObj.GetLocalDBData("orders", fields, string.Format("acc_code = {0} and ord_no = {1}", AccountCode, number));

                if (data == null)
                {
                    res.Success = true;
                    res.Data = new DataTable();
                    return res;
                }

                res.Data = ConvertToTable(data, fields);

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }
        
        public CallResult GetExecutedOrderByNumber(string number)
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                string fields = "ord_no, b_s, price, qty, status, place_name, p_code, comments";
                var data = AlfaDirectConnection.adObj.GetLocalDBData("orders", fields, string.Format("acc_code = {0} and ord_no = {1} and status = 'M'", AccountCode, number));

                if (data == null)
                {
                    res.Success = true;
                    res.Data = new DataTable();
                    return res;
                }

                res.Data = ConvertToTable(data, fields);

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }

        /// <summary>
        /// Возвращает текущие заявки портфеля
        /// </summary>
        /// <returns></returns>
        public CallResult GetOrders()
        {
            var res = new CallResult();

            try
            {

                AlfaDirectConnection.Connect();

                string fields = "ord_no, b_s, price, qty, status, place_name, p_code, comments";
                var data = AlfaDirectConnection.adObj.GetLocalDBData("orders", fields, string.Format("acc_code = {0}", AccountCode));
                 
                res.Data = ConvertToTable(data, fields);

                res.Message = AlfaDirectConnection.adObj.LastResultMsg;
                res.Success = true;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }

        /// <summary>
        /// Создает заказ
        /// </summary>
        /// <param name="buyOrSell">B или S</param>
        /// <returns></returns>
        public CallResult CreateSTPOrder(int quantity, double price, string buyOrSell, double slippage)
        {
            var res = new CallResult();

            try
            { 
                AlfaDirectConnection.Connect();

                int orderNumber = AlfaDirectConnection.adObj.CreateStopOrder(AccountCode, StoreCode, ItemCode, DateTime.Now.AddDays(1), "AlfaDirectAutomation", "RUR", buyOrSell, quantity, price, slippage, null, 20);

                res.Message = "Заявка №" + orderNumber + "\r\n" + AlfaDirectConnection.adObj.LastResultMsg;

                if (orderNumber == 0)
                    throw new Exception(res.Message);

                res.Data = orderNumber;
                res.Success = true;

                return res;


            }
            catch (Exception ex)
            {

                res.Message = ex.Message;
                res.Exception = ex;
                res.Success = false;

                return res;
            }
        }
    }
}
