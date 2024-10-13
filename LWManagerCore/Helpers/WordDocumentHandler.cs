using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LWManagerCore.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LWManagerCore.Helpers
{
    public class WordDocumentHandler
    {
        public static void CreateAndSaveWordDocument(string filePath, string content)
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());
                Paragraph para = body.AppendChild(new Paragraph());
                Run run = para.AppendChild(new Run());
                run.AppendChild(new Text(content));

                // Save changes to the document
                mainPart.Document.Save();
            }
        }

        public static void WriteToWordDocument(string filePath, string content)
        {
            using (var wordDocument = WordprocessingDocument.Open(filePath, true))
            {
                var body = wordDocument?.MainDocumentPart?.Document.Body;
                var para = body?.AppendChild(new Paragraph());
                var run = para?.AppendChild(new Run());
                run?.AppendChild(new Text(content));

                // Save changes to the document
                wordDocument?.MainDocumentPart?.Document.Save();
            }
        }

        public static string ReadWordDocument(string filePath)
        {
            string allContent = "";
            using (var wordDocument = WordprocessingDocument.Open(filePath, false))
            {
                var body = wordDocument?.MainDocumentPart?.Document.Body;
                foreach (var para in body.Elements<Paragraph>())
                {
                    foreach (var run in para.Elements<Run>())
                    {
                        foreach (var text in run.Elements<Text>())
                        {
                            allContent += text.Text;
                        }
                    }
                }
            }
            return allContent;
        }

        public static void FindAndReplace(string filePath, string searchText, string newText)
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(filePath, true))
            {
                var body = wordDocument.MainDocumentPart?.Document.Body;

                foreach (var text in body.Descendants<Text>()) // Find all text elements
                {
                    if (text.Text.Contains(searchText))
                    {
                        text.Text = text.Text.Replace(searchText, newText); // Replace text
                    }
                }

                wordDocument.MainDocumentPart?.Document.Save(); // Save the changes
            }
        }

        /*
        public static void FindAndReplaceToList(string filePath, string searchText, List<string> newStringList)
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(filePath, true))
            {
                var body = wordDocument.MainDocumentPart.Document.Body;

                // Поиск параграфа, содержащего специфический текст для замены
                var paragraphs = body.Descendants<Paragraph>()
                                     .Where(p => p.InnerText.Contains(searchText));

                // Предполагается, что такой текст встречается один раз
                if (paragraphs.Count() == 1)
                {
                    Paragraph paraToReplace = paragraphs.First();

                    // Создаем новые элементы для каждого продукта
                    foreach (var item in newStringList)
                    {
                        Paragraph newPara = new Paragraph(new Run(new Text($"{item}")));
                        // Вставляем перед параграфом, который нужно заменить
                        body.InsertBefore(newPara, paraToReplace);
                    }

                    // Удаляем исходный параграф
                    paraToReplace.Remove();
                }

                // Сохраняем изменения
                wordDocument.MainDocumentPart.Document.Save();
            }
        }*/

        /// <summary>
        /// Saves the currently open Word document as a new file.
        /// </summary>
        /// <param name="sourceFilePath">The path of the source Word document.</param>
        /// <param name="newFilePath">The path for the new Word document.</param>
        public static void SaveDocumentAs(string sourceFilePath, string newFilePath)
        {
            // Ensure the destination directory exists
            var directory = Path.GetDirectoryName(newFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Open the source document
            using (WordprocessingDocument sourceDocument = WordprocessingDocument.Open(sourceFilePath, false))
            {
                // Create a copy of the source document
                using (WordprocessingDocument newDocument = (WordprocessingDocument)sourceDocument.Clone(newFilePath))
                {
                    // You can make any changes to the new document here

                    // Save changes to the new document
                    //newDocument.MainDocumentPart?.Document.Save();
                }
            }
        }

        /// <summary>
        /// Opens a Word document using the Microsoft Word application.
        /// </summary>
        /// <param name="filePath">The full path to the Word document.</param>
        public static void OpenWordDocument(string filePath)
        {
            try
            {
                // Ensure the path to the document is wrapped in quotes in case there are spaces.
                filePath = $"\"{filePath}\"";

                // Start Microsoft Word and open the document.
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winword.exe", // Command to start Word.
                        Arguments = filePath,
                        UseShellExecute = true // Required to start a process from a different application.
                    }
                };
                process.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static void PrintWordDocument(string filePath)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(filePath)
                {
                    Verb = "Print",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                Process printProcess = new Process();
                printProcess.StartInfo = info;
                printProcess.Start();

                printProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while printing the document: {ex.Message}");
            }
        }
    }
}
