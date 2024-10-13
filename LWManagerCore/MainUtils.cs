using LWManagerCore.Models;
using Microsoft.Office.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Reflection.Metadata;
using Microsoft.Office.Interop.Word;
using System.Threading;
using ScottPlot.Palettes;
using System.Reflection;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using LWManagerCore.Helpers;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml;

namespace LWManagerCore
{
    public static class MainUtils
    {
        public static void AutoSizeWindowAndElements(DataGrid DataGridForResize)
        {
            double DefaultPaddingPerColumn = 20;
            double FreeSpaceForPaddingPerColumn = 0;
            double SummOfWidthAllColumns = 0;
            foreach (DataGridColumn col in DataGridForResize.Columns)
            {
                SummOfWidthAllColumns += col.ActualWidth;
            }

            if (DataGridForResize.ActualWidth > (SummOfWidthAllColumns - 8))
                FreeSpaceForPaddingPerColumn = (DataGridForResize.ActualWidth - SummOfWidthAllColumns - 8) / (double)DataGridForResize.Columns.Count;
            else
                FreeSpaceForPaddingPerColumn = DefaultPaddingPerColumn;

            for (int i = 0; i < DataGridForResize.Columns.Count; i++)
            {
                DataGridForResize.Columns[i].Width = DataGridForResize.Columns[i].ActualWidth + FreeSpaceForPaddingPerColumn;
            }
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }


        public static void GenerationInvoiceWithExcel(ReturnedLeaseContract contract, ApplicationContext dataBaseAC)
        {

            Client client = dataBaseAC.Clients.Where(q => q.Id == contract.Client_id).FirstOrDefault();
            List<Payment> payments = dataBaseAC.Payments.Where(q => q.Order_id == contract.Order_id).ToList();
            List<OrderProduct> orderProducts = dataBaseAC.OrderProducts.Where(q => q.Order_id == contract.Order_id).ToList();

            int paymentCount = payments.Count;
            int usedDays = (UnixTimeStampToDateTime(contract.Return_datetime) - UnixTimeStampToDateTime(contract.Create_datetime)).Days;
            int shouldPay = contract.Price_per_day * (usedDays + contract.Used_days) + contract.Delivery_amount;
            int debt = shouldPay - contract.Paid_amount;
            string cellNumberFormat = "#,#";

            try
            {
                /// Document initialization
                var xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlApp.DefaultSaveFormat = Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook;
                if (xlApp != null)
                {
                    var xlWorkBook = xlApp.Workbooks.Add();
                    var sheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                    /// Company info
                    /// Company logo
                    if (File.Exists($".\\{Properties.Settings.Default.CompanyLogoImageName}"))
                    {
                        Microsoft.Office.Interop.Excel.Range oRange = (Microsoft.Office.Interop.Excel.Range)sheet.Cells[1, 2];
                        float Left = (float)((double)oRange.Left);
                        float Top = (float)((double)oRange.Top);
                        const float ImageSize = 60;
                        sheet.Shapes.AddPicture($"{AppDomain.CurrentDomain.BaseDirectory}\\{Properties.Settings.Default.CompanyLogoImageName}", MsoTriState.msoFalse, MsoTriState.msoCTrue, Left, Top, ImageSize, ImageSize);
                    }
                    /// Header text
                    int row = 1;
                    var range = sheet.Range[$"A{row}:H{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.MergeCells = true;
                    range.Font.Size = 24;
                    range.Value = "ЧЕК";


                    /// Company name
                    row += 3;
                    range = sheet.Range[$"B{row}:D{row}"];
                    range.MergeCells = true;
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.Font.Size = 14;
                    range.Value = Properties.Settings.Default.CompanyName;

                    /// Other company infos
                    row += 1;
                    range = sheet.Range[$"B{row}:D{row}"];
                    range.Merge(Type.Missing);
                    range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                    //range.HorizontalAlignment = 3;
                    range.RowHeight = 75;
                    /*range.EntireRow.AutoFit();
                    range.EntireColumn.AutoFit();*/
                    range.Font.Size = 14;
                    range.Value = Properties.Settings.Default.CompanyAddress.Replace("/n", "\n"); 
                    row += 1;
                    range = sheet.Range[$"B{row}:C{row}"];
                    //range.MergeCells = true;
                    range.Merge(Type.Missing);
                    range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                    //range.WrapText = true;
                    range.Font.Bold = true;
                    //range.HorizontalAlignment = 3;
                    range.Font.Size = 14;
                    range.Value = Properties.Settings.Default.CompanyPhone;
                    row += 1;
                    range = sheet.Range[$"B{row}:C{row}"];
                    range.Font.Bold = true;
                    //range.HorizontalAlignment = 3;
                    range.Merge(Type.Missing);
                    range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                    range.Font.Size = 14;
                    range.Value = Properties.Settings.Default.CompanyCard;
                    row += 1;
                    range = sheet.Range[$"B{row}:D{row}"];
                    range.Merge(Type.Missing);
                    range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                    //range.HorizontalAlignment = 3;
                    range.Font.Size = 14;
                    range.Value = Properties.Settings.Default.CompanyOwnerName;

                    /// First header table
                    sheet.Cells.Range[$"B{row + 2}:G{row + 4}"].Borders.LineStyle = 1;
                    /// First row of header table
                    row += 2;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Номер договора:";
                    sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4] = $"{contract.Contract_id}";
                    sheet.Range[$"D{row}:G{row}"].MergeCells = true;
                    /// Second row of header table
                    row++;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Ф.И.О. заказчика:";
                    sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4] = $"{client.Name} {client.Surname}";
                    sheet.Range[$"D{row}:G{row}"].MergeCells = true;
                    /// Third row of header table
                    row++;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Номер телефон заказчика:";
                    sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4] = $"{client.Phone_number}";
                    sheet.Range[$"D{row}:G{row}"].MergeCells = true;

                    /// Second header table
                    sheet.Cells.Range[$"B{row + 2}:G{row + 4}"].Borders.LineStyle = 1;
                    /// First row of header table
                    row += 2;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Место доставки:";
                    sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4] = $"{contract.Delivery_address}";
                    sheet.Range[$"D{row}:G{row}"].MergeCells = true;
                    /// Second row of header table
                    row++;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Цена доставки:";
                    sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4].NumberFormat = "#,#";
                    sheet.Cells[row, 4] = $"{contract.Delivery_amount}";
                    sheet.Range[$"D{row}:G{row}"].MergeCells = true;
                    /// Third row of header table
                    row++;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Дата заказа:";
                    sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4] = $"{UnixTimeStampToDateTime(contract.Create_datetime).ToString("d MMM yyyy")} - {UnixTimeStampToDateTime(contract.Return_datetime).ToString("d MMM yyyy")}"; ;
                    sheet.Range[$"D{row}:G{row}"].MergeCells = true;

                    /// Payments table
                    /// Header text
                    row += 2;
                    range = sheet.Range[$"B{row}:G{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.MergeCells = true;
                    range.Font.Size = 18;
                    range.Value = "ОПЛАТЫ ЗАКАЗЧИКА";
                    /// Table body
                    /// Table column headers row
                    row++;
                    sheet.Cells[row, 3] = "№";
                    sheet.Cells[row, 4] = "Дата оплаты";
                    sheet.Cells[row, 5] = "Сумма оплаты";
                    sheet.Cells[row, 6] = "Способ оплаты";
                    range = sheet.Range[$"C{row}:F{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.Borders.LineStyle = 1;
                    range.Borders.Weight = 3;
                    /// Table column value rows
                    row++;
                    int total = 0;
                    int rowNum = 0;
                    foreach (Payment payment in payments)
                    {
                        rowNum++;
                        sheet.Cells[row, 3] = rowNum;
                        sheet.Cells[row, 4] = UnixTimeStampToDateTime(payment.Datetime).ToString("d MMM yyyy");
                        sheet.Cells[row, 5] = payment.Payment_type;
                        sheet.Cells[row, 6].NumberFormat = cellNumberFormat;
                        sheet.Cells[row, 6] = payment.Amount;
                        sheet.Range[$"C{row}:F{row}"].Borders.LineStyle = 1;
                        row++;

                        total += payment.Amount;
                    }
                    /// No payments option/
                    if (payments.Count == 0)
                    {
                        range = sheet.Range[$"C{row}:F{row}"];
                        range.HorizontalAlignment = 3;
                        range.Borders.LineStyle = 1;
                        range.MergeCells = true;
                        range.Value = "Не оплачено";
                    }
                    /// Table footer
                    sheet.Cells[row, 5] = "Общее:";
                    sheet.Cells[row, 6].NumberFormat = cellNumberFormat;
                    sheet.Cells[row, 6] = total;
                    range = sheet.Range[$"E{row}:F{row}"];
                    range.Borders.LineStyle = 1;
                    range.Font.Bold = true;
                    range.Font.Size = 16;

                    /// Orders table
                    /// Header text
                    row += 2;
                    range = sheet.Range[$"B{row}:G{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.MergeCells = true;
                    range.Font.Size = 18;
                    range.Value = "ЗАКАЗАННЫЕ ПРОДУКТЫ (за 1 день)";
                    /// Table body
                    /// Table column headers row
                    row++;
                    sheet.Cells[row, 3] = "Название";
                    sheet.Cells[row, 4] = "Количество";
                    sheet.Cells[row, 5] = "Сумма(1 шт.)";
                    sheet.Cells[row, 6] = "Сумма";
                    range = sheet.Range[$"C{row}:F{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.Borders.LineStyle = 1;
                    range.Borders.Weight = 3;
                    /// Table column value rows
                    row++;
                    total = 0;
                    string productName = "";
                    foreach (OrderProduct orderProduct in orderProducts)
                    {
                        sheet.Range[$"E{row}:F{row}"].NumberFormat = cellNumberFormat;
                        sheet.Range[$"C{row}:F{row}"].Borders.LineStyle = 1;
                        productName = dataBaseAC.Products.Where(p => p.Id == orderProduct.Product_id).FirstOrDefault().Name;
                        sheet.Cells[row, 3] = productName;
                        sheet.Cells[row, 4] = orderProduct.Count;
                        sheet.Cells[row, 5] = orderProduct.Price;
                        sheet.Cells[row, 6] = (orderProduct.Count * orderProduct.Price);
                        row++;

                        total += orderProduct.Count * orderProduct.Price;
                    }
                    /// No payments option
                    if (orderProducts.Count == 0)
                    {
                        range = sheet.Range[$"B{row}:E{row}"];
                        range.HorizontalAlignment = 3;
                        range.Borders.LineStyle = 1;
                        range.MergeCells = true;
                        range.Value = "Нет продутов";
                    }
                    /// Table footer
                    sheet.Cells[row, 5] = "Общее:";
                    sheet.Cells[row, 6].NumberFormat = cellNumberFormat;
                    sheet.Cells[row, 6] = total;
                    range = sheet.Range[$"E{row}:F{row}"];
                    range.Borders.LineStyle = 1;
                    range.Font.Bold = true;
                    range.Font.Size = 16;

                    /// Total table part
                    /// Header text
                    row += 2;
                    range = sheet.Range[$"B{row}:F{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.MergeCells = true;
                    range.Value = "ИТОГО";
                    /// Table body
                    /// Table column headers row
                    row++;
                    sheet.Cells[row, 2] = "Кол-во дней";
                    sheet.Cells[row, 3] = "Оплата за 1 день";
                    sheet.Cells[row, 4] = "Цена доставки";
                    sheet.Cells[row, 5] = "Надо оплатить";
                    sheet.Cells[row, 6] = "Оплачено";
                    sheet.Cells[row, 7] = "Долг";
                    range = sheet.Range[$"B{row}:G{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.Borders.LineStyle = 1;
                    range.Borders.Weight = 3;
                    /// Table column value rows
                    row++;
                    sheet.Range[$"C{row}:G{row}"].NumberFormat = cellNumberFormat;
                    sheet.Cells[row, 2] = $"{usedDays} + {contract.Used_days}";
                    sheet.Cells[row, 3] = total;
                    sheet.Cells[row, 4] = contract.Delivery_amount;
                    sheet.Cells[row, 5] = shouldPay;
                    sheet.Cells[row, 6] = contract.Paid_amount;
                    if (debt > 0)
                    {
                        sheet.Cells[row, 7].NumberFormat = cellNumberFormat;
                        sheet.Cells[row, 7] = debt;
                    }
                    else
                        sheet.Cells[row, 7] = "Долгов нету";
                    sheet.Range[$"B{row}:G{row}"].Borders.LineStyle = 1;
                    row++;

                    /// Print settings
                    sheet.Columns["A:H"].EntireColumn.AutoFit();
                    sheet.PageSetup.PrintArea = "$A$1:$H$" + (row + 1).ToString();
                    sheet.PageSetup.Zoom = false;
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.FitToPagesTall = false;
                    xlApp.UserControl = true;
                    xlApp.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*public static void GenerateAgreementWithWord2(LeaseContract leaseContract, ApplicationContext dataBaseAC)
        {
            DateTime tempDateTime;
            TimeSpan tempTimeSpan;

            tempDateTime = UnixTimeStampToDateTime(leaseContract.Create_datetime);
            tempTimeSpan = DateTime.Now - tempDateTime;
            int usedDaysTotal = tempTimeSpan.Days + leaseContract.Used_days;

            Client client = dataBaseAC.Clients.Where(q => q.Id == leaseContract.Client_id).FirstOrDefault();
            List<OrderProduct> orderProducts = dataBaseAC.OrderProducts.Where(q => q.Order_id == leaseContract.Id).ToList();
            StringBuilder orderProductList = new StringBuilder("");
            Product tmpProd = new Product();
            int t = 0;
            foreach (OrderProduct orderProduct in orderProducts)
            {
                tmpProd = dataBaseAC.Products.Where(p => p.Id == orderProduct.Product_id).FirstOrDefault();
                orderProductList.Append($"{++t}. {tmpProd} ({orderProduct.Count} шт)\r\n");
            }

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
            {
                { "companyOwnerName", Properties.Settings.Default.CompanyOwnerName },
                { "clientName", $"{client.Surname} {client.Name} {client.Middle_name}" },
                { "clientPass", $"{client.Pass_number}" },
                { "clientPhone", $"{client.Phone_number}" },
                { "clientPhone2", $"{client.Phone_number2}" },
                { "currentDate", DateTime.Now.ToShortDateString() },
                { "contractId",  leaseContract.Contract_id},
                { "pricePerDay", leaseContract.Price_per_day.ToString() },
                { "productsTotalAmount", (leaseContract.Price_per_day * usedDaysTotal).ToString()},
                { "deliveryAddress", leaseContract.Delivery_address },
                { "deliveryPrice", leaseContract.Delivery_amount.ToString() },
                { "orderProductsList", orderProductList.ToString() }
            };

            try
            {
                var wdApp = new Microsoft.Office.Interop.Word.Application();
                string wordFilePath = Properties.Settings.Default.AgreementWordFileName;
                Microsoft.Office.Interop.Word.Document? doc;
                if (File.Exists(wordFilePath))
                {
                    doc = wdApp.Documents.Open(@$"{AppDomain.CurrentDomain.BaseDirectory}/{wordFilePath}");
                }
                else
                {
                    throw new FileNotFoundException($"Не найден пример договора с именем {wordFilePath}");
                }

                /// Fill check words
                Find findObject = wdApp.Selection.Find;
                object findText;
                foreach(KeyValuePair<string, string> pair in keyValuePairs)
                {
                    findObject.ClearFormatting();
                    findObject.Text = pair.Key;
                    findObject.Replacement.ClearFormatting();
                    findObject.Replacement.Text = pair.Value;

                    findObject.Execute(Forward: true, Wrap: WdFindWrap.wdFindContinue, Replace: WdReplace.wdReplaceAll);
                }


                object start = 0;
                object end = 0;
                Microsoft.Office.Interop.Word.Range tableLocation = doc.Range(ref start, ref end);
                doc.Tables.Add(tableLocation, 3, 4);


                if (MessageBox.Show("Напечатать договор?","Печать",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    doc.PrintOut();

                string newAgreementFilename = $@"{leaseContract.Contract_id}{keyValuePairs["clientName"]}.doc";
                object newAgreementFilePath = @$"{AppDomain.CurrentDomain.BaseDirectory}{Properties.Settings.Default.AgreementArchiveFolderName}/{leaseContract.Contract_id}{keyValuePairs["clientName"]}.doc";
                doc.SaveAs2(ref newAgreementFilePath);
                doc.Close();
                doc = null;
                wdApp.Quit();
                wdApp = null;
                MessageBox.Show($"Договор сохранень в файл {Properties.Settings.Default.AgreementArchiveFolderName}/{newAgreementFilename}");


                /// Re-open file
                //wdApp = new Microsoft.Office.Interop.Word.Application();
                //doc = wdApp.Documents.Open(filename, Visible: true);
                //wdApp.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }*/

        public static void GenerateAgreementWithWord(LeaseContract leaseContract, ApplicationContext dataBaseAC)
        {
            DateTime tempDateTime;
            TimeSpan tempTimeSpan;

            tempDateTime = UnixTimeStampToDateTime(leaseContract.Create_datetime);
            tempTimeSpan = DateTime.Now - tempDateTime;
            int usedDaysTotal = tempTimeSpan.Days + leaseContract.Used_days;

            Client client = dataBaseAC.Clients.Where(q => q.Id == leaseContract.Client_id).FirstOrDefault();
            List<OrderProduct> orderProducts = dataBaseAC.OrderProducts.Where(q => q.Order_id == leaseContract.Id).ToList();
            //List<string> orderProductList = new List<string>();
            StringBuilder orderProductList = new StringBuilder();
            Product tmpProd = new Product();
            int t = 0;
            foreach (OrderProduct orderProduct in orderProducts)
            {
                tmpProd = dataBaseAC.Products.Where(p => p.Id == orderProduct.Product_id).FirstOrDefault();
                orderProductList.Append($"{tmpProd.Name} ({orderProduct.Count} шт),\t");
            }

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
            {
                { "companyOwnerName", Properties.Settings.Default.CompanyOwnerName },
                { "clientName", $"{client.Surname} {client.Name} {client.Middle_name}" },
                { "clientPass", $"{client.Pass_number}" },
                { "clientPhone", $"{client.Phone_number}" },
                { "clientPhone2", $"{client.Phone_number2}" },
                { "currentDate", DateTime.Now.ToShortDateString() },
                { "contractId",  leaseContract.Contract_id},
                { "pricePerDay", leaseContract.Price_per_day.ToString("N0") },
                { "productsTotalAmount", (leaseContract.Price_per_day * usedDaysTotal).ToString("N0")},
                { "usedDays", usedDaysTotal.ToString()},
                { "deliveryAddress", leaseContract.Delivery_address },
                { "deliveryPrice", leaseContract.Delivery_amount.ToString("N0") },
                { "orderProductsList", orderProductList.ToString() }
            };
            string wordFilePath = Properties.Settings.Default.AgreementWordFileName;

            /*try
            {*/
                string templateDocumentText = "";
            if (File.Exists(wordFilePath))
            {
                string newAgreementFilename = $@"{leaseContract.Contract_id}{keyValuePairs["clientName"]}.docx";
                string newAgreementFilePath = @$"{AppDomain.CurrentDomain.BaseDirectory}{Properties.Settings.Default.AgreementArchiveFolderName}/{leaseContract.Contract_id}{keyValuePairs["clientName"]}.doc";
                WordDocumentHandler.SaveDocumentAs(@$"{AppDomain.CurrentDomain.BaseDirectory}/{wordFilePath}", newAgreementFilePath);
                /// Fill check words
                foreach (KeyValuePair<string, string> pair in keyValuePairs)
                {
                    WordDocumentHandler.FindAndReplace(newAgreementFilePath, pair.Key, pair.Value);
                }
                MessageBox.Show($"Договор сохранень в файл {Properties.Settings.Default.AgreementArchiveFolderName}/{newAgreementFilename}");
                /*if (MessageBox.Show("Напечатать договор?", "Печать", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    WordDocumentHandler.PrintWordDocument(newAgreementFilePath);*/
                WordDocumentHandler.OpenWordDocument(newAgreementFilePath);
            }
            else
            {
                throw new FileNotFoundException($"Не найден пример договора с именем {wordFilePath}");
            }

        }

        public static void GenerationClientInvoiceWithExcel(Client client, ApplicationContext dataBaseAC, bool toPrintArchive = true)
        {
            int totalCount = 0;
            int totalReturnedCount = 0;
            int totalPrice = 0;
            int totalDelivery = 0;
            int totalNeedToPay = 0;
            int totalPaid = 0;
            int currentOrdexCount = 0;
            int usedDaysTotal = 0;
            int returnedProductCount = 0;
            string productName = "";
            DateTime tempDateTime = new DateTime();
            TimeSpan tempTimeSpan = new TimeSpan();
            List<ArchiveLeaseContract> archiveLeaseContracts = new List<ArchiveLeaseContract>();
            List<LeaseContract> leaseContracts = new List<LeaseContract>();
            List<OrderProduct> orderProducts = new List<OrderProduct>();
            List<Payment> payments = new List<Payment>();
            List<ReturnedProduct> ReturnedProducts = new List<ReturnedProduct>();
            List<ReturnedLeaseContract> returnedLeaseContracts = new List<ReturnedLeaseContract>();
            archiveLeaseContracts.AddRange(dataBaseAC.ArchiveLeaseContracts.Where(q => q.Client_id == client.Id).ToList());
            leaseContracts.AddRange(dataBaseAC.LeaseContracts.Where(q => q.Client_id == client.Id).ToList());
            returnedLeaseContracts.AddRange(dataBaseAC.ReturnedLeaseContracts.Where(q => q.Client_id == client.Id).ToList());

            /*int paymentCount = payments.Count;
            int usedDays = (UnixTimeStampToDateTime(contract.Return_datetime) - UnixTimeStampToDateTime(contract.Create_datetime)).Days;
            int shouldPay = contract.Price_per_day * (usedDays + contract.Used_days) + contract.Delivery_amount;
            int debt = shouldPay - contract.Paid_amount;*/
            string cellNumberFormat = "#,#";

            try
            {
                /// Document initialization
                var xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlApp.DefaultSaveFormat = Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook;
                if (xlApp != null)
                {
                    var xlWorkBook = xlApp.Workbooks.Add();
                    var sheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                    /// Client info
                    /// Header
                    /// Header title
                    int row = 1;
                    var range = sheet.Range[$"A{row}:H{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.MergeCells = true;
                    range.Font.Size = 24;
                    range.Value = "ИСТОРИЯ";

                    /// First row of header table
                    row += 2;
                    sheet.Cells.Range[$"B{row}:G{row + 3}"].Borders.LineStyle = 1;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Ф.И.О. заказчика:";
                    //sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4] = $"{client.Surname} {client.Name} {client.Middle_name}";
                    sheet.Range[$"C{row}:G{row}"].MergeCells = true;
                    /// Second row of header table
                    row++;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Номер телефон заказчика:";
                    //sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4] = $"{client.Phone_number}; {client.Phone_number2}";
                    sheet.Range[$"C{row}:G{row}"].MergeCells = true;
                    /// Third row of header table
                    row++;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Паспорт:";
                    //sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4] = $"{client.Pass_number}";
                    sheet.Range[$"C{row}:G{row}"].MergeCells = true;
                    /// Fourth row of header table
                    row++;
                    /// Column left part
                    sheet.Cells[row, 2].Font.Bold = true;
                    sheet.Cells[row, 2].HorizontalAlignment = 4;
                    sheet.Cells[row, 2] = "Адрес:";
                    //sheet.Range[$"B{row}:C{row}"].MergeCells = true;
                    /// Column right part
                    sheet.Cells[row, 4].HorizontalAlignment = 3;
                    sheet.Cells[row, 4].NumberFormat = "#,#";
                    sheet.Cells[row, 4] = $"{client.Address}";
                    sheet.Range[$"C{row}:G{row}"].MergeCells = true;
                    /*
                    /// Payment table
                                        /// Header text
                                        row += 2;
                                        range = sheet.Range[$"B{row}:G{row}"];
                                        range.Font.Bold = true;
                                        range.HorizontalAlignment = 3;
                                        range.MergeCells = true;
                                        range.Font.Size = 18;
                                        range.Value = "ОПЛАТЫ ЗАКАЗЧИКА";
                                        /// Table body
                                        /// Table column headers row
                                        row++;
                                        sheet.Cells[row, 3] = "№";
                                        sheet.Cells[row, 4] = "Дата оплаты";
                                        sheet.Cells[row, 5] = "Сумма оплаты";
                                        sheet.Cells[row, 6] = "Способ оплаты";
                                        range = sheet.Range[$"C{row}:F{row}"];
                                        range.Font.Bold = true;
                                        range.HorizontalAlignment = 3;
                                        range.Borders.LineStyle = 1;
                                        range.Borders.Weight = 3;
                                        /// Table column value rows
                                        row++;
                                        int total = 0;
                                        int rowNum = 0;
                                        foreach (Payment payment in payments)
                                        {
                                            rowNum++;
                                            sheet.Cells[row, 3] = rowNum;
                                            sheet.Cells[row, 4] = UnixTimeStampToDateTime(payment.Datetime).ToString("d MMM yyyy");
                                            sheet.Cells[row, 5] = payment.Payment_type;
                                            sheet.Cells[row, 6].NumberFormat = cellNumberFormat;
                                            sheet.Cells[row, 6] = payment.Amount;
                                            sheet.Range[$"C{row}:F{row}"].Borders.LineStyle = 1;
                                            row++;

                                            total += payment.Amount;
                                        }
                                        /// No payments option/
                                        if (payments.Count == 0)
                                        {
                                            range = sheet.Range[$"C{row}:F{row}"];
                                            range.HorizontalAlignment = 3;
                                            range.Borders.LineStyle = 1;
                                            range.MergeCells = true;
                                            range.Value = "Не оплачено";
                                        }
                                        /// Table footer
                                        sheet.Cells[row, 5] = "Общее:";
                                        sheet.Cells[row, 6].NumberFormat = cellNumberFormat;
                                        sheet.Cells[row, 6] = total;
                                        range = sheet.Range[$"E{row}:F{row}"];
                                        range.Borders.LineStyle = 1;
                                        range.Font.Bold = true;
                                        range.Font.Size = 16;
                    */
                    /// Current orders table
                    /// Header text
                    row += 2;
                    range = sheet.Range[$"B{row}:G{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.MergeCells = true;
                    range.Font.Size = 18;
                    range.Value = "ТЕКУЩИЕ ЗАКАЗЫ";
                    /// Table body
                    /// Table column headers row
                    row++;
                    sheet.Cells[row, 1] = "№";
                    sheet.Cells[row, 2] = "Дата заказа";
                    sheet.Cells[row, 3] = "Название";
                    sheet.Cells[row, 4] = "Количество";
                    sheet.Cells[row, 5] = "Сумма";
                    sheet.Cells[row, 6] = "Сумма за продукты \n(До текущего числа)";
                    sheet.Cells[row, 7] = "Доставка";
                    sheet.Cells[row, 8] = "Оплачено";
                    range = sheet.Range[$"A{row}:H{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.Borders.LineStyle = 1;
                    range.Borders.Weight = 3;
                    /// Table column value rows
                    totalPrice = 0;
                    foreach (LeaseContract leaseContract in leaseContracts)
                    {
                        row++;
                        sheet.Cells[row, 1] = ++currentOrdexCount;
                        sheet.Cells[row, 2] = MainUtils.UnixTimeStampToDateTime(leaseContract.Create_datetime).ToString("dd.MM.yyyy");
                        orderProducts = dataBaseAC.OrderProducts.Where(q => q.Order_id == leaseContract.Id).ToList();
                        foreach (OrderProduct orderProduct in orderProducts)
                        {
                            sheet.Range[$"E{row}:E{row}"].NumberFormat = cellNumberFormat;
                            sheet.Range[$"A{row}:H{row}"].Borders.LineStyle = 1;
                            productName = dataBaseAC.Products.Where(p => p.Id == orderProduct.Product_id).FirstOrDefault().Name;
                            sheet.Cells[row, 3] = productName;
                            sheet.Cells[row, 4] = orderProduct.Count;
                            sheet.Cells[row, 5] = (orderProduct.Count * orderProduct.Price);

                            if (orderProduct.Price > 0)
                            {
                                totalCount += orderProduct.Count;
                                sheet.Cells[row, 3].Font.Bold = true;
                                sheet.Cells[row, 4].Font.Bold = true;
                            }
                            totalPrice += orderProduct.Count * orderProduct.Price;
                            row++;
                        }

                        /// Price calculation until today
                        tempDateTime = MainUtils.UnixTimeStampToDateTime(leaseContract.Create_datetime);
                        tempTimeSpan = DateTime.Now - tempDateTime;
                        usedDaysTotal = tempTimeSpan.Days + leaseContract.Used_days;

                        sheet.Range[$"F{row}:H{row}"].NumberFormat = cellNumberFormat;
                        sheet.Range[$"A{row}:H{row}"].Borders.LineStyle = 1;
                        sheet.Range[$"A{row}:H{row}"].Borders.Weight = 3;
                        //sheet.Cells[$"A{row}:H{row}"].Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Weight = 1d;
                        //sheet.Cells[row, 3] = "";
                        sheet.Cells[row, 6] = leaseContract.Price_per_day * usedDaysTotal;
                        sheet.Cells[row, 7] = leaseContract.Delivery_amount;
                        sheet.Cells[row, 8] = leaseContract.Paid_amount;

                        totalDelivery += leaseContract.Delivery_amount;
                        totalNeedToPay += leaseContract.Price_per_day * usedDaysTotal;
                        totalPaid += leaseContract.Paid_amount;

                        /// No payments option
                        if (orderProducts.Count == 0)
                        {
                            range = sheet.Range[$"C{row}:E{row}"];
                            range.HorizontalAlignment = 3;
                            range.Borders.LineStyle = 1;
                            range.MergeCells = true;
                            range.Value = "Нет продутов";
                        }
                    }


                    /// Table footer
                    row++;
                    sheet.Range[$"D{row}:H{row}"].NumberFormat = cellNumberFormat;
                    sheet.Cells[row, 3] = "Общее:";
                    sheet.Cells[row, 4] = totalCount;
                    sheet.Cells[row, 5] = totalPrice;
                    sheet.Cells[row, 6] = totalNeedToPay;
                    sheet.Cells[row, 7] = totalDelivery;
                    sheet.Cells[row, 8] = totalPaid;
                    range = sheet.Range[$"C{row}:H{row}"];
                    range.Borders.LineStyle = 1;
                    range.Font.Bold = true;
                    range.Font.Size = 16;


                    /// Returned orders table
                    /// Header text

                    totalCount = 0;
                    totalReturnedCount = 0;
                    totalPrice = 0;
                    totalDelivery = 0;
                    totalNeedToPay = 0;
                    totalPaid = 0;
                    currentOrdexCount = 0;
                    usedDaysTotal = 0;
                    returnedProductCount = 0;

                    row += 2;
                    range = sheet.Range[$"B{row}:G{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.MergeCells = true;
                    range.Font.Size = 18;
                    range.Value = "ДОЛГОВЫЕ ЗАКАЗЫ";
                    /// Table body
                    /// Table column headers row
                    row++;
                    sheet.Cells[row, 1] = "№";
                    sheet.Cells[row, 2] = "Дата заказа";
                    sheet.Cells[row, 3] = "Название";
                    sheet.Cells[row, 4] = "Кол-во\n(Вернули из Доставлено)";
                    sheet.Cells[row, 5] = "Сумма";
                    sheet.Cells[row, 6] = "Сумма за продукты";
                    sheet.Cells[row, 7] = "Доставка";
                    sheet.Cells[row, 8] = "Оплачено";
                    range = sheet.Range[$"A{row}:H{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.Borders.LineStyle = 1;
                    range.Borders.Weight = 3;
                    /// Table column value rows
                    totalPrice = 0;
                    foreach (ReturnedLeaseContract returnedLeaseContract in returnedLeaseContracts)
                    {
                        row++;
                        sheet.Cells[row, 1] = ++currentOrdexCount;
                        sheet.Cells[row, 2] = $"{MainUtils.UnixTimeStampToDateTime(returnedLeaseContract.Create_datetime).ToString("dd.MM.yyyy")} - {MainUtils.UnixTimeStampToDateTime(returnedLeaseContract.Return_datetime).ToString("dd.MM.yyyy")}";
                        orderProducts = dataBaseAC.OrderProducts.Where(q => q.Order_id == returnedLeaseContract.Order_id).ToList();
                        foreach (OrderProduct orderProduct in orderProducts)
                        {
                            sheet.Range[$"E{row}:E{row}"].NumberFormat = cellNumberFormat;
                            sheet.Range[$"A{row}:H{row}"].Borders.LineStyle = 1;
                            productName = dataBaseAC.Products.Where(p => p.Id == orderProduct.Product_id).FirstOrDefault().Name;
                            if (dataBaseAC.ReturnedProducts.Where(rp => rp.Order_id == orderProduct.Order_id && rp.Product_id == orderProduct.Product_id).FirstOrDefault() != null)
                                returnedProductCount = dataBaseAC.ReturnedProducts.Where(rp => rp.Order_id == orderProduct.Order_id && rp.Product_id == orderProduct.Product_id).FirstOrDefault().Count;
                            else
                                returnedProductCount = 0;

                            sheet.Cells[row, 3] = productName;

                            if (returnedProductCount != orderProduct.Count)
                                sheet.Cells[row, 4].Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                            sheet.Cells[row, 4] = $"{returnedProductCount} из {orderProduct.Count}";
                            sheet.Cells[row, 5] = (orderProduct.Count * orderProduct.Price);

                            if (orderProduct.Price > 0)
                            {
                                totalCount += orderProduct.Count;
                                totalReturnedCount += returnedProductCount;
                                sheet.Cells[row, 3].Font.Bold = true;
                                sheet.Cells[row, 4].Font.Bold = true;
                            }

                            totalPrice += orderProduct.Count * orderProduct.Price;
                            row++;
                        }

                        /// Price calculation until today
                        tempDateTime = MainUtils.UnixTimeStampToDateTime(returnedLeaseContract.Create_datetime);
                        tempTimeSpan = DateTime.Now - tempDateTime;
                        usedDaysTotal = tempTimeSpan.Days + returnedLeaseContract.Used_days;

                        sheet.Range[$"F{row}:H{row}"].NumberFormat = cellNumberFormat;
                        sheet.Range[$"A{row}:H{row}"].Borders.LineStyle = 1;
                        sheet.Range[$"A{row}:H{row}"].Borders.Weight = 3;
                        //sheet.Cells[$"A{row}:H{row}"].Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Weight = 1d;
                        //sheet.Cells[row, 3] = "";
                        sheet.Cells[row, 6] = returnedLeaseContract.Price_per_day * usedDaysTotal;
                        sheet.Cells[row, 7] = returnedLeaseContract.Delivery_amount;
                        sheet.Cells[row, 8] = returnedLeaseContract.Paid_amount;

                        totalDelivery += returnedLeaseContract.Delivery_amount;
                        totalNeedToPay += returnedLeaseContract.Price_per_day * usedDaysTotal;
                        totalPaid += returnedLeaseContract.Paid_amount;

                        /// No payments option
                        if (orderProducts.Count == 0)
                        {
                            range = sheet.Range[$"C{row}:E{row}"];
                            range.HorizontalAlignment = 3;
                            range.Borders.LineStyle = 1;
                            range.MergeCells = true;
                            range.Value = "Нет продутов";
                        }
                    }


                    /// Table footer
                    row++;
                    sheet.Range[$"D{row}:H{row}"].NumberFormat = cellNumberFormat;
                    sheet.Cells[row, 3] = "Общее:";
                    sheet.Cells[row, 4] = $"{totalReturnedCount} из {totalCount}";
                    sheet.Cells[row, 5] = totalPrice;
                    sheet.Cells[row, 6] = totalNeedToPay;
                    sheet.Cells[row, 7] = totalDelivery;
                    sheet.Cells[row, 8] = totalPaid;
                    range = sheet.Range[$"C{row}:H{row}"];
                    range.Borders.LineStyle = 1;
                    range.Font.Bold = true;
                    range.Font.Size = 16;
                    /// Table second footer
                    row++;
                    sheet.Range[$"H{row}:H{row}"].NumberFormat = cellNumberFormat;
                    sheet.Cells[row, 7] = "Долг:";
                    sheet.Cells[row, 8] = totalNeedToPay + totalDelivery - totalPaid;
                    range = sheet.Range[$"G{row}:H{row}"];
                    range.Borders.LineStyle = 1;
                    range.Font.Bold = true;
                    range.Font.Size = 16;


                    if (toPrintArchive)
                    {
                        /// Archived orders table
                        /// Header text
                        totalCount = 0;
                        totalReturnedCount = 0;
                        totalPrice = 0;
                        totalDelivery = 0;
                        totalNeedToPay = 0;
                        totalPaid = 0;
                        currentOrdexCount = 0;
                        usedDaysTotal = 0;
                        returnedProductCount = 0;

                        row += 2;
                        range = sheet.Range[$"B{row}:G{row}"];
                        range.Font.Bold = true;
                        range.HorizontalAlignment = 3;
                        range.MergeCells = true;
                        range.Font.Size = 18;
                        range.Value = "АРХИВНЫЕ ЗАКАЗЫ";
                        /// Table body
                        /// Table column headers row
                        row++;
                        sheet.Cells[row, 1] = "№";
                        sheet.Cells[row, 2] = "Дата заказа";
                        sheet.Cells[row, 3] = "Название";
                        sheet.Cells[row, 4] = "Кол-во\n(Вернули из Доставлено)";
                        sheet.Cells[row, 5] = "Сумма";
                        sheet.Cells[row, 6] = "Сумма за продукты";
                        sheet.Cells[row, 7] = "Доставка";
                        sheet.Cells[row, 8] = "Оплачено";
                        range = sheet.Range[$"A{row}:H{row}"];
                        range.Font.Bold = true;
                        range.HorizontalAlignment = 3;
                        range.Borders.LineStyle = 1;
                        range.Borders.Weight = 3;
                        /// Table column value rows
                        totalPrice = 0;
                        foreach (ArchiveLeaseContract archivedLeaseContract in archiveLeaseContracts)
                        {
                            row++;
                            sheet.Cells[row, 1] = ++currentOrdexCount;
                            sheet.Cells[row, 2] = $"{MainUtils.UnixTimeStampToDateTime(archivedLeaseContract.Create_datetime).ToString("dd.MM.yyyy")} - {MainUtils.UnixTimeStampToDateTime(archivedLeaseContract.Return_datetime).ToString("dd.MM.yyyy")}";
                            orderProducts = dataBaseAC.OrderProducts.Where(q => q.Order_id == archivedLeaseContract.Order_id).ToList();
                            foreach (OrderProduct orderProduct in orderProducts)
                            {
                                sheet.Range[$"E{row}:E{row}"].NumberFormat = cellNumberFormat;
                                sheet.Range[$"A{row}:H{row}"].Borders.LineStyle = 1;
                                productName = dataBaseAC.Products.Where(p => p.Id == orderProduct.Product_id).FirstOrDefault().Name;

                                sheet.Cells[row, 3] = productName;
                                sheet.Cells[row, 4] = orderProduct.Count;
                                sheet.Cells[row, 5] = (orderProduct.Count * orderProduct.Price);

                                if (orderProduct.Price > 0)
                                {
                                    totalCount += orderProduct.Count;
                                    sheet.Cells[row, 3].Font.Bold = true;
                                    sheet.Cells[row, 4].Font.Bold = true;
                                }
                                totalPrice += orderProduct.Count * orderProduct.Price;
                                row++;
                            }

                            /// Price calculation until today
                            tempDateTime = MainUtils.UnixTimeStampToDateTime(archivedLeaseContract.Create_datetime);
                            tempTimeSpan = DateTime.Now - tempDateTime;
                            usedDaysTotal = tempTimeSpan.Days + archivedLeaseContract.Used_days;

                            sheet.Range[$"F{row}:H{row}"].NumberFormat = cellNumberFormat;
                            sheet.Range[$"A{row}:H{row}"].Borders.LineStyle = 1;
                            sheet.Range[$"A{row}:H{row}"].Borders.Weight = 3;
                            //sheet.Cells[$"A{row}:H{row}"].Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Weight = 1d;
                            //sheet.Cells[row, 3] = "";
                            sheet.Cells[row, 6] = archivedLeaseContract.Price_per_day * usedDaysTotal;
                            sheet.Cells[row, 7] = archivedLeaseContract.Delivery_amount;
                            sheet.Cells[row, 8] = archivedLeaseContract.Paid_amount;

                            totalDelivery += archivedLeaseContract.Delivery_amount;
                            totalNeedToPay += archivedLeaseContract.Price_per_day * usedDaysTotal;
                            totalPaid += archivedLeaseContract.Paid_amount;

                            /// No payments option
                            if (orderProducts.Count == 0)
                            {
                                range = sheet.Range[$"C{row}:E{row}"];
                                range.HorizontalAlignment = 3;
                                range.Borders.LineStyle = 1;
                                range.MergeCells = true;
                                range.Value = "Нет продутов";
                            }
                        }


                        /// Table footer
                        row++;
                        sheet.Range[$"D{row}:H{row}"].NumberFormat = cellNumberFormat;
                        sheet.Cells[row, 3] = "Общее:";
                        sheet.Cells[row, 4] = totalCount;
                        sheet.Cells[row, 5] = totalPrice;
                        sheet.Cells[row, 6] = totalNeedToPay;
                        sheet.Cells[row, 7] = totalDelivery;
                        sheet.Cells[row, 8] = totalPaid;
                        range = sheet.Range[$"C{row}:H{row}"];
                        range.Borders.LineStyle = 1;
                        range.Font.Bold = true;
                        range.Font.Size = 16;
                    }

                    /*
                    /// Total table part
                    /// Header text
                    row += 2;
                    range = sheet.Range[$"B{row}:F{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.MergeCells = true;
                    range.Value = "ИТОГО";
                    /// Table body
                    /// Table column headers row
                    row++;
                    sheet.Cells[row, 2] = "Кол-во дней";
                    sheet.Cells[row, 3] = "Оплата за 1 день";
                    sheet.Cells[row, 4] = "Цена доставки";
                    sheet.Cells[row, 5] = "Надо оплатить";
                    sheet.Cells[row, 6] = "Оплачено";
                    sheet.Cells[row, 7] = "Долг";
                    range = sheet.Range[$"B{row}:G{row}"];
                    range.Font.Bold = true;
                    range.HorizontalAlignment = 3;
                    range.Borders.LineStyle = 1;
                    range.Borders.Weight = 3;
                    /// Table column value rows
                    row++;
                    sheet.Range[$"C{row}:G{row}"].NumberFormat = cellNumberFormat;
                    sheet.Cells[row, 2] = $"{usedDays} + {contract.Used_days}";
                    sheet.Cells[row, 3] = total;
                    sheet.Cells[row, 4] = contract.Delivery_amount;
                    sheet.Cells[row, 5] = shouldPay;
                    sheet.Cells[row, 6] = contract.Paid_amount;
                    if (debt > 0)
                    {
                        sheet.Cells[row, 7].NumberFormat = cellNumberFormat;
                        sheet.Cells[row, 7] = debt;
                    }
                    else
                        sheet.Cells[row, 7] = "Долгов нету";
                    sheet.Range[$"B{row}:G{row}"].Borders.LineStyle = 1;
                    row++;
                    */


                    /// Print settings
                    sheet.Columns["A:H"].EntireColumn.AutoFit();
                    sheet.PageSetup.PrintArea = "$A$1:$H$" + (row + 1).ToString();
                    sheet.PageSetup.Zoom = false;
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.FitToPagesTall = false;
                    xlApp.UserControl = true;
                    xlApp.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string _SystemTracert(string input)
        {
            string tmpOutput = "";
            string output = "";
            int hr, hr2;
            int n = input.Length;
            int code;
            for (int i = 0; i < n; i++)
            {
                if (input[i] < '0' || input[i] > '{')
                    return "outOfAccessCharsRange";
                code = input[i];
                code *= (i + 1);
                hr = code / 124;
                hr2 = (code % 124) / 48;
                tmpOutput += $"{(hr)}{((code % 124) + ((hr2 == 0) ? 48 : 0))}{(hr2)}";
            }
            n = tmpOutput.Length;
            for (int i = 1; i < n; i += 2)
            {
                code = Convert.ToInt32($"{tmpOutput[i - 1]}{tmpOutput[i]}");
                hr = code / 48;
                output += $"{(char)((code) + ((hr == 0) ? 48 : 0))}{hr}";
            }
            if (n % 2 == 1)
            {
                code = Convert.ToInt32($"{tmpOutput[n - 1]}");
                hr = code / 48;
                output += $"{(char)((code) + ((hr == 0) ? 48 : 0))}";
            }

            return output + $"{n % 2}";
        }
    }
}
