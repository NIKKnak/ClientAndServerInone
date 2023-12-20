using ChatApp;
using System.Net;

internal class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            var s = new Server<IPEndPoint>(new NetMQMessageSourceServer());
            await s.Start();
        }
        else
        if (args.Length == 1)
        {
            var c = new Client<IPEndPoint>(new NetMQMessageSourceClient(), args[0]);
            await c.Start();
        }
        else
        {

            Console.WriteLine("Для запуска сервера введите ник-нейм как параметр запуска приложения");
            Console.WriteLine("Для запуска клиента введите ник-нейм и IP сервера как параметры запуска приложения");
        }

        Console.ReadKey(true);
    }
}