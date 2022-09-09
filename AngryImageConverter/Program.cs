using System.Configuration;
using AngryImageConverter;


PrintHelper.Print("Запуск...", ConsoleColor.Yellow);

var processMode = (ProcessMode)Convert.ToInt32(ConfigurationManager.AppSettings.Get("ProcessMode"));
(string fromFullPath, string toFullPath) paths = (string.Empty, string.Empty);

if (processMode != ProcessMode.AutoscanAndAutoConvert)
{
    paths = new PathRetriever().GetFullPaths();
    Directory.CreateDirectory(paths.toFullPath);
}

var files = processMode == ProcessMode.AutoscanAndAutoConvert 
    ? Directory.GetFiles(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get("AutoScanFolderPath"))
        ? throw new ArgumentException("Ошибка папки сбора изображений")
        : ConfigurationManager.AppSettings.Get("AutoScanFolderPath")!, "*.*", SearchOption.AllDirectories) 
    : Directory.GetFiles(paths.fromFullPath == string.Empty 
        ? throw new ArgumentException("Ошибка папки сбора изображений") 
        : paths.fromFullPath);

var processingImageFormats = ConfigurationManager.AppSettings.Get("ProcessingImageFormats")
    ?.Split(",")
    .Select(x => x.Replace(" ", ""))
    .ToArray();

if (processingImageFormats == null || processingImageFormats.Length == 0)
{
    var error = "Не указаны форматы изображений для обработки";
    PrintHelper.Print(error, ConsoleColor.Red);

    throw new ArgumentException(error);
}

files = files
    .Where(x => processingImageFormats.Contains(Path.GetExtension(x).ToLower()))
    .ToArray();

PrintHelper.Print($"Надено файлов для обработки = {files.Length}", ConsoleColor.Yellow);

var taskBatch = new List<Task>();
var i = 0;

foreach (var file in files)
{
    i++;

    var toFullPath = processMode == ProcessMode.AutoscanAndAutoConvert 
        ? Path.GetDirectoryName(file)!
        : paths.toFullPath == string.Empty 
            ? throw new ArgumentException("Ошибка папки сбора изображений") 
            : paths.toFullPath;

    var task = new ImageProcessor().ProcessImageFileAsync(file, i, toFullPath);
    taskBatch.Add(task);
}

var allTasks = Task.WhenAll(taskBatch);

try
{
    await allTasks;
}
catch (Exception ex)
{
    throw allTasks.Exception ?? ex;
}
