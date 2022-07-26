using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace custom_file_system_watcher
{
    class Program
    {
        static void Main(string[] args)
        {
            const int SPACING = 500;
            string appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "custom_file_system_watcher");

            // Ensure that the root directory exists
            Directory.CreateDirectory(appData);

            // Make an instance of MachineWatcher
            var mw = new MachineWatcher(
                null, // In minimal reproducible sample this is unused
                appData, 
                "*.*");

            // Force Delete (if exists)
            var testFile = Path.Combine(appData, "testFile.txt");
            File.Delete(testFile);
            Thread.Sleep(SPACING);

            // Force Create + Change
            File.WriteAllText(
                    testFile, 
                    $"{DateTime.Now}{Environment.NewLine}");
            Thread.Sleep(SPACING);


            // Force N Changes
            var N = 5;
            for (int i = 1; i <= N; i++)
            {
                // Using Append because File.WriteAllText is two events not one.
                File.AppendAllText(testFile, $"Change #{i}{Environment.NewLine}");
                Thread.Sleep(SPACING);
            }

            // Force Rename
            var testFileRenamed = Path.Combine(appData, "testFile.Renamed.txt");
            File.Move(testFile, testFileRenamed, overwrite: true);
            Thread.Sleep(SPACING);

            // Prove that if the Excel file LastWriteTime changes, we'll see it
            var excelFile = Path.Combine(appData, "ExcelTestFile.xlsx");
            var fileInfo = new FileInfo(excelFile);
            if(fileInfo.Exists)
            {
                Console.WriteLine();
                Console.WriteLine("Proves that if the Excel file LastWriteTime changes, we'll see it:");
                fileInfo.LastWriteTime = DateTime.Now;
            }
            Thread.Sleep(SPACING);

            Console.WriteLine();
            Console.WriteLine("Waiting for Excel file changes...");
            Console.ReadKey();
        }
    }
    public class MachineWatcher
    {
        public MachineWatcher(string type, string directoryStock, string fileFilter)
        {
            watcher = new FileSystemWatcher(directoryStock, fileFilter);
            watcher.Created += onModified;
            watcher.Changed += onModified;
            watcher.Renamed += onModified;
            watcher.Deleted += onModified; 
            // watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            watcher.EnableRaisingEvents = true;
        }
        FileSystemWatcher watcher { get; }

        private void onModified(object sender, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    OnFeedBackNesterCreated(sender, e);
                    break;
                case WatcherChangeTypes.Deleted:
                    Console.WriteLine($"Deleted: {e.Name}");
                    break;
                case WatcherChangeTypes.Changed:
                    var ext = Path.GetExtension(e.FullPath);
                    switch (ext)
                    {
                        case ".xlsx":
                            Console.WriteLine($"Changed: {e.Name}");
                            break;
                        case ".txt":
                            Console.WriteLine($"Changed: {e.Name} {File.ReadAllLines(e.FullPath).Last()}");
                            break;
                        case "":
                            break;
                        default:
                            Console.WriteLine($"The '{ext}' extension is not supported");
                            break;
                    }
                    break;
                case WatcherChangeTypes.Renamed:
                    Console.WriteLine($"Renamed: {e.Name}");
                    break;
                default:
                    break;
            }
        }

        private void OnFeedBackNesterCreated(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"Created: {e.Name}");
        }
    }
}
