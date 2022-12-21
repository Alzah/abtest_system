using UnityEngine.Networking;

namespace Classes.Helpers
{
    public class AcceptAllCertificatesHandler : CertificateHandler
    {
        public static CertificateHandler Instance = new AcceptAllCertificatesHandler();
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}