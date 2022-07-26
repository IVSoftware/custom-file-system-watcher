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

            // Force Create + Change
            File.WriteAllText(
                    testFile, 
                    $"{DateTime.Now}{Environment.NewLine}");

            Thread.Sleep(1000);


            // Force 10 Changes
            for (int i = 1; i < 11; i++)
            {
                // For testing purposes, overwrite the file each time.
                File.AppendAllText(testFile, $"Change #{i}{Environment.NewLine}");
                Thread.Sleep(1000);
            }

            // Force Rename
            var testFileRenamed = Path.Combine(appData, "testFile.Renamed.txt");
            File.Move(testFile, testFileRenamed, overwrite: true);
            Console.ReadKey();
        }
    }
    public class MachineWatcher
    {
        public MachineWatcher(string type, string directoryStock, string fileFilter)
        {
            fw = new FileSystemWatcher(directoryStock, fileFilter);
            fw.Created += onModified;
            fw.Changed += onModified;
            fw.Renamed += onModified;
            fw.Deleted += onModified;
            fw.EnableRaisingEvents = true;
        }
        FileSystemWatcher fw { get; }

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
                    Console.WriteLine($"Changed: {e.Name} {File.ReadAllLines(e.FullPath).Last()}");
                    break;
                case WatcherChangeTypes.Renamed:
                    Console.WriteLine($"Name change: {e.Name}");
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
