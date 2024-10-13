using DddApi.Models;

namespace DddApi.RabbitMq;

public interface IMessageProducer
{
    void SendMessageToQueue<T>(Message<T> bodyMessage) where T : class;
}
