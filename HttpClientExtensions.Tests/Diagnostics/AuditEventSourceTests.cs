
namespace HttpClientExtensions.Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using HttpClientExtensions.Diagnostics;
    using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;

    /// <summary>
    /// Summary description for AuditEventSourceTests
    /// </summary>
    [TestClass]
    public class AuditEventSourceTests
    {
        [TestMethod]
        public void ShouldValidateEventSource()
        {
            EventSourceAnalyzer.InspectAll(AuditEventSource.Log);
        }
    }
}
