using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lesson6.Abstracts;
using Lesson6.Models;
using NetMQ;
using NetMQ.Sockets;

namespace Lesson6.Services
{
    public class Server
    {
        private readonly Dictionary<string, IPEndPoint> _clients = new Dictionary<string, IPEndPoint>();
        private readonly IMessageSource _messageSource;

        public Server(IMessageSource messageSource)
        {
            _messageSource = messageSource;
        }

        private async Task Register(NetMessage message)
        {
            Console.WriteLine($" Ник = {message.NickNameFrom}");

            if (_clients.TryAdd(message.NickNameFrom, message.EndPoint))
            {
                using (var context = new ChatContext())
                {
                    context.Users.Add(new User() { FullName = message.NickNameFrom });
                    await context.SaveChangesAsync();
                }
            }
        }

        private async Task RelyMessage(NetMessage message)
        {
            if (_clients.TryGetValue(message.NickNameTo, out IPEndPoint ep))
            {
                int? id = 0;
                using (var ctx = new ChatContext())
                {
                    var fromUser = ctx.Users.First(x => x.FullName == message.NickNameFrom);
                    var toUser = ctx.Users.First(x => x.FullName == message.NickNameTo);
                    var msg = new Message { UserFrom = fromUser, UserTo = toUser, IsSent = false, Text = message.Text };
                    ctx.Messages.Add(msg);

                    ctx.SaveChanges();

                    id = msg.MessageId;
                }

                message.Id = id;

                await _messageSource.SendAsync(message, ep);

                Console.WriteLine($"От = {message.NickNameFrom} Кому = {message.NickNameTo}");
            }
            else
            {
                Console.WriteLine("Пользователь не найден.");
            }
        }

        private async Task ConfirmMessageReceived(int? id)
        {
            Console.WriteLine("id = " + id);

            using (var ctx = new ChatContext())
            {
                var msg = ctx.Messages.FirstOrDefault(x => x.MessageId == id);

                if (msg != null)
                {
                    msg.IsSent = true;
                    await ctx.SaveChangesAsync();
                }
            }
        }

        private async Task ProcessMessage(NetMessage message)
        {
            switch (message.Command)
            {
                case Command.Register: await Register(message); break;
                case Command.Message: await RelyMessage(message); break;
                case Command.Confirmation: await ConfirmMessageReceived(message.Id); break;
            }
        }

        public async Task Start()
        {
            Console.WriteLine("Сервер ожидает сообщения ");

            using (var subscriber = new SubscriberSocket())
            {
                subscriber.Bind("12345"); 

                while (true)
                {
                    try
                    {
                        var message = _messageSource.Receive();
                        Console.WriteLine(message.ToString());
                        await ProcessMessage(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }
    }
}
