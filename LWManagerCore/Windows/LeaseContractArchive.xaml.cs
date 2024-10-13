using LWManagerCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для LeaseContractArchive.xaml
    /// </summary>
    public partial class LeaseContractArchive : Window
    {
        ApplicationContext dataBaseAC;
        List<MWViewContract> viewContracts;
        MWViewContract tempViewContract;
        Client tempClient;
        DateTime tempDateTime;
        TimeSpan tempTimeSpan;
        public LeaseContractArchive(ApplicationContext dbap)
        {
            InitializeComponent();
            dataBaseAC = dbap;
            
            LoadFromSQLiteDB();
            GetDbToDataGrid();
        }

        void GetDbToDataGrid()
        {
            int i = 0;
            viewContracts = new List<MWViewContract>();

            foreach (ArchiveLeaseContract contract in dataBaseAC.ArchiveLeaseContracts)
            {
                i++;
                tempViewContract = new MWViewContract();
                tempViewContract.OrderId = contract.Order_id;
                tempViewContract.RowNumber = i;

                tempClient = dataBaseAC.Clients.FirstOrDefault(client => client.Id == contract.Client_id);
                tempViewContract.FISH = (tempClient.Surname + " " + tempClient.Name + " " + tempClient.Middle_name);

                tempDateTime = MainUtils.UnixTimeStampToDateTime(contract.Create_datetime);
                tempViewContract.CreationDateTime = tempDateTime.ToShortDateString();

                if (contract.Return_datetime == 0)
                    tempTimeSpan = DateTime.Now - tempDateTime;
                else
                    tempTimeSpan = MainUtils.UnixTimeStampToDateTime(contract.Return_datetime) - tempDateTime;
                tempViewContract.UsedDays = $"{tempTimeSpan.Days} " + ((contract.Used_days > 0) ? ($"(+{contract.Used_days})") : (""));
                int usedDaysTotal = tempTimeSpan.Days + contract.Used_days;

                if (dataBaseAC.OrderProducts.Where(op => op.Order_id == contract.Order_id && op.Product_id == 5).FirstOrDefault() != null)
                    tempViewContract.BLease = dataBaseAC.OrderProducts.Where(op => op.Order_id == contract.Order_id && op.Product_id == 5).FirstOrDefault().Count.ToString();
                if (dataBaseAC.OrderProducts.Where(op => op.Order_id == contract.Order_id && op.Product_id == 6).FirstOrDefault() != null)
                    tempViewContract.LLease = dataBaseAC.OrderProducts.Where(op => op.Order_id == contract.Order_id && op.Product_id == 6).FirstOrDefault().Count.ToString();
                if (dataBaseAC.OrderProducts.Where(op => op.Order_id == contract.Order_id && op.Product_id == 4).FirstOrDefault() != null)
                    tempViewContract.Wheel = dataBaseAC.OrderProducts.Where(op => op.Order_id == contract.Order_id && op.Product_id == 4).FirstOrDefault().Count.ToString();
                tempViewContract.Phone = tempClient.Phone_number;
                tempViewContract.DeliveryPrice = contract.Delivery_amount;
                tempViewContract.DeliveryAddress = contract.Delivery_address;
                tempViewContract.PaidAmount = contract.Paid_amount;
                tempViewContract.Sum = contract.Paid_amount - (contract.Price_per_day * usedDaysTotal + contract.Delivery_amount);
                tempViewContract.OrderStatus = 2;
                viewContracts.Add(tempViewContract);

            }
            this.DataContext = viewContracts;


        }

        private void LoadFromSQLiteDB()
        {
            dataBaseAC = new ApplicationContext();
            dataBaseAC.LeaseContracts.Load();
            dataBaseAC.Clients.Load();
        }


        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (leaseContractDataGrid.SelectedItem == null)
                return;
            MWViewContract mWViewContract = leaseContractDataGrid.SelectedItem as MWViewContract;
            LeaseContractViewer leaseContractViewer = new LeaseContractViewer(dataBaseAC, mWViewContract);
            leaseContractViewer.ShowDialog();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }


        private void leaseContractDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = searchTxtBox.Text;
            List<MWViewContract> searchResult = new List<MWViewContract>();
            GetDbToDataGrid();
            //viewContracts = dataBaseAC.ArchiveLeaseContracts.ToList();
            if (text == null || text == "")
            {
                GetDbToDataGrid();
                return;
            }

            foreach (MWViewContract tmpContract in viewContracts)
            {
                if (tmpContract.FISH != null && tmpContract.FISH.Contains(text))
                    searchResult.Add(tmpContract);
                else if (tmpContract.Phone != null && tmpContract.Phone.Contains(text))
                    searchResult.Add(tmpContract);
                else if (tmpContract.DeliveryAddress != null && tmpContract.DeliveryAddress.Contains(text))
                    searchResult.Add(tmpContract);
            }

            this.DataContext = searchResult;
        }

        private void leaseContractDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            MainUtils.AutoSizeWindowAndElements(leaseContractDataGrid);
        }
    }
}
