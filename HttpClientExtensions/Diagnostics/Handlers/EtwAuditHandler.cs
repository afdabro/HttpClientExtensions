

namespace HttpClientExtensions.Diagnostics.Handlers
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Event Tracing for Windows Audit handler.
    /// </summary>
    public class EtwAuditHandler : DelegatingHandler
    {
        private readonly bool auditAllRequests;

        /// <summary>
        /// Initializes a new instance of the <see cref="EtwAuditHandler"/> class.
        /// </summary>
        /// <param name="auditAllRequests">if set to <c>true</c> [audit all requests].</param>
        public EtwAuditHandler(bool auditAllRequests = false)
        {
            this.auditAllRequests = auditAllRequests;
        }

        /// <inheritdoc/>
        protected async override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if(this.auditAllRequests && response.IsSuccessStatusCode)
            {
                AuditEventSource.Log.AuditRequestSucceeded(request, response);
            }

            if(!response.IsSuccessStatusCode)
            {
                AuditEventSource.Log.AuditRequestFailure(request, response);
            }

            return response;
        }
    }
}
