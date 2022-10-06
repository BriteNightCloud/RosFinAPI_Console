using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    partial class Program
    {
        static X509Certificate2 getCert(string thumbprint)
        {
            Console.WriteLine();
            // Формуруем коллекцию отображаемых сертификатов.
            X509Store store = new X509Store("MY", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection collection =
                (X509Certificate2Collection)store.Certificates;

            X509Certificate2Collection found =
                store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            X509Certificate2 cert;

            if (found.Count > 0)
            {
                Console.WriteLine("Сертификат найден: " + thumbprint);
                cert = found[0];
            }
            else
            {
                Console.WriteLine("Не найден сертификат с отпечатоком: " + thumbprint);
                Console.WriteLine("Убедитесь что все необходимые сертификаты установлены в хранилища локального компьютера! (не пользователя)");
                return null; 
            }

            try
            {
                // Получаем секретный ключ соответствующий данному сертификату.
                AsymmetricAlgorithm asym = cert.PrivateKey;
                if (asym == null)
                {
                    Console.WriteLine("Нет секретного ключа соответствующего искомому сертификату.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Нет секретного ключа соответствующего искомому сертификату.");
                Console.WriteLine(ex.Message); 
                return null;
            }

            Console.WriteLine("Найден секретный ключ, соответствующего искомому сертификату.");
            return cert;
        }
    }
}
