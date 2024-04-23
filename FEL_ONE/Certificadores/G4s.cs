using FEL_ONE.Clases;
using FEL_ONE.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FEL_ONE.Certificadores
{
    class G4s
    {
        public static void EnviaDocumentoFEL(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string Pais, string DocEntry, bool ProcesarBatch = false)
        {
            string dirXMLSinAutorizar = "";
            string dirXMLauth = "";
            string dirXMLres = "";
            string dirXMLerr = "";
            string dirXMLPDF = "";

            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string xmlResp = "";
            string xmlResA = "";
            string xmlFile = "";
            string filename;

            string TipoDocFEL;
            string SerieAprobada;
            // G4S
            string paramrequestor = "";
            string paramtrans = "";
            string parampais = "";
            string paramentity = "";
            string paramusername = "";
            string paramdata1 = "";
            string paramdata3 = "";
            string paramurl = "";
            // FEL
            string uuid = "";
            string serieFel = "";
            string documentoFel = "";
            string fechaFel = "";
            string errorFEL = "";
            try
            {
                if (Utils.ValidaSerie(OCompany, SBO_Application, CurrSerie, ProcesarBatch) && Utils.ExisteDocumento(OCompany, SBO_Application, CurrSerie, DocEntry, Tipo, ProcesarBatch))
                {

                    // Direcciones xml
                    dirXMLSinAutorizar = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXML"); // Direccion xml Sin autorizar
                    dirXMLPDF = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHPDF"); // Direccion xml pdf
                    dirXMLauth = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLaut"); // Direccion xml Autorizado
                    dirXMLres = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLres"); // Direccion xml Respuesta
                    dirXMLerr = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error

                    // G4S
                    paramrequestor = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Requestor");
                    paramtrans = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Trans");
                    parampais = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Country");
                    paramentity = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Entity");
                    paramusername = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UserName");
                    paramdata1 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Data1");
                    paramdata3 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Data3");
                    paramurl = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_WS");

                    // SAP
                    if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                    {
                        SerieAprobada = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH("CALL FELONE_G4S_" + TipoDocFEL + " (" + DocEntry + ",'" + TipoDocFEL + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato("SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato("SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato("EXEC  [dbo].[FELONE_G4S_" + TipoDocFEL + "] " + DocEntry + ",'1'");
                    }



                    string base64EncodedCert;
                    byte[] dataCert;
                    dataCert = System.Text.ASCIIEncoding.ASCII.GetBytes(xmlResp);
                    base64EncodedCert = System.Convert.ToBase64String(dataCert);
                    com.g4sdocumenta.pruebasfel.FactWSFront G4S = new com.g4sdocumenta.pruebasfel.FactWSFront();
                    G4S.Url = paramurl;
                    var resultadojsonCert = G4S.RequestTransaction(paramrequestor, paramtrans, parampais, paramentity, paramrequestor, paramusername, paramdata1, base64EncodedCert, paramdata3);



                    if (resultadojsonCert.Response.Result == true)
                    {
                        string xml = Encoding.UTF8.GetString(Convert.FromBase64String(resultadojsonCert.ResponseData.ResponseData1.ToString()));
                        XmlDocument xmlRespuesta = new XmlDocument();
                        xmlRespuesta.LoadXml(xml.Replace("dte:", ""));

                        xmlResA = xmlRespuesta.InnerXml.ToString();

                        XmlNodeList RegCatNodesListRespuesta = xmlRespuesta.SelectNodes("GTDocumento/SAT/DTE/Certificacion");
                        foreach (XmlNode xnDet in RegCatNodesListRespuesta)
                        {
                            serieFel = xnDet.SelectSingleNode("NumeroAutorizacion").Attributes[0].Value.ToString();
                            documentoFel = xnDet.SelectSingleNode("NumeroAutorizacion").Attributes[1].Value.ToString();
                            uuid = xnDet.SelectSingleNode("NumeroAutorizacion").InnerText;
                            fechaFel = xnDet.SelectSingleNode("FechaHoraCertificacion").InnerText;
                        }
                    }
                    else
                        errorFEL = resultadojsonCert.Response.Data.ToString();

                    if (Utils.GrabarXml(OCompany, xmlResp, SerieAprobada, DocNum, Tipo, ref xmlFile))
                    {
                        if (resultadojsonCert.Response.Result == true )
                        {
                            if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                                QryStr = "CALL FELONE_UTILS ('True','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','" + documentoFel + "','" + serieFel + "','','" + fechaFel + "','','','') ";
                            else
                                QryStr = "EXEC FELONE_UTILS 'True','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','" + documentoFel + "','" + serieFel + "','','" + fechaFel + "','','','' ";

                            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            RecSet.DoQuery(QryStr);

                            StreamWriter escritor;
                            filename = dirXMLres + @"\Aprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            escritor = File.AppendText(filename);
                            escritor.Write(xmlResp.ToString());
                            escritor.Flush();
                            escritor.Close();

                            StreamWriter escritor2;
                            filename = dirXMLauth + @"\Autorizada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            escritor2 = File.AppendText(filename);
                            escritor2.Write(xmlResA.ToString());
                            escritor2.Flush();
                            escritor2.Close();
                            
                        }
                        else
                        {
                            if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                                QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','" + errorFEL.Replace("'", "") + "','','','','','','','') ";
                            else
                                QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','" + errorFEL.Replace("'", "") + "','','','','','','','' ";
                            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            RecSet.DoQuery(QryStr);

                            StreamWriter escritor;
                            filename = dirXMLerr + @"\Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            escritor = File.AppendText(filename);
                            escritor.Write(errorFEL.Replace("'", "").ToString());
                            escritor.Flush();
                            escritor.Close();                            
                        }
                    }
                    else if (resultadojsonCert.Response.Result == true)
                    {
                        if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                            QryStr = "CALL FELONE_UTILS ('True','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','" + documentoFel + "','" + serieFel + "','','" + fechaFel + "','','','') ";
                        else
                            QryStr = "EXEC FELONE_UTILS 'True','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','" + documentoFel + "','" + serieFel + "','','" + fechaFel + "','','','' ";
                        RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        RecSet.DoQuery(QryStr);

                        StreamWriter escritor;
                        filename = dirXMLres + @"\Aprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor = File.AppendText(filename);
                        escritor.Write(xmlResp.ToString());
                        escritor.Flush();
                        escritor.Close();

                        StreamWriter escritor2;
                        filename = dirXMLauth + @"\Autorizada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor2 = File.AppendText(filename);
                        escritor2.Write(xmlResA.ToString());
                        escritor2.Flush();
                        escritor2.Close();
                        
                    }
                    else
                    {
                        if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                            QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','" + errorFEL.Replace("'", "") + "','','','','','','','') ";
                        else
                            QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','" + errorFEL.Replace("'", "") + "','','','','','','','' ";
                        RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        RecSet.DoQuery(QryStr);

                        StreamWriter escritor;
                        filename = dirXMLerr + @"\Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor = File.AppendText(filename);
                        escritor.Write(errorFEL.Replace("'", "").ToString());
                        escritor.Flush();
                        escritor.Close();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                StreamWriter escritor;
                filename = dirXMLerr + @"\ErrorSistema_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                escritor = File.AppendText(filename);
                escritor.Write("No.Sap: " + DocNum.ToString() + "Error VB: " + ex.ToString());
                escritor.Flush();
                escritor.Close();
                if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                    QryStr = "CALL FELONE_UTILS ('TrueError','" + DocEntry + "','" + Tipo + "','" + ex.ToString() + "','','','','','','','') ";
                else
                    QryStr = "EXEC FELONE_UTILS 'TrueError','" + DocEntry + "','" + Tipo + "','" + ex.ToString() + "','','','','','','','' ";
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(QryStr);
                if (ProcesarBatch == false)
                    SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short);
            }
        }
        public static void EnviaDocumentoFELA(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string Pais, string DocEntry, bool ProcesarBatch = false)
        {
            string dirXMLSinAutorizar = "";
            string dirXMLauth = "";
            string dirXMLres = "";
            string dirXMLerr = "";
            string dirXMLPDF = "";

            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string xmlResp = "";
            string xmlResA = "";
            string xmlFile = "";
            string filename;
            string SerieAprobada;

            string TipoDocFEL;
            // G4S
            string paramrequestor = "";
            string paramtrans = "";
            string parampais = "";
            string paramentity = "";
            string paramusername = "";
            string paramdata1 = "";
            string paramdata3 = "";
            string paramurl = "";
            // FEL
            string uuid = "";
            string fechaFel = "";
            string errorFEL = "";
            try
            {
                if (Utils.ValidaSerie(OCompany, SBO_Application, CurrSerie, ProcesarBatch) && Utils.ExisteDocumentoANULAR(OCompany, SBO_Application, CurrSerie, DocEntry, Tipo))
                {
                    // Direcciones xml
                    dirXMLSinAutorizar = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Pathsaut"); // Direccion xml Sin autorizar
                    dirXMLPDF = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHPDF"); // Direccion xml pdf
                    dirXMLauth = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Pathaut"); // Direccion xml Autorizado
                    dirXMLres = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Pathres"); // Direccion xml Respuesta
                    dirXMLerr = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                                                                                                // G4S
                    paramrequestor = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Requestor");
                    paramtrans = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Trans");
                    parampais = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Country");
                    paramentity = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Entity");
                    paramusername = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UserName");
                    paramdata1 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Data1");
                    paramdata3 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Data3");
                    paramurl = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_WS");
                    // SAP



                    if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                    {
                        SerieAprobada = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH("CALL FELONE_G4S_ANUL (" + DocEntry + ",'" + TipoDocFEL + "','" + paramentity + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        xmlResp = Utils.TraeDato("EXEC FELONE_G4S_ANUL " + DocEntry + ",'" + TipoDocFEL + "','" + paramentity + "'");
                    }



                    string base64EncodedCert;
                    byte[] dataCert;
                    dataCert = System.Text.ASCIIEncoding.ASCII.GetBytes(xmlResp);
                    base64EncodedCert = System.Convert.ToBase64String(dataCert);
                    com.g4sdocumenta.pruebasfel.FactWSFront G4S = new com.g4sdocumenta.pruebasfel.FactWSFront();
                    G4S.Url = paramurl;
                    var resultadojsonCert = G4S.RequestTransaction(paramrequestor, paramtrans, parampais, paramentity, paramrequestor, paramusername, "VOID_DOCUMENT", base64EncodedCert, "XML");

                    if (resultadojsonCert.Response.Result == true)
                    {
                        string xml = Encoding.UTF8.GetString(Convert.FromBase64String(resultadojsonCert.ResponseData.ResponseData1.ToString()));

                        XmlDocument xmlRespuesta = new XmlDocument();
                        xmlRespuesta.LoadXml(xml.Replace("dte:", ""));

                        xmlResA = xmlRespuesta.InnerXml.ToString();

                        XmlNodeList RegCatNodesListRespuesta = xmlRespuesta.SelectNodes("GTAnulacionDocumento/SAT/AnulacionDTE/Certificacion");
                        foreach (XmlNode xnDet in RegCatNodesListRespuesta)
                            fechaFel = xnDet.SelectSingleNode("FechaHoraCertificacion").InnerText;
                    }
                    else
                        errorFEL = resultadojsonCert.Response.Data.ToString();

                    if (Utils.GrabarXml(OCompany, xmlResp, SerieAprobada, DocNum, Tipo + "_ANUL", ref xmlFile))
                    {
                        if (resultadojsonCert.Response.Result == true)
                        {
                            if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                                QryStr = "CALL FELONE_UTILS ('TrueA','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','','','','','','','') ";
                            else
                                QryStr = "EXEC FELONE_UTILS 'TrueA','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','','','','','','','' ";
                            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            RecSet.DoQuery(QryStr);

                            StreamWriter escritor;
                            filename = dirXMLres + @"\ANUL_Aprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            escritor = File.AppendText(filename);
                            escritor.Write(xmlResp.ToString());
                            escritor.Flush();
                            escritor.Close();

                            StreamWriter escritor2;
                            filename = dirXMLauth + @"\ANUL_Autorizada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            escritor2 = File.AppendText(filename);
                            escritor2.Write(xmlResA.ToString());
                            escritor2.Flush();
                            escritor2.Close();                          
                        }
                        else
                        {
                            if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                                QryStr = "CALL FELONE_UTILS ('FalseA','" + DocEntry + "','" + Tipo + "','" + errorFEL.Replace("'", "") + "','','','','','','','') ";
                            else
                                QryStr = "EXEC FELONE_UTILS 'FalseA','" + DocEntry + "','" + Tipo + "','" + errorFEL.Replace("'", "") + "','','','','','','','' ";
                            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            RecSet.DoQuery(QryStr);

                            StreamWriter escritor;
                            filename = dirXMLerr + @"\ANUL_Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            escritor = File.AppendText(filename);
                            escritor.Write(xmlResp.ToString());
                            escritor.Flush();
                            escritor.Close();
                            
                        }
                    }
                    else if (resultadojsonCert.Response.Result == true)
                    {
                        if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                            QryStr = "CALL FELONE_UTILS ('TrueA','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','','','','','','','') ";
                        else
                            QryStr = "EXEC FELONE_UTILS 'TrueA','" + DocEntry + "','" + Tipo + "','" + uuid.ToString() + "','','','','','','','' ";
                        RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        RecSet.DoQuery(QryStr);

                        StreamWriter escritor;
                        filename = dirXMLres + @"\ANUL_Aprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor = File.AppendText(filename);
                        escritor.Write(xmlResp.ToString());
                        escritor.Flush();
                        escritor.Close();

                        StreamWriter escritor2;
                        filename = dirXMLauth + @"\ANUL_Autorizada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor2 = File.AppendText(filename);
                        escritor2.Write(xmlResA.ToString());
                        escritor2.Flush();
                        escritor2.Close();
                        
                    }
                    else
                    {
                        if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                            QryStr = "CALL FELONE_UTILS ('FalseA','" + DocEntry + "','" + Tipo + "','" + errorFEL.Replace("'", "") + "','','','','','','','') ";
                        else
                            QryStr = "EXEC FELONE_UTILS 'FalseA','" + DocEntry + "','" + Tipo + "','" + errorFEL.Replace("'", "") + "','','','','','','','' ";
                        RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        RecSet.DoQuery(QryStr);

                        StreamWriter escritor;
                        filename = dirXMLerr + @"\ANUL_Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        escritor = File.AppendText(filename);
                        escritor.Write(xmlResp.ToString());
                        escritor.Flush();
                        escritor.Close();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                if ((OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                    QryStr = "CALL FELONE_UTILS ('False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','') ";
                else
                    QryStr = "EXEC FELONE_UTILS 'False','" + DocEntry + "','" + Tipo + "','" + ex.Message.ToString() + "','','','','','','','' ";
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(QryStr);
                if (ProcesarBatch == false)
                    SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short);

                StreamWriter escritor;
                filename = dirXMLerr + @"\ANUL_ErrorSistema_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                escritor = File.AppendText(filename);
                escritor.Write("No.Sap: " + DocNum.ToString() + "Error VB: " + ex.ToString());
                escritor.Flush();
                escritor.Close();
                
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
                    QryStr = "Select * from \"@FEL_PARAMETROS\"";
                else
                    QryStr = "Select * from [@FEL_PARAMETROS]";

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
                        case "Requestor":
                            oItem = oForm.Items.Item("Requestor");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Trans":
                            oItem = oForm.Items.Item("Trans");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UR_WS":
                            oItem = oForm.Items.Item("UR_WS");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Country":
                            oItem = oForm.Items.Item("Country");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Entity":
                            oItem = oForm.Items.Item("Entity");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UserName":
                            oItem = oForm.Items.Item("UserName");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Data1":
                            oItem = oForm.Items.Item("Data1");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Data3":
                            oItem = oForm.Items.Item("Data3");
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

                oItem = oForm.Items.Item("Requestor");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "Requestor", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Trans");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "Trans", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("UR_WS");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_WS", oEdit.Value);
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

                oItem = oForm.Items.Item("Country");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "Country", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Entity");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "Entity", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("UserName");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "UserName", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Data1");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "Data1", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("Data3");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "Data3", oEdit.Value);
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
                ProgressBar = null;
                GC.Collect();
                SBO_Application.MessageBox(ex.Message);
            }
        }
    }
}
