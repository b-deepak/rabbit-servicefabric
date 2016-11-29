# Projects

## RabbitMQ-Producer-Service (MessagingService-RabbitClient)
This project is a .NET Core 1.1 API that acts as producer.

Sample Curl Call:

```
curl -X POST -H "Content-Type: application/json" -H "Cache-Control: no-cache" -d '{
	"clientMessageId": "1",
	"payload": "Hello World 1!"
}' "http://localhost:5000/api/messages"
```

## ServiceFabric_RabbitMQ_Consumer
This application runs in Azure ServiceFabric's Local Cluster and comprises of the service listed below.

## Stateless-Consumer
As the name suggests, this application is a stateless service that acts as the consumer. It is based on Service Fabric's Reliable Services application model. Currently, the Reliable Services model does not support .NET Core framework. As a result, this consumer is targeting full .NET framework (4.5.2 specifically). If you have a .NET Core application, then you should follow the 'Guest Executable' application model in Service Fabric.

# References

* [Service Fabric Learning Path](https://azure.microsoft.com/en-us/documentation/learning-paths/service-fabric/)
* [Service Fabric Application Model](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-application-model)
* [.NET Core support](https://blogs.msdn.microsoft.com/dotnet/2016/10/13/hosting-net-core-services-on-service-fabric/)

