using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Test
{
    partial class Program
    {
        static void GetCatalogID(string catalogName, X509Certificate2 cert, string token, out string ID, out DateTime Date)
        {
            Console.WriteLine();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = (HttpWebRequest)WebRequest.Create($"{addressAPI}/suspect-catalogs/{catalogName}");
            request.Credentials = CredentialCache.DefaultCredentials;
            request.ClientCertificates.Add(cert);

            request.Method = "POST";
            request.ContentLength = 0;

            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Bearer " + token);

            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                var sr = new StreamReader(response.GetResponseStream());

                dynamic resp = JsonConvert.DeserializeObject(sr.ReadToEnd());
                Console.WriteLine("Информация о перечне \"" + catalogName + "\":");
                Console.WriteLine(resp);

                if (JSONout)
                {
                    StreamWriter sw = new StreamWriter(TEMP_PATH + $"\\{catalogName}-{response.LastModified.ToString("dd.MM.yyyy HH-mm")}.json");
                    sw.Write(JsonConvert.SerializeObject(resp, Formatting.Indented));
                    sw.Close();
                }

                if (resp?.idXml == null)
                {
                    Console.WriteLine("Не удалось получить сведения о перечне: " + catalogName);
                    ID = null;
                    Date = new DateTime();
                }
                else
                {
                    ID = resp.idXml;
                    Date = resp.date;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось получить сведения о перечне: \"" + catalogName + "\" Произошла ошибка:");
                Console.WriteLine(ex.Message);
                ID = null;
                Date = new DateTime();
            }
        }
    }
}
