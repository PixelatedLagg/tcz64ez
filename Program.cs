namespace tcz64ez
{
    public class Program
    {
        const string kernel = "6.6.8-tinycore64";
        static string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
        private static readonly char[] separator = ['\r', '\n'];

        public static async Task Main(string[] args)
        {
            try
            {
                switch (args.Length)
                {
                    case 0:
                        Console.Write("ENTER TCZ NAME (WITHOUT .TCZ): ");
                        string name = Console.ReadLine() ?? throw new ArgumentException("INVALID TCZ NAME");
                        Console.Write($"ENTER DOWNLOAD PATH (DEFAULT IS {downloadPath}): ");
                        string path = Console.ReadLine() ?? "";
                        if (path != "")
                        {
                            downloadPath = path;
                            downloadPath = Path.GetFullPath(downloadPath);
                        }
                        if (!Directory.Exists(downloadPath))
                        {
                            Directory.CreateDirectory(downloadPath);
                        }
                        await GetTCZ(name);
                        break;
                    case 1:
                        downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
                        if (!Directory.Exists(downloadPath))
                        {
                            Directory.CreateDirectory(downloadPath);
                        }
                        await GetTCZ(args[0]);
                        break;
                    case 2:
                        downloadPath = args[1];
                        downloadPath = Path.GetFullPath(downloadPath);
                        if (!Directory.Exists(downloadPath))
                        {
                            Directory.CreateDirectory(downloadPath);
                        }
                        await GetTCZ(args[0]);
                        break;
                    default:
                        Console.WriteLine("USAGE: tcz64ez <TCZ NAME> [DOWNLOAD PATH]");
                        return;
                }
                Console.WriteLine("DONE - PRESS ANY KEY");
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AN ERROR OCCURRED: {ex.Message}");
            }
        }

        public static async Task DownloadFile(string url, string name)
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using FileStream fileStream = new(Path.Combine(downloadPath, name), FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);
            Console.WriteLine($"DOWNLOADED: {url}");
        }

        public static async Task<bool> UrlExists(string url)
        {
            using HttpClient client = new();
            try
            {
                HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task GetTCZ(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            if (name.Contains("KERNEL"))
            {
                name = name.Replace("KERNEL", kernel);
            }
            string depUrl = $"http://tinycorelinux.net/15.x/x86_64/tcz/{name}.tcz.dep";
            if (!await UrlExists(depUrl)) //no dependencies
            {
                if (!HasFile($"{name}.tcz"))
                {
                    await DownloadFile($"http://tinycorelinux.net/15.x/x86_64/tcz/{name}.tcz", $"{name}.tcz");
                }
                else
                {
                    Console.WriteLine($"FOUND {name}.tcz");
                }
                if (!HasFile($"{name}.tcz.md5.txt"))
                {
                    await DownloadFile($"http://tinycorelinux.net/15.x/x86_64/tcz/{name}.tcz.md5.txt", $"{name}.tcz.md5.txt");
                }
                else
                {
                    Console.WriteLine($"FOUND {name}.tcz.md5.txt");
                }
            }
            else
            {
                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(depUrl);
                response.EnsureSuccessStatusCode();
                string dep = await response.Content.ReadAsStringAsync();
                string[] deps = dep.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string depName in deps)
                {
                    if (depName.Length < 4)
                    {
                        continue;
                    }
                    await GetTCZ(depName[..^4]);
                }
                if (!HasFile($"{name}.tcz"))
                {
                    await DownloadFile($"http://tinycorelinux.net/15.x/x86_64/tcz/{name}.tcz", $"{name}.tcz");
                }
                else
                {
                    Console.WriteLine($"FOUND {name}.tcz");
                }
                if (!HasFile($"{name}.tcz.dep"))
                {
                    await DownloadFile($"http://tinycorelinux.net/15.x/x86_64/tcz/{name}.tcz.dep", $"{name}.tcz.dep");
                }
                else
                {
                    Console.WriteLine($"FOUND {name}.tcz.dep");
                }
                if (!HasFile($"{name}.tcz.md5.txt"))
                {
                    await DownloadFile($"http://tinycorelinux.net/15.x/x86_64/tcz/{name}.tcz.md5.txt", $"{name}.tcz.md5.txt");
                }
                else
                {
                    Console.WriteLine($"FOUND {name}.tcz.md5.txt");
                }
            }
        }

        public static bool HasFile(string name) => File.Exists(Path.Combine(downloadPath, name));

        public static void Debug(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}