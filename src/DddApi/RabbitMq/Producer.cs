using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using DddApi.Models;

namespace DddApi.RabbitMq;
public class Producer : IMessageProducer
{
    private readonly string _queueName;
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _user;
    private readonly string _password;
    public Producer(string hostName, string queueName, int port, string user, string password)
    {
        _queueName = queueName;
        _hostName = hostName;
        _port = port;
        _user = user;
        _password = password;
    }

    public void SendMessageToQueue<T>(Message<T> bodyMessage) where T : class
    {
        var factory = new ConnectionFactory() { HostName = _hostName, Port = _port, UserName = _user, Password = _password };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var messageJson = JsonSerializer.Serialize(bodyMessage);
            var body = Encoding.UTF8.GetBytes(messageJson);

            channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}
