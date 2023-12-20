using System.Text;
using System.Threading.Tasks;
using Lesson6.Models;
using NetMQ;
using NetMQ.Sockets;

namespace ChatAppLib
{
    public class NetMQMessageSource : IMessageSource
    {
        private readonly string _address;
        private readonly int _port;
        private readonly NetMQSocket _subscriber;

        public NetMQMessageSource(string address = "tcp://127.0.0.1", int port = 5527)
        {
            _address = address;
            _port = port;

            _subscriber = new SubscriberSocket();
            _subscriber.Connect($"{_address}:{_port}");
            _subscriber.SubscribeToAnyTopic();
        }

        public NetMessage Receive()
        {
            string message = _subscriber.ReceiveFrameString();
            return NetMessage.DeserializeMessgeFromJSON(message) ?? new NetMessage();
        }
    }

    public class NetMQMessageSourceClient : IMessageSourceClient
    {
        private readonly string _address;
        private readonly int _port;
        private readonly NetMQSocket _publisher;

        public NetMQMessageSourceClient(string address = "tcp://127.0.0.1", int port = 5527)
        {
            _address = address;
            _port = port;

            _publisher = new PublisherSocket();
            _publisher.Bind($"{_address}:{_port}");
        }

        public async Task SendAsync(NetMessage message)
        {
            string serializedMessage = message.SerialazeMessageToJSON();
            await _publisher.SendFrameAsync(serializedMessage);
        }
    }
}
