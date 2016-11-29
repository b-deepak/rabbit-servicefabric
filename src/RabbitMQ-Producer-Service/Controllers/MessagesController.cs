using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MessagingService2.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("dequeue")]
        public string Get(int id)
        {
            //return "value";
            var factory = new ConnectionFactory();
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.VirtualHost = "/";
            factory.HostName = "52.175.209.90";
            factory.Port = 5672;

            using (var connection = factory.CreateConnection())
            {
                //Debug.WriteLine("RabbitMQ Broker Connection Established.");
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

                    //Debug.WriteLine("Queue Name:" + queueName);

                    channel.QueueBind(
                        exchange: "orders-exchange",
                        queue: queueName,
                        routingKey: routingKey);

                    //Debug.WriteLine("Queue Bound:" + queueName);

                    var message = string.Empty;
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received +=
                        (model, ea) => 
                        {
                            var body = ea.Body;
                            message = Encoding.UTF8.GetString(body);
                            //Debug.WriteLine("Message Received:" + message);
                        };
                    

                    channel.BasicConsume(
                        queue: queueName,
                        noAck: true,
                        consumer: consumer);

                    //wont see anything here as the code will exit after this statement. The message is consumed async via Received event which may happen at any time.
                    return message;
                }
            }
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]MessageDto messageDto)
        {
            var brokeredMessage = new BrokeredMessage
            {
                Id = Guid.NewGuid(),
                Payload = JsonConvert.SerializeObject(messageDto)
            };

            try
            {
                await Task.Run(() =>
                {
                    var factory = new ConnectionFactory();
                    factory.UserName = "guest";
                    factory.Password = "guest";
                    factory.VirtualHost = "/";
                    factory.HostName = "52.175.209.90";
                    factory.Port = 5672;

                    using (var connection = factory.CreateConnection())
                    {
                        using (var channel = connection.CreateModel())
                        {
                            channel.ExchangeDeclare(
                                exchange: "orders-exchange",
                                type: "topic",
                                durable: true,
                                autoDelete: false,
                                arguments: null);

                            var routingKey = "orders.created";

                            var messageProperties = channel.CreateBasicProperties();
                            messageProperties.DeliveryMode = 2; //persistent
                        messageProperties.ContentType = "application/json";
                            messageProperties.CorrelationId = brokeredMessage.Id.ToString();
                        //messageProperties.Expiration = 
                        //messageProperties.Priority =
                        //and many more

                        channel.BasicPublish(
                                exchange: "orders-exchange",
                                routingKey: routingKey,
                                basicProperties: messageProperties,
                                body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(brokeredMessage)));
                        }
                    }
                }).ConfigureAwait(false);

                return Created("http://localhost:5000/api/messages", brokeredMessage.Id);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
