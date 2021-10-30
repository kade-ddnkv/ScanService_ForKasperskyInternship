using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScanService_ForKasperskyInternship.Models
{
    /// <summary>
    /// Модель ответа (принимается приложением scan_util).
    /// </summary>
    public class TaskAnswer
    {
        /// <summary>
        /// Статус ответа (ok, нет ответа, несуществующая задача).
        /// </summary>
        public string Status { get; private set; }
        /// <summary>
        /// Результат сканирования директории (null - если статус не ok).
        /// </summary>
        public DirScanStatus Result { get; private set; }

        /// <summary>
        /// Контруктор при отсутвии результата сканирования.
        /// </summary>
        /// <param name="status"></param>
        public TaskAnswer(string status)
        {
            Status = status;
        }

        /// <summary>
        /// Конструктор при уже выполненном сканировании.
        /// </summary>
        /// <param name="result"></param>
        public TaskAnswer(DirScanStatus result)
        {
            Status = "ok";
            Result = result;
        }
    }
}
