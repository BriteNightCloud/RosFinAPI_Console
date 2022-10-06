using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    partial class Program
    {
        static void GetFilesDate()
        {
            try
            {
                // Проверяем наличие файла и открываем его для чтения
                StreamReader sr = new StreamReader($"{TEMP_PATH}\\FilesDate.json");

                // Если он есть и открылся, считываем из него данные
                LatestFiles = JsonConvert.DeserializeObject(sr.ReadToEnd());

                sr.Close();
            }
            catch (System.IO.FileNotFoundException)
            {
                // Задаем шаблон файла
                var defaultCfg = new Dictionary<string, string>
                {
                    { "te2", new DateTime().ToString() },
                    { "mvk", new DateTime().ToString() },
                    { "omu", new DateTime().ToString() }
                };

                // Создаем новый файл и открываем его для записи
                StreamWriter sw = new StreamWriter($"{TEMP_PATH}\\FilesDate.json");

                // Записываем шаблонные данные в файл
                sw.Write(JsonConvert.SerializeObject(defaultCfg, Formatting.Indented));
                sw.Close();

                GetFilesDate();
            }
        }
    }
}
