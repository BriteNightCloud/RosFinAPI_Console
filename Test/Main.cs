using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal partial class Program
    {
        static string addressAPI = "https://portal.fedsfm.ru:8081/Services/fedsfm-service";
        //static string addressAPI = "https://portal.fedsfm.ru:8081/Services/fedsfm-service/test-contur";

        // Если True, сохраняет запросы и ответы в файлах в формате .json
        static bool JSONout = false;
        static LatestFiles LatestDownloads;
        static Config config = GetConfig();

        // Путь, куда программа будет сохранять все временные файлы
        static readonly string TEMP_PATH = config.TempPath;
        // Путь, куда программа будет сохранять распакованные и готовые xml файлы
        static readonly string OUT_PATH = config.XMLOutPath;

        static readonly string LOGIN = config.Login;
        static readonly string PASSWORD = config.Password;
        // Отпечаток сертификата
        static readonly string CERT_THUMBPRINT = config.CertThumbprint;

        static void Main(string[] args)
        {
            if (!Directory.Exists(TEMP_PATH) ||
                !Directory.Exists($"{TEMP_PATH}\\logs") ||
                !Directory.Exists(OUT_PATH))
            {
                Directory.CreateDirectory(TEMP_PATH);
                Directory.CreateDirectory($"{TEMP_PATH}\\logs");
                Directory.CreateDirectory(OUT_PATH);
            }

            var LogFile = File.Open($"{TEMP_PATH}\\logs\\{DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss")}.txt", FileMode.Create);
            StreamWriter streamwriter = new StreamWriter(LogFile);
            streamwriter.AutoFlush = true;

            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);

            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Начало выполнения программы: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            GetFilesDate();
            MainAsync().GetAwaiter().GetResult();
        }
        static async Task MainAsync()
        {
            string te2Id = null;
            string mvkId = null;
            string omuId = null;

            DateTime NewTe2Date;
            DateTime NewMvkDate;
            DateTime NewOmuDate;

            X509Certificate2 cert = getCert(CERT_THUMBPRINT);

            if (cert == null)
            {
                Console.WriteLine("Ошибка выбора сертификата!");
                return;
            }

            string token = AuthGetToken(LOGIN, PASSWORD, cert);

            if (token == null)
            {
                Console.WriteLine("Ошибка получения токена авторизации! Возможно неправильно указан логин/пароль/сертификат.");
                return;
            }

            GetCatalogID("current-te2-catalog", cert, token, out te2Id, out NewTe2Date);
            GetCatalogID("current-mvk-catalog", cert, token, out mvkId, out NewMvkDate);
            GetCatalogID("current-omu-catalog", cert, token, out omuId, out NewOmuDate);

            if (String.IsNullOrEmpty(te2Id) && String.IsNullOrEmpty(mvkId) && String.IsNullOrEmpty(omuId))
            {
                Console.WriteLine("Ошибка получения сведений о перечнях!");
                return;
            }

            if (te2Id != null) await GetFileFromID("current-te2-file", te2Id, cert, token, NewTe2Date, LFtype.te2);
            if (mvkId != null) await GetFileFromID("mvk-catalog-file", mvkId, cert, token, NewMvkDate, LFtype.mvk);
            if (omuId != null) await GetFileFromID("omu-catalog-file", omuId, cert, token, NewOmuDate, LFtype.omu);

            Console.WriteLine("\nПрограмма завершила свою работу!");
            Console.WriteLine("Конец выполнения программы: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
        }
    }
}
