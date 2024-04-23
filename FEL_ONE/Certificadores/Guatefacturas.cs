
using FEL_ONE.Clases;
using FEL_ONE.Forms;
using Org.BouncyCastle.Utilities;
using SAPbobsCOM;
using SAPbouiCOM;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;

namespace FEL_ONE.Certificadores
{
    class Guatefacturas
    {
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
            string usuario = "";
            string password = "";
            string tipo_Resuesta = "";
            string Nit;
            string dirUR_t;
            string xmlResp = "";
            string xmlFile = "";
            string filename;
            int tipoDocFelGuate;
            XmlDataDocument document = new XmlDataDocument();
            try
            {
                if (Utils.ExisteDocumento(oCompany, SBO_Application, CurrSerie, DocEntry, Tipo, ProcesarBatch))
                {
                    dirXMLSinAutorizar = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXML"); // Direccion xml Sin autorizar
                    dirXMLPDF = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHPDF"); // Direccion xml pdf
                    certificado = Utils.ObtieneValorParametro(oCompany, SBO_Application, "USR_WS"); // Direccion Certificado
                    passcertificad = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PASS_WS"); // Contraseña Certificado
                    dirXMLauth = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLaut"); // Direccion xml Autorizado
                    dirXMLres = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLres"); // Direccion xml Respuesta
                    dirUR_t = Utils.ObtieneValorParametro(oCompany, SBO_Application, "UR_t"); // Direccion URL Token
                    usuario = Utils.ObtieneValorParametro(oCompany, SBO_Application, "usuario"); // usuario cliente ws
                    password = Utils.ObtieneValorParametro(oCompany, SBO_Application, "password"); // Tipo de respuesta esperada Default R
                    tipo_Resuesta = Utils.ObtieneValorParametro(oCompany, SBO_Application, "tipo_resp"); // password cliente ws
                    dirXMLerr = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(oCompany, SBO_Application, "NitEmi"); // Nit emisor para el token

                    string SerieAprobada;
                    string TipoDocFEL;

                    if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH(@"SELECT ""U_SERIE"" FROM ""@FEL_RESOLUCION"" WHERE ""U_SERIE"" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH(@"SELECT ""U_TIPO_DOC"" FROM ""@FEL_RESOLUCION"" WHERE ""U_SERIE"" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH(@"CALL FELONE_GUATE_" + TipoDocFEL + " (" + DocEntry + ",'" + TipoDocFEL + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato(@"SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato(@"SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato(@"EXEC FELONE_GUATE_" + TipoDocFEL + " " + DocEntry + ",'1'");
                    }

                    XmlDocument XmlRespuesta = new XmlDocument();

                    if (Utils.GrabarXml(oCompany, xmlResp, SerieAprobada, DocNum, TipoDocFEL, ref xmlFile))
                    {
                        if (TipoDocFEL == "FEXP") { TipoDocFEL = "FACT"; }
                        string[] TypeGuateFact = new string[] { "FACT", "FCAM", "FPEQ", "FCAP", "FESP", "NABN", "RDON", "RECI", "NDEB", "NCRE" };
                        tipoDocFelGuate = Array.IndexOf(TypeGuateFact, TipoDocFEL) + 1;


                        com.guatefacturas.dte.Guatefac WS = new com.guatefacturas.dte.Guatefac();
                        WS.Url = dirUR_t;
                        WS.Timeout = 800000;
                        WS.CookieContainer = new System.Net.CookieContainer();
                        WS.UnsafeAuthenticatedConnectionSharing = true;
                        WS.Credentials = new NetworkCredential(certificado, passcertificad);
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                        Respuesta = WS.generaDocumento(usuario, password, Nit, 1, tipoDocFelGuate, "1", tipo_Resuesta, xmlResp).Replace("&", "&amp;");

                        try
                        {
                            XmlRespuesta.LoadXml(Respuesta);
                        }
                        catch(Exception ex)
                        {
                            string data = "<Errores><Error><No_Sap>" + DocNum.ToString() + "</No_Sap><parsingXML>" + ex + "</parsingXML><Descripcion>" + Respuesta + "</Descripcion></Error></Errores>";
                            filename = dirXMLerr + "/ErrorSistemaParse_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
                            
                        }
                        
                        
                        XmlNodeList XmlDatos = XmlRespuesta.GetElementsByTagName("Resultado");

                        //FEL_ONE.GuatefactSB.GuatefacClient guatefac = new FEL_ONE.GuatefactSB.GuatefacClient();                        
                        //Respuesta = guatefac.generaDocumento(usuario, password, Nit, 1, tipoDocFelGuate, "1", tipo_Resuesta, xmlResp);
                        //XmlRespuesta.LoadXml(Respuesta);
                        //XmlNodeList XmlDatos = XmlRespuesta.GetElementsByTagName("Resultado");

                        if (Respuesta == "") { throw new Exception("Xml respuesta vacio"); }

                        try
                        {
                            document.LoadXml(Respuesta);
                            filename = dirXMLres + "/Resp_" + TipoDocFEL + "_" + SerieAprobada + "_" + DocNum.ToString() + ".xml";
                            document.Save(filename);

                            if (Respuesta.Contains("DUPLICADO"))
                            {
                                try
                                {
                                    string data = document.InnerXml.ToString();
                                    filename = dirXMLerr + "/ErrorDuplicado_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "TrueError", Respuesta, "", "", "", "", "", "", "");
                                }
                                catch (Exception exception1)
                                {
                                    string data = exception1.Message.ToString();
                                    filename = dirXMLerr + "/ErrorInterno_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "False", exception1.Message.ToString(), "", "", "", "", "", "", "");
                                }
                            }
                            else if (!document.InnerXml.Contains("Preimpreso"))
                            {
                                string data = document.InnerXml.ToString();
                                filename = dirXMLerr + "/Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "False", document.InnerXml.ToString().Replace("'", ""), "", "", "", "", "", "", "");
                            }
                            else
                            {
                                string UUID = "";
                                string Serie = "";
                                string numero = "";
                                string nombre = "";
                                string direccion = "";
                                string telefono = "";
                                string referencia = "";
                                XmlNodeList list = document.SelectNodes("Resultado");
                                IEnumerator enumerator = null;
                                try
                                {
                                    enumerator = list.GetEnumerator();
                                    while (true)
                                    {
                                        if (!enumerator.MoveNext())
                                        {
                                            break;
                                        }
                                        XmlNode current = (XmlNode)enumerator.Current;
                                        UUID = current.SelectSingleNode("NumeroAutorizacion").InnerText.ToString();
                                        Serie = current.SelectSingleNode("Serie").InnerText.ToString();
                                        numero = current.SelectSingleNode("Preimpreso").InnerText;
                                        nombre = current.SelectSingleNode("Nombre").InnerText;
                                        direccion = current.SelectSingleNode("Direccion").InnerText;
                                        telefono = current.SelectSingleNode("Telefono").InnerText;
                                        referencia = current.SelectSingleNode("Referencia").InnerText;
                                    }
                                    string data = document.InnerXml;
                                    filename = dirXMLauth + "/Aprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "True", UUID, numero, Serie, "", DateTime.Now.ToString(), nombre, direccion, telefono);

                                }
                                catch (Exception)
                                {
                                    string data = document.InnerXml;
                                    filename = dirXMLauth + "/TrueErrorInterno_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "TrueError", "Error interno, verifique portal web", "", "", "", "", "", "", "");
                                }
                                finally
                                {
                                    if (enumerator is IDisposable)
                                    {
                                        (enumerator as IDisposable).Dispose();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string data = "<Errores><Error><Descripcion>" + ex.ToString() + "</Descripcion><NoDoc>" + DocNum.ToString() + "</NoDoc></Error></Errores>";
                            filename = dirXMLerr + "/ErrorInterno_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "False", ex.ToString(), "", "", "", "", "", "", "");
                        }
                    }
                    else
                    {
                        string data = "<Errores><Error><Descripcion>Error de permisos</Descripcion><NoDoc>" + DocNum.ToString() + "</NoDoc></Error></Errores>";
                        filename = dirXMLerr + "/VerificarError_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "False", "No se pudo guardar el xml, verifique permisos", "", "", "", "", "", "", "");
                    }
                }
            }
            catch (Exception ex)
            {
                string data = "<Errores><Error><No_Sap>" + DocNum.ToString() + "</No_Sap><Descripcion>" + ex.ToString() + "</Descripcion></Error></Errores>";
                filename = dirXMLerr + "/ErrorSistema_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
            }
        }

        public static void EnviaDocumentoFELA(SAPbobsCOM.Company oCompany, Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string pais, string DocEntry, bool ProcesarBatch = false)
        {
            string Respuesta = "";
            string certificado = "";
            string passcertificad = "";
            string dirXMLSinAutorizar = "";
            string dirXMLauth = "";
            string dirXMLres = "";
            string dirXMLerr = "";
            string dirXMLPDF = "";
            string usuario = "";
            string password = "";
            string tipo_Resuesta = "";
            string Nit;
            string dirUR_t;
            string xmlResp = "";
            string xmlFile = "";
            string filename;
            CultureInfo provider = CultureInfo.InvariantCulture;
            XmlDataDocument document = new XmlDataDocument();
            XmlDataDocument Datadocumento = new XmlDataDocument();
            try
            {
                if (Utils.ExisteDocumentoANULAR(oCompany, SBO_Application, CurrSerie, DocEntry, Tipo))
                {
                    dirXMLSinAutorizar = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXML"); // Direccion xml Sin autorizar
                    dirXMLPDF = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHPDF"); // Direccion xml pdf
                    certificado = Utils.ObtieneValorParametro(oCompany, SBO_Application, "USR_WS"); // Direccion Certificado
                    passcertificad = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PASS_WS"); // Contraseña Certificado
                    dirXMLauth = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLaut"); // Direccion xml Autorizado
                    dirXMLres = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLres"); // Direccion xml Respuesta
                    dirUR_t = Utils.ObtieneValorParametro(oCompany, SBO_Application, "UR_t"); // Direccion URL Token
                    usuario = Utils.ObtieneValorParametro(oCompany, SBO_Application, "usuario"); // usuario cliente ws
                    password = Utils.ObtieneValorParametro(oCompany, SBO_Application, "password"); // Tipo de respuesta esperada Default R
                    tipo_Resuesta = Utils.ObtieneValorParametro(oCompany, SBO_Application, "tipo_resp"); // password cliente ws
                    dirXMLerr = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(oCompany, SBO_Application, "NitEmi"); // Nit emisor para el token

                    string SerieAprobada;
                    string TipoDocFEL;

                    if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH(@"SELECT ""U_SERIE"" FROM ""@FEL_RESOLUCION"" WHERE ""U_SERIE"" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH(@"SELECT ""U_TIPO_DOC"" FROM ""@FEL_RESOLUCION"" WHERE ""U_SERIE"" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH(@"CALL FELONE_GUATE_ANUL(" + DocEntry + ",'" + TipoDocFEL + "','" + Nit + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato(@"SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato(@"SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato(@"EXEC FELONE_GUATE_ANUL " + DocEntry + ",'" + TipoDocFEL + "','" + Nit + "'");
                    }

                    XmlDocument XmlRespuesta = new XmlDocument();

                    if (Utils.GrabarXml(oCompany, xmlResp, SerieAprobada, DocNum, TipoDocFEL + "_ANUL", ref xmlFile))
                    {
                        string aSerie = "";
                        string aPreimpreso = "";
                        string aNit = "";
                        string aFechaAnulacion = "";
                        string aMotivoAnulacion = "";

                        Datadocumento.LoadXml(xmlResp);
                        XmlNode datosAnulacion = Datadocumento.SelectSingleNode("Anulacion");
                        aSerie = datosAnulacion.SelectSingleNode("Serie").InnerText.ToString();
                        aPreimpreso = datosAnulacion.SelectSingleNode("Preimpreso").InnerText.ToString();
                        aNit = datosAnulacion.SelectSingleNode("Nit").InnerText;
                        aFechaAnulacion = datosAnulacion.SelectSingleNode("Fecha").InnerText;
                        aMotivoAnulacion = datosAnulacion.SelectSingleNode("Motivo").InnerText;

                        com.guatefacturas.dte.Guatefac WS = new com.guatefacturas.dte.Guatefac();
                        WS.Url = dirUR_t;
                        WS.Timeout = 800000;
                        WS.CookieContainer = new System.Net.CookieContainer();
                        WS.UnsafeAuthenticatedConnectionSharing = true;
                        WS.Credentials = new NetworkCredential(certificado, passcertificad);
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                        Respuesta = WS.anulaDocumento(usuario, password, Nit, aSerie, aPreimpreso, aNit, aFechaAnulacion, aMotivoAnulacion).Replace("&", "&amp;");

                        try
                        {
                            XmlRespuesta.LoadXml(Respuesta);
                        }
                        catch (Exception ex)
                        {
                            string data = "<Errores><Error><No_Sap>" + DocNum.ToString() + "</No_Sap><parsingXML>" + ex + "</parsingXML><Descripcion>" + Respuesta + "</Descripcion></Error></Errores>";
                            filename = dirXMLerr + "/ErrorSistemaParse_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");

                        }

                        XmlNodeList XmlDatos = XmlRespuesta.GetElementsByTagName("Resultado");

                        //FEL_ONE.GuatefacturasTest.GuatefacClient guatefac = new FEL_ONE.GuatefacturasTest.GuatefacClient();
                        //Respuesta = guatefac.anulaDocumento(usuario, password, Nit, aSerie, aPreimpreso, aNit, aFechaAnulacion, aMotivoAnulacion);
                        //XmlRespuesta.LoadXml(Respuesta);
                        //XmlNodeList XmlDatos = XmlRespuesta.GetElementsByTagName("Resultado");

                        if (Respuesta == "") { throw new Exception("Xml respuesta vacio"); }

                        try
                        {
                            document.LoadXml(Respuesta.Replace("&", "&amp;"));
                            filename = dirXMLres + "/Resp_" + TipoDocFEL + "_" + SerieAprobada + "_" + DocNum.ToString() + ".xml";
                            document.Save(filename);

                            if (!document.InnerXml.Contains("ANULADO"))
                            {
                                string data = document.InnerXml.ToString();
                                filename = dirXMLerr + "/ErrorAnulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "FalseA", document.InnerXml.ToString().Replace("'", ""), "", "", "", "", "", "", "");
                            }
                            else
                            {
                                try
                                {
                                    string data = document.InnerXml;
                                    filename = dirXMLres + "/Anulada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "TrueA", "", "", "", "", "", "", "", "");
                                }
                                catch (Exception)
                                {
                                    string data = document.InnerXml;
                                    filename = dirXMLauth + "/TrueErrorInternoAnulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                                    Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "TrueErrorA", "Error interno, verifique portal web", "", "", "", "", "", "", "");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string data = "<Errores><Error><Descripcion>" + ex.ToString() + "</Descripcion><NoDoc>" + DocNum.ToString() + "</NoDoc></Error></Errores>";
                            filename = dirXMLerr + "/ErrorInternoAnulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                            Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "FalseA", ex.ToString(), "", "", "", "", "", "", "");
                        }
                    }
                    else
                    {
                        string data = "<Errores><Error><Descripcion>Error de permisos</Descripcion><NoDoc>" + DocNum.ToString() + "</NoDoc></Error></Errores>";
                        filename = dirXMLerr + "/VerificarErrorAnulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".xml";
                        Utils.GrabarArchivo(oCompany, TipoDocFEL, DocEntry, data, filename, "FalseA", "No se pudo guardar el xml, verifique permisos", "", "", "", "", "", "", "");
                    }
                }
            }
            catch (Exception ex)
            {
                string data = "<Errores><Error><No_Sap>" + DocNum.ToString() + "</No_Sap><Descripcion>" + ex.ToString() + "</Descripcion></Error></Errores>";
                filename = dirXMLerr + "/ErrorSistemaAnulada_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".xml";
                Utils.GrabarArchivo(oCompany, Tipo, DocEntry, data, filename, "FalseA", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
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

                oItem = oForm.Items.Item("UR_t");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_t", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("USR_WS");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "USR_WS", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("PASS_WS");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "PASS_WS", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("usuario");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "usuario", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("password");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "password", oEdit.Value);
                ProgressBar.Value += 1;

                oItem = oForm.Items.Item("tipo_resp");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "tipo_resp", oEdit.Value);
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
                        case "USRDB":
                            oItem = oForm.Items.Item("txtUsuario");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "PASSDB":
                            oItem = oForm.Items.Item("txtPass");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "USR_WS":
                            oItem = oForm.Items.Item("USR_WS");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "PASS_WS":
                            oItem = oForm.Items.Item("PASS_WS");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "usuario":
                            oItem = oForm.Items.Item("usuario");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "password":
                            oItem = oForm.Items.Item("password");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "tipo_resp":
                            oItem = oForm.Items.Item("tipo_resp");
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
                        case "UR_t":
                            oItem = oForm.Items.Item("UR_t");
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