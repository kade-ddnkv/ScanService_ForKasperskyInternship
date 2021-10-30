using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScanService_ForKasperskyInternship
{
    /// <summary>
    /// Класс со строками-константами - подозрительным содержимым для поиска в файлах.
    /// </summary>
    public class SuspiciousContent
    {
        public const string JsContent = @"<script>evil_script()</script>";
        public const string RmrfContent = @"rm -rf %userprofile%\Documents";
        public const string RundllContent = @"Rundll32 sus.dll SusEntry";
    }
}
