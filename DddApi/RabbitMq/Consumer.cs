using System.Text;
using DddApi.Models;
using DddApi.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DddApi.RabbitMq;
public class RabbitMqConsumer<T> where T : class
{
    private readonly string _hostName;
    private readonly string _queueName;
    private readonly IRegiaoService _regiaoService;

    public RabbitMqConsumer(string hostName, string queueName, IRegiaoService regiaoService)
    {
        _hostName = hostName;
        _queueName = queueName;
        _regiaoService = regiaoService;
    }

    public void StartConsumer()
    {
        var factory = new ConnectionFactory() { HostName = _hostName };
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
        if (message.Payload is Regiao regiao)
        {
            _regiaoService.UpdateCache(message.EventType, regiao);
        }
    }
}
