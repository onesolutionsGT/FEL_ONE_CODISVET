using FEL_ONE.Clases;
using FEL_ONE.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

namespace FEL_ONE.Certificadores
{
    internal class Megaprint
    {
        #region variables
        private static string token = "";
        private static string dirXMLauth;
        private static string dirXMLres;
        private static string dirXMLerr;
        private static string dirXMLPDF;
        private static string trnNum;
        private static string tipoDoc;
        private static string certificado = "";
        private static string passcertificad = "";
        private static string dirXMLSinAutorizar = "";
        private static string apikey = "";
        private static string Nit;
        private static string dirUR_t;
        private static string dirUR_r;
        private static string dirUR_p;
        private static string dirUR_v;
        private static string dirUR_a;
        private static SAPbobsCOM.Recordset RecSet;
        private static string QryStr;
        private static string xmlResp = "";
        private static string xmlFile = "";
        private static string filename;
        private static string filepdfwebName;
        private static string uuidVal = "null";
        private static string RetError;
        private static string InnerXMLrespuestaVal = "1";
        private static string filenamepdfVal = "null";
        private static bool recuperado = false;
        private static string verificarFEL = "";

        #endregion

        [Obsolete]
        public static void EnviaDocumentoFEL(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string DocEntry, bool esBatch = false)
        {
            dirXMLauth = "";
            dirXMLres = "";
            dirXMLerr = "";
            dirXMLPDF = "";
            certificado = "";
            passcertificad = "";
            dirXMLSinAutorizar = "";
            apikey = "";
            Nit = "";
            dirUR_t = "";
            dirUR_r = "";
            dirUR_p = "";
            dirUR_v = "";
            RecSet = null;
            QryStr = "";
            xmlResp = "";
            xmlFile = "";
            filename = "";
            verificarFEL = "";

            trnNum = "0000-000000000000";
            tipoDoc = "";
            uuidVal = "";
            RetError = "";
            InnerXMLrespuestaVal = "1";
            filenamepdfVal = "null";
            filepdfwebName = "null";
            recuperado = false;

            try
            {
                if (Utils.ExisteDocumento(OCompany, SBO_Application, CurrSerie, DocEntry, Tipo, esBatch))
                {
                    dirXMLSinAutorizar = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXML"); // Direccion xml Sin autorizar
                    dirXMLPDF = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHPDF"); // Direccion xml pdf
                    certificado = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLc"); // Direccion Certificado
                    passcertificad = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLcp"); // Contraseña Certificado
                    dirXMLauth = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLaut"); // Direccion xml Autorizado
                    dirXMLres = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLres"); // Direccion xml Respuesta
                    apikey = Utils.ObtieneValorParametro(OCompany, SBO_Application, "ApiKey"); // ApiKey
                    dirUR_r = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_r"); // Direccion URL Request
                    dirUR_t = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_t"); // Direccion URL Token
                    dirUR_p = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_p"); // Direccion URL Pdf
                    dirUR_v = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_v"); // Direccion URL Validacion
                    dirXMLerr = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(OCompany, SBO_Application, "NitEmi"); // Nit emisor para el token
                    verificarFEL = Utils.ObtieneValorParametro(OCompany, SBO_Application, "VerFel"); // Nit emisor para el token

                    string SerieAprobada;
                    string TipoDocFEL;

                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH("CALL FELONE_MEGA_" + TipoDocFEL + " (" + DocEntry + ",'" + TipoDocFEL + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato("SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato("SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato("EXEC FELONE_MEGA_" + TipoDocFEL + " " + DocEntry + ",'1'");
                    }

                    if (TipoDocFEL == "FACT" || TipoDocFEL == "NDEB" || TipoDocFEL == "FEXP" || TipoDocFEL == "FCAM" || TipoDocFEL == "FRES")
                    { tipoDoc = "1"; }
                    else
                    {
                        tipoDoc = TipoDocFEL == "NCRE" || TipoDocFEL == "NABN" ? "2" : TipoDocFEL == "FESP" ? "3" : "4";
                    }



                    if (Utils.GrabarXml(OCompany, xmlResp, SerieAprobada, DocNum, Tipo, ref xmlFile))
                    {
                        trnNum = "0000-" + int.Parse(DocEntry).ToString("000000000000");
                        //Verificador de Documento FEL MEGA
                        if (verificarFEL == "1")
                        {
                            if (VerificaDocumento(DocEntry))
                            {
                                string pdf = PidePDF(uuidVal.ToString(), dirUR_p);
                                XmlDocument xmlDocpdf = new XmlDocument();
                                xmlDocpdf.LoadXml(pdf);

                                XmlNodeList RegDocResponsePDF = xmlDocpdf.SelectNodes("RetornaPDFResponse");
                                foreach (XmlNode xnDoc in RegDocResponsePDF)
                                {
                                    InnerXMLrespuestaVal = xnDoc.SelectSingleNode("tipo_respuesta").InnerText;
                                    if (InnerXMLrespuestaVal == "0")
                                    {
                                        try
                                        {
                                            StreamWriter escritorPDF;
                                            string filenamePDFXML = dirXMLPDF + @"/" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                            escritorPDF = File.AppendText(filenamePDFXML);
                                            escritorPDF.Write(pdf.ToString());
                                            escritorPDF.Flush();
                                            escritorPDF.Close();
                                        }
                                        catch { }

                                        string pdfbyte = xnDoc.SelectSingleNode("pdf").InnerText;

                                        byte[] bytes;
                                        bytes = System.Convert.FromBase64String(pdfbyte);
                                        filenamepdfVal = dirXMLPDF + @"/" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".pdf";
                                        filepdfwebName = TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".pdf";
                                        System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(filenamepdfVal, System.IO.FileMode.Create));
                                        writer.Write(bytes);
                                        writer.Close();
                                    }
                                }



                                QryStr = OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB
                                    ? "CALL FELONE_UTILS ('True','" + DocEntry + "','" + Tipo + "','" + uuidVal.ToString() + "','null','null','" + filenamepdfVal + "','00-00-0000T00:00:00','00-00-0000T00:00:00','','" + filepdfwebName + "') "
                                    : "EXEC FELONE_UTILS 'True','" + DocEntry + "','" + Tipo + "','" + uuidVal.ToString() + "','null','null','" + filenamepdfVal + "','00-00-0000T00:00:00','00-00-0000T00:00:00','','" + filepdfwebName + "' ";
                                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                RecSet.DoQuery(QryStr);

                                return;
                            }
                        }



                        XmlDocument xmlFirmado = FirmaDocumento.FirmarDocumento(certificado, passcertificad, xmlFile);
                        string XmlaRegistrar = "<RegistraDocumentoXMLRequest id=\"A00B00C0-A714-44CE-" + trnNum + "\" ><xml_dte><![CDATA[" + xmlFirmado.InnerXml.ToString() + "]]></xml_dte></RegistraDocumentoXMLRequest>";



                        StreamWriter escritor;
                        filename = dirXMLauth + "/Auth_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor = File.AppendText(filename);
                        escritor.Write(XmlaRegistrar.ToString());
                        escritor.Flush();
                        escritor.Close();

                        string respuetaMega = EnviaDocumento(XmlaRegistrar, apikey, dirUR_t, dirUR_r, Nit);

                        XmlDocument xmlRespuesta = new XmlDocument();
                        xmlRespuesta.LoadXml(respuetaMega);
                        string InnerXMLrespuesta = "1";
                        List<string> cod_error = new List<string>();
                        List<string> desc_error = new List<string>();
                        string uuid = "";
                        string serieFel = "";
                        string documentoFel = "";
                        string autorizacionFel = "";
                        string filenamepdf = "";
                        string fechaautorizada = "";
                        string FechaHoraEmision = "";

                        XmlNodeList RegDocResponse = xmlRespuesta.SelectNodes("RegistraDocumentoXMLResponse");
                        foreach (XmlNode xnDocs in RegDocResponse)
                        {
                            InnerXMLrespuesta = xnDocs.SelectSingleNode("tipo_respuesta").InnerText;
                            if (InnerXMLrespuesta == "0")
                            {
                                string CatNodesList = xnDocs.SelectSingleNode("xml_dte").InnerText.Replace("dte:", "");

                                XmlDocument CatNodesListRespuesta = new XmlDocument();
                                CatNodesListRespuesta.LoadXml(CatNodesList);

                                XmlNodeList RegCatNodesListRespuesta = CatNodesListRespuesta.SelectNodes("GTDocumento/SAT/DTE/Certificacion");
                                foreach (XmlNode xnDet in RegCatNodesListRespuesta)
                                {
                                    string certificacionnode = xnDet.SelectSingleNode("NumeroAutorizacion").InnerText;
                                    serieFel = xnDet.SelectSingleNode("NumeroAutorizacion").Attributes[1].Value.ToString();
                                    documentoFel = xnDet.SelectSingleNode("NumeroAutorizacion").Attributes[0].Value.ToString();
                                    autorizacionFel = xnDet.SelectSingleNode("NumeroAutorizacion").InnerText;
                                    fechaautorizada = xnDet.SelectSingleNode("FechaHoraCertificacion").InnerText;
                                }

                                XmlNodeList RegCatNodesListRespuesta2 = CatNodesListRespuesta.SelectNodes("GTDocumento/SAT/DTE/DatosEmision");
                                foreach (XmlNode xnDet in RegCatNodesListRespuesta2)
                                {
                                    FechaHoraEmision = xnDet.SelectSingleNode("DatosGenerales").Attributes[1].Value.ToString();
                                }

                                uuid = xnDocs.SelectSingleNode("uuid").InnerText;
                            }
                            else if (InnerXMLrespuesta == "1")
                            {
                                XmlNodeList CatNodesList = xnDocs.SelectNodes("listado_errores/error");
                                foreach (XmlNode xnDet in CatNodesList)
                                {
                                    cod_error.Add(xnDet.SelectSingleNode("cod_error").InnerText);
                                    desc_error.Add(xnDet.SelectSingleNode("desc_error").InnerText);
                                }
                            }
                        }

                        try
                        {
                            if (InnerXMLrespuesta == "0")
                            {
                                try
                                {
                                    string pdf = PidePDF(uuid.ToString(), dirUR_p);
                                    XmlDocument xmlDocpdf = new XmlDocument();
                                    xmlDocpdf.LoadXml(pdf);

                                    XmlNodeList RegDocResponsePDF = xmlDocpdf.SelectNodes("RetornaPDFResponse");
                                    foreach (XmlNode xnDoc in RegDocResponsePDF)
                                    {
                                        InnerXMLrespuesta = xnDoc.SelectSingleNode("tipo_respuesta").InnerText;
                                        if (InnerXMLrespuesta == "0")
                                        {
                                            string pdfbyte = xnDoc.SelectSingleNode("pdf").InnerText;

                                            byte[] bytes;
                                            bytes = System.Convert.FromBase64String(pdfbyte);
                                            filenamepdf = dirXMLPDF + @"/" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".pdf";
                                            filepdfwebName = TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".pdf";
                                            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(filenamepdf, System.IO.FileMode.Create));
                                            writer.Write(bytes);
                                            writer.Close();
                                        }
                                        else if (InnerXMLrespuesta == "1")
                                        {
                                            XmlNodeList CatNodesList = xnDoc.SelectNodes("listado_errores/error");
                                            foreach (XmlNode xnDet in CatNodesList)
                                            {
                                                cod_error.Add(xnDet.SelectSingleNode("cod_error").InnerText);
                                                desc_error.Add(xnDet.SelectSingleNode("desc_error").InnerText);
                                            }
                                        }
                                    }

                                    Utils.ActualizaCamposDocumento(OCompany, TipoDocFEL, DocEntry, documentoFel, serieFel);

                                    string data = respuetaMega.ToString();
                                    filename = dirXMLres + "/Aprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "True", uuid.ToString(), documentoFel, serieFel, filenamepdf, fechaautorizada, FechaHoraEmision, "", filepdfwebName);
                                }

                                catch (Exception ex)
                                {
                                    string data = respuetaMega.ToString();
                                    filename = dirXMLerr + "/ErrorSistemaAprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data + "<Exception>" + ex.Message + "</Exception>", filename, "TrueError", "Error Interno Verifique portal web", "", "", "", "", "", "", "");
                                }
                            }
                            else if (InnerXMLrespuesta == "1")
                            {
                                string errores = ""; ;
                                int cont = cod_error.Count;
                                for (cont = 0; cont <= cod_error.Count - 1; cont++) { errores += "Error: No." + cont + " Cod: " + cod_error[cont] + "Descripcion: " + desc_error[cont] + "|..| "; }

                                string data = respuetaMega.ToString();
                                filename = dirXMLerr + "/Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "False", errores, "", "", "", "", "", "", "");

                            }
                        }
                        catch (Exception ex)
                        {
                            string data = respuetaMega.ToString();
                            filename = dirXMLerr + "/ErrorSistema_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString(), "", "", "", "", "", "", "");
                            SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " + ex.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                            return;
                        }
                    }
                    else
                    {
                        string data = "<error><errorDescripcion>Error de permisos</errorDescripcion><No.Sap>" + DocNum.ToString() + "</No.Sap></error>"; ;
                        filename = dirXMLerr + "/VerificarError_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "False", "No se pudo guardar el xml, verifique permisos", "", "", "", "", "", "", "");
                    }
                }
            }
            catch (Exception ex)
            {
                string data = "<error><No.Sap> " + DocNum.ToString() + "</No.Sap> <ErrorCS> " + ex.ToString() + "</ErrorCS></error>";
                filename = dirXMLerr + "/ErrorSistema_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString(), "", "", "", "", "", "", "");
            }
        }
        [Obsolete]
        public static void EnviaDocumentoFELA(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string DocEntry)
        {
            dirXMLauth = "";
            dirXMLres = "";
            dirXMLerr = "";
            dirXMLPDF = "";
            certificado = "";
            passcertificad = "";
            dirXMLSinAutorizar = "";
            apikey = "";
            Nit = "";
            dirUR_t = "";
            dirUR_r = "";
            dirUR_p = "";
            dirUR_v = "";
            RecSet = null;
            QryStr = "";
            xmlResp = "";
            xmlFile = "";
            filename = "";
            verificarFEL = "";
            dirUR_a = "";
            trnNum = "0000-000000000000";
            tipoDoc = "";
            uuidVal = "";
            RetError = "";
            InnerXMLrespuestaVal = "1";
            filenamepdfVal = "null";
            recuperado = false;

            string uuidbase = "";

            try
            {
                if (Utils.ExisteDocumentoANULAR(OCompany, SBO_Application, CurrSerie, DocEntry, Tipo))
                {
                    dirXMLSinAutorizar = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXML"); // Direccion xml Sin autorizar
                    dirXMLPDF = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHPDF"); // Direccion xml pdf
                    certificado = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLc"); // Direccion Certificado
                    passcertificad = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLcp"); // Contraseña Certificado
                    dirXMLauth = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLaut"); // Direccion xml Autorizado
                    dirXMLres = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLres"); // Direccion xml Respuesta
                    apikey = Utils.ObtieneValorParametro(OCompany, SBO_Application, "ApiKey"); // ApiKey
                    dirUR_r = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_r"); // Direccion URL Request
                    dirUR_t = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_t"); // Direccion URL Token
                    dirUR_p = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_p"); // Direccion URL Pdf
                    dirUR_v = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_v"); // Direccion URL Validacion
                    dirUR_a = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_a"); // Direccion URL Validacion
                    dirXMLerr = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(OCompany, SBO_Application, "NitEmi"); // Nit emisor para el token
                    verificarFEL = Utils.ObtieneValorParametro(OCompany, SBO_Application, "VerFel"); // Nit emisor para el token

                    string SerieAprobada;
                    string TipoDocFEL;


                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);

                        if (TipoDocFEL == "FACT" | TipoDocFEL == "FCAM" | TipoDocFEL == "RDON" | TipoDocFEL == "NDEB" | TipoDocFEL == "RECI" | TipoDocFEL == "FEXP")
                        {
                            uuidbase = Utils.TraeDatoH("SELECT \"U_NUMERO_DOCUMENTO_NC\" FROM \"OINV\" WHERE \"DocEntry\" = " + DocEntry);
                        }
                        else if (TipoDocFEL == "NCRE" | TipoDocFEL == "NABN")
                        {
                            uuidbase = Utils.TraeDatoH("SELECT \"U_NUMERO_DOCUMENTO_NC\" FROM \"ORIN\" WHERE \"DocEntry\" = " + DocEntry);
                        }
                        else if (TipoDocFEL == "FESP")
                        {
                            uuidbase = Utils.TraeDatoH("SELECT \"U_NUMERO_DOCUMENTO_NC\" FROM \"OPCH\" WHERE \"DocEntry\" = " + DocEntry);
                        }
                        xmlResp = Utils.TraeDatoH("CALL FELONE_MEGA_ANUL (" + DocEntry + ",'" + TipoDocFEL + "','" + Nit + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato("SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato("SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);

                        if (TipoDocFEL == "FACT" | TipoDocFEL == "FCAM" | TipoDocFEL == "RDON" | TipoDocFEL == "NDEB" | TipoDocFEL == "RECI" | TipoDocFEL == "FEXP")
                        {
                            uuidbase = Utils.TraeDato("SELECT U_NUMERO_DOCUMENTO_NC FROM OINV WHERE DocEntry = " + DocEntry);
                        }
                        else if (TipoDocFEL == "NCRE" | TipoDocFEL == "NABN")
                        {
                            uuidbase = Utils.TraeDato("SELECT U_NUMERO_DOCUMENTO_NC FROM ORIN WHERE DocEntry = " + DocEntry);
                        }
                        else if (TipoDocFEL == "FESP")
                        {
                            uuidbase = Utils.TraeDato("SELECT U_NUMERO_DOCUMENTO_NC FROM OPCH WHERE DocEntry = " + DocEntry);
                        }

                        xmlResp = Utils.TraeDato("EXEC FELONE_MEGA_ANUL " + DocEntry + "," + TipoDocFEL + "," + Nit);
                    }



                    if (Utils.GrabarXml(OCompany, xmlResp, SerieAprobada, DocNum, "Anulacion_", ref xmlFile))
                    {
                        XmlDocument xmlFirmado = FirmaDocumentoA.FirmarDocumento(certificado, passcertificad, xmlFile);



                        string ComplementoHexadecimal = "0000-" + int.Parse(DocEntry).ToString("000000000000");

                        string XmlaRegistrar = "<AnulaDocumentoXMLRequest id=\"F40D43B4-A814-44CE-" + ComplementoHexadecimal + "\" ><xml_dte><![CDATA[" + xmlFirmado.InnerXml.ToString() + "]]></xml_dte></AnulaDocumentoXMLRequest>";
                        StreamWriter escritor;
                        filename = dirXMLauth + "/Auth_Anulacion_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor = File.AppendText(filename);
                        escritor.Write(XmlaRegistrar.ToString());
                        escritor.Flush();
                        escritor.Close();

                        string respuetaMega = EnviaDocumentoA(XmlaRegistrar, apikey, dirUR_t, dirUR_a, Nit);
                        string filenamepdf = "";
                        string filepdfwebName = "";

                        XmlDocument xmlRespuesta = new XmlDocument();
                        xmlRespuesta.LoadXml(respuetaMega);
                        string InnerXMLrespuesta = "1";
                        List<string> cod_error = new List<string>();
                        List<string> desc_error = new List<string>();
                        string uuid = "";

                        XmlNodeList RegDocResponse = xmlRespuesta.SelectNodes("AnulaDocumentoXMLResponse");
                        foreach (XmlNode xnDocs in RegDocResponse)
                        {
                            InnerXMLrespuesta = xnDocs.SelectSingleNode("tipo_respuesta").InnerText;
                            if (InnerXMLrespuesta == "0")
                            {
                                uuid = xnDocs.SelectSingleNode("uuid").InnerText;
                            }
                            else if (InnerXMLrespuesta == "1")
                            {
                                XmlNodeList CatNodesList = xnDocs.SelectNodes("listado_errores/error");
                                foreach (XmlNode xnDet in CatNodesList)
                                {
                                    cod_error.Add(xnDet.SelectSingleNode("cod_error").InnerText);
                                    desc_error.Add(xnDet.SelectSingleNode("desc_error").InnerText);
                                }
                            }
                        }
                        try
                        {
                            if (InnerXMLrespuesta == "0")
                            {
                                try
                                {
                                    // Anulacion Asincrona por lo tanto await for 5 seconds
                                    Thread.Sleep(5000);

                                    string pdf = PidePDF(uuidbase, dirUR_p);
                                    XmlDocument xmlDocpdf = new XmlDocument();
                                    xmlDocpdf.LoadXml(pdf);

                                    XmlNodeList RegDocResponsePDF = xmlDocpdf.SelectNodes("RetornaPDFResponse");
                                    foreach (XmlNode xnDoc in RegDocResponsePDF)
                                    {
                                        InnerXMLrespuesta = xnDoc.SelectSingleNode("tipo_respuesta").InnerText;
                                        if (InnerXMLrespuesta == "0")
                                        {
                                            try
                                            {
                                                StreamWriter escritorPDF;
                                                string filenamePDFXML = dirXMLPDF + @"/" + TipoDocFEL + "_ANULADO_" + CurrSerieName + "_" + DocNum + ".xml";
                                                escritorPDF = File.AppendText(filenamePDFXML);
                                                escritorPDF.Write(pdf.ToString());
                                                escritorPDF.Flush();
                                                escritorPDF.Close();
                                            }
                                            catch{}       
                                            
                                            string pdfbyte = xnDoc.SelectSingleNode("pdf").InnerText;
                                            byte[] bytes;
                                            bytes = System.Convert.FromBase64String(pdfbyte);
                                            filenamepdf = dirXMLPDF + @"/" + TipoDocFEL + "_ANULADO_" + CurrSerieName + "_" + DocNum + ".pdf";
                                            filepdfwebName = TipoDocFEL + "_ANULADO_" + CurrSerieName + "_" + DocNum + ".pdf";
                                            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(filenamepdf, System.IO.FileMode.Create));
                                            writer.Write(bytes);
                                            writer.Close();
                                        }
                                        else if (InnerXMLrespuesta == "1")
                                        {
                                            XmlNodeList CatNodesList = xnDoc.SelectNodes("listado_errores/error");
                                            foreach (XmlNode xnDet in CatNodesList)
                                            {
                                                cod_error.Add(xnDet.SelectSingleNode("cod_error").InnerText);
                                                desc_error.Add(xnDet.SelectSingleNode("desc_error").InnerText);
                                            }
                                        }
                                    }

                                    string data = respuetaMega.ToString();
                                    filename = dirXMLres + "/AprobadaAnulada_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "TrueA", "", "", filenamepdf, "", "", "", "", filepdfwebName);

                                }
                                catch (Exception)
                                {
                                    string data = respuetaMega.ToString();
                                    filename = dirXMLerr + "/ErrorSistemaAprobadaAnulada_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "TrueErrorA", "Error Interno Verifique portal web", "", "", "", "", "", "", "");
                                }
                            }
                            else if (InnerXMLrespuesta == "1")
                            {
                                string errores = ""; ;
                                int cont = cod_error.Count;
                                for (cont = 0; cont <= cod_error.Count - 1; cont++) { errores += "Error: No." + cont + " Cod: " + cod_error[cont] + "Descripcion: " + desc_error[cont] + "|..| "; }

                                string data = respuetaMega.ToString();
                                filename = dirXMLres + "/Error_Anulada_" + CurrSerieName + "_" + DocNum + ".xml";
                                Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "FalseA", errores, "", "", "", "", "", "", "");
                            }
                        }
                        catch (Exception ex)
                        {
                            string data = respuetaMega.ToString();
                            filename = dirXMLerr + "/ErrorSistemaAnulada_" + CurrSerieName + "_" + DocNum + ".xml";
                            Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "FalseA", ex.Message.ToString(), "", "", "", "", "", "", "");
                            SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " + ex.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                            return;
                        }
                    }
                    else
                    {
                        string data = "Documento se encuentra firmado No.Sap: " + DocNum.ToString();
                        filename = dirXMLerr + "/VerificarErrorAnulada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "FalseA", "No se pudo guardar el xml, verifique permisos", "", "", "", "", "", "", "");
                    }
                }
            }
            catch (Exception ex)
            {
                string data = "<error><No.Sap> " + DocNum.ToString() + "</No.Sap> <ErrorCS> " + ex.ToString() + "</ErrorCS></error>";
                filename = dirXMLerr + "/ErrorSistema_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "FalseA", ex.Message.ToString(), "", "", "", "", "", "", "");
            }
        }


        public static void LLenaParametros(SAPbobsCOM.Company oCompany, SAPbouiCOM.Form oForm, SAPbouiCOM.Application SBO_Application)
        {
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            long RecCount;
            long RecIndex;
            SAPbouiCOM.EditText oEdit;
            SAPbouiCOM.ComboBox oCmb;
            SAPbouiCOM.Item oItem = null;
            string Valor = "";
            try
            {
                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                QryStr = oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB
                    ? "Select * from \"@FEL_PARAMETROS\""
                    : "Select * from [@FEL_PARAMETROS]";
                RecSet.DoQuery(QryStr);
                RecCount = RecSet.RecordCount;
                RecSet.MoveFirst();

                for (RecIndex = 0; RecIndex <= RecCount - 1; RecIndex++)
                {
                    switch (RecSet.Fields.Item("U_PARAMETRO").Value)
                    {
                        case "PATHXML":
                            oItem = oForm.Items.Item("Pathsaut");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "PATHXMLerr":
                            oItem = oForm.Items.Item("Patherr");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "PATHPDF":
                            oItem = oForm.Items.Item("Pathpdf");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "PATHXMLaut":
                            oItem = oForm.Items.Item("Pathaut");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "PATHXMLres":
                            oItem = oForm.Items.Item("Pathres");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UR_t":
                            oItem = oForm.Items.Item("UR_t");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UR_r":
                            oItem = oForm.Items.Item("UR_r");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UR_p":
                            oItem = oForm.Items.Item("UR_p");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UR_a":
                            oItem = oForm.Items.Item("UR_a");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UR_v":
                            oItem = oForm.Items.Item("UR_v");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "ApiKey":
                            oItem = oForm.Items.Item("ApiKey");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "USRDB":
                            oItem = oForm.Items.Item("txtUsuario");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "PASSDB":
                            oItem = oForm.Items.Item("txtPass");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "PATHXMLc":
                            oItem = oForm.Items.Item("cert");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "PATHXMLcp":
                            oItem = oForm.Items.Item("certpass");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Nemi":
                            oItem = oForm.Items.Item("Nemi");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "NitEmi":
                            oItem = oForm.Items.Item("NitEmi");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Tafilia":
                            oItem = oForm.Items.Item("Tafilia");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Correo":
                            oItem = oForm.Items.Item("Correo");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "VerFel":
                            oItem = oForm.Items.Item("VerFel");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                    }
                    if (oItem != null)
                    {
                        if (oItem.Type == SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX)
                        {
                            oCmb = oItem.Specific;
                            oCmb.Select(Valor, SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        else
                        {
                            oEdit = oItem.Specific;
                            oEdit.Value = Valor;
                        }
                    }
                    RecSet.MoveNext();
                }

                _ = System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                RecSet = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                _ = SBO_Application.MessageBox(ex.Message);
            }
        }
        public static void GuardarParametros(SAPbobsCOM.Company oCompany, SAPbouiCOM.Form oForm, SAPbouiCOM.Application SBO_Application)
        {
            SAPbobsCOM.UserTable oUsrTbl;
            SAPbouiCOM.EditText oEdit;
            SAPbouiCOM.ComboBox oCombo;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Folder oTab;
            SAPbouiCOM.ProgressBar ProgressBar = null;
            try
            {
                ProgressBar = SBO_Application.StatusBar.CreateProgressBar("Guardando parámetros por favor espere...", 18, false);
                oUsrTbl = oCompany.UserTables.Item("FEL_PARAMETROS");

                oItem = oForm.Items.Item("Nemi");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Nombre del Emisor");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "Nemi", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("NitEmi");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el numero de NIT");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "NitEmi", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Tafilia");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Tipo de Afiliacion");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "Tafilia", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Correo");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Nombre del Emisor");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "Correo", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("UR_t");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del token");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_t", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("UR_r");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del Request");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_r", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("UR_p");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del PDF");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_p", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("UR_a");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path de anulacion");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_a", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("UR_v");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path de anulacion");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_v", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Pathsaut");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del XML Sin Autorizacion");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXML", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Pathaut");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del XML de Autorizacion");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXMLaut", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Pathres");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path  respuesta");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXMLres", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Patherr");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del XML Error");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXMLerr", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Pathpdf");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del XML PDF");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHPDF", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("ApiKey");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del Apikey");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "ApiKey", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("cert");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del certificado");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXMLc", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("certpass");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabFACE");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el Path del pass de certificado");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXMLcp", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("txtUsuario");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabCNN");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el usuario de la base de datos");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "USRDB", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("txtPass");
                oEdit = oItem.Specific;
                if (oEdit.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    oEdit.Active = true;
                    oItem = oForm.Items.Item("tabCNN");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar el password de la base de datos");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PASSDB", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("VerFel");
                oCombo = oItem.Specific;
                if (oCombo.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    oCombo.Active = true;
                    oItem = oForm.Items.Item("tabCNN");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de Ingresar si el documento se verificara en FEL");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "VerFel", oCombo.Value);
                ProgressBar.Value += 1;



                ParametrosForm.GuardaDatosSeries();
                ProgressBar.Value += 1;
                ProgressBar.Stop();
                _ = System.Runtime.InteropServices.Marshal.ReleaseComObject(ProgressBar);
                ProgressBar = null;
                GC.Collect();
                SBO_Application.SetStatusBarMessage("Parámetros guardados exítosamente", SAPbouiCOM.BoMessageTime.bmt_Short, false);
            }
            catch (Exception ex)
            {
                ProgressBar.Stop();
                _ = System.Runtime.InteropServices.Marshal.ReleaseComObject(ProgressBar);
                GC.Collect();
                _ = SBO_Application.MessageBox(ex.Message);
            }
        }


        public static string PidePDF(string uuid, string dirUR_p)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dirUR_p);
            UTF8Encoding enc;
            enc = new System.Text.UTF8Encoding();
            string postData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><RetornaPDFRequest><uuid>" + uuid + "</uuid></RetornaPDFRequest>";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(postData);
            byte[] data = enc.GetBytes(xmlDoc.InnerXml);
            request.Headers.Add("authorization", "Bearer " + token.ToString());
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (Stream Stream = request.GetRequestStream())
            {
                Stream.Write(data, 0, data.Length);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            XmlDocument xmlDoc2 = new XmlDocument();
            xmlDoc2.LoadXml(responseString);
            return xmlDoc2.InnerXml;
        }
        public static string SolicitarToken()
        {
            string XML = "<SolicitaTokenRequest><usuario>" + Nit + "</usuario><apikey>" + apikey + "</apikey></SolicitaTokenRequest>";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dirUR_t);
            string respuesta = "";
            string postData = XML;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(postData);
            byte[] data = Encoding.ASCII.GetBytes(xmlDoc.InnerXml);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            XmlDocument xmlDoc2 = new XmlDocument();
            xmlDoc2.LoadXml(responseString);
            XmlNodeList nodo = xmlDoc2.SelectNodes("SolicitaTokenResponse");
            foreach (XmlNode childnode in nodo)
            {
                respuesta = childnode.SelectSingleNode("tipo_respuesta").InnerText == "1"
                    ? throw new Exception("[Error:OS01]: Solicitud de token [..] [Error:" + childnode.SelectSingleNode("listado_errores/error/cod_error").InnerText + "]: " + childnode.SelectSingleNode("listado_errores/error/desc_error").InnerText)
                    : childnode.SelectSingleNode("token").InnerText;
            }
            return respuesta;
        }


        public static string EnviaDocumento(string XML, string apikey, string UR_t, string UR_r, string nit)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            token = SolicitarToken();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UR_r);
            UTF8Encoding enc;
            enc = new System.Text.UTF8Encoding();
            string postData = XML;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(postData);
            byte[] data = enc.GetBytes(xmlDoc.InnerXml);
            request.Headers.Add("authorization", "Bearer " + token.ToString());
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (Stream Stream = request.GetRequestStream())
            {
                Stream.Write(data, 0, data.Length);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            XmlDocument xmlDoc2 = new XmlDocument();
            xmlDoc2.LoadXml(responseString);
            return xmlDoc2.InnerXml;
        }
        public static string EnviaDocumentoA(string XML, string apikey, string UR_t, string UR_a, string nit)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            token = SolicitarToken();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UR_a);
            UTF8Encoding enc;
            enc = new System.Text.UTF8Encoding();
            string postData = XML;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(postData);
            byte[] data = enc.GetBytes(xmlDoc.InnerXml);
            request.Headers.Add("authorization", "Bearer " + token.ToString());
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (Stream Stream = request.GetRequestStream())
            {
                Stream.Write(data, 0, data.Length);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            XmlDocument xmlDoc2 = new XmlDocument();
            xmlDoc2.LoadXml(responseString);
            return xmlDoc2.InnerXml;
        }
        public static string EnviarXMLMegaPrintV(string XMLCert)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            byte[] data = Encoding.UTF8.GetBytes(XMLCert);
            WebRequest req = WebRequest.Create(dirUR_v);
            req.Headers.Add("authorization", "Bearer " + token);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = data.Length;
            Stream stream = req.GetRequestStream();
            stream.Write(data, 0, data.Length);
            WebResponse res = req.GetResponse();
            string responseString = new StreamReader(res.GetResponseStream()).ReadToEnd();
            return responseString;
        }


        public static bool VerificaDocumento(string DocEntry)
        {
            //Genera XML de envio
            string ComplementoDocumento = tipoDoc + "000";
            string ComplementoHexaDecimal = int.Parse(DocEntry).ToString("000000000000");
            string xml = "<?xml version =\"1.0\" encoding =\"UTF-8\"?>";
            trnNum = ComplementoDocumento + "-" + ComplementoHexaDecimal;
            xml += "<VerificaDocumentoRequest id =\"A00B00C0-A714-44CE-" + trnNum + "\" />";
            string XMLCert = xml;

            token = SolicitarToken();
            if (token != "")
            {
                //Solicita la respuesta de verificacion y almacena en XML
                string respuesta_certificador = EnviarXMLMegaPrintV(XMLCert);
                XmlDocument respuesta_xml = new XmlDocument();
                respuesta_xml.LoadXml(respuesta_certificador);
                StringWriter sw = new StringWriter();
                XmlTextWriter tx = new XmlTextWriter(sw);
                respuesta_xml.WriteTo(tx);

                //Validacion de respuesta 0 correcta
                try
                {
                    recuperado = false;
                    XmlNodeList nodos_res = respuesta_xml.SelectNodes("VerificaDocumentoResponse/listado_documentos/estado_documento");
                    for (int i = 0; i < nodos_res.Count; i++)
                    {
                        if (nodos_res.Item(i).SelectSingleNode("tipo_respuesta").InnerText == "0")
                        {
                            RetError = "DOCUMENTO RECUPERADO DE VALIDAION; ESTE DOCUMENTO YA FUE CERTIFICADO;  ya no sebe de enviar nuevamente";
                            uuidVal = nodos_res.Item(i).SelectSingleNode("uuid").InnerText;
                            recuperado = true;
                            return true;
                        }
                        else
                        {
                            RetError = "DOCUMENTO RECUPERADO DE VALIDAION;  hay un error en ese documento y el uuid NO es un uuid de certificación se registró en la plataforma pero no se envió a la SAT, sebe de generar un nuevo uuid de transacción y enviar nuevamente el xml, por medio del registrarXML";
                            bool valido = false;
                            int newUUID = 0;
                            while (!valido)
                            {
                                newUUID += 1;
                                valido = VerificaDocumentoError(DocEntry, newUUID);
                                if (newUUID > 98)
                                {
                                    valido = true;
                                }
                            }

                            return recuperado == true || valido != true;
                        }
                    }
                    RetError = "DOCUMENTO RECUPERADO DE VALIDACION;  hay un error en ese documento ";
                    uuidVal = "";
                    nodos_res = respuesta_xml.SelectNodes("VerificaDocumentoResponse");
                    return nodos_res.Count == 1 && nodos_res.Item(0).SelectSingleNode("tipo_respuesta").InnerText != "0";
                }
                catch (Exception)
                {
                    recuperado = true;
                    RetError = "DOCUMENTO RECUPERADO DE VALIDACION; Ocurrio una Excepcion interna";
                    uuidVal = "";
                    return false;
                }
            }
            else
            {
                recuperado = true;
                RetError = "DOCUMENTO RECUPERADO DE VALIDACION; Ocurrio una Excepcion interna";
                uuidVal = "";
                return false;
            }
        }
        public static bool VerificaDocumentoError(string DocEntry, int newuuid)
        {
            //Genera XML
            string ComplementoDocumento = tipoDoc + newuuid.ToString("000");
            string ComplementoHexaDecimal = int.Parse(DocEntry).ToString("000000000000");
            trnNum = ComplementoDocumento + "-" + ComplementoHexaDecimal;
            string xml = "<?xml version =\"1.0\" encoding =\"UTF-8\"?>";
            xml += "<VerificaDocumentoRequest id =\"A00B00C0-A714-44CE-" + trnNum + "\" />";
            string XMLCert = xml;


            string token = SolicitarToken();
            if (token != "")
            {
                string respuesta_certificador = EnviarXMLMegaPrintV(XMLCert);
                XmlDocument respuesta_xml = new XmlDocument();
                respuesta_xml.LoadXml(respuesta_certificador);
                StringWriter sw = new StringWriter();
                _ = new XmlTextWriter(sw);
                try
                {
                    RetError = "DOCUMENTO RECUPERADO DE VALIDACION; Ocurrio un error interno";
                    uuidVal = "";
                    XmlNodeList nodos_res = respuesta_xml.SelectNodes("VerificaDocumentoResponse/listado_documentos/estado_documento");
                    for (int i = 0; i < nodos_res.Count; i++)
                    {
                        if (nodos_res.Item(i).SelectSingleNode("tipo_respuesta").InnerText == "1")
                        {
                            return false;
                        }
                        if (nodos_res.Item(i).SelectSingleNode("tipo_respuesta").InnerText == "0")
                        {
                            RetError = "DOCUMENTO RECUPERADO DE VALIDAION; ESTE DOCUMENTO YA FUE CERTIFICADO;  ya no sebe de enviar nuevamente";
                            uuidVal = nodos_res.Item(i).SelectSingleNode("uuid").InnerText;
                            recuperado = true;
                            return true;
                        }
                    }
                    nodos_res = respuesta_xml.SelectNodes("VerificaDocumentoResponse");
                    if (nodos_res.Count == 1)
                    {
                        if (nodos_res.Item(0).SelectSingleNode("tipo_respuesta").InnerText == "0")
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    recuperado = true;
                    RetError = "DOCUMENTO RECUPERADO DE VALIDACION; Ocurrio una Excepcion interna";
                    uuidVal = "";
                    return true;
                }
            }
            recuperado = true;
            RetError = "DOCUMENTO RECUPERADO DE VALIDACION; Ocurrio una Excepcion interna";
            uuidVal = "";
            return false;
        }

    }
}
