using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirmaXadesNet;
using FirmaXadesNet.Crypto;
using FirmaXadesNet.Signature;
using FirmaXadesNet.Signature.Parameters;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace FEL_ONE.Clases
{
    public class FirmaDocumentoA
    {
        [Obsolete]
        public static XmlDocument FirmarDocumento(string rutaCertificado, string contraseñaCertificado, string rutaDocumento, string ubicacionDestino)
        {
            X509Certificate2 cert = new X509Certificate2(rutaCertificado, contraseñaCertificado, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            SignatureParameters parametros = ParametrosdeFirma();
            var nombredocumento = Path.GetFileNameWithoutExtension(rutaDocumento);
            Signer temp = new Signer(cert);
            using (CSharpImpl.__Assign(ref temp, new Signer(cert)))
            {
                parametros.Signer = temp;
                var documento = FirmaXades(parametros, rutaDocumento);
                AlmacenamientoDocumento(documento, ubicacionDestino, nombredocumento);
                return documento.Document;
            }
        }

        [Obsolete]
        public static XmlDocument FirmarDocumento(string rutaCertificado, string contraseñaCertificado, string rutaDocumento)
        {
            X509Certificate2 cert = new X509Certificate2(rutaCertificado, contraseñaCertificado, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            SignatureParameters parametros = ParametrosdeFirma();

            Signer temp = new Signer(cert);
            using (CSharpImpl.__Assign(ref temp, new Signer(cert)))
            {
                parametros.Signer = temp;
                return FirmaXades(parametros, rutaDocumento).Document;
            }
        }

        private static SignatureDocument FirmaXades(SignatureParameters sp, string ruta)
        {
            XadesService xadesService = new XadesService();

            using (FileStream fs = new FileStream(ruta, FileMode.Open))
            {
                var documento = xadesService.Sign(fs, sp);
                MoverNodoFirma(documento);
                return documento;
            }
        }

        private static void AlmacenamientoDocumento(SignatureDocument sd, string ruta, string nombre)
        {
            ruta = $@"{ruta}\{nombre}-Firmado.xml";
            sd.Save(ruta);
        }

        private static SignatureParameters ParametrosdeFirma()
        {
            SignatureParameters parametros = new SignatureParameters()
            {
                SignaturePackaging = SignaturePackaging.INTERNALLY_DETACHED,
                InputMimeType = "text/xml",
                ElementIdToSign = "DatosAnulacion",
                SignatureMethod = SignatureMethod.RSAwithSHA256,
                DigestMethod = DigestMethod.SHA256
            };
            return parametros;
        }

        private static void MoverNodoFirma(SignatureDocument sd)
        {
            var documento = sd.Document;
            var NodoFirma = documento.GetElementsByTagName("ds:Signature")[0];
            NodoFirma.ParentNode.RemoveChild(NodoFirma);
            documento.DocumentElement.AppendChild(NodoFirma);
        }

        private static class CSharpImpl
        {
            [Obsolete("Please refactor calling code to use normal Visual Basic assignment")]
            public static T __Assign<T>(ref T target, T value)
            {
                target = value;
                return value;
            }
        }
    }


}
