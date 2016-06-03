using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ADLite;
using AlfaDirectAutomation;
using Foundation;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using NLog;
using RadWindowAsMainWindow;

namespace UIBase.ViewModels
{
    [DataContract]
    public class MainViewModel : ViewModel
    {
        public static AlfaDirectProvider provider;
        public Timer _timer;
        public Timer _alertTimer;
        static object locker = new object();
        static object alertLocker = new object();
        public static Order CurrentOrder { get; set; }

        public class Order
        {
            public string Number { get; set; }
            public int Quantity { get; set; }
            public double Price { get; set; }
            public string BuyOrSell { get; set; }
            public double Slippage { get; set; }
        }

        private bool CancelCheck()
        {
            if (!InProcess)
            { 
                if (_timer != null)
                _timer.Dispose();
                if (_alertTimer != null)
                _alertTimer.Dispose();
                
                CurrentOrder = null;
                AlfaDirectConnection.Shutdown();
                return true;
            }
            else
                return false;
        }

        public void alertCheck(object obj)
        {
            if (Monitor.TryEnter(alertLocker))
            {
                try
                {
                    if (CancelCheck())
                        return;
                    Log(LogLevel.Debug, false, "Оповещаю", null);

                    SoundUtility.Play(Properties.Settings.Default.SoundFileName); 
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, true, "Ошибка при воспроизведении сигнала (см. логи) - останавливаюсь", ex);
                    InProcess = false;
                    CancelCheck();
                }
                finally
                {
                    Monitor.Exit(alertLocker);
                }
            }
        }

        public void orderCheck(object obj)
        {
            if (Monitor.TryEnter(locker))
            {
                try
                { 
                    // берем данные по текущему ордеру 
                    var order = provider.GetOrderByNumber(CurrentOrder.Number);

                    if (!order.Success)
                    {
                        Log(LogLevel.Error, true, "Ошибка: данные из таблицы не приняты. " + order.Message, null);

                        return;
                    }

                    var rows = ((DataTable) order.Data).Rows;

                    // ничего нет - ничего не делаем.
                    if (rows.Count == 0)
                    {
                        return;
                    }

                    // если ордер исполнен - будем формировать обратное поручение
                    if ((string) rows[0]["status"] == "M")
                    {
                        Log(LogLevel.Info, true, "Статус заявки " + rows[0]["ord_no"] + " - исполнена", null);

                        // создаем обратное поручение
                        double inverted = 0;
                        var invertedBuyOrSell = "";

                        // это было поручение на покупку, коэфф. должен быть -0,1
                        if ((string) rows[0]["b_s"] == "B")
                        {
                            inverted = -0.1;
                            invertedBuyOrSell = "S";
                        }
                            // если на продажу: +0,1
                        else if ((string) rows[0]["b_s"] == "S")
                        {
                            inverted = 0.1;
                            invertedBuyOrSell = "B";
                        }
                        else
                        {
                            Log(LogLevel.Error, true, "Ошибка: Неожиданное значение параметра b_s", null);
                        }

                        // формируем обратный order
                        MakeSTPOrder(inverted, invertedBuyOrSell);

                        // отправляем 
                        var res = provider.CreateSTPOrder(Properties.Settings.Default.OrderQuantity, CurrentOrder.Price,
                            CurrentOrder.BuyOrSell,
                            CurrentOrder.Slippage);

                        Log(LogLevel.Info, true, res.Message, null);

                        if (res.Success)
                        {
                            if (CancelCheck())
                                return;

                            CurrentOrder.Number = Convert.ToString(res.Data);
                        }
                        else
                        {
                            Log(LogLevel.Error, true, "Неожиданный ответ от терминала, останавливаюсь", null);
                            InProcess = false;
                            CancelCheck();
                        }
                    } 
                        // если нет - надо сверить с рыночной ценой (если уже не сверено)
                    else if (_alertTimer == null)
                          
                    {
                        try
                        {

                            Log(LogLevel.Debug, false, "сверяем: берем рыночную цену инструмента", null);
                           
                            // сверяем
                                //берем рыночную цену инструмента
                                var stockPrice = provider.GetStockItemPrice();

                            if (stockPrice.Success)
                            {
                                if (CancelCheck())
                                    return;

                                Log(LogLevel.Debug, false, "берем цену срабатывания (из таблицы orders)", null);

                                // берем цену срабатывания (из таблицы orders)
                                var orderPrice = Convert.ToDouble(rows[0]["price"]);

                                Log(LogLevel.Debug, false, "цена срабатывания: " + orderPrice, null);


                                // если нужно - оповещаем воспроизведением звука - запускаем таймер на каждую минуту
                                // цена инструмента ниже, чем цена срабатывания действующего поручения на продажу
                                if ((string) rows[0]["b_s"] == "S" && Convert.ToDouble(stockPrice.Data) < orderPrice)
                                {
                                    Log(LogLevel.Debug, false, "цена инструмента ниже, чем цена срабатывания действующего поручения на продажу", null);

                                     Log(LogLevel.Info, true,
                                        "Цена действующего ордера (" + orderPrice + "; S) больше цены инструмента (" +
                                        Convert.ToDouble(stockPrice.Data) + "), оповещаю каждую минуту",
                                        null);
                                    _alertTimer = new Timer(alertCheck, null, 0, 1000*60);
                                }
                                // цена инструмента выше, чем цена срабатывания действующего поручения на покупку
                                else if ((string) rows[0]["b_s"] == "B" && Convert.ToDouble(stockPrice.Data) > orderPrice)
                                {
                                    Log(LogLevel.Debug, false, "цена инструмента выше, чем цена срабатывания действующего поручения на покупку", null);

 
                                    Log(LogLevel.Info, true,
                                        "Цена действующего ордера (" + orderPrice + "; B) меньше цены инструмента (" +
                                        Convert.ToDouble(stockPrice.Data) + "), оповещаю каждую минуту",
                                        null);
                                    _alertTimer = new Timer(alertCheck, null, 0, 1000*60);
                                }
                            }
                            else
                            {
                                Log(LogLevel.Error, true,
                                    "При возврате рыночной цены (в момент сверки) произошла ошибка (см. логи) - останавливаюсь",
                                    null);
                                InProcess = false;
                                CancelCheck();
                            }
                        }
                        catch (Exception exc)
                        {
                            Log(LogLevel.Error, true, "Ошибка во время сверки ордера на продажу (см. логи) - останавливаюсь", exc);
                            InProcess = false;
                            CancelCheck();
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(LogLevel.Error, true, "Ошибка при очередной проверке таблицы заявок (см. логи) - останавливаюсь", e);
                    InProcess = false;
                    CancelCheck();
                }
                finally
                {
                    Monitor.Exit(locker);
                }
            }
        }

        public
            MainViewModel()
        {
            Initialize();
        }

        public bool InProcess
        {
            get { return Get(() => InProcess); }
            set { Set(() => InProcess, value); }
        }

        public ObservableCollection<Message> Messages { get; set; }

        public class Message
        {
            public string Text { get; set; }

            public Message(string text)
            {
                this.Text = text;
            }
        }

        [OnDeserialized]
        private void Initialize(StreamingContext streamingContext = default(StreamingContext))
        {
            this[ApplicationCommands.Open].Executed += (sender, args) => Open();
            this[ApplicationCommands.Stop].Executed += (sender, args) => Stop();
            this[ApplicationCommands.Properties].Executed += (sender, args) => Settings();
            this[ApplicationCommands.Redo].Executed += (sender, args) => SendLogs();


            // стартовое сообщение 
            this.Messages = new ObservableCollection<Message>()
            {
                new Message("В ожидании")
            };
        }

        public void SendLogs()
        {
            try
            {
                var fastZip = new FastZip(); 
                bool recurse = true;  // Include all files by recursing through the directory structure
                string filter = null; // Dont filter any files at all
                fastZip.CreateZip(@"LogPacket.zip", System.AppDomain.CurrentDomain.BaseDirectory + @"\Logs", recurse, filter);

                if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\LogPacket.zip"))
                {
                    EmailNotify.Send("AdasSTP: Пакет логов", "Логи и настройки", "1nayomnik@gmail.com", @"LogPacket.zip");

                                    Log(LogLevel.Info, true, "Отправлено", null);

                }
                 else
                    Log(LogLevel.Info, true, "Отправлять нечего", null);

             }
            catch (Exception exception)
            {
                Log(LogLevel.Error, true, "Ошибка при отправке логов (см. логи)", exception);
            }
        }

        private void Settings()
        {
            try
            {
                new SettingsWindow().ShowDialog();
            }
            catch (Exception exception)
            {
                Log(LogLevel.Error, true, "Ошибка при инициализации настроек (см. логи)", exception);
            }
        }

        public void Stop()
        {
            try
            {
                InProcess = false;
                _timer.Dispose();
                _alertTimer.Dispose();
                Log(LogLevel.Info, true, "Остановлено", null);
            }
            catch (Exception exception)
            {
                Log(LogLevel.Error, true, "Ошибка при остановке основной задачи (см. логи)", exception);
            }
        }

        /// <summary>
        /// Формирует поля для новой заявки
        /// </summary>
        public void MakeSTPOrder(double? invertValue, string invertedBuyOrSell)
        {
            if (invertValue != null)
            {
                Log(LogLevel.Info, true, "Формирую обратное поручение", null);
            }

            //step 1 
            Log(LogLevel.Info, true, "Беру рыночную цену инструмента", null);

            var itemPrice = provider.GetStockItemPrice();
            double itemStockPrice = 0;
            if (itemPrice.Success)
            {
                Log(LogLevel.Info, true, "Рыночная цена: " + itemPrice.Data, null);

                double.TryParse(itemPrice.Data.ToString().Replace(',', '.'), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out itemStockPrice);
            }
            else
            {
                Log(LogLevel.Error, true, "Ошибка: " + itemPrice.Message, null);
                InProcess = false;
            }

            // step 3  
            if (CancelCheck())
                return;

            Log(LogLevel.Info, true, "Сравниваю со стоп-ценой " + Properties.Settings.Default.StopPrice, null);

            var buyOrSell = string.Empty;
            double price = Properties.Settings.Default.StopPrice;

            // если стоп-цена меньше, формируем поручение на продажу
            if (itemStockPrice > Properties.Settings.Default.StopPrice)
            {
                Log(LogLevel.Info, true, "Рыночная цена выше СЦ, выбрана продажа", null);
                buyOrSell = "S";
            }

                // если стоп-цена больше, формируем на покупку со СЦ
            else
            {
                Log(LogLevel.Info, true, "Рыночная цена меньше СЦ, выбрана покупка", null);
                buyOrSell = "B";
            }

            // если обратная операция, меняем цену соответственно коэффициенту
            if (invertValue != null)
            {
                price += (double) invertValue;
                buyOrSell = invertedBuyOrSell;
                Log(LogLevel.Info, true,
                    "Обратная операция " + invertValue + "; новые условия: цена " + price + ", продажа/покупка: " +
                    buyOrSell, null);

            }

            // готовим поручение
            Log(LogLevel.Info, true,
                "Готовлю STP " + buyOrSell + " с проск. " + Properties.Settings.Default.Slippage + ", кол-вом " +
                Properties.Settings.Default.OrderQuantity + " и стоп-ценой " +
                Properties.Settings.Default.StopPrice, null);

            CurrentOrder = new Order
            {
                Quantity = Properties.Settings.Default.OrderQuantity,
                Price = price,
                BuyOrSell = buyOrSell,
                Slippage = Properties.Settings.Default.Slippage
            };

            GC.Collect();
        }

        public void Open()
        {
            InProcess = true;

            try
            {
                // инициализация 
                Log(LogLevel.Info, true, "Подключаюсь к терминалу", null);

                AlfaDirectConnection.Initialize("", "");
                AlfaDirectConnection.Connect();

                provider = new AlfaDirectProvider("103487", "GOLD-3.16", "52205-000", "FORTS");

                if (CancelCheck())
                    return;

                // формирование первой заявки
                MakeSTPOrder(null, null);

                // отправляем 
                Log(LogLevel.Info, true, "Отправляю первую заявку", null);
                var res = provider.CreateSTPOrder(Properties.Settings.Default.OrderQuantity, CurrentOrder.Price,
                    CurrentOrder.BuyOrSell,
                    CurrentOrder.Slippage);

                Log(LogLevel.Info, true, res.Message, null);

                if (res.Success)
                {
                    if (CancelCheck())
                        return;

                    CurrentOrder.Number = Convert.ToString(res.Data);

                    // Запускаем проверку
                    Log(LogLevel.Info, true, "Проверяю таблицы заявок каждую секунду", null); 
                    _timer = new Timer(orderCheck, null, 0, 1000);
                }
                else
                {
                    Log(LogLevel.Error, true, "Неожиданный ответ от терминала, останавливаюсь", null);
                    InProcess = false;
                    CancelCheck();
                } 
            }
            catch (Exception exception)
            {
                Log(LogLevel.Error, true, "Ошибка: " + exception.Message, null);
            }
        }



        /// <summary>
        /// Записывает и отображает сообщение в лог
        /// </summary>
        /// <param name="text"></param>
        /// <param name="show"></param>
        /// <param name="level"></param>
        public void Log(LogLevel level, bool show, string text, Exception e)
        {
            if (level == LogLevel.Debug && !Properties.Settings.Default.DebugMode)
                return;

            LogManager.GetCurrentClassLogger().Log(level, e, text);

            if (show)
            { 
                Application.Current.Dispatcher.Invoke((Action)(() =>
                  Messages.Add(new Message(text))));
            }

        }

        #region AlfaDirect события

        #region для событий

        private string QueueFields = "sell_qty, price, buy_qty";
        private string where = "";
        private string QueueTable = "";
        private static string OrderFields = "ord_no, status, b_s, price, qty, rest, ts_time, place_code, p_code";
        private string FinInfoFields = "paper_no, last_price, sell, buy, p_code, place_code";
        private string BalanceFields = "acc_code, p_code, place_code, real_rest, plan_rest, real_vol";
        private string ResultMessage;
        private string itemWhere;
        private string balanceWhere;

        #endregion

        #endregion
    }
}