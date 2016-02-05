#Diagnostics

Diagnosing, performance tuning, and logging are a part of everyday programming. The windows operating system provides a low-overhead and scalable tracing system named [Event Tracing for Windows (ETW)](https://msdn.microsoft.com/en-us/library/ee517330(v=vs.110).aspx). 

Originally released in .NET 4, Event Tracing for Windows (ETW) has two core components. An `EventSource` enables high performance semantic logging of events. An event may consist of a simple trace message, a performance statistic, or a more complex exception log. While an `EventListener` allows subscription to events. Normally, a production application will not have a need to implement an Event Listener. This is handled by an out of process application. I have included two listeners in the library which are used for prototyping or testing ETW events. For production logging, I use the out of process [Azure Diagnostics plugin](https://azure.microsoft.com/en-us/documentation/articles/cloud-services-dotnet-diagnostics/).

The [Enterprise library Semantic Logging Application Block](https://msdn.microsoft.com/en-us/library/dn774985(v=pandp.20).aspx#sec10) contains the `EventSourceAnalyzer` class which makes writing unit tests to validate event sources extremely easy. However, I found it lacking when it comes to testing a custom component (ie. Http Message Handler) which lead me to create my own listeners. Yes this does break the rule of a unit test should test only one thing! For example, sometimes when an exception is thrown internal information needs to be logged while a more generic exception is rethrown. This is especially true when passing application boundaries. A unit test suite on a given component should validate both "happy paths" and negative paths. I can't count how many times I have seen a null reference exception propagate due to an exception being caught but the catch block contains a bug. The hardest bugs to track down are the ones which are hidden by other bugs. Unit tests are your friends. Take time and get to know them. They will help you when you need it the most.

The following is an example unit test which validates both the http response and the ETW event:

        [TestMethod]
        public void TestRequestSucceededAuditEnabledNoContentOrHeaders()
        {
            var listener = new CollectionEtwListener();
            listener.EnableEvents(AuditEventSource.Log, EventLevel.Informational);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            var auditHandler = new EtwAuditHandler(true)
            {
                InnerHandler = new TestHandler()
            };

            var client = new HttpClient(auditHandler);
            var result = client.SendAsync(httpRequestMessage).Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(1, listener.Events.Count);

            var etwData = listener.Events.First();
            Assert.AreEqual(EventLevel.Informational, etwData.Level);
            Assert.AreEqual(2, etwData.Payload.Count);

            var requestData = (string)etwData.Payload[0];
            Assert.AreEqual("Request Method: GET\r\nRequest Uri: http://test.com/\r\nRequest Headers: \r\n\r\nRequest Content is Empty\r\n", requestData);

            var responseData = (string)etwData.Payload[1];
            Assert.AreEqual("Response Status: OK OK\r\nResponse Headers: \r\n\r\nResponse Content is Empty\r\n", responseData);
        }

This unit test is not following [Data-Driven](https://msdn.microsoft.com/en-us/library/ms182527.aspx) or [Behavior driven development](http://www.specflow.org/) best practices.
