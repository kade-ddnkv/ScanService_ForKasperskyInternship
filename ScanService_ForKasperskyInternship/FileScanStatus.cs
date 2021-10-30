using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScanService_ForKasperskyInternship
{
    /// <summary>
    /// Результат сканирования файла (ok или какой-то тип ошибки).
    /// </summary>
    public enum FileScanStatus
    {
        JsDetected,
        RmrfDetected,
        RundllDetected,
        Error,
        Ok
    }
}
