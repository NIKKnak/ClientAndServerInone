using System;
using System.Net;
using System.Threading.Tasks;
using ChatAppLib;
using Lesson6.Abstracts;
using Lesson6.Models;
using NetMQ;
using NetMQ.Sockets;

namespace Lesson6.Services
{
    public class Client
    {
        private readonly string _name;
        private readonly IMessageSource _messageSource;
        private IPEndPoint _remoteEndPoint;

        public Client(string name, string address, int port)
        {
            _name = name;
            _messageSource = new NetMQMessageSourceClient(address, port);
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

            Register(_remoteEndPoint);
        }

        private async Task ClientListener()
        {
            using (var subscriber = new SubscriberSocket())
            {
                subscriber.Connect($"tcp://{_remoteEndPoint.Address}:{_remoteEndPoint.Port}");
                subscriber.SubscribeToAnyTopic();

                while (true)
                {
                    try
                    {
                        var messageReceived = _messageSource.Receive();

                        Console.WriteLine($"Получено сообщение от {messageReceived.NickNameFrom}:");
                        Console.WriteLine(messageReceived.Text);

                        await Confirm(messageReceived, _remoteEndPoint);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка при получении сообщения: " + ex.Message);
                    }
                }
            }
        }

        private async Task Confirm(NetMessage message, IPEndPoint remoteEndPoint)
        {
            message.Command = Command.Confirmation;
            await _messageSource.SendAsync(message, remoteEndPoint);
        }

        private void Register(IPEndPoint remoteEndPoint)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            var message = new NetMessage() { NickNameFrom = _name, NickNameTo = null, Text = null, Command = Command.Register, EndPoint = ep };
            _messageSource.SendAsync(message, remoteEndPoint);
        }

        private async Task ClientSender()
        {
            while (true)
            {
                try
                {
                    Console.Write("Введите  имя получателя: ");
                    var nameTo = Console.ReadLine();

                    Console.Write("Введите сообщение и нажмите Enter: ");
                    var messageText = Console.ReadLine();

                    var message = new NetMessage() { Command = Command.Message, NickNameFrom = _name, NickNameTo = nameTo, Text = messageText };

                    await _messageSource.SendAsync(message, _remoteEndPoint);

                    Console.WriteLine("Сообщение отправлено.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
                }
            }
        }

        public async Task Start()
        {
            var listenerTask = ClientListener();
            var senderTask = ClientSender();

            await Task.WhenAll(listenerTask, senderTask);

            // Здесь можно добавить код для закрытия ресурсов, если необходимо
        }
    }
}
