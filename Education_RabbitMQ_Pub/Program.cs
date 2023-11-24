using MassTransit;
using RabbitMQ.Client;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Education_RabbitMQ_Pub
{
    class Program
    {
        public enum LogNames
        {
            Critical = 1,
            Error = 2,
            Warning = 3,
            Info = 4
        }
        static void Main(string[] args)
        {
            Main_Default(args);
            // Main_Queue(args);
            //Main_Fanout(args);
            //  Main_Direct(args);
            // Main_Topic(args);
            //   Main_Header(args);
            //Main_Mass(args);
        }
        static void Main_Default(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("hello-queue", false, false, false);

            string message = "hello world";
            var messageBody = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);

            Console.WriteLine("Mesajınız gönderilmiştir");
            Console.ReadLine();
        }

        static void Main_Queue(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("hello-queue", false, false, false);

            Enumerable.Range(0, 100).ToList().ForEach(x =>
            {
                string message = $"Messsage{x}";
                var messageBody = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);
                Console.WriteLine($"Mesajınız gönderilmiştir:{message}");
            });

            Console.ReadLine();
        }

        static void Main_Fanout(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("logs-fanout", durable: true, type: ExchangeType.Fanout);


            Enumerable.Range(0, 50).ToList().ForEach(x =>
            {
                string message = $"Messsage{x}";
                var messageBody = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish("logs-fanout", "", null, messageBody);

                Console.WriteLine($"Mesajınız gönderilmiştir:{message}");
            });

            Console.ReadLine();
        }

        static void Main_Direct(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("logs-direct", durable: true, type: ExchangeType.Direct);

            Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
            {
                var routeKey = $"route-{x}";
                var queueName = $"direct-queue-{x}";

                channel.QueueDeclare(queueName, true, false, false);
                channel.QueueBind(queueName, "logs-direct", routeKey, null);

            });

            Enumerable.Range(0, 50).ToList().ForEach(x =>
            {
                LogNames log = (LogNames)new Random().Next(1, 5);

                string message = $"log-type-{log}";
                var messageBody = Encoding.UTF8.GetBytes(message);

                var routeKey = $"route-{log}";
                channel.BasicPublish("logs-direct", routeKey, null, messageBody);

                Console.WriteLine($"Log gönderilmiştir:{message}");
            });

            Console.ReadLine();
        }

        static void Main_Topic(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("logs-topic", durable: true, type: ExchangeType.Topic);


            Enumerable.Range(0, 50).ToList().ForEach(x =>
            {
                LogNames log1 = (LogNames)new Random().Next(1, 5);
                LogNames log2 = (LogNames)new Random().Next(1, 5);
                LogNames log3 = (LogNames)new Random().Next(1, 5);

                var routeKey = $"{log1}.{log2}.{log3}";

                string message = $"log-type-{log1}-{log2}-{log3}";
                var messageBody = Encoding.UTF8.GetBytes(message);


                channel.BasicPublish("logs-topic", routeKey, null, messageBody);

                Console.WriteLine($"Log gönderilmiştir:{message}");
            });

            Console.ReadLine();
        }

        static void Main_Header(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://vrdzegiv:H9m2mJ6y7DP0kn5u_WrrOzqM9LsBb7ED@toad.rmq.cloudamqp.com/vrdzegiv");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);

            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("format", "pdf");
            headers.Add("shape2", "a4");

            var properties = channel.CreateBasicProperties();
            properties.Headers = headers;

            var messageBody = Encoding.UTF8.GetBytes("header mesajım");
            channel.BasicPublish("header-exchange", string.Empty, properties, messageBody);

            Console.WriteLine($"Mesajınız gönderilmiştir");

            Console.ReadLine();
        }
        public class Message : IMessage
        {
            public string Text { get; set; }
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
            });

            var sentoUri = new Uri($"{rabbitMqUri}/{queue}");
            var endPoint = await bus.GetSendEndpoint(sentoUri);

            await Task.Run(async () =>
            {
                Console.Write("Mesaj yaz : ");
                Message message = new Message
                {
                    Text = Console.ReadLine()
                };
                //if (message.Text.ToUpper() == "C")
                //    break;
                await endPoint.Send<IMessage>(message);
                Console.WriteLine("");
            });
            Console.ReadLine();
        }
    }
}
