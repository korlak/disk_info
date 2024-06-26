using System;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Controls;

namespace dsk_inf
{
    public partial class MainWindow : Window
    {
        private FileSystemWatcher watcher;
        public MainWindow()
        {
            InitializeComponent();
            LoadDrives();
            DisplaySystemInfo();
            DisplayDirectoriesInfo();
            SetupFileSystemWatcher(); // Додаємо ініціалізацію слідкувача
        }

        private void LoadDrives()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in allDrives)
            {
                drivesListBox.Items.Add(drive.Name);
            }
        }

        private void DrivesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedDrive = (string)drivesListBox.SelectedItem;
            if (selectedDrive != null)
            {
                DriveInfo driveInfo = new DriveInfo(selectedDrive);
                selectedDriveLabel.Content = $"Selected Drive: {selectedDrive}";
                driveInfoTextBlock.Text = $"Drive Type: {driveInfo.DriveType}\n" +
                                          $"Drive Format: {driveInfo.DriveFormat}\n" +
                                          $"Total Size: {driveInfo.TotalSize} bytes\n" +
                                          $"Free Space: {driveInfo.TotalFreeSpace} bytes";
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedDrive = (string)drivesListBox.SelectedItem;
            if (selectedDrive != null)
            {
                // Оновлюємо шлях до вибраного диска
                watcher.Path = selectedDrive;

                // Включаємо відслідковування
                watcher.EnableRaisingEvents = true;

                logListBox.Items.Add($"Started observing changes in directory: {selectedDrive}");
            }
            else
            {
                MessageBox.Show("Please select a drive to observe changes.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Вимикаємо відслідковування
            watcher.EnableRaisingEvents = false;

            string selectedDrive = (string)drivesListBox.SelectedItem;
            if (selectedDrive != null)
            {
                logListBox.Items.Add($"Stopped observing changes in directory: {selectedDrive}");
            }
            else
            {
                MessageBox.Show("No drive is currently being observed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplaySystemInfo()
        {
            string machineName = Environment.MachineName;
            string userName = Environment.UserName;
            string totalMemory = $"{(long)PerformanceInfo.GetTotalMemory()}";

            systemInfoTextBlock.Text = $"Computer Name: {machineName}\n" +
                                       $"User Name: {userName}\n" +
                                       $"Total Memory: {totalMemory} bytes";
        }

        private void DisplayDirectoriesInfo()
        {
            string systemDirectory = Environment.SystemDirectory;
            string tempDirectory = Path.GetTempPath();
            string currentDirectory = Directory.GetCurrentDirectory();

            directoriesInfoTextBlock.Text = $"System Directory: {systemDirectory}\n" +
                                             $"Temp Directory: {tempDirectory}\n" +
                                             $"Current Directory: {currentDirectory}";
        }

        private void SetupFileSystemWatcher()
        {
            // Створення та налаштування FileSystemWatcher
            watcher = new FileSystemWatcher();
            watcher.Path = Directory.GetCurrentDirectory();
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;
            watcher.EnableRaisingEvents = true;
        }

        // Обробник події для зміни/створення/видалення файлів або каталогів
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Виведення повідомлення про зміни у лог
            logListBox.Dispatcher.Invoke(() => logListBox.Items.Add($"File: {e.FullPath} {e.ChangeType}"));
        }

        // Обробник події для перейменування файлів або каталогів
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Виведення повідомлення про перейменування у лог
            logListBox.Dispatcher.Invoke(() => logListBox.Items.Add($"File: {e.OldFullPath} renamed to {e.FullPath}"));
        }
    }

    public static class PerformanceInfo
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool GlobalMemoryStatusEx([System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] MEMORYSTATUSEX lpBuffer);

        public static ulong GetTotalMemory()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                return memStatus.ullTotalPhys;
            }
            else
            {
                return 0;
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }
    }
}