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

        static void CheckFildeSydsadastemChanges()
        {
            string directory = @"C:\Windows";
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);

            Console.WriteLine($"Измененные файлы за последние 24 часа в директории {directory}:");

            try
            {
                // Получение всех файлов в директории
                GetModifiedFiles(directoryInfo);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Ошибка доступа к директории: {ex.Message}");
            }
        }

        //static List<FileInfo> CheckFileSystemChanges(string directory)
        //{
        //    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
        //    List<FileInfo> modifiedFiles = new List<FileInfo>();

        //    Console.WriteLine($"Измененные файлы за последние 24 часа в директории {directory}:");

        //    try
        //    {
        //        foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
        //        {
        //            // Check if the file was modified in the last 24 hours
        //            if (file.LastWriteTime >= DateTime.Now.AddHours(-24))
        //            {
        //                Console.WriteLine(file.FullName);
        //                modifiedFiles.Add(file);
        //            }
        //        }
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        Console.WriteLine($"Ошибка доступа к директории: {ex.Message}");
        //    }

        //    return modifiedFiles;
        //}

        static void GetModifiedFiles(DirectoryInfo directoryInfo)
        {
            try
            {
                // Получение всех файлов в текущей директории
                FileInfo[] allFiles = directoryInfo.GetFiles("*.*");

                foreach (FileInfo file in allFiles)
                {
                    try
                    {
                        if (file.LastWriteTime >= DateTime.Now.AddDays(-1))
                        {
                            Console.WriteLine($"Файл: {file.FullName}, Изменён: {file.LastWriteTime}");
                            
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine($"Нет доступа к файлу: {file.FullName}. Ошибка: {ex.Message}");
                    }
                }

                // Получение всех подкаталогов в текущей директории
                DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    // Рекурсивный вызов для подкаталогов
                    GetModifiedFiles(subDirectory);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Нет доступа к директории: {directoryInfo.FullName}. Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке директории: {directoryInfo.FullName}. Ошибка: {ex.Message}");
            }
        }


        static void CreateZipArchive(List<FileInfo> files, string archivePath, string password)
        {
            using (FileStream fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write))
            using (ZipOutputStream zipStream = new ZipOutputStream(fs))
            {
                zipStream.SetLevel(9); // Maximum compression level
                zipStream.Password = password; // Set the password for the zip archive

                try
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            // Extract relative path for the zip entry
                            string entryName = Path.GetFileName(file.FullName); // Use the file name directly
                            ZipEntry entry = new ZipEntry(entryName)
                            {
                                DateTime = file.LastWriteTime,
                                Size = file.Length
                            };

                            zipStream.PutNextEntry(entry); // Start a new entry

                            using (FileStream fileStream = File.OpenRead(file.FullName))
                            {
                                // Copy the file contents to the zip stream
                                fileStream.CopyTo(zipStream);
                                Console.WriteLine($"Файл {file.FullName} записан в архив");
                            }

                            zipStream.CloseEntry(); // Close the current entry
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Console.WriteLine($"Нет доступа к файлу: {file.FullName}. Пропуск...");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при обработке файла: {file.FullName}. Ошибка: {ex.Message}");
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Нет доступа к директории: {archivePath}. Ошибка: {ex.Message}");
                }

                zipStream.Finish(); // Finalize the zip stream
            }

            Console.WriteLine($"Архив {archivePath} создан с паролем {password}");
        }

        static List<FileInfo> CheckFileSystemChanges(string directory)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            List<FileInfo> modifiedFiles = new List<FileInfo>();

            Console.WriteLine($"Измененные файлы за последние 24 часа в директории {directory}:");

            try
            {
                // Attempt to get all files in the directory and its subdirectories
                GetFilesRecursively(directoryInfo, modifiedFiles);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Ошибка доступа к директории: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при доступе к директории: {ex.Message}");
            }

            return modifiedFiles;
        }

        // Recursive method to get files and handle access exceptions
        static void GetFilesRecursively(DirectoryInfo directoryInfo, List<FileInfo> modifiedFiles)
        {
            try
            {
                // Get all files in the current directory
                foreach (var file in directoryInfo.GetFiles())
                {
                    // Check if the file was modified in the last 24 hours
                    // and if it has the specified extensions
                    if (file.LastWriteTime >= DateTime.Now.AddHours(-24) &&
                        (file.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
                         file.Extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.WriteLine(file.FullName);
                        modifiedFiles.Add(file);
                    }
                }

                // Get all subdirectories
                foreach (var subDirectory in directoryInfo.GetDirectories())
                {
                    GetFilesRecursively(subDirectory, modifiedFiles); // Recurse into subdirectories
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Нет доступа к директории: {directoryInfo.FullName}. Пропуск...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при доступе к директории: {directoryInfo.FullName}. Ошибка: {ex.Message}");
            }
        }



        //static void CreateZiddpArchive(List<FileInfo> files, string archivePath, string password)
        //{
        //    using (FileStream fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write))
        //    using (ZipOutputStream zipStream = new ZipOutputStream(fs))
        //    {
        //        zipStream.SetLevel(9); // Максимальный уровень сжатия
        //        zipStream.Password = password; // Устанавливаем пароль

        //        try
        //        {
        //            foreach (var file in files)
        //            {
        //                try
        //                {
        //                    FileInfo fileInfo = file;
        //                    string entryName = fileInfo.FullName.Substring(sourceDirectory.Length + 1);

        //                    ZipEntry entry = new ZipEntry(entryName);
        //                    entry.DateTime = fileInfo.LastWriteTime;
        //                    entry.Size = fileInfo.Length;

        //                    zipStream.PutNextEntry(entry);

        //                    using (FileStream fileStream = File.OpenRead(file))
        //                    {

        //                        fileStream.CopyTo(zipStream);
        //                        Console.WriteLine($"Файл {fileInfo.FullName} записан в архив");
        //                    }

        //                    zipStream.CloseEntry();
        //                }
        //                catch (UnauthorizedAccessException)
        //                {
        //                    Console.WriteLine($"Нет доступа к файлу: {file}. Пропуск...");

        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"Ошибка при обработке файла: {file}. Ошибка: {ex.Message}");
        //                }
        //            }
        //        }
        //        catch (UnauthorizedAccessException ex)
        //        {
        //            Console.WriteLine($"Нет доступа к директории: {sourceDirectory}. Ошибка: {ex.Message}");
        //            CreateZipArchive(sourceDirectory, archivePath, password);
        //        }

        //        zipStream.Finish();
        //        zipStream.Close();
        //    }

        //    Console.WriteLine($"Архив {archivePath} создан с паролем {password}");
        //}

        static void Main(string[] args)
        {
            TimeToPackFunction();

            

            string path = @"C:\Windows"; // Укажите вашу директорию
            string archiveDirectory = @"C:\Users\User\source\Hackathon\uploads"; // Директория для хранения архива
            DateTime dateNow = DateTime.Now;

            // Название архива в формате YYYY-MM-DD
            string archiveName = dateNow.ToString("yyyy-MM-dd") + ".zip";
            string archivePath = Path.Combine(archiveDirectory, archiveName);

            // Пароль в формате DD/MM/YYYY--
            string password = dateNow.ToString("dd/MM/yyyy") + "--";
            
            //CheckFileSystemChanges();
            List<FileInfo> modifiedFiles = CheckFileSystemChanges(path);
            // Создаем архив
            CreateZipArchive(modifiedFiles, archivePath, password);
        }
    }

}
