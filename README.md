# Http Client Extensions Library

As an IoT developer of Azure microservices, I often find myself extending `HttpClient`. I'm building this library based on my experience of cloud architecture, design patterns, security, testing, and code reuse.

Diagnostics:

Event Tracing for Windows (ETW) has two core components. An `EventSource` enables high performance semantic logging of events. While an `EventListener` allows subscription to events. I have included two listeners which are used for prototyping or testing ETW. For production logging, I use the out of process Azure Diagnostics plugin.
 
Security:

 
