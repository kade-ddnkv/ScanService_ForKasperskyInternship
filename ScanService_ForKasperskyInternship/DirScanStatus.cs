using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ScanService_ForKasperskyInternship
{
    /// <summary>
    /// Результат сканирования директории.
    /// </summary>
    public class DirScanStatus
    {
        // Используются свойства для автоматического парса в json.
        /// <summary>
        /// Сканируемая директория.
        /// </summary>
        public string Directory { get; private set; }

        /// <summary>
        /// Количество статусов каждого типа (пример: ошибокJS = 0, ошибокRmrf = 5 и т.д.).
        /// </summary>
        public Dictionary<FileScanStatus, int> StatusesCount { get; private set; }
        /// <summary>
        /// Время сканирования директории.
        /// </summary>
        public string ExecutionTime { get; private set; }

        private Stopwatch stopwatch = new Stopwatch();

        public DirScanStatus(DirectoryInfo dir)
        {
            // При создании задачи сканирования запускается замер времени.
            stopwatch.Start();
            Directory = dir.FullName;
            StatusesCount = new Dictionary<FileScanStatus, int>();
            foreach (FileScanStatus status in Enum.GetValues(typeof(FileScanStatus)))
            {
                StatusesCount.Add(status, 0);
            }
        }

        public void AddStatuses(FileScanStatus[] statuses)
        {
            foreach (FileScanStatus status in statuses)
            {
                StatusesCount[status]++;
            }
            // При добавлении статусов считается, что директория закончила свое сканирование.
            stopwatch.Stop();
            ExecutionTime = stopwatch.Elapsed.ToString();
        }
    }
}
