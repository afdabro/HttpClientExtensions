
namespace HttpClientExtensions.Tests.TestHelpers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handler for unit testing Custom handlers
    /// </summary>
    public class TestHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage,
       CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestHandler"/> class.
        /// </summary>
        public TestHandler()
        {
            _handlerFunc = (r, c) => Return200();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestHandler"/> class.
        /// </summary>
        /// <param name="handlerFunc">The handler function.</param>
        public TestHandler(Func<HttpRequestMessage,
            CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handlerFunc(request, cancellationToken);
        }

        /// <summary>
        /// Return default Http Status Code of 200 OK.
        /// </summary>
        /// <returns></returns>
        public static Task<HttpResponseMessage> Return200()
        {
            return Task.Factory.StartNew(
                () => new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
