using static System.Configuration.ConfigurationManager;

namespace AngryImageConverter;

public class PathRetriever
{
    private readonly ProcessMode _processModel;

    public PathRetriever()
    {
        _processModel = (ProcessMode)Convert.ToInt32(AppSettings.Get("ProcessMode"));
    }

    public (string fromFullPath, string toFullPath) GetFullPaths()
    {
        string? from = null;
        string? to = null; 

        switch (_processModel)
        {
            case ProcessMode.PreSelectedFolders:
                from = AppSettings.Get("FromFullFolderPath") ?? throw new ArgumentException("Не удалось определить папки");
                to = AppSettings.Get("ToFullFolderPath") ?? throw new ArgumentException("Не удалось определить папки");
                break;

            case ProcessMode.FoldersFromDesktop:
            {
                PrintHelper.Print("Укажи название папки откуда брать изображения на рабочем столе:", ConsoleColor.White);
                var getFolderName = Console.ReadLine();

                PrintHelper.Print("Укажи название папки куда класть изображения на рабочем столе:", ConsoleColor.White);
                var postFolderName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(getFolderName) || string.IsNullOrWhiteSpace(postFolderName))
                {
                    throw new ArgumentException("Не удалось определить папки");
                }

                from = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), getFolderName);
                to = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), postFolderName);
                break;
            }
        }

        return (from, to)!;
    }
}