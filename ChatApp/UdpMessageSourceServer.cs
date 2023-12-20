using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatApp;
using ChatCommon.Abstractions;
using ChatCommon.Models;
using Lesson6.Models;
using NetMQ;
using NetMQ.Sockets;

namespace ChatApp
{
    public class NetMQMessageSourceServer : IMessageSourceServer<IPEndPoint>
    {
        private readonly PublisherSocket _publisher;
        private readonly SubscriberSocket _subscriber;

        public NetMQMessageSourceServer()
        {
            _publisher = new PublisherSocket();
            _subscriber = new SubscriberSocket();
        }

        public IPEndPoint CopyEndpoint(IPEndPoint ep)
        {
            return new IPEndPoint(ep.Address, ep.Port);
        }

        public IPEndPoint CreateEndpoint()
        {
            return  new IPEndPoint(IPAddress.Any, 0);
        }

        public NetMessage Receive(ref IPEndPoint ep)
        {
            string str = _subscriber.ReceiveFrameString();
            return NetMessage.DeserializeMessgeFromJSON(str) ?? new NetMessage();
        }

        public async Task SendAsync(NetMessage message, IPEndPoint ep)
        {
            _publisher.SendMoreFrame("").SendFrame(message.SerialazeMessageToJSON());
            await Task.CompletedTask;
        }

        public void Connect(IPEndPoint endpoint)
        {
            _subscriber.Connect(endpoint.ToString());
            _subscriber.SubscribeToAnyTopic();
        }

        public void Bind(IPEndPoint endpoint)
        {
            _publisher.Bind(endpoint.ToString());
        }

    }
}
