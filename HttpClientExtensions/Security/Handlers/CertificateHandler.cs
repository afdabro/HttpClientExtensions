
namespace HttpClientExtensions.Security.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Client Certificate handler
    /// </summary>
    public class CertificateHandler : WebRequestHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHandler"/> class.
        /// </summary>
        /// <param name="certThumbprint">The certificate thumbprint.</param>
        public CertificateHandler(string certThumbprint)
        {
            this.ClientCertificateOptions = ClientCertificateOption.Manual;
            this.UseDefaultCredentials = false;
            var certificate = GetStoreCertificate(certThumbprint);
            this.ClientCertificates.Add(certificate);
        }

        /// <summary>
        /// Gets the store certificate matching the thumbprint. The algorithm looks in both the current user and local machine stores and returns the first occurrence. 
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns>Client certificate</returns>
        /// <exception cref="System.ArgumentException">A Certificate with Thumbprint '{0}' could not be located.</exception>
        private static X509Certificate2 GetStoreCertificate(string thumbprint)
        {
            var locations = new List<StoreLocation>
            {
                StoreLocation.CurrentUser,
                StoreLocation.LocalMachine
            };

            foreach(var location in locations)
            {
                var store = new X509Store("My", location);
                try
                {
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection certificates = store.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        thumbprint,
                        false);
                    if(certificates.Count == 1)
                    {
                        return certificates[0];
                    }
                }
                finally
                {
                    store.Close();
                }
            }

            throw new ArgumentException(string.Format(Resources.Errors.CertificateNotFound, thumbprint));
        }
    }
}
