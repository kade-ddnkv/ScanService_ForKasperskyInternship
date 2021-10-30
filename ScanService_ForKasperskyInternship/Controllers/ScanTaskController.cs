using Microsoft.AspNetCore.Mvc;
using ScanService_ForKasperskyInternship.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ScanService_ForKasperskyInternship.Controllers
{
    /// <summary>
    /// Контроллер, отвечающий за обработку задач на сканирование директорий.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ScanTaskController : Controller
    {
        private static readonly object locker = new object();
        /// <summary>
        /// Словарь, хранящий для каждой задачи отчет о сканировании или "задача еще выполняется".
        /// </summary>
        private static readonly Dictionary<int, TaskAnswer> taskAnswers = new Dictionary<int, TaskAnswer>();
        /// <summary>
        /// Последняя поставленная задача.
        /// </summary>
        private static Task lastTask = Task.CompletedTask;

        /// <summary>
        /// Создает новую задачу на сканирование директории (путь передается в строке запроса).
        /// </summary>
        /// <param name="pathToDir"></param>
        /// <returns></returns>
        [HttpGet("create-new-task")]
        public IActionResult CreateNewTask([FromQuery(Name = "path-to-dir")] string pathToDir)
        {
            // Предполагается, что pathToDir корректный (проверяется в консольном приложении ScanUtil).
            int newTaskIndex = taskAnswers.Count;
            // Блокируется доступ к taskAnswers и lastTask, на случай множественных запросов в одно время.
            lock (locker)
            {
                // В словарь ответов на задачи добавляется новая пара значений.
                taskAnswers.Add(newTaskIndex, new TaskAnswer("Scan task in progress, please wait"));
                // В очередь задач ставится новая задача просканировать директорию.
                lastTask = lastTask.ContinueWith(antecedent => ScanDirectoryAsync(new DirectoryInfo(pathToDir), newTaskIndex)).Unwrap();
            }
            // Возвращается строка с указанием номера задачи.
            return Ok("Scan task was created with ID: " + newTaskIndex);
        }

        /// <summary>
        /// Сканирует каждый файл в директории на наличие подозрительного контента (параллельно).
        /// Завершается после сканирования всех файлов.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="taskIndex"></param>
        /// <returns></returns>
        [NonAction]
        public async Task ScanDirectoryAsync(DirectoryInfo dir, int taskIndex)
        {
            DirScanStatus dirScanStatus = new DirScanStatus(dir);
            // Ожидает сканирования всех файлов.
            var fileStatuses = await Task.WhenAll(dir.GetFiles().Select(file => Task.Run(() => ScanFileInParallelAsync(file))));
            dirScanStatus.AddStatuses(fileStatuses);
            taskAnswers[taskIndex] = new TaskAnswer(dirScanStatus);
        }

        /// <summary>
        /// Сканирует файл на наличие подозрительного контента.
        /// Запускает параллельную проверку каждой строки в файле.
        /// Завершается после проверки всех строк.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [NonAction]
        public async Task<FileScanStatus> ScanFileInParallelAsync(FileInfo file)
        {
            FileScanStatus scanStatus = FileScanStatus.Ok;
            bool lookForJsSuspiciosContent = file.Extension == ".js";

            try
            {
                var checkFileForSuspiciosContent = Task.Run(() =>
                {
                    // Запуск параллельной проверки всех строк.
                    Parallel.ForEach(System.IO.File.ReadLines(file.FullName), (line, _, lineNumber) =>
                    {
                        if (lookForJsSuspiciosContent && line == SuspiciousContent.JsContent)
                        {
                            scanStatus = FileScanStatus.JsDetected;
                        }
                        else if (line == SuspiciousContent.RmrfContent)
                        {
                            scanStatus = FileScanStatus.RmrfDetected;
                        }
                        else if (line == SuspiciousContent.RundllContent)
                        {
                            scanStatus = FileScanStatus.RundllDetected;
                        }
                    });
                });
                await Task.WhenAll(checkFileForSuspiciosContent);
            }
            // Обработка ошибки (например, если файл закрыт на чтение).
            catch (Exception)
            {
                return FileScanStatus.Error;
            }
            return scanStatus;
        }

        /// <summary>
        /// Сканирует файл на наличие подозрительного контента.
        /// Проверяет все строки в одном потоке, сверху вниз.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Obsolete("Is slower than method ScanFileInParallel. Shouldn't be used.")]
        [NonAction]
        public FileScanStatus ScanFile(FileInfo file)
        {
            bool lookForJsSuspiciosContent = file.Extension == ".js";

            try
            {
                using StreamReader sr = new StreamReader(file.FullName);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (lookForJsSuspiciosContent && line == SuspiciousContent.JsContent)
                    {
                        return FileScanStatus.JsDetected;
                    }
                    else if (line == SuspiciousContent.RmrfContent)
                    {
                        return FileScanStatus.RmrfDetected;
                    }
                    else if (line == SuspiciousContent.RundllContent)
                    {
                        return FileScanStatus.RundllDetected;
                    }
                }
            }
            catch (Exception)
            {
                return FileScanStatus.Error;
            }
            return FileScanStatus.Ok;
        }

        /// <summary>
        /// Получает ответ TaskAnswer для задачи (номер задачи передается в строке запроса).
        /// </summary>
        /// <param name="taskIndex"></param>
        /// <returns></returns>
        [HttpGet("get-task-status")]
        public IActionResult GetTaskStatus([FromQuery(Name = "task-index")] int taskIndex)
        {
            if (taskAnswers.ContainsKey(taskIndex))
            {
                return Ok(taskAnswers[taskIndex]);
            }
            return BadRequest(new TaskAnswer("No task with such index"));
        }
    }
}
