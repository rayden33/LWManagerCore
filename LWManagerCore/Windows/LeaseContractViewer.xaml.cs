using LWManagerCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

namespace LWManagerCore.Windows
{
    /// <summary>
    /// Логика взаимодействия для LeaseContractViewer.xaml
    /// </summary>
    public partial class LeaseContractViewer : Window
    {
        private class OtherProductListItem
        {
            public int Order_product_id { get; set; }
            public string Name { get; set; }
            public int Return_count { get; set; }
            public int Count { get; set; }
            public int Price { get; set; }
            public int PricePerProduct { get; set; }
        }
        private class PaymentListItem
        {
            public string DateTime { get; set; }
            public string PaymentType { get; set; }
            public int Amount { get; set; }
        }
        ApplicationContext dataBaseAC;
        MWViewContract selectedContract;
        DateTime tempDateTime;
        TimeSpan tempTimeSpan;
        public LeaseContractViewer( ApplicationContext dbAp, MWViewContract tmpContract)
        {
            InitializeComponent();
            dataBaseAC = dbAp;
            selectedContract = tmpContract;
            FillData();
        }

        private void FillData()
        {
            Client client = new Client();

            List<OrderProduct> orderProducts = new List<OrderProduct>();
            OrderProduct orderProduct = new OrderProduct();
            List<OtherProductListItem> otherProductListItems = new List<OtherProductListItem>();
            OtherProductListItem otherProductListItem = new OtherProductListItem();

            List<Payment> payments = new List<Payment>();
            List<PaymentListItem> paymentListItems = new List<PaymentListItem>();
            PaymentListItem paymentListItem;

            switch (selectedContract.OrderStatus)
            {
                case 0:/// New contracts
                    LeaseContract selectedLeaseContract = dataBaseAC.LeaseContracts.Find(selectedContract.OrderId);
                    client = dataBaseAC.Clients.Find(selectedLeaseContract.Client_id);
                    payments = dataBaseAC.Payments.Where(p => p.Order_id == selectedLeaseContract.Id).ToList();

                    creationDateTimeLbl.Content = MainUtils.UnixTimeStampToDateTime(selectedLeaseContract.Create_datetime).ToShortDateString();
                    if (selectedLeaseContract.Return_datetime > 0)
                        returnDateTimeLbl.Content = MainUtils.UnixTimeStampToDateTime(selectedLeaseContract.Return_datetime).ToShortDateString();

                    this.DataContext = selectedLeaseContract;
                    break;
                case 1:/// Returned lease contracts
                    ReturnedLeaseContract returnedLeaseContract = dataBaseAC.ReturnedLeaseContracts.Where(r => r.Order_id == selectedContract.OrderId).FirstOrDefault();
                    client = dataBaseAC.Clients.Find(returnedLeaseContract.Client_id);
                    payments = dataBaseAC.Payments.Where(p => p.Order_id == returnedLeaseContract.Order_id).ToList();

                    creationDateTimeLbl.Content = MainUtils.UnixTimeStampToDateTime(returnedLeaseContract.Create_datetime).ToShortDateString();
                    if (returnedLeaseContract.Return_datetime > 0)
                        returnDateTimeLbl.Content = MainUtils.UnixTimeStampToDateTime(returnedLeaseContract.Return_datetime).ToShortDateString();

                    // Feature for sum amount
                    debtLbl.Visibility = Visibility.Visible;
                    debtAmountLbl.Visibility = Visibility.Visible;
                    tempDateTime = MainUtils.UnixTimeStampToDateTime(returnedLeaseContract.Create_datetime);
                    if (returnedLeaseContract.Return_datetime == 0)
                        tempTimeSpan = DateTime.Now - tempDateTime;
                    else
                        tempTimeSpan = MainUtils.UnixTimeStampToDateTime(returnedLeaseContract.Return_datetime) - tempDateTime;
                    int usedDaysTotal = tempTimeSpan.Days + returnedLeaseContract.Used_days;

                    var tmpDebtAmount = returnedLeaseContract.Paid_amount - (returnedLeaseContract.Price_per_day * usedDaysTotal + returnedLeaseContract.Delivery_amount);
                    debtAmountLbl.Content = tmpDebtAmount.ToString("N0", CultureInfo.CreateSpecificCulture("sv-SE"));
                    //debtAmountLbl.ContentStringFormat = "N0";
                    this.DataContext = returnedLeaseContract;

                    dataBaseAC.Entry(returnedLeaseContract).State = EntityState.Unchanged;
                    dataBaseAC.SaveChanges();
                    break;
                case 2:/// Closed contracts
                    ArchiveLeaseContract archiveLeaseContract = dataBaseAC.ArchiveLeaseContracts.Where(c => c.Order_id == selectedContract.OrderId).FirstOrDefault();
                    client = dataBaseAC.Clients.Find(archiveLeaseContract.Client_id);
                    payments = dataBaseAC.Payments.Where(p => p.Order_id == archiveLeaseContract.Order_id).ToList();
                    

                    creationDateTimeLbl.Content = MainUtils.UnixTimeStampToDateTime(archiveLeaseContract.Create_datetime).ToShortDateString();
                    if (archiveLeaseContract.Return_datetime > 0)
                        returnDateTimeLbl.Content = MainUtils.UnixTimeStampToDateTime(archiveLeaseContract.Return_datetime).ToShortDateString();

                    this.DataContext = archiveLeaseContract;
                    break;

            }

            clientFIOLbl.Content = client.Surname + " " + client.Name + " " + client.Middle_name;
            clientPhoneNumberLbl.Content = client.Phone_number;
            clientPassNumberLbl.Content = client.Pass_number;
            clientPhoneNumber2Lbl.Content = client.Phone_number2;
            clientAddressLbl.Content = client.Address;
            orderDaysLbl.Content = selectedContract.UsedDays;
            noteTxtBlock.Text = selectedContract.Note;

            orderProducts = dataBaseAC.OrderProducts.Where(op => op.Order_id == selectedContract.OrderId).ToList();
            foreach(OrderProduct op in orderProducts)
            {
                otherProductListItem = new OtherProductListItem();
                otherProductListItem.Name = dataBaseAC.Products.Find(op.Product_id).Name;
                otherProductListItem.Count = op.Count;
                otherProductListItem.Price = op.Count * op.Price;
                otherProductListItem.PricePerProduct = op.Price;
                if (dataBaseAC.ReturnedProducts.Where(rp => rp.Order_id == op.Order_id && rp.Product_id == op.Product_id).FirstOrDefault() != null)
                    otherProductListItem.Return_count = dataBaseAC.ReturnedProducts.Where(rp => rp.Order_id == op.Order_id && rp.Product_id == op.Product_id).FirstOrDefault().Count;
                otherProductListItems.Add(otherProductListItem);
            }
            otherProductListBox.ItemsSource = otherProductListItems;


            foreach (Payment payment in payments)
            {
                paymentListItem = new PaymentListItem();
                paymentListItem.Amount = payment.Amount;
                paymentListItem.DateTime = MainUtils.UnixTimeStampToDateTime(payment.Datetime).ToShortDateString();
                paymentListItem.PaymentType = payment.Payment_type;
                paymentListItems.Add(paymentListItem);
                
            }
            paymentListBox.ItemsSource = paymentListItems;

            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }


    }
}
