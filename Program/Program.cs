using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32.TaskScheduler;

namespace Program
{
    internal class Program
    {
        static void TimeToPackFunction()
        {
            using (TaskService ts = new TaskService())
            {
                Microsoft.Win32.TaskScheduler.Task newTask = ts.FindTask("TimeToPack");

                if (newTask == null)
                {
                    Console.WriteLine($"{newTask} is not found, wait a minute... ");

                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "создать с указанием времени 12:00 и действия с запуском этой же программы";

                    td.Triggers.Add(new DailyTrigger { StartBoundary = DateTime.Today.AddHours(12) });

                    td.Actions.Add(new ExecAction("C: \\Users\\User\\source\\Hackathon\\uploads\\Program\\Program.sln"));

                    ts.RootFolder.RegisterTaskDefinition("TimeToPack", td);
                }
                else
                {
                    Console.WriteLine($"{newTask} is ready");
                }
            }
        }

        static void CheckFileSystemChanges()
        {
            string directory = @"C:\Test"; 

            DirectoryInfo directoryInfo = new DirectoryInfo(directory);

            // Получение всех файлов в директории
            FileInfo[] allFiles = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);

            Console.WriteLine($"Измененные файлы за последние 24 часа в директории {directory}:");

            foreach (FileInfo file in allFiles)
            {
                if (file.LastWriteTime >= DateTime.Now.AddDays(-1))
                {
                    Console.WriteLine($"Файл: {file.FullName}, Изменён: {file.LastWriteTime}");
                }
            }
        }

        static void CreateZipArchive(string sourceDirectory, string archivePath, string password)
        {
            // Создаем новый zip-файл
            using (FileStream fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write))
            using (ZipOutputStream zipStream = new ZipOutputStream(fs))
            {
                zipStream.SetLevel(9); // Уровень сжатия (0-9)
                zipStream.Password = password; // Установка пароля

                // Получение всех файлов с расширениями .txt, .csv, .xlsx, измененных за последние сутки
                var files = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
                    .Where(f => (f.EndsWith(".txt") || f.EndsWith(".csv") || f.EndsWith(".xlsx"))
                                && File.GetLastWriteTime(f) >= DateTime.Now.AddDays(-1));

                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    string entryName = fileInfo.FullName.Substring(sourceDirectory.Length + 1); // Получаем имя файла в архиве

                    // Создаем новый entry в архиве
                    ZipEntry entry = new ZipEntry(entryName);
                    entry.DateTime = fileInfo.LastWriteTime;
                    entry.Size = fileInfo.Length;

                    zipStream.PutNextEntry(entry);

                    // Считываем содержимое файла и записываем в архив
                    using (FileStream fileStream = File.OpenRead(file))
                    {
                        fileStream.CopyTo(zipStream);
                    }

                    zipStream.CloseEntry();
                }

                zipStream.Finish();
                zipStream.Close();
            }

            Console.WriteLine($"Архив {archivePath} создан с паролем {password}");
        }

        static void Main(string[] args)
        {
            TimeToPackFunction();

            CheckFileSystemChanges();

            string path = @"C:\Test"; // Укажите вашу директорию
            string archiveDirectory = @"http://localhost/uploads"; // Директория для хранения архива
            DateTime dateNow = DateTime.Now;

            // Название архива в формате YYYY-MM-DD
            string archiveName = dateNow.ToString("yyyy-MM-dd") + ".zip";
            string archivePath = Path.Combine(archiveDirectory, archiveName);

            // Пароль в формате DD/MM/YYYY--
            string password = dateNow.ToString("dd/MM/yyyy") + "--";

            // Создаем архив
            CreateZipArchive(path, archivePath, password);
        }
    }

}
