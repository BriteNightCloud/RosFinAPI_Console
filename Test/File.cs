﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace Test
{
    partial class Program
    {
        static async Task GetFileFromID(string fileName, string fileId, X509Certificate2 cert, string token, DateTime NewDate, LFtype type)
        {
            Console.WriteLine();
            DateTime OldDate;

            if (type == LFtype.te2)
                OldDate = DateTime.Parse(LatestDownloads.te2);
            else if (type == LFtype.mvk)
                OldDate = DateTime.Parse(LatestDownloads.mvk);
            else if (type == LFtype.omu)
                OldDate = DateTime.Parse(LatestDownloads.omu);
            else
                throw new Exception("LFtype не распознана в методе GetFileFromID!");

            if (NewDate <= OldDate && Directory.Exists($"{OUT_PATH}\\{fileName}") && Directory.EnumerateFiles($"{OUT_PATH}\\{fileName}", "*.*", SearchOption.AllDirectories).Count() != 0)
            {
                Console.WriteLine("Файл \"" + fileName + "\" уже является актуальным.");
                return;
            }

            if (!Directory.Exists($"{OUT_PATH}\\{fileName}"))
                Directory.CreateDirectory($"{OUT_PATH}\\{fileName}");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);
            handler.Credentials = CredentialCache.DefaultCredentials;

            HttpClient client = new HttpClient(handler);

            var vars = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("id", fileId)
            };
            var content = new FormUrlEncodedContent(vars);

            if (JSONout)
            {
                StreamWriter sw = new StreamWriter(TEMP_PATH + $"\\{fileName}-{DateTime.Now.ToString("dd.MM.yyyy HH-mm")}.json");
                sw.Write(JsonConvert.SerializeObject(vars, Formatting.Indented));
                sw.Close();
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var zipName = TEMP_PATH + $"\\{fileName}-{DateTime.Now.ToString("dd.MM.yyyy HH-mm")}.zip";

            try
            {
                var response = await client.PostAsync($"{addressAPI}/suspect-catalogs/{fileName}", content);

                if (response?.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    throw new Exception("Ошибка 500. На сервере произошла ошибка, при попытке загрузить файл: \"" + fileName + "\"");

                BinaryWriter fw = new BinaryWriter(File.Open(zipName, FileMode.OpenOrCreate));
                BinaryReader fr = new BinaryReader(await response.Content.ReadAsStreamAsync());
                fw.Write(fr.ReadBytes((int)response.Content.Headers.ContentLength));
                fr.Close();
                fw.Close();

                ZipFile.ExtractToDirectory(zipName, $"{OUT_PATH}\\{fileName}", Encoding.GetEncoding(866));

                Console.WriteLine("Файл \n" + fileName + "\" успешно загружен и распакован по пути: \n" + $"{OUT_PATH}\\{fileName}");

                if (type == LFtype.te2)
                    LatestDownloads.te2 = NewDate.ToString();
                else if (type == LFtype.mvk)
                    LatestDownloads.mvk = NewDate.ToString();
                else if (type == LFtype.omu)
                    LatestDownloads.omu = NewDate.ToString();

                StreamWriter sw = new StreamWriter($"{TEMP_PATH}\\FilesDate.json");

                // Записываем шаблонные данные в файл
                sw.Write(JsonConvert.SerializeObject(LatestDownloads, Formatting.Indented));
                sw.Close();

                File.Delete(zipName);
            }
            catch (InvalidDataException ex)
            {
                File.Copy(zipName, $"{OUT_PATH}\\{fileName}\\{NewDate.ToString("dd.MM.yyyy - новый")}.xml", true);

                Console.WriteLine("Файл \n" + fileName + "\" успешно загружен и перемещен по пути: \n" + $"{OUT_PATH}\\{fileName}");

                if (type == LFtype.te2)
                    LatestDownloads.te2 = NewDate.ToString();
                else if (type == LFtype.mvk)
                    LatestDownloads.mvk = NewDate.ToString();
                else if (type == LFtype.omu)
                    LatestDownloads.omu = NewDate.ToString();

                StreamWriter sw = new StreamWriter($"{TEMP_PATH}\\FilesDate.json");

                // Записываем шаблонные данные в файл
                sw.Write(JsonConvert.SerializeObject(LatestDownloads, Formatting.Indented));
                sw.Close();

                File.Delete(zipName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось получить файл: \"" + fileName + "\" Произошла ошибка:");
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
