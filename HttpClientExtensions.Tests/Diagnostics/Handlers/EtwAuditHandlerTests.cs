

namespace HttpClientExtensions.Tests.Diagnostics.Handlers
{
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using HttpClientExtensions.Diagnostics;
    using HttpClientExtensions.Diagnostics.Handlers;
    using HttpClientExtensions.Tests.TestHelpers;
    using HttpClientExtensions.Utilities.Etw.Listeners;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Unit test Etw Audit Handler
    /// </summary>
    [TestClass]
    public class EtwAuditHandlerTests
    {
        [TestMethod]
        public void TestRequestSucceededAuditDisabled()
        {
            var listener = new CollectionEtwListener();
            listener.EnableEvents(AuditEventSource.Log, EventLevel.Informational);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            var auditHandler = new EtwAuditHandler()
            {
                InnerHandler = new TestHandler()
            };

            var client = new HttpClient(auditHandler);
            var result = client.SendAsync(httpRequestMessage).Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(0, listener.Events.Count);
        }

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

        [TestMethod]
        public void TestRequestSucceededAuditEnabled()
        {
            var listener = new CollectionEtwListener();
            listener.EnableEvents(AuditEventSource.Log, EventLevel.Informational);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            httpRequestMessage.Content = new StringContent("Sending Test Data", Encoding.UTF8, "text/plain");
            httpRequestMessage.Headers.Add("RequestTestHeader", "RequestValue");
            var auditHandler = new EtwAuditHandler(true)
            {
                InnerHandler = new TestHandler((c,r) => {
                    return Task.Factory.StartNew(
                            () =>
                            {
                                var response = new HttpResponseMessage(HttpStatusCode.OK);
                                response.Headers.Add("TestHeader", "TestValue");
                                response.Content = new StringContent("Returning Test Data", Encoding.UTF8, "text/plain");
                                return response;
                            });
                })
            };

            var client = new HttpClient(auditHandler);
            var result = client.SendAsync(httpRequestMessage).Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(1, listener.Events.Count);

            var etwData = listener.Events.First();
            Assert.AreEqual(EventLevel.Informational, etwData.Level);
            Assert.AreEqual(2, etwData.Payload.Count);

            var requestData = (string)etwData.Payload[0];
            Assert.AreEqual("Request Method: GET\r\nRequest Uri: http://test.com/\r\nRequest Headers: \r\nRequestTestHeader: RequestValue; \r\n\r\n\r\nRequest Content:\r\nSending Test Data\r\n", requestData);

            var responseData = (string)etwData.Payload[1];
            Assert.AreEqual("Response Status: OK OK\r\nResponse Headers: \r\nTestHeader: TestValue; \r\n\r\n\r\nResponse Content:\r\nReturning Test Data\r\n", responseData);
        }

        [TestMethod]
        public void TestRequestFailure()
        {
            var listener = new CollectionEtwListener();
            listener.EnableEvents(AuditEventSource.Log, EventLevel.Informational);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            httpRequestMessage.Content = new StringContent("Sending Test Data", Encoding.UTF8, "text/plain");
            httpRequestMessage.Headers.Add("RequestTestHeader", "RequestValue");
            var auditHandler = new EtwAuditHandler(true)
            {
                InnerHandler = new TestHandler((c, r) =>
                {
                    return Task.Factory.StartNew(
                            () =>
                            {
                                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                                response.Headers.Add("TestHeader", "TestValue");
                                response.Content = new StringContent("Returning Test Data", Encoding.UTF8, "text/plain");
                                return response;
                            });
                })
            };

            var client = new HttpClient(auditHandler);
            var result = client.SendAsync(httpRequestMessage).Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual(1, listener.Events.Count);

            var etwData = listener.Events.First();
            Assert.AreEqual(EventLevel.Error, etwData.Level);
            Assert.AreEqual(2, etwData.Payload.Count);

            var requestData = (string)etwData.Payload[0];
            Assert.AreEqual("Request Method: GET\r\nRequest Uri: http://test.com/\r\nRequest Headers: \r\nRequestTestHeader: RequestValue; \r\n\r\n\r\nRequest Content:\r\nSending Test Data\r\n", requestData);

            var responseData = (string)etwData.Payload[1];
            Assert.AreEqual("Response Status: BadRequest Bad Request\r\nResponse Headers: \r\nTestHeader: TestValue; \r\n\r\n\r\nResponse Content:\r\nReturning Test Data\r\n", responseData);
        }
    }
}
