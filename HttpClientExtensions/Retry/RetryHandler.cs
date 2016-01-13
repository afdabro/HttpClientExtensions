
namespace HttpClientExtensions.Retry
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Retry based on retry function result.
    /// </summary>
    public class RetryHandler : DelegatingHandler
    {
        private readonly int maxRetries;
        private Func<HttpResponseMessage, bool> retryFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryHandler" /> class.
        /// </summary>
        /// <param name="retryFunc">The retry function.</param>
        /// <param name="maxRetries">The maximum number of retries.</param>
        public RetryHandler(Func<HttpResponseMessage, bool> retryFunc, int maxRetries = 10)
        {
            this.maxRetries = maxRetries;
            this.retryFunc = retryFunc;
        }

        /// <inheritdoc/>
        protected async override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            var count = 0;
            do
            {
                response = await base.SendAsync(request, cancellationToken);

                if (!this.retryFunc(response))
                {
                    break;
                }

                count++;
            }
            while (count < this.maxRetries);

            return response;
        }
    }
}