using Lesson6.Abstracts;
using Lesson6.Models;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lesson6.Services
{
    internal class NetMQMessageSource : IMessageSource
    {
        private readonly SubscriberSocket _subscriberSocket;
        private readonly IPEndPoint _ep;

        public NetMQMessageSource(IPEndPoint ep)
        {
            _subscriberSocket = new SubscriberSocket();
            _ep = ep;
        }

        public NetMessage Receive(ref IPEndPoint ep)
        {
            byte[] data = _subscriberSocket.ReceiveFrameBytes();
            string str = Encoding.UTF8.GetString(data);
            return NetMessage.DeserializeMessgeFromJSON(str) ?? new NetMessage();
        }

        public void Subscribe(string topic)
        {
            _subscriberSocket.Subscribe(topic);
        }
    }

    internal class NetMQMessageSourceClient : IMessageSourceClient
    {
        private readonly RequestSocket _requestSocket;
        private readonly IPEndPoint _ep;

        public NetMQMessageSourceClient(IPEndPoint ep)
        {
            _requestSocket = new RequestSocket();
            _ep = ep;
        }

        public void Connect()
        {
            _requestSocket.Connect(_ep);
        }

        public void Disconnect()
        {
            _requestSocket.Disconnect(_ep);
        }

        public void Send(NetMessage message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message.SerialazeMessageToJSON());
            _requestSocket.SendFrame(buffer);
        }

        public NetMessage Receive()
        {
            byte[] data = _requestSocket.ReceiveFrameBytes();
            string str = Encoding.UTF8.GetString(data);
            return NetMessage.DeserializeMessgeFromJSON(str) ?? new NetMessage();
        }
    }
}