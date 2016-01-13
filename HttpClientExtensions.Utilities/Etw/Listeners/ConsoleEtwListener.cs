

namespace HttpClientExtensions.Utilities.Etw.Listeners
{
    using System;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// Console Event Tracing for Windows Listener
    /// </summary>
    public class ConsoleEtwListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Console.WriteLine("Event: {0}", eventData.Message);
            foreach (var payloadItem in eventData.Payload)
            {
                Console.WriteLine(payloadItem);
            }
        }
    }
}
