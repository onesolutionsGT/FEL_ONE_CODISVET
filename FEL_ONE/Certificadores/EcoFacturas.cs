using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FEL_ONE.Clases;
using SAPbobsCOM;
using SAPbouiCOM;
using System.Globalization;
using FEL_ONE.Forms;

namespace FEL_ONE.Certificadores
{
    public class EcoFacturas
    {
        // Implementacion FEL Ecofacturas
        public static void EnviaDocumentoFEL(SAPbobsCOM.Company oCompany, Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string pais, string DocEntry, bool ProcesarBatch = false)
        {
            string Respuesta = "";
            string certificado = "";
            string passcertificad = "";
            string dirXMLSinAutorizar = "";
            string dirXMLauth = "";
            string dirXMLres = "";
            string dirXMLerr = "";
            string dirXMLPDF = "";
            string Nit;
            string email;
            string dirUR_ci;
            string dirUR_r;
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string xmlResp = "";
            string xmlFile = "";
            string filename;
            string filenamePDF;
            CultureInfo provider = CultureInfo.InvariantCulture;

            try
            {
                if (Utils.ExisteDocumento(oCompany, SBO_Application, CurrSerie, DocEntry, Tipo, ProcesarBatch))
                {
                    dirXMLSinAutorizar = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXML"); // Direccion xml Sin autorizar
                    dirXMLPDF = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHPDF"); // Direccion xml pdf
                    certificado = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLc"); // Direccion Certificado
                    passcertificad = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLcp"); // Contraseña Certificado
                    dirXMLauth = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLaut"); // Direccion xml Autorizado
                    dirXMLres = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLres"); // Direccion xml Respuesta
                    dirUR_r = Utils.ObtieneValorParametro(oCompany, SBO_Application, "UR_r"); // Direccion URL Request
                    dirUR_ci = Utils.ObtieneValorParametro(oCompany, SBO_Application, "UR_ci"); // Direccion URL Token
                    dirXMLerr = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(oCompany, SBO_Application, "NitEmi"); // Nit emisor para el token
                    email = Utils.ObtieneValorParametro(oCompany, SBO_Application, "Correo"); // Correo cliente
                    string SerieAprobada;
                    string TipoDocFEL;

                    if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH(@"SELECT ""U_SERIE"" FROM ""@FEL_RESOLUCION"" WHERE ""U_SERIE"" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH(@"SELECT ""U_TIPO_DOC"" FROM ""@FEL_RESOLUCION"" WHERE ""U_SERIE"" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH(@"CALL FELONE_ECOFACTURAS_" + TipoDocFEL + " (" + DocEntry + ",'" + TipoDocFEL + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato(@"SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato(@"SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato(@"EXEC FELONE_ECOFACTURAS_" + TipoDocFEL + " " + DocEntry + ",'1'");
                    }

                    string rutasalidacertificado;

                    XmlDocument XmlDocumento = new XmlDocument();
                    XmlDocumento.LoadXml(xmlResp);

                    if (Utils.GrabarXml(oCompany, xmlResp, SerieAprobada, DocNum, Tipo, ref xmlFile))
                    {
                        rutasalidacertificado = xmlFile;
                        XmlDocument XmlRespuesta = new XmlDocument();
                        if (dirUR_r.Contains("pruebas.ecofactura.com.gt:8080"))
                        {
                            FEL_ONE.DocumentoTest.DocumentoSoapPortClient ws = new DocumentoTest.DocumentoSoapPortClient();
                            Respuesta = ws.Execute(Nit, certificado, passcertificad, Nit, XmlDocumento.InnerXml);
                            XmlRespuesta.LoadXml(Respuesta);
                        }
                        else
                        {
                            FEL_ONE.Documento.DocumentoSoapPortClient ws = new Documento.DocumentoSoapPortClient();
                            Respuesta = ws.Execute(Nit, certificado, passcertificad, Nit, XmlDocumento.InnerXml);
                            XmlRespuesta.LoadXml(Respuesta);
                        }

                        XmlNodeList XmlDatos = XmlRespuesta.GetElementsByTagName("DTE");
                        XmlNodeList XmlDatos2 = XmlRespuesta.GetElementsByTagName("Error");

                        try
                        {
                            if (XmlDatos.Count == 1)
                            {
                                try
                                {

                                    string fechaemision = XmlDatos[0].Attributes["FechaCertificacion"].Value.Substring(0, 10);
                                    filenamePDF = dirXMLPDF + @"\" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + DateTime.Now.ToString("-yyyy-MM-ddTHH-mm-ss") + ".pdf";

                                    string data = XmlRespuesta.ToString();
                                    filename = dirXMLres + "/Aprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "True", XmlDatos[0].Attributes["NumeroAutorizacion"].Value, XmlDatos[0].Attributes["Numero"].Value, XmlDatos[0].Attributes["Serie"].Value, filenamePDF, XmlDatos[0].Attributes["FechaCertificacion"].Value, fechaemision, "", "");


                                    XmlNodeList xmlPdf = ((XmlElement)XmlDatos[0]).GetElementsByTagName("Pdf");

                                    byte[] bytes = Convert.FromBase64String(xmlPdf[0].InnerText);

                                    System.IO.FileStream stream = new FileStream(filenamePDF, FileMode.CreateNew);
                                    System.IO.BinaryWriter writer = new BinaryWriter(stream);
                                    writer.Write(bytes, 0, bytes.Length);
                                    writer.Close();


                                    //(denarium) actualizacion de referencia de documento 
                                    SAPbobsCOM.Documents oInv;
                                    oInv = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);
                                    oInv.GetByKey(Convert.ToInt32(DocEntry));
                                    oInv.NumAtCard = XmlDatos[0].Attributes["Numero"].Value;
                                    oInv.Update();
                                }
                                catch (Exception)
                                {
                                    string data = XmlRespuesta.ToString();
                                    filename = dirXMLerr + "/ErrorSistemaAprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "TrueError", "Error Interno Verifique portal web", "", "", "", "", "", "", "");

                                }
                            }
                            if (XmlDatos2.Count >= 1)
                            {
                                string errores;
                                errores = XmlDatos2[0].Attributes["Codigo"].Value + " - " + XmlDatos2[0].InnerText + "|..| ";
                                for (int i = 1; i < XmlDatos2.Count; i++) { errores += XmlDatos2[i].Attributes["Codigo"].Value + " - " + XmlDatos2[i].InnerText + "|..| "; }

                                string data = XmlRespuesta.ToString();
                                filename = dirXMLerr + "/Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", errores.Replace("'", ""), "", "", "", "", "", "", "");
                            }
                        }
                        catch (Exception ex)
                        {
                            string data = XmlRespuesta.ToString();
                            filename = dirXMLerr + "/ErrorSistema_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
                        }
                    }
                    else
                    {
                        string data = "Error de permisos: " + DocNum.ToString();
                        filename = dirXMLerr + "/VerificarError_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", "No se pudo guardar el xml, verifique permisos", "", "", "", "", "", "", "");
                    }
                }
            }
            catch (Exception ex)
            {
                string data = "<error><No.Sap> " + DocNum.ToString() + "</No.Sap> <ErrorCS> " + ex.ToString() + "</ErrorCS></error>";
                filename = dirXMLerr + "/ErrorSistema_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
            }

        }

        public static void EnviaDocumentoFELA(SAPbobsCOM.Company oCompany, Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string pais, string DocEntry, bool procesarBatch)
        {
            string Respuesta = "";
            string certificado = "";
            string passcertificad = "";
            string dirXMLSinAutorizar = "";
            string dirXMLauth = "";
            string dirXMLres = "";
            string dirXMLerr = "";
            string dirXMLPDF = "";
            string Nit;
            string email;
            string dirUR_a;
            string dirUR_ci;
            string dirUR_r;
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string xmlResp = "";
            string xmlFile = "";
            string filename;
            string filenamePDF;
            try
            {
                if (Utils.ExisteDocumentoANULAR(oCompany, SBO_Application, CurrSerie, DocEntry, Tipo))
                {
                    dirXMLSinAutorizar = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXML"); // Direccion xml Sin autorizar
                    dirXMLPDF = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHPDF"); // Direccion xml pdf
                    certificado = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLc"); // Direccion Certificado
                    passcertificad = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLcp"); // Contraseña Certificado
                    dirXMLauth = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLaut"); // Direccion xml Autorizado
                    dirXMLres = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLres"); // Direccion xml Respuesta
                    dirUR_a = Utils.ObtieneValorParametro(oCompany, SBO_Application, "UR_a");
                    dirUR_r = Utils.ObtieneValorParametro(oCompany, SBO_Application, "UR_r"); // Direccion URL Request
                    dirUR_ci = Utils.ObtieneValorParametro(oCompany, SBO_Application, "UR_ci"); // Direccion URL Token
                    dirXMLerr = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(oCompany, SBO_Application, "NitEmi"); // Nit emisor para el token
                    email = Utils.ObtieneValorParametro(oCompany, SBO_Application, "Correo"); // Correo cliente
                    string SerieAprobada;
                    string TipoDocFEL;

                    if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH(@"SELECT ""U_SERIE"" FROM ""@FEL_RESOLUCION"" WHERE ""U_SERIE"" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH(@"SELECT ""U_TIPO_DOC"" FROM ""@FEL_RESOLUCION"" WHERE ""U_SERIE"" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH(@"CALL FELONE_ECOFACTURAS_ANUL(" + DocEntry + ",'" + TipoDocFEL + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato(@"SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato(@"SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato(@"EXEC FELONE_ECOFACTURAS_ANUL " + DocEntry + ",'" + TipoDocFEL + "'");
                    }

                    XmlDocument XmlRespuesta = new XmlDocument();

                    if (dirUR_r.Contains("pruebas.ecofactura.com.gt:8080"))
                    {
                        FEL_ONE.AnulacionTest.AnulacionSoapPortClient Anula = new AnulacionTest.AnulacionSoapPortClient();
                        Respuesta = Anula.Execute(Nit, certificado, passcertificad, Nit, xmlResp, "DOCUMENTO ANULADO");
                        XmlRespuesta.LoadXml(Respuesta);
                    }
                    else
                    {
                        FEL_ONE.Anulacion.AnulacionSoapPortClient Anula = new Anulacion.AnulacionSoapPortClient();
                        Respuesta = Anula.Execute(Nit, certificado, passcertificad, Nit, xmlResp, "DOCUMENTO ANULADO");
                        XmlRespuesta.LoadXml(Respuesta);
                    }


                    XmlRespuesta.LoadXml(Respuesta);



                    if (Utils.GrabarXml(oCompany, "<xml>" + xmlResp + "</xml>", SerieAprobada, DocNum, Tipo + "_ANUL", ref xmlFile))
                    {

                        try
                        {
                            XmlNodeList XmlDatos = XmlRespuesta.GetElementsByTagName("DTE");
                            if (XmlDatos.Count == 1)
                            {
                                try
                                {

                                    filenamePDF = dirXMLPDF + @"\" + "Anulado_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + "_" + xmlResp + ".pdf";
                                    XmlNodeList xmlPdf = ((XmlElement)XmlDatos[0]).GetElementsByTagName("Pdf");

                                    byte[] bytes = Convert.FromBase64String(xmlPdf[0].InnerText);
                                    System.IO.FileStream stream = new FileStream(filenamePDF, FileMode.CreateNew);
                                    System.IO.BinaryWriter writer = new BinaryWriter(stream);
                                    writer.Write(bytes, 0, bytes.Length);
                                    writer.Close();

                                    string data = XmlRespuesta.ToString();
                                    filename = dirXMLres + "/Anulada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "TrueA", "", "", "", filenamePDF, "", "", "", "");
                                }
                                catch (Exception)
                                {
                                    string data = XmlRespuesta.ToString();
                                    filename = dirXMLerr + "/ErrorSistemaAnulada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "TrueErrorA", "Error Interno Verifique portal web", "", "", "", "", "", "", "");
                                }
                            }
                            XmlNodeList XmlDatos2 = XmlRespuesta.GetElementsByTagName("Error");
                            if (XmlDatos2.Count >= 1)
                            {
                                string errores;
                                errores = XmlDatos2[0].Attributes["Codigo"].Value + " - " + XmlDatos2[0].InnerText + "|..| ";
                                for (int i = 1; i < XmlDatos2.Count; i++) { errores += XmlDatos2[i].Attributes["Codigo"].Value + " - " + XmlDatos2[i].InnerText + "|..| "; }

                                string data = XmlRespuesta.ToString();
                                filename = dirXMLerr + "/Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", errores.Replace("'", ""), "", "", "", "", "", "", "");
                            }
                        }
                        catch (Exception ex)
                        {
                            string data = XmlRespuesta.ToString();
                            filename = dirXMLerr + "/ErrorSistemaAnulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString(), "", "", "", "", "", "", "");
                            SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " + ex.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                        }
                    }
                    else
                    {
                        string data = "Documento se encuentra firmado No.Sap: " + DocNum.ToString();
                        filename = dirXMLerr + "/VerificarErrorAnulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "FalseA", "No se pudo guardar el xml, verifique permisos", "", "", "", "", "", "", "");
                    }
                }
            }
            catch (Exception ex)
            {
                string data = "<error><No.Sap> " + DocNum.ToString() + "</No.Sap> <ErrorCS> " + ex.ToString() + "</ErrorCS></error>";
                filename = dirXMLerr + "/ErrorSistemaAnulacion_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
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

                oItem = oForm.Items.Item("UR_ci");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_ci", oEdit.Value);
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

                oItem = oForm.Items.Item("codcom");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "CodCom", oEdit.Value);
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
                        case "UR_ci":
                            oItem = oForm.Items.Item("UR_ci");
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
                        case "CodCom":
                            oItem = oForm.Items.Item("codcom");
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

    }
}
