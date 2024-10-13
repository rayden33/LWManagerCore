using DocumentFormat.OpenXml.InkML;
using LWManagerCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private bool companyLogoChanged = false;
        private ApplicationContext dataBaseAC;
        public Settings(ApplicationContext dbac)
        {
            InitializeComponent();

            debtLimitTxtBox.Text = Properties.Settings.Default.DebtLimit.ToString();
            companyNameTxtBox.Text = Properties.Settings.Default.CompanyName;
            companyAddressTxtBox.Text = Properties.Settings.Default.CompanyAddress;
            companyCardTxtBox.Text = Properties.Settings.Default.CompanyCard;
            companyOwnerNameTxtBox.Text = Properties.Settings.Default.CompanyOwnerName;
            companyPhoneTxtBox.Text = Properties.Settings.Default.CompanyPhone;
            agreementTemplateTxtBox.Text = Properties.Settings.Default.AgreementWordFileName;
            if (File.Exists($".\\{Properties.Settings.Default.CompanyLogoImageName}"))
                companyLogoImg.Source = new BitmapImage(new Uri($@"file:///{AppDomain.CurrentDomain.BaseDirectory}/{Properties.Settings.Default.CompanyLogoImageName}"));

            dataBaseAC = dbac;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void ExportBtn(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SQLite (*.db)|*.db";
            if (saveFileDialog.ShowDialog() == true)
                File.Copy($".\\{Properties.Settings.Default.DatabaseFileName}", saveFileDialog.FileName, true);
        }

        private void ImportBtn(object sender, RoutedEventArgs e)
        {
            string dir = $".\\{Properties.Settings.Default.DatabaseArchiveFolder}";
            // If directory does not exist, create it
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.Copy($".\\{Properties.Settings.Default.DatabaseFileName}", dir + $"\\{Properties.Settings.Default.DatabaseFileName}_" + DateTime.Now.ToString("yyMMddHHmmss"));

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "SQLite (*.db)|*.db";
            if (openFileDialog.ShowDialog() == true)
            {
                File.Copy(openFileDialog.FileName, $".\\{Properties.Settings.Default.DatabaseFileName}", true);
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }


        }

        private void SaveMainInfo()
        {
            if(!string.IsNullOrEmpty(companyNameTxtBox.Text))
                Properties.Settings.Default.CompanyName = companyNameTxtBox.Text;
            if (!string.IsNullOrEmpty(companyOwnerNameTxtBox.Text))
                Properties.Settings.Default.CompanyOwnerName = companyOwnerNameTxtBox.Text;
            if (!string.IsNullOrEmpty(companyAddressTxtBox.Text))
                Properties.Settings.Default.CompanyAddress = companyAddressTxtBox.Text;
            if (!string.IsNullOrEmpty(companyPhoneTxtBox.Text))
                Properties.Settings.Default.CompanyPhone = companyPhoneTxtBox.Text;
            if (!string.IsNullOrEmpty(companyCardTxtBox.Text))
                Properties.Settings.Default.CompanyCard = companyCardTxtBox.Text;

            if(!string.IsNullOrEmpty(agreementTemplateTxtBox.Text))
                Properties.Settings.Default.AgreementWordFileName = agreementTemplateTxtBox.Text;
        }

        private void SaveDebtLimit()
        {
            if (!string.IsNullOrEmpty(debtLimitTxtBox.Text))
            {
                if (debtLimitTxtBox.Text[0] != '-')
                {
                    Properties.Settings.Default.DebtLimit = Convert.ToInt32(debtLimitTxtBox.Text.Replace(" ", ""));
                }
                else
                {
                    MessageBox.Show("Долговой лимит не может быть отрицательным");
                    return;
                }
            }
        }

        private void SavePassword()
        {
            if (currentPasswordTxtBox.Text.Length != 0 && newPasswordTxtBox.Text.Length != 0)
            {
                if (currentPasswordTxtBox.Text == Properties.Settings.Default.Password)
                {
                    Properties.Settings.Default.Password = newPasswordTxtBox.Text;
                }
                else
                {
                    MessageBox.Show("Неправильный пароль");
                    return;
                }
            }
        }

        private void SaveSettings()
        {
            SaveMainInfo();
            SaveDebtLimit();
            SavePassword();
            Properties.Settings.Default.Save();
            MessageBox.Show("Успешно сохранен!!!");

            if(File.Exists($".\\~{Properties.Settings.Default.CompanyLogoImageName}"))
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            DialogResult = true;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

        }
        private async void ResetDatabase()
        {
            dataBaseAC.ArchiveLeaseContracts.ExecuteDelete();
            dataBaseAC.Clients.ExecuteDelete();
            dataBaseAC.LeaseContracts.ExecuteDelete();
            dataBaseAC.OrderProducts.ExecuteDelete();
            dataBaseAC.Payments.ExecuteDelete();
            dataBaseAC.ReturnedLeaseContracts.ExecuteDelete();
            dataBaseAC.ReturnedProducts.ExecuteDelete();

            // Reset autoincrement
            await dataBaseAC.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM sqlite_sequence WHERE name = 'ArchiveLeaseContracts';");
            await dataBaseAC.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM sqlite_sequence WHERE name = 'Clients';");
            await dataBaseAC.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM sqlite_sequence WHERE name = 'LeaseContracts';");
            await dataBaseAC.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM sqlite_sequence WHERE name = 'OrderProducts';");
            await dataBaseAC.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM sqlite_sequence WHERE name = 'Payments';");
            await dataBaseAC.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM sqlite_sequence WHERE name = 'ReturnedLeaseContracts';");
            await dataBaseAC.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM sqlite_sequence WHERE name = 'ReturnedProducts';");

            await dataBaseAC.Database.ExecuteSqlInterpolatedAsync($"UPDATE Products SET count=0, price=0;");
            await dataBaseAC.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM Products WHERE reference_id=0;");

            Application.Current.Shutdown();
        }

        private void companyLogoBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.bmp;*.png;*.jpg)|*.bmp;*.png;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                File.Copy(openFileDialog.FileName, $".\\~{Properties.Settings.Default.CompanyLogoImageName}", true);

                companyLogoImg.Source = new BitmapImage(new Uri(openFileDialog.FileName));

                //File.Copy(openFileDialog.FileName, ".\\~companyLogo2.png", true);
                /*
                 * File.Copy(".\\~companyLogo.png", ".\\companyLogo.png", true);
                 * File.Delete(".\\~companyLogo.png");
                 */

                //File.Delete(".\\~companyLogo.png");
                //MessageBox.Show("dis");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MySQL_BackupController backupController = new MySQL_BackupController("localhost","lwmanager","root","root");
            backupController.Backup(dataBaseAC);
        }

        private void ResetDataBtn(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Вы точно хотите очистит данные?","Вы уверены?",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ResetDatabase();
            }
        }
    }
}