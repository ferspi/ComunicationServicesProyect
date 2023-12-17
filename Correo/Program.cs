using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Correo;
class Program
{
    static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchange: "Compras", type: ExchangeType.Fanout);

            var queueName = channel.QueueDeclare().QueueName; //Declaro una queue por defecto

            channel.QueueBind(queue: queueName,
                              exchange: "Compras",
                              routingKey: "");

            Console.WriteLine(" [*] Esperando por Compras.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Compra compra = JsonSerializer.Deserialize<Compra>(message);

                Console.WriteLine(" [x] Enviando correo a "+ compra.Usuario+" por comprar producto "+ compra.NombreProducto);
                await Task.Delay(5000);
            };
            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }

}