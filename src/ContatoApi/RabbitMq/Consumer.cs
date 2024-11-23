using System.Text;
using ContatoApi.Models;
using ContatoApi.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ContatoApi.RabbitMq;

public class RabbitMqConsumer<T> where T : class
{
    private readonly string _queueName;
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _user;
    private readonly string _password;
    private readonly IDddService _dddService;

    public RabbitMqConsumer(string hostName, string queueName, int port, string user, string password, IDddService dddService)
    {
        _hostName = hostName;
        _queueName = queueName;
        _port = port;
        _user = user;
        _password = password;
        _dddService = dddService;
    }

    public void StartConsumer()
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

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject<Message<T>>(Encoding.UTF8.GetString(body));

                if (message != null)
                {
                    ProcessMessage(message);
                }
                else
                {
                    Console.WriteLine("Falha ao deserializar a mensagem.");
                }
            };

            channel.BasicConsume(queue: _queueName,
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");
            while (true) ; // Mantém o consumidor em execução
        }
    }

    private void ProcessMessage(Message<T> message)
    {
        Console.WriteLine("Mensagem recebida da fila de DDD.");
        if (message.Payload is Ddd ddd)
        {
            _ = _dddService.UpdateCache(message.EventType, ddd);
        }
    }
}