using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    partial class Program
    {
        static string AuthGetToken(string userName, string password, X509Certificate2 cert)
        {
            Console.WriteLine();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{addressAPI}/authenticate");

            var postData = $"{{ \"userName\": \"{userName}\", \"password\": \"{password}\" }}";
            var data = Encoding.UTF8.GetBytes(postData);

            // Устанавливаем сведения для проверки подлинности
            request.Credentials = CredentialCache.DefaultCredentials;
            request.ClientCertificates.Add(cert);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            try
            {
                var stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);

                var response = (HttpWebResponse)request.GetResponse();

                string token;

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    dynamic resp = JsonConvert.DeserializeObject(sr.ReadToEnd());
                    token = resp?.value?.accessToken;
                }

                if (token == null)
                    return null;

                Console.WriteLine("Токен получен: \n" + token);
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
