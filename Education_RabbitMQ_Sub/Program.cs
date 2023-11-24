using MassTransit;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Education_RabbitMQ_Sub
{
    class Program
    {
        static void Main(string[] args)
        {
            Main_Default(args);
            //Main_Queue(args);
            // Main_Fanout(args);
            // Main_Direct(args);
            // Main_Topic(args);
            //Main_Header(args);
           // Main_Mass(args);
        }
        static void Main_Default(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("hello-queue", false, false, false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume("hello-queue", true, consumer);

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {

                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine("Gelen mesaj:" + message);
            };

            Console.ReadLine();
        }

        static void Main_Queue(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);


            channel.QueueDeclare("hello-queue", false, false, false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume("hello-queue", true, consumer);

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {

                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Thread.Sleep(1500);
                Console.WriteLine("Gelen mesaj:" + message);

              //  channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }

        static void Main_Fanout(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");
            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, "logs-fanout", "", null);
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
            Console.WriteLine("Loglar dinleniyor");
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Thread.Sleep(1500);
                Console.WriteLine("Gelen mesaj:" + message);
            };
            Console.ReadLine();
        }

        static void Main_Direct(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");
            
            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
          
             channel.BasicQos(0, 1, false);

            var queueName = "direct-queue-Error";

            Console.WriteLine("Loglar dinleniyor");

            var consumer = new EventingBasicConsumer(channel);
            
            
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Thread.Sleep(1500);
                Console.WriteLine("Gelen mesaj:" + message);
            };
            channel.BasicConsume(queueName, true, consumer);

            Console.ReadLine();
        }

        static void Main_Topic(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);

            var queueName = channel.QueueDeclare().QueueName;

            var routeKey = "info.#";

            channel.QueueBind(queueName, "logs-topic", routeKey);

            var consumer = new EventingBasicConsumer(channel);

            channel.BasicConsume(queueName, true, consumer);

            Console.WriteLine("Loglar dinleniyor");


            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Thread.Sleep(1500);
                Console.WriteLine("Gelen mesaj:" + message);
            };
           
            Console.ReadLine();
        }
        public class MessageConsumer : IConsumer<IMessage>
        {
            public async Task Consume(ConsumeContext<IMessage> context)
                => Console.WriteLine($"Gelen mesaj : {context.Message.Text}");
        }
        static async Task Main_Mass(string[] args)
        {
            string rabbitMqUri = "amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv";
            string queue = "test-queue";

            string userName = "vrdzegiv";
            string password = "H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED";


            var bus = Bus.Factory.CreateUsingRabbitMq(factory =>
            {
                factory.Host(rabbitMqUri, configurator =>
                {

                    configurator.Username(userName);
                    configurator.Password(password);
                });

                factory.ReceiveEndpoint(queue, endpoint => endpoint.Consumer<MessageConsumer>());
            });
            await bus.StartAsync();
            Console.ReadLine();
            await bus.StopAsync();
        }
    }
}
