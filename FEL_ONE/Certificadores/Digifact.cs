using FEL_ONE.Clases;
using FEL_ONE.Forms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FEL_ONE.Certificadores
{
    class Digifact
    {
        private static string token = "";

        public static void EnviaDocumentoFEL(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string Pais, string DocEntry, bool ProcesarBatch = false)
        {
            string certificado = "";
            string passcertificad = "";
            string dirXMLSinAutorizar = "";
            string dirXMLauth = "";
            string dirXMLres = "";
            string dirXMLerr = "";
            string dirXMLPDF = "";
            string Nit;
            string dirUR_t;
            string dirUR_r;
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string xmlResp = "";
            string xmlFile = "";
            string filename;
            string SerieAprobada;
            string TipoDocFEL;
            string TokenUser;
            try
            {
                if (Utils.ValidaSerie(OCompany, SBO_Application, CurrSerie, ProcesarBatch) & Utils.ExisteDocumento(OCompany, SBO_Application, CurrSerie, DocEntry, Tipo, ProcesarBatch))
                {
                    dirXMLSinAutorizar = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXML"); // Direccion xml Sin autorizar
                    dirXMLPDF = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHPDF"); // Direccion xml pdf
                    certificado = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLc"); // Direccion Certificado
                    passcertificad = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLcp"); // Contraseña Certificado
                    dirXMLauth = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLaut"); // Direccion xml Autorizado
                    dirXMLres = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLres"); // Direccion xml Respuesta
                    dirUR_r = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_r"); // Direccion URL Request
                    dirUR_t = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_t"); // Direccion URL Token
                    dirXMLerr = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(OCompany, SBO_Application, "NitEmi"); // Nit emisor para el token
                    TokenUser = Utils.ObtieneValorParametro(OCompany, SBO_Application, "ApiKey"); // Nit emisor para el token

                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH("CALL FELONE_DIGIFACT_" + TipoDocFEL + " (" + DocEntry + ",'" + TipoDocFEL + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato("SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato("SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato("EXEC  [dbo].[FELONE_DIGIFACT_" + TipoDocFEL + "] " + DocEntry + ",'1'");
                    }


                    if (Utils.GrabarXml(OCompany, xmlResp, SerieAprobada, DocNum, Tipo, ref xmlFile))
                    {
                        var respuetaDIGI = EnviaDocumentoDIGI(xmlResp, dirUR_t, dirUR_r, Nit, certificado, passcertificad, TokenUser);
                        StreamWriter escritor;

                        JObject jsonResponse = JObject.Parse(respuetaDIGI);
                        List<JToken> data = jsonResponse.Children().ToList();

                        string InnerXMLrespuesta = "1";
                        string desc_error = "";
                        string uuid = "";
                        string serieFel = "";
                        string documentoFel = "";
                        string autorizacionFel = "";
                        string filenamepdf = "";
                        string fechaautorizada = "";
                        string FechaHoraEmision = "";
                        string XML = "";
                        string PDF = "";

                        foreach (JProperty item in data)
                        {
                            item.CreateReader();
                            switch (item.Name)
                            {
                                case "Codigo":
                                    InnerXMLrespuesta = item.Value.ToString();
                                    break;
                                case "Mensaje":
                                    desc_error = item.Value.ToString();
                                    break;
                                case "AcuseReciboSAT":
                                    autorizacionFel = item.Value.ToString();
                                    break;
                                case "ResponseDATA1":
                                    XML = item.Value.ToString();
                                    break;
                                case "ResponseDATA3":
                                    PDF = item.Value.ToString();
                                    break;
                                case "Autorizacion":
                                    uuid = item.Value.ToString();
                                    break;
                                case "Serie":
                                    serieFel = item.Value.ToString();
                                    break;
                                case "NUMERO":
                                    documentoFel = item.Value.ToString();
                                    break;
                                case "Fecha_DTE":
                                    if (DateTime.TryParseExact(item.Value.ToString(), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                                    {
                                        FechaHoraEmision = fecha.ToString("yyyy-MM-ddTHH:mm:ss");
                                    }
                                    else
                                    {
                                        FechaHoraEmision = item.Value.ToString();
                                    }
                                    break;
                                case "Fecha_de_certificacion":
                                    if (DateTime.TryParseExact(item.Value.ToString(), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaA))
                                    {
                                        fechaautorizada = fechaA.ToString("yyyy-MM-ddTHH:mm:ss");
                                    }
                                    else
                                    {
                                        fechaautorizada = item.Value.ToString();
                                    }
                                    break;
                            }
                        }

                        XmlDocument xmlDocCert = new XmlDocument();
                        try
                        {                       
                            byte[] b = Convert.FromBase64String(XML);
                            xmlDocCert.LoadXml(System.Text.Encoding.UTF8.GetString(b));
                        }
                        catch(Exception) { }

                        try
                        {
                            if (InnerXMLrespuesta == "1")
                            {
                                try
                                {
                                    byte[] c = Convert.FromBase64String(PDF);
                                    filenamepdf = dirXMLPDF + @"\" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".pdf";
                                    System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(filenamepdf, System.IO.FileMode.Create));
                                    writer.Write(c);
                                    writer.Close();

                                    filename = dirXMLauth + "/Auth_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    escritor = File.AppendText(filename);
                                    escritor.Write(xmlDocCert.InnerXml.ToString());
                                    escritor.Flush();
                                    escritor.Close();

                                    filename = dirXMLres + "/Aprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                    escritor = File.AppendText(filename);
                                    escritor.Write(respuetaDIGI.ToString());
                                    escritor.Flush();
                                    escritor.Close();


                                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                                    {
                                        QryStr = "CALL FELONE_UTILS ('True','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','" + documentoFel + "','" + serieFel + "','" + filenamepdf + "','" + fechaautorizada + "','" + FechaHoraEmision + "','','') ";
                                    }
                                    else
                                    {
                                        QryStr = "EXEC FELONE_UTILS 'True','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','" + documentoFel + "','" + serieFel + "','" + filenamepdf + "','" + fechaautorizada + "','" + FechaHoraEmision + "','','' ";
                                    }

                                    RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                    RecSet.DoQuery(QryStr);

                                    SAPbobsCOM.Documents oInv;
                                    oInv = (SAPbobsCOM.Documents)OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);
                                    oInv.GetByKey(Convert.ToInt32(DocEntry));
                                    oInv.Comments = documentoFel + "-" + serieFel;
                                    oInv.Update();
                                }

                                catch (Exception)
                                {
                                    filename = dirXMLerr + "/ErrorSistemaAprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    escritor = File.AppendText(filename);
                                    escritor.Write(respuetaDIGI.ToString());
                                    escritor.Flush();
                                    escritor.Close();


                                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                                    {
                                        QryStr = "CALL FELONE_UTILS ('TrueError','" + DocEntry + "','" + Tipo + "','Error Interno Verifique portal web','','','','','','','') ";
                                    }
                                    else
                                    {
                                        QryStr = "EXEC FELONE_UTILS 'TrueError','" + DocEntry + "','" + Tipo + "','Error Interno Verifique portal web','','','','','','','' ";
                                    }
                                    RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                    RecSet.DoQuery(QryStr);
                                }
                            }
                            else if (InnerXMLrespuesta == "0")
                            {
                                filename = dirXMLerr + "/Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                escritor = File.AppendText(filename);
                                escritor.Write(respuetaDIGI.ToString());
                                escritor.Flush();
                                escritor.Close();

                                string errores = "";


                                errores += "Error: Cert Descripcion: " + desc_error + "|..| ";


                                if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                                {
                                    QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','" + errores + "','','','','','','','') ";
                                }
                                else
                                {
                                    QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','" + errores + "','','','','','','','' ";
                                }

                                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                RecSet.DoQuery(QryStr);
                            }
                            else
                            {
                                filename = dirXMLerr + "/Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                escritor = File.AppendText(filename);
                                escritor.Write(respuetaDIGI.ToString());
                                escritor.Flush();
                                escritor.Close();

                                string errores = "";
                                errores += "Error: " + InnerXMLrespuesta + " Descripcion: " + desc_error + "|..| " + XML + "|..| ";

                                if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                                {
                                    QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','" + errores + "','','','','','','','') ";
                                }
                                else
                                {
                                    QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','" + errores + "','','','','','','','' ";
                                }

                                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                RecSet.DoQuery(QryStr);
                            }
                        }
                        catch (Exception ex)
                        {
                            filename = dirXMLerr + "/ErrorSistema_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            escritor = File.AppendText(filename);
                            escritor.Write(respuetaDIGI.ToString());
                            escritor.Flush();
                            escritor.Close();

                            if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                            {
                                QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','') ";
                            }
                            else
                            {
                                QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','' ";
                            }

                            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            RecSet.DoQuery(QryStr);
                            SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " + ex.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                            return;
                        }
                    }
                    else
                    {
                        StreamWriter escritor;
                        filename = dirXMLerr + "/VerificarError_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor = File.AppendText(filename);
                        escritor.Write("Error de permisos: " + DocNum.ToString());
                        escritor.Flush();
                        escritor.Close();
                        if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','No se pudo guardar el xml, verifique permisos','','','','','','','') ";
                        }
                        else
                        {
                            QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','No se pudo guardar el xml, verifique permisos','','','','','','','' ";
                        }
                        RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        RecSet.DoQuery(QryStr);
                    }
                }
            }
            catch (Exception ex)
            {
                StreamWriter escritor;
                filename = dirXMLerr + "/ErrorSistema_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                escritor = File.AppendText(filename);
                escritor.Write("No.Sap: " + DocNum.ToString() + "Error VB: " + ex.ToString());
                escritor.Flush();
                escritor.Close();
                if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','') ";
                }
                else
                {
                    QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','' ";
                }
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(QryStr);
            }
        }
        public static void EnviaDocumentoFELA(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string Pais, string DocEntry, bool ProcesarBatch = false)
        {
            string certificado = "";
            string passcertificad = "";
            string dirXMLSinAutorizar = "";
            string apikey = "";
            string dirXMLauth = "";
            string dirXMLres = "";
            string dirXMLerr = "";
            string dirXMLPDF = "";
            string Nit;
            string dirUR_t;
            string dirUR_a;
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string xmlResp = "";
            string xmlFile = "";
            string filename;
            string tokenUser;
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
                    dirUR_a = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_a"); // Direccion URL Request
                    dirUR_t = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_t"); // Direccion URL Token
                    dirXMLerr = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(OCompany, SBO_Application, "NitEmi"); // Nit emisor para el token
                    tokenUser = Utils.ObtieneValorParametro(OCompany, SBO_Application, "ApiKey"); // Nit emisor para el token
                    string SerieAprobada;
                    string TipoDocFEL;

                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH("CALL FELONE_DIGIFACT_ANUL (" + DocEntry + ",'" + TipoDocFEL + "'," + Nit + ")");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato("SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato("SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato("EXEC FELONE_DIGIFACT_ANUL " + DocEntry + ",'" + TipoDocFEL + "'," + Nit);
                    }

                    if (Utils.GrabarXml(OCompany, xmlResp, SerieAprobada, DocNum, "Anulacion_", ref xmlFile))
                    {
                        StreamWriter escritor;
                        filename = dirXMLauth + "/Auth_Anulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor = File.AppendText(filename);
                        escritor.Write(xmlResp.ToString());
                        escritor.Flush();
                        escritor.Close();

                        var respuetaDIGI = EnviaDocumentoDIGIA(xmlResp, dirUR_t, dirUR_a, Nit, certificado, passcertificad, tokenUser);
                        JObject jsonResponse = JObject.Parse(respuetaDIGI);
                        List<JToken> data = jsonResponse.Children().ToList();


                        string InnerXMLrespuesta = "1";
                        string desc_error = ""; ;
                        string uuid = "";
                        string serieFel = "";
                        string documentoFel = "";
                        string autorizacionFel = "";
                        string fechaautorizada = "";
                        string FechaHoraEmision = "";

                        foreach (JProperty item in data)
                        {
                            item.CreateReader();
                            switch (item.Name)
                            {
                                case "Codigo":
                                    InnerXMLrespuesta = item.Value.ToString();
                                    break;
                                case "Mensaje":
                                    desc_error = item.Value.ToString();
                                    break;
                                case "AcuseReciboSAT":
                                    autorizacionFel = item.Value.ToString();
                                    break;
                                case "Autorizacion":
                                    uuid = item.Value.ToString();
                                    break;
                                case "Serie":
                                    serieFel = item.Value.ToString();
                                    break;
                                case "NUMERO":
                                    documentoFel = item.Value.ToString();
                                    break;
                                case "Fecha_DTE":
                                    if (DateTime.TryParseExact(item.Value.ToString(), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                                    {
                                        FechaHoraEmision = fecha.ToString("yyyy-MM-ddTHH:mm:ss");
                                    }
                                    else
                                    {
                                        FechaHoraEmision = item.Value.ToString();
                                    }
                                    break;
                                case "Fecha_de_certificacion":
                                    if (DateTime.TryParseExact(item.Value.ToString(), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaA))
                                    {
                                        fechaautorizada = fechaA.ToString("yyyy-MM-ddTHH:mm:ss");
                                    }
                                    else
                                    {
                                        fechaautorizada = item.Value.ToString();
                                    }
                                    break;
                            }
                        }
                        try
                        {
                            if (InnerXMLrespuesta == "1")
                            {
                                try
                                {
                                    filename = dirXMLres + "/AprobadaAnulada_" + CurrSerieName + "_" + DocNum + ".xml";
                                    escritor = File.AppendText(filename);
                                    escritor.Write(respuetaDIGI.ToString());
                                    escritor.Flush();
                                    escritor.Close();

                                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                                    {
                                        QryStr = "CALL FELONE_UTILS ('TrueA','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','','','','','','','') ";
                                    }
                                    else
                                    {
                                        QryStr = "EXEC FELONE_UTILS 'TrueA','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','','','','','','','' ";
                                    }
                                    RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                    RecSet.DoQuery(QryStr);
                                }
                                catch (Exception)
                                {
                                    filename = dirXMLerr + "/ErrorSistemaAprobadaAnulada_" + CurrSerieName + "_" + DocNum + ".xml";
                                    escritor = File.AppendText(filename);
                                    escritor.Write(respuetaDIGI.ToString());
                                    escritor.Flush();
                                    escritor.Close();
                                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                                    {
                                        QryStr = "CALL FELONE_UTILS ('TrueErrorA','" + DocEntry + "','" + Tipo + "','Error Interno Verifique portal web','','','','','','','') ";
                                    }
                                    else
                                    {
                                        QryStr = "EXEC FELONE_UTILS 'TrueErrorA','" + DocEntry + "','" + Tipo + "','Error Interno Verifique portal web','','','','','','','' ";
                                    }
                                    RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                    RecSet.DoQuery(QryStr);
                                }
                            }
                            else if (InnerXMLrespuesta == "0")
                            {
                                // Dim escritor As StreamWriter
                                filename = dirXMLres + "/Error_Anulada_" + CurrSerieName + "_" + DocNum + ".xml";
                                escritor = File.AppendText(filename);
                                escritor.Write(respuetaDIGI.ToString());
                                escritor.Flush();
                                escritor.Close();
                                string errores = "";
                                errores += "Error: No." + InnerXMLrespuesta + "Descripcion: " + desc_error + "|..| ";

                                if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                                {
                                    QryStr = "CALL FELONE_UTILS ('FalseA','" + DocEntry + "','" + Tipo + "','" + errores + "','','','','','','','') ";
                                }
                                else
                                {
                                    QryStr = "EXEC FELONE_UTILS 'FalseA','" + DocEntry + "','" + Tipo + "','" + errores + "','','','','','','','' ";
                                }
                                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                RecSet.DoQuery(QryStr);
                            }
                            else
                            {
                                // VERIFICAR ERRORES FUERA DE RANGO
                                filename = dirXMLerr + "/Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                escritor = File.AppendText(filename);
                                escritor.Write(respuetaDIGI.ToString());
                                escritor.Flush();
                                escritor.Close();

                                string errores = "";
                                errores += "Error: " + InnerXMLrespuesta + " Descripcion: " + desc_error + "|..| ";
                                if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                                {
                                    QryStr = "CALL FELONE_UTILS ('FalseA','" + DocEntry + "','" + Tipo + "','" + errores + "','','','','','','','') ";
                                }
                                else
                                {
                                    QryStr = "EXEC FELONE_UTILS 'FalseA','" + DocEntry + "','" + Tipo + "','" + errores + "','','','','','','','' ";
                                }
                                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                RecSet.DoQuery(QryStr);
                            }
                        }
                        catch (Exception ex)
                        {

                            // Dim escritor As StreamWriter
                            filename = dirXMLerr + "/ErrorSistemaAnulada_" + CurrSerieName + "_" + DocNum + ".xml";
                            escritor = File.AppendText(filename);
                            escritor.Write(respuetaDIGI.ToString());
                            escritor.Flush();
                            escritor.Close();

                            if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                            {
                                QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','') ";
                            }
                            else
                            {
                                QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','' ";
                            }

                            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            RecSet.DoQuery(QryStr);
                            SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " + ex.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                            return;
                        }
                    }
                    else
                    {
                        StreamWriter escritor;
                        filename = dirXMLerr + "/VerificarErrorAnulada_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor = File.AppendText(filename);
                        escritor.Write("Documento se encuentra firmado No.Sap: " + DocNum.ToString());
                        escritor.Flush();
                        escritor.Close();
                        if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','No se pudo guardar el xml, verifique permisos','','','','','','','') ";
                        }
                        else
                        {
                            QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','No se pudo guardar el xml, verifique permisos','','','','','','','' ";
                        }
                        RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        RecSet.DoQuery(QryStr);
                    }
                }
            }
            catch (Exception ex)
            {
                StreamWriter escritor;
                filename = dirXMLerr + "/ErrorSistema_" + CurrSerieName + "_" + DocNum + ".xml";
                escritor = File.AppendText(filename);
                escritor.Write("No.Sap: " + DocNum.ToString() + "Error VB: " + ex.ToString());
                escritor.Flush();
                escritor.Close();
                if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','') ";
                }
                else
                {
                    QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','' ";
                }
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(QryStr);
            }
        }

        public static string SolicitarTokenJSON( string user, string pass, string UR_t)
        {

            string JSON = @"
                {
                    ""Username"":""" + user + @""",
                    ""Password"":""" + pass + @"""
                }";
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = (HttpWebRequest)WebRequest.Create(UR_t);
            string respuesta = null;
            var postData = "";
            postData = JSON;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            var response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            dataStream.Close();
            JObject json2 = JObject.Parse(responseString);
            List<JToken> data = json2.Children().ToList();

            foreach (JProperty item in data)
            {
                item.CreateReader();
                switch (item.Name)
                {
                    case "Token":
                        respuesta = item.Value.ToString();
                        break;
                }
            }
            return respuesta;
        }
        public static string EnviaDocumentoDIGI(string XML, string UR_t, string UR_r, string nit, string user, string pass, string tokenUser)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            token = SolicitarTokenJSON(tokenUser, pass, UR_t);
            var @params = "?NIT=" + nit + "&TIPO=CERTIFICATE_DTE_XML_TOSIGN&FORMAT=XML,PDF&USERNAME=" + user + "";
            WebRequest request = WebRequest.Create(UR_r + @params);
            var postData = "";
            UTF8Encoding enc;
            enc = new System.Text.UTF8Encoding();
            postData = XML;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(postData);
            var data = enc.GetBytes(xmlDoc.InnerXml);
            request.Headers.Add("authorization", "Bearer " + token.ToString());
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (var Stream = request.GetRequestStream())
            {
                Stream.Write(data, 0, data.Length);
            }
            try
            {
                WebResponse response = request.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch (WebException ex)
            {
                WebResponse response = ex.Response;
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
        }
        public static string EnviaDocumentoDIGIA(string XML, string UR_t, string UR_a, string nit, string user, string pass, string tokenUser)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            token = SolicitarTokenJSON(tokenUser, pass, UR_t);
            string link = UR_a + "?NIT=" + nit + "&TIPO=ANULAR_FEL_TOSIGN&FORMAT=XML&USERNAME=" + user + "";
            WebRequest request = WebRequest.Create(link);
            var postData = "";
            UTF8Encoding enc;
            enc = new System.Text.UTF8Encoding();
            postData = XML;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(postData);
            var data = enc.GetBytes(xmlDoc.InnerXml);
            request.Headers.Add("authorization", "Bearer " + token.ToString());
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (var Stream = request.GetRequestStream())
            {
                Stream.Write(data, 0, data.Length);
            }
            try
            {
                WebResponse response = request.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch (WebException ex)
            {
                WebResponse response = ex.Response;
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
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
                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    QryStr = "Select * from \"@FEL_PARAMETROS\"";
                }
                else
                {
                    QryStr = "Select * from [@FEL_PARAMETROS]";
                }
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
                        case "UR_a":
                            oItem = oForm.Items.Item("UR_a");
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
                        case "ApiKey":
                            oItem = oForm.Items.Item("ApiKey");
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
                        case "Unique":
                            oItem = oForm.Items.Item("Unique");
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

                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                RecSet = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
            }
        }
        public static void GuardarParametros(SAPbobsCOM.Company oCompany, SAPbouiCOM.Form oForm, SAPbouiCOM.Application SBO_Application)
        {
            SAPbobsCOM.UserTable oUsrTbl;
            SAPbouiCOM.EditText oEdit;
            SAPbouiCOM.ComboBox oComboBox;
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

                oItem = oForm.Items.Item("Unique");
                oComboBox = oItem.Specific;
                if (oComboBox.Value.ToString().Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    oComboBox.Active = true;
                    oItem = oForm.Items.Item("tabCNN");
                    oTab = oItem.Specific;
                    oTab.Select();
                    throw new Exception("Debe de definir si el documento se validara en FEEL");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "Unique", oComboBox.Value.ToString().Trim());
                ProgressBar.Value += 1;

                ParametrosForm.GuardaDatosSeries();
                ProgressBar.Value += 1;
                ProgressBar.Stop();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ProgressBar);
                ProgressBar = null;
                GC.Collect();
                SBO_Application.SetStatusBarMessage("Parámetros guardados exítosamente", SAPbouiCOM.BoMessageTime.bmt_Short, false);
            }
            catch (Exception ex)
            {
                ProgressBar.Stop();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ProgressBar);
                ProgressBar = null/* TODO Change to default(_) if this is not a reference type */;
                GC.Collect();
                SBO_Application.MessageBox(ex.Message);
            }
        } 

    }
}