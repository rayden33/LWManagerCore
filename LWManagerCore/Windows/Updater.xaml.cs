using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
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
    /// Логика взаимодействия для Updater.xaml
    /// </summary>
    public partial class Updater : Window
    {
        private readonly string manifestUrl = "https://github.com/rayden33/LWManagerCore/releases/download/latest/update.json"; // Заменить на свой
        private readonly string downloadPath = "update.zip";

        public Updater()
        {
            InitializeComponent();
        }

        private async void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProgressBar.Value = 0;
                var updateInfo = await GetUpdateInfo();
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

                if (currentVersion != updateInfo.version)
                {
                    MessageBox.Show($"Доступна новая версия {updateInfo.version}. Загружаем...");
                    await DownloadFile(updateInfo.url);
                    ApplyUpdate();
                }
                else
                {
                    MessageBox.Show("Вы используете последнюю версию.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления: " + ex.Message);
            }
        }

        private async Task<(string version, string url)> GetUpdateInfo()
        {
            using HttpClient client = new HttpClient();
            var json = await client.GetStringAsync(manifestUrl);
            var doc = JsonDocument.Parse(json);
            var version = doc.RootElement.GetProperty("version").GetString();
            var url = doc.RootElement.GetProperty("url").GetString();
            return (version, url);
        }

        private async Task DownloadFile(string url)
        {
            using HttpClient client = new HttpClient();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None);
            var buffer = new byte[8192];
            long totalRead = 0;
            int read;
            while ((read = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read));
                totalRead += read;
                if (canReportProgress)
                {
                    double progress = (double)totalRead / totalBytes * 100;
                    ProgressBar.Value = progress;
                }
            }
        }

        private void ApplyUpdate()
        {
            string extractPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update_temp");
            ZipFile.ExtractToDirectory(downloadPath, extractPath, true);

            string exePath = System.IO.Path.Combine(extractPath, "LWManagerCore.exe");
            Process.Start(new ProcessStartInfo(exePath));

            Application.Current.Shutdown();
        }
    }
}
