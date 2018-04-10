using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace EZLC
{
    static class Authentication
    {
        /// <summary>
        /// Mehtod to generate the encoded 2-factor token 
        /// </summary>
        /// <returns>Token</returns>
        public static string GenerateToken( string IssuerName, string CertTemplateName, string CertTemplateGuidName)
        {
            string Token = string.Empty;
            EncryptionEngine encryptionEngine = new EncryptionEngine();
            string CertificateKey = GetCertificate(IssuerName, CertTemplateName, CertTemplateGuidName);
            if (!CertificateKey.Equals(string.Empty))
            {
                Token = System.Environment.UserDomainName + " " + CertificateKey + " ";
                return encryptionEngine.Base64Encode(Token);
            }
            else// authentication failed
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Method to fetch the certificate 
        /// </summary>
        /// <returns>certificate issuer name</returns>
        public static string GetCertificate(string IssuerName, string CertTemplateName, string CertTemplateGuidName)
        {
            bool isCertValid = false;
          
            try
            {
                X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);

                // Try to open the store.
                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollectionClient = certStore.Certificates.Find(X509FindType.FindByIssuerName, IssuerName, true);
                foreach (var item in certCollectionClient)
                {
                    foreach (X509Extension extension in item.Extensions)
                    {
                        AsnEncodedData asndata = new AsnEncodedData(extension.Oid, extension.RawData);
                        string strtemplatename = asndata.Format(true);
                        if (strtemplatename.Contains(CertTemplateGuidName))
                        {
                            if (Convert.ToDateTime(item.GetExpirationDateString()).ToLocalTime() > DateTime.Now) 
                            {
                                isCertValid = true;
                            }
                           
                            break;
                        }
                    }
                }
                certStore.Close();
                if (isCertValid)
                    return CertTemplateName;
                else
                    return string.Empty;
                //  return "AppDevV2";
            }
            catch (CryptographicException)
            {
                return string.Empty;
            }
        }
    }


}
