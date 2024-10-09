using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        static void FileSystemWatcher()
        {
            using (FileSystemWatcher FSW = new FileSystemWatcher())
            {
                string path = @"C:\\test";

            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Директория {path} не существует.");
                return;
            }
            else
            {
                Console.WriteLine($"Directory {path}");
            }

                FSW.NotifyFilter = NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName;


                FSW.Changed += OnChanged;
            FSW.Created += OnChanged;
            FSW.Deleted += OnChanged;
            FSW.Renamed += OnRenamed;

            //FSW.EnableRaisingEvents = true;

            Console.WriteLine("Наблюдение за директорией началось. Нажмите [Enter], чтобы завершить.");
            Console.ReadLine();
            }
            
        }

        static void Main(string[] args)
        {
            TimeToPackFunction();

            FileSystemWatcher();
        }

        private static void OnChanged(object source, FileSystemEventArgs e) =>
        Console.WriteLine($"Файл {e.FullPath} был {e.ChangeType}");

        private static void OnRenamed(object source, RenamedEventArgs e) =>
            Console.WriteLine($"Файл {e.OldFullPath} был переименован в {e.FullPath}");

    }

}
