using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

namespace Stateless_Consumer
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Stateless_Consumer : StatelessService
    {
        public Stateless_Consumer(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            //long iterations = 0;

            //while (true)
            //{
            //    cancellationToken.ThrowIfCancellationRequested();

            //    ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

            //    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            //}

            //while (true)
            //{
                //try
                //{
                    await Task.Run(() =>
                    {
                        try
                        {
                            var factory = new ConnectionFactory();
                            factory.UserName = "guest";
                            factory.Password = "guest";
                            factory.VirtualHost = "/";
                            factory.HostName = "52.175.209.90";
                            factory.Port = 5672;

                            using (var connection = factory.CreateConnection())
                            {
                                ServiceEventSource.Current.ServiceMessage(this.Context, "RabbitMQ Broker Connection Established.");
                                using (var channel = connection.CreateModel())
                                {
                                    channel.ExchangeDeclare(
                                    exchange: "orders-exchange",
                                    type: "topic",
                                    durable: true,
                                    autoDelete: false,
                                    arguments: null);

                                    var queueName = "order-created";//channel.QueueDeclare().QueueName;
                                    var routingKey = "orders.created";

                                    ServiceEventSource.Current.ServiceMessage(this.Context, "Queue Name:" + queueName);

                                    channel.QueueBind(
                                    exchange: "orders-exchange",
                                    queue: queueName,
                                    routingKey: routingKey);

                                    ServiceEventSource.Current.ServiceMessage(this.Context, "Queue Bound:" + queueName);

                                    var message = string.Empty;
                                    var consumer = new EventingBasicConsumer(channel);

                                    consumer.Received += (model, ea) =>
                                    {
                                        try
                                        {
                                            var body = ea.Body;
                                            message = Encoding.UTF8.GetString(body);
                                            var brokeredMessage = JsonConvert.DeserializeObject<BrokeredMessage>(message);
                                            ServiceEventSource.Current.ServiceMessage(this.Context, "Message Received: " + brokeredMessage.Id);
                                        }
                                        catch (Exception ex)
                                        {
                                            ServiceEventSource.Current.ServiceMessage(this.Context, "Exception Occurred:" + ex.ToString());
                                        }
                                    };

                                    channel.BasicConsume(
                                    queue: queueName,
                                    noAck: true,
                                    consumer: consumer);

                                    //run forever
                                    ServiceEventSource.Current.ServiceMessage(this.Context, "Listening for messages...");
                                    while (true)
                                    { }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ServiceEventSource.Current.ServiceMessage(this.Context, "Exception Occurred:" + ex.ToString());
                        }
                    }).ConfigureAwait(false);
                //}
                //catch (Exception ex)
                //{
                //    ServiceEventSource.Current.ServiceMessage(this.Context, "Exception Occurred:" + ex.ToString());
                //}
            //}
        }
    }
}
