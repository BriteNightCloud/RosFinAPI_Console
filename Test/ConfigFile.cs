using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Test
{
    class Config
    {
        public Config() { }

        public string Login = "";
        public string Password = "";
        public string CertThumbprint = "";
        public string TempPath = "";
        public string XMLOutPath = "";

        public static Config Parse(dynamic obj)
        {
            Config config = new Config();
            config.Login = obj?.Login;
            config.Password = obj?.Password;
            config.CertThumbprint = obj?.CertThumbprint;
            config.TempPath = obj?.TempPath;
            config.XMLOutPath = obj?.XMLOutPath;
            return config;
        }
    }
    partial class Program
    {
        static Config GetConfig()
        {
            try
            {
                // Проверяем наличие файла и открываем его для чтения
                StreamReader sr = new StreamReader($"{Assembly.GetExecutingAssembly().Location}\\..\\config.json");

                // Если он есть и открылся, считываем из него данные
                var tmp = Config.Parse(JsonConvert.DeserializeObject(sr.ReadToEnd()));

                sr.Close();

                return tmp;
            }
            catch (System.IO.FileNotFoundException)
            {
                // Задаем шаблон файла
                var defaultCfg = new Dictionary<string, string>
                {
                    { "Login", "" },
                    { "Password", "" },
                    { "CertThumbprint", "" },
                    { "TempPath", @"C:\RosFinMonitoring" },
                    { "XMLOutPath", @"C:\RosFinMonitoring\XMLout" }
                };

                // Создаем новый файл и открываем его для записи
                StreamWriter sw = new StreamWriter($"{Assembly.GetExecutingAssembly().Location}\\..\\config.json");

                // Записываем шаблонные данные в файл
                sw.Write(JsonConvert.SerializeObject(defaultCfg, Newtonsoft.Json.Formatting.Indented));
                sw.Close();

                Console.WriteLine("Был создан новый файл config.json, т.к. не удалось обнаружить предыдущий!");

                Environment.Exit(0);
                return null;
            }
        }
    }
}
