
namespace HttpClientExtensions.Utilities.Etw.Listeners
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// Collection based event tracing for windows.
    /// Purpose: The main purpose of the collection etw listener is for Unit Testing classes which create ETW events.
    /// </summary>
    public class CollectionEtwListener : EventListener
    {
        private readonly ICollection<EventWrittenEventArgs> events;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionEtwListener"/> class.
        /// </summary>
        public CollectionEtwListener()
        {
            this.events = new Collection<EventWrittenEventArgs>();
        }

        /// <summary>
        /// Gets the collection of events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        public ICollection<EventWrittenEventArgs> Events
        {
            get
            {
                return this.events;
            }
        }

        /// <inheritdoc/>
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            this.events.Add(eventData);
        }
    }
}
