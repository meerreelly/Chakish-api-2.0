using System.Text;
using chakish_2._0_api.SignalRConroller;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace chakish_2._0_api;

public class KrolikMQ
{
    private IConnection _connection;
    private ConnectionFactory _factory;
    private IModel _channel;
    private readonly IHubContext<ChatHub> _hubContext;
    public KrolikMQ(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
        _factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
    }
    
    
    public void SendMessage(string message, Guid chatId, Guid userId)
    {
        using (var connection = _factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                var payload = new
                {
                    UserId = userId,
                    Message = message
                };
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));

                channel.BasicPublish(exchange: "",
                    routingKey: chatId.ToString(),
                    basicProperties: null,
                    body: body);
            }
        }
    }
    
    public void CreateQueue(Guid chatId)
    {
        using (var connection = _factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: chatId.ToString(),
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }
    }
    
    public void StartReceivingMessages(Guid chatId)
    {
        _channel.QueueDeclare(queue: chatId.ToString(),
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var jsonMessage = Encoding.UTF8.GetString(body);
            
            var payload = JsonConvert.DeserializeObject<dynamic>(jsonMessage);
            string userId = payload.UserId;
            string message = payload.Message;
            await _hubContext.Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", userId, message);
        };

        _channel.BasicConsume(queue: chatId.ToString(),
            autoAck: true,
            consumer: consumer);
    }
}