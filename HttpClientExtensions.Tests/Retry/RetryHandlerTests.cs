
namespace HttpClientExtensions.Tests.Retry
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HttpClientExtensions.Retry;
    using HttpClientExtensions.Tests.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Retry Handler Tests
    /// </summary>
    [TestClass]
    public class RetryHandlerTests
    {
        [TestMethod]
        public void RetryHandlerReturnBadRequest()
        {
            var retryFunc = new Func<HttpResponseMessage, bool>(r =>
            {

                if (r.StatusCode == HttpStatusCode.BadRequest)
                {
                    return true;
                }

                return false;
            }
            );
            var retryHandler = new RetryHandler(retryFunc)
            {
                InnerHandler = new TestHandler((c, r) =>
                {
                    return Task.Factory.StartNew(
                            () =>
                            {

                                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                                return response;
                            });
                })
            };

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            var client = new HttpClient(retryHandler);
            var result = client.SendAsync(httpRequestMessage).Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void RetryHandlerReturnOk()
        {
            var retryFunc = new Func<HttpResponseMessage, bool>(r =>
            {
                if (r.StatusCode == HttpStatusCode.BadRequest)
                {
                    return true;
                }

                return false;
            }
            );
            var retryHandler = new RetryHandler(retryFunc)
            {
                InnerHandler = new TestHandler()
            };

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            var client = new HttpClient(retryHandler);
            var result = client.SendAsync(httpRequestMessage).Result;
            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
