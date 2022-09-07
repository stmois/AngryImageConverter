using ImageMagick;
using static System.Configuration.ConfigurationManager;

namespace AngryImageConverter;

public class ImageProcessor
{
    private readonly SemaphoreSlim _semaphore;
    private readonly bool _needToDeleteProcessedFile;

    public ImageProcessor()
    {
        var maxParallelRequests = Convert.ToInt32(AppSettings.Get("MaxParallelRequests"));

        _semaphore = new SemaphoreSlim(maxParallelRequests, maxParallelRequests);
        _needToDeleteProcessedFile = Convert.ToBoolean(AppSettings.Get("NeedToDeleteProcessedFile"));
    }

    public async Task ProcessImageFileAsync(
        string fileFullPath,
        int counter,
        string toFullPath)
    {
        await _semaphore.WaitAsync();

        var generateRandomNames = Convert.ToBoolean(AppSettings.Get("GenerateRandomNames"));

        var newName = generateRandomNames
            ? Guid.NewGuid().ToString().Replace("-", "")
            : Path.GetFileName(fileFullPath);

        try
        {
            if (File.Exists(fileFullPath))
            {
                using var image = new MagickImage(fileFullPath);
                await image.WriteAsync($"{toFullPath}/{newName}.jpg");

                var logMessage = $"{counter}) {DateTime.Now} Успешно обработано фото: {fileFullPath}";

                if (_needToDeleteProcessedFile)
                {
                    File.Delete(fileFullPath);
                    logMessage = $"{logMessage}. Исходный файл удален.";
                }

                PrintHelper.Print(logMessage, ConsoleColor.Green);
            }
            else
            {
                throw new Exception($"Файл не существует {fileFullPath}");
            }
        }
        catch (Exception exception)
        {
            PrintHelper.Print(
                $"{counter}) Не удалось обработать фото {fileFullPath}\nОшибка:\n{exception.Message}",
                ConsoleColor.Red);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}