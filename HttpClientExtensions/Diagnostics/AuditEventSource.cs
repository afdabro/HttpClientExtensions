

namespace HttpClientExtensions.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Net.Http;
    using System.Text;

    [EventSource(Name = "HttpClientExtensions--Audit-EventSource")]
    public class AuditEventSource : EventSource
    {
        private static AuditEventSource log = new AuditEventSource();

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public static AuditEventSource Log
        {
            get
            {
                return log;
            }
        }

        /// <summary>
        /// Audit successful request.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="responseMessage">The response message.</param>
        [Event(1, Level = EventLevel.Informational)]
        public void AuditRequestSucceeded(string requestMessage, string responseMessage)
        {
            this.WriteEvent(1, requestMessage, responseMessage);
        }

        /// <summary>
        /// Audit failed request.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="responseMessage">The response message.</param>
        [Event(2, Level = EventLevel.Error)]
        public void AuditRequestFailure(string requestMessage, string responseMessage)
        {
            this.WriteEvent(2, requestMessage, responseMessage);
        }

        #region Non Events
        [NonEvent]
        public void AuditRequestSucceeded(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage)
        {
            this.AuditRequestSucceeded(FormatRequest(requestMessage), FormatResponse(responseMessage));
        }

        [NonEvent]
        public void AuditRequestFailure(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage)
        {
            this.AuditRequestFailure(FormatRequest(requestMessage), FormatResponse(responseMessage));
        }

        [NonEvent]
        private string FormatRequest(HttpRequestMessage requestMessage)
        {
            var requestMessageBuilder = new StringBuilder();
            requestMessageBuilder.AppendFormat(Resources.Messages.RequestMethod, requestMessage.Method).AppendLine();
            requestMessageBuilder.AppendFormat(Resources.Messages.RequestUri, requestMessage.RequestUri).AppendLine();

            var requestHeaders = FormatHeaders(requestMessage.Headers);

            requestMessageBuilder.AppendFormat(Resources.Messages.RequestHeaders, requestHeaders).AppendLine();

            if (requestMessage.Content != null)
            {
                requestMessageBuilder.AppendFormat(Resources.Messages.RequestContent, requestMessage.Content.ReadAsStringAsync().Result).AppendLine();
            }
            else
            {
                requestMessageBuilder.Append(Resources.Messages.RequestContentEmpty).AppendLine();
            }

            return requestMessageBuilder.ToString();
        }

        [NonEvent]
        private string FormatResponse(HttpResponseMessage responseMessage)
        {
            var responseMessageBuilder = new StringBuilder();
            responseMessageBuilder.AppendFormat(Resources.Messages.ResponseStatus, responseMessage.StatusCode, responseMessage.ReasonPhrase).AppendLine();

            var responseHeaders = FormatHeaders(responseMessage.Headers);
            responseMessageBuilder.AppendFormat(Resources.Messages.ResponseHeaders, responseHeaders).AppendLine();

            if (responseMessage.Content != null)
            {
                responseMessageBuilder.AppendFormat(Resources.Messages.ResponseContent, responseMessage.Content.ReadAsStringAsync().Result).AppendLine();
            }
            else
            {
                responseMessageBuilder.Append(Resources.Messages.ResponseContentEmpty).AppendLine();
            }

            return responseMessageBuilder.ToString();
        }

        [NonEvent]
        private string FormatHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            var headersBuilder = new StringBuilder();

            foreach (var header in headers)
            {
                var headerValuesBuilder = new StringBuilder();
                foreach(var headerValue in header.Value)
                {
                    headerValuesBuilder.AppendLine(string.Format("{0}; ", headerValue));
                }

                headersBuilder.AppendLine(string.Format("{0}: {1}", header.Key, headerValuesBuilder.ToString()));
            }

            return headersBuilder.ToString();
        }
        #endregion
    }
}
