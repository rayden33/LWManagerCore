using DocumentFormat.OpenXml.Drawing.Charts;
using LWManagerCore.Models;
using Microsoft.EntityFrameworkCore;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static LWManagerCore.Windows.MainStats;

namespace LWManagerCore.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainStats.xaml
    /// </summary>
    public partial class MainStats : Window
    {
        ApplicationContext dataBaseAC;

        public MainStats(ApplicationContext dbap)
        {
            InitializeComponent();
            dataBaseAC = dbap;

            GenerateOrdersPlot();
            GeneratePaymentsPlot();
            GenerateTopClientByOrderCountPlot();
            CalculateLabelContent();

        }

        private void GenerateOrdersPlot()
        {
            List<double> dates = new List<double>();
            List<double> ordersCount = new List<double>();
            SortedDictionary<double, double> keyValuePairs = new SortedDictionary<double, double>();
            DateTime tmpDateTime = new DateTime();
            foreach (ArchiveLeaseContract contract in dataBaseAC.ArchiveLeaseContracts)
            {
                //dates.Add();
                tmpDateTime = MainUtils.UnixTimeStampToDateTime(contract.Create_datetime);
                tmpDateTime = new DateTime(tmpDateTime.Year, tmpDateTime.Month, 1);

                //keyValuePairs[tmpDateTime.ToOADate()] = (keyValuePairs[tmpDateTime.ToOADate()] == null ? 0 : keyValuePairs[tmpDateTime.ToOADate()]) + 1;
                if(!keyValuePairs.ContainsKey(tmpDateTime.ToOADate()))
                        keyValuePairs[tmpDateTime.ToOADate()] = 1;
                else
                    keyValuePairs[tmpDateTime.ToOADate()] = keyValuePairs[tmpDateTime.ToOADate()] + 1;

                //MainOrderPlot.Plot.AddPoint(tmpDateTime.ToOADate(), keyValuePairs[tmpDateTime.ToOADate()]);
            }
            

            double[] dts = keyValuePairs.Keys.ToArray();
            double[] values = keyValuePairs.Values.ToArray();
            if (dts.Count() < 1 || values.Count() < 1)
                return;
            var bar = OrderPlot.Plot.AddBar(values, dts);
            OrderPlot.Plot.YAxis.DateTimeFormat(true);
            bar.Orientation = ScottPlot.Orientation.Horizontal;

            // define tick spacing as 1 day (every day will be shown)

            bar.BarWidth = (30) * .8;
            bar.ShowValuesAboveBars = true;
            bar.Font.Size = 18;
            bar.Font.Bold = true;
            //OrderPlot.Plot.XAxis.ManualTickSpacing(1, ScottPlot.Ticks.DateTimeUnit.Month);
            //OrderPlot.Plot.XAxis.TickLabelStyle(rotation: 45);

            //OrderPlot.Plot.SetAxisLimits(yMin: 0);
            //OrderPlot.Plot.Layout(right: 20);
            // add some extra space for rotated ticks
            //OrderPlot.Plot.XAxis.SetSizeLimit(min: 50);
            OrderPlot.Plot.Title("Аренды");

            OrderPlot.Refresh();

        }

        

        private void GeneratePaymentsPlot()
        {
            List<double> dates = new List<double>();
            List<double> ordersCount = new List<double>();
            SortedDictionary<double, double> keyValuePairs = new SortedDictionary<double, double>();
            DateTime tmpDateTime = new DateTime();
            foreach (Payment payment in dataBaseAC.Payments)
            {
                //dates.Add();
                tmpDateTime = MainUtils.UnixTimeStampToDateTime(payment.Datetime);
                tmpDateTime = new DateTime(tmpDateTime.Year, tmpDateTime.Month, 1);

                //keyValuePairs[tmpDateTime.ToOADate()] = (keyValuePairs[tmpDateTime.ToOADate()] == null ? 0 : keyValuePairs[tmpDateTime.ToOADate()]) + 1;
                if (!keyValuePairs.ContainsKey(tmpDateTime.ToOADate()))
                    keyValuePairs[tmpDateTime.ToOADate()] = payment.Amount;
                else
                    keyValuePairs[tmpDateTime.ToOADate()] = keyValuePairs[tmpDateTime.ToOADate()] + payment.Amount;

                //MainOrderPlot.Plot.AddPoint(tmpDateTime.ToOADate(), keyValuePairs[tmpDateTime.ToOADate()]);
            }


            double[] dts = keyValuePairs.Keys.ToArray();
            double[] values = keyValuePairs.Values.ToArray();
            if (dts.Count() < 1 || values.Count() < 1)
                return;
            var bar = PaymentPlot.Plot.AddBar(values, dts);
            PaymentPlot.Plot.XAxis.DateTimeFormat(true);

            bar.BarWidth = (30) * .8;

            // define tick spacing as 1 day (every day will be shown)
            /*PaymentPlot.Plot.XAxis.ManualTickSpacing(1, ScottPlot.Ticks.DateTimeUnit.Month);
            PaymentPlot.Plot.XAxis.TickLabelStyle(rotation: 45);*/

            // add some extra space for rotated ticks
            PaymentPlot.Plot.XAxis.SetSizeLimit(min: 50);
            PaymentPlot.Plot.Title("Оплаты");

            PaymentPlot.Refresh();

        }

        private void GenerateTopClientByOrderCountPlot()
        {
            List<ClientWithOrderCount> clientsOrder = new List<ClientWithOrderCount>();
            List<string> clientNames = new List<string>();
            List<double> clientOrderCounts = new List<double>();
            Product tmpProduct = new Product();
            double orderCount = 0;
            double sumCountOfOrder = 0;
            if (dataBaseAC.Clients.Count() < 1)
                return;
            foreach (Client client in dataBaseAC.Clients)
            {
                orderCount = dataBaseAC.LeaseContracts.Where(l => l.Client_id == client.Id).Count();
                orderCount += dataBaseAC.ReturnedLeaseContracts.Where(l => l.Client_id == client.Id).Count();
                orderCount += dataBaseAC.ArchiveLeaseContracts.Where(l => l.Client_id == client.Id).Count();
                sumCountOfOrder += orderCount;
                clientsOrder.Add(new ClientWithOrderCount { ClientFullName = client.Name + " " + client.Surname, OrderCount = orderCount });
            }
            

            int maxTopClientsCount = 10;
            clientsOrder.Sort();
            for (int i = Math.Max(clientsOrder.Count - maxTopClientsCount, 0); i <= clientsOrder.Count - 1; i++)
            {
                clientNames.Add(clientsOrder[i].ClientFullName);
                clientOrderCounts.Add(clientsOrder[i].OrderCount);
            }

            double[] xs = DataGen.Consecutive(clientNames.Count);
            double[] values = clientOrderCounts.ToArray();
            string[] labels = clientNames.ToArray();
            if (labels.Count() < 1 || values.Count() < 1 || xs.Count() < 1)
                return;

            //var bar = TopClientByOrderCount.Plot.AddBar(values,xs);
            //bar.HorizontalOrientation = true;
            var bar = TopClientByOrderCount.Plot.AddBar(values, xs);
            TopClientByOrderCount.Plot.YTicks(xs, labels);
            bar.Orientation = ScottPlot.Orientation.Horizontal;

            /*var bar = OrderPlot.Plot.AddBar(values, dts);
            OrderPlot.Plot.XAxis.DateTimeFormat(true);

            // define tick spacing as 1 day (every day will be shown)

            bar.BarWidth = (30) * .8;*/
            //OrderPlot.Plot.XAxis.ManualTickSpacing(1, ScottPlot.Ticks.DateTimeUnit.Month);
            //OrderPlot.Plot.XAxis.TickLabelStyle(rotation: 45);
            TopClientByOrderCount.Plot.SetAxisLimits(yMin: 0);
            TopClientByOrderCount.Plot.Layout(right: 20);
            // add some extra space for rotated ticks
            //OrderPlot.Plot.XAxis.SetSizeLimit(min: 50);
            TopClientByOrderCount.Plot.Title("Топ 10 клиенты");

            TopClientByOrderCount.Refresh();

        }

        private void CalculateLabelContent()
        {
            int inPaymentAmount = 0;
            int outPaymentAmount = 0;
            foreach(Payment payment in dataBaseAC.Payments)
            {
                if(payment.Amount > 0)
                    inPaymentAmount += payment.Amount;
                else
                    outPaymentAmount += payment.Amount;

            }
            outPaymentAmount *= -1;
            inPaymentAmountLbl.Content = inPaymentAmount;
            outPaymentAmountLbl.Content = outPaymentAmount;
            inPaymentAmountLbl.ContentStringFormat = "N0";
            outPaymentAmountLbl.ContentStringFormat = "N0";
        }

        private void LoadFromSQLiteDB()
        {
            dataBaseAC = new ApplicationContext();
            dataBaseAC.LeaseContracts.Load();
            dataBaseAC.Clients.Load();
        }

        public class ClientWithOrderCount : IComparable<ClientWithOrderCount>
        {
            public string ClientFullName { get; set; }
            public double OrderCount { get; set; }

            public int CompareTo(ClientWithOrderCount? other)
            {
                return (int)(this.OrderCount - other.OrderCount);
            }
        }

    }
}