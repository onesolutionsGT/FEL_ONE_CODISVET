using FEL_ONE.Clases;
using FEL_ONE.Forms;
using Newtonsoft.Json;
using RawPrint;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PdfiumViewer;

namespace FEL_ONE.Certificadores
{
    class Infile
    {

        public  void EnviaDocumentoFEL(SAPbobsCOM.Company OCompany, string tabla, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string DocEntry, bool esBatch = false)
        {
            string certificado = "";
            string passcertificad = "";
            string dirXMLSinAutorizar = "";
            string apikey = "";
            string dirXMLauth = "";
            string dirXMLres = "";
            string dirXMLerr = "";
            string dirXMLPDF = "";
            string validar_FEL = "";
            string Nit;
            string email;
            string dirUR_t;
            string dirUR_r;
            string dirUR_p;
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string xmlResp = "";
            string xmlFile = "";
            string identificador = "";
            string filename;
            string imprimePdf = "";
            string impresora = "";
            string numeroCopias = "";
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
                    dirXMLerr = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(OCompany, SBO_Application, "NitEmi"); // Nit emisor para el token
                    email = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Correo"); // Correo cliente
                    validar_FEL = Utils.ObtieneValorParametro(OCompany, SBO_Application, "VALFEL"); // Validacion de ID documetno FEL
                    

                    string SerieAprobada;
                    string TipoDocFEL;

                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        imprimePdf = Utils.TraeDatoH("SELECT \"U_IMPRIME_PDF\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        impresora = Utils.TraeDatoH("SELECT \"U_IMPRESORA\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH("CALL FELONE_INFILE_" + TipoDocFEL + " (" + DocEntry + ",'" + TipoDocFEL + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato("SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato("SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        imprimePdf = Utils.TraeDato("SELECT U_IMPRIME_PDF FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        impresora = Utils.TraeDato("SELECT U_IMPRESORA FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato("EXEC FELONE_INFILE_" + TipoDocFEL + " " + DocEntry + ",'1'");
                    }

                    switch (TipoDocFEL)
                    {
                        case "FACT":     // FACTURA
                            tabla = "OINV";  break;
                        case "FACTA":   // FACTURA ANTICIPO
                            tabla = "ODPI";  break;
                        case "NCRE":     // NOTA DE CREDITO
                            tabla = "ORIN";  break;
                        case "NDEB":   // NOTA DE DEBITO
                            tabla = "OINV";  break;
                        case "FESP":     // FACTURA DE PROVEEDORES
                            tabla = "OPCH";  break;
                    }

                    if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                    {
                        string cardCode = Utils.TraeDatoH(@"select c.""CardCode"" from " + tabla + @" c where ""DocEntry"" = '" + DocEntry + "'");

                        numeroCopias = Utils.TraeDatoH(@"select c.""U_NUMERO_COPIAS"" from ocrd c where ""CardCode"" = '" + cardCode + "'");
                    }
                    else
                    {
                        string cardcode = Utils.TraeDato(@"select c.""CardCode"" from " + tabla + @" c where ""DocEntry"" = '" + DocEntry + "'");

                        numeroCopias = Utils.TraeDato(@"select c.""U_NUMERO_COPIAS"" from ocrd c where ""CardCode"" = '" + cardcode + "'");
                    }

                    string rutasalidacertificado;

                    if (Utils.GrabarXml(OCompany, xmlResp, SerieAprobada, DocNum, TipoDocFEL, ref xmlFile))
                    {

                        rutasalidacertificado = xmlFile;
                        string base64EncodedCert;
                        byte[] dataCert;
                        dataCert = System.Text.ASCIIEncoding.UTF8.GetBytes(xmlResp);
                        base64EncodedCert = System.Convert.ToBase64String(dataCert);
                        string es_anulacion;
                        es_anulacion = "N";

                        var jsonstringCert = @"{ 
                                         ""llave"":""" + apikey + @""",   
                                         ""archivo"": """ + base64EncodedCert + @""", 
                                         ""codigo"": """ + apikey + @""",     
                                         ""alias"":  """ + certificado + @""",    
                                         ""es_anulacion"": """ + es_anulacion + @"""   	
                                          }";

                        Uri uriCert = new Uri(dirUR_t);
                        var jsonDataBytesCert = Encoding.UTF8.GetBytes(jsonstringCert);

                        WebRequest reqCert = WebRequest.Create(uriCert);
                        reqCert.ContentType = "application/json";
                        reqCert.Method = "POST";
                        reqCert.ContentLength = jsonDataBytesCert.Length;
                        var streamCert = reqCert.GetRequestStream();
                        streamCert.Write(jsonDataBytesCert, 0, jsonDataBytesCert.Length);
                        streamCert.Close();
                        var responseJSONCert = reqCert.GetResponse().GetResponseStream();
                        StreamReader readerCert = new StreamReader(responseJSONCert);
                        var resCert = readerCert.ReadToEnd();
                        readerCert.Close();
                        responseJSONCert.Close();
                        var arrCert = JsonConvert.DeserializeObject<firma>(resCert.ToString());
                        string resultadojsonCert;
                        resultadojsonCert = arrCert.resultado.ToString();

                        if (resultadojsonCert == "true")
                        {
                            string archivo;
                            archivo = arrCert.archivo.ToString();
                            var jsonstring = "{\"nit_emisor\": \"" + Nit + @""",
                                        ""correo_copia"": """ + email + @""",
                                        ""xml_dte"": """ + archivo + @"""
                                        }";

                            Uri uri = new Uri(dirUR_r);
                            var jsonDataBytes = Encoding.UTF8.GetBytes(jsonstring);

                            WebRequest req = WebRequest.Create(uri);
                            req.ContentType = "application/json";
                            req.Method = "POST";
                            req.Headers.Add("usuario", certificado);
                            req.Headers.Add("llave", passcertificad);
                            if (validar_FEL == "1")
                            {
                                identificador = TipoDocFEL + "_" + CurrSerie + "_" + SerieAprobada.Replace(" ", "") + "_" + DocEntry + "_" + DocNum; // GENERACION DE ID UNICO DOCUMENTO
                                req.Headers.Add("Identificador", identificador);
                            }
                            req.ContentLength = jsonDataBytes.Length;

                            var stream = req.GetRequestStream();
                            stream.Write(jsonDataBytes, 0, jsonDataBytes.Length);
                            stream.Close();

                            var responseJSON = req.GetResponse().GetResponseStream();

                            StreamReader reader = new StreamReader(responseJSON);
                            var res = reader.ReadToEnd();
                            reader.Close();
                            responseJSON.Close();
                            string resultadojson;
                            List<string> descripcionerrores = new List<string>();
                            List<string> cantidaderrores = new List<string>();
                            int rescount;
                            rescount = res.IndexOf("\"resultado\":false");
                            DataTable erroresjson = new DataTable();
                            encabezado arr = null;
                            encabezadoerrores arr2 = null;
                            if (rescount > 0)
                            {
                                arr2 = JsonConvert.DeserializeObject<encabezadoerrores>(res.ToString());
                                resultadojson = arr2.resultado.ToString();
                                string Json1 = "{ 'root': " + res + " }";
                                XmlDocument doc = JsonConvert.DeserializeXmlNode(Json1);
                                string result = doc.ChildNodes[0].InnerXml;

                                XmlNodeList CatNodesList = doc.SelectNodes("root");
                                foreach (XmlNode xnDet in CatNodesList)
                                {
                                    cantidaderrores.Add(xnDet.SelectSingleNode("cantidad_errores").InnerText);
                                }
                                XmlNodeList CatNodesList2 = doc.SelectNodes("root/descripcion_errores");
                                foreach (XmlNode xnDet in CatNodesList2)
                                {
                                    descripcionerrores.Add(xnDet.SelectSingleNode("mensaje_error").InnerText);
                                }
                            }
                            else
                            {
                                arr = JsonConvert.DeserializeObject<encabezado>(res.ToString());
                                resultadojson = arr.resultado.ToString();
                            }
                            try
                            {
                                if (resultadojson == "true")
                                {
                                    try
                                    {
                                        Utils.ActualizaCamposDocumento(OCompany, TipoDocFEL, DocEntry, arr.numero.ToString(), arr.serie);

                                        string data = res.ToString();
                                        filename = dirXMLres + "/Aprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                        Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "True", arr.uuid.ToString(), arr.numero.ToString(), arr.serie, dirUR_p + arr.uuid.ToString(), arr.fecha.ToString(), "", "", "");
                                        //impresion automatica del pdf
                                        if (impresora != "" && imprimePdf!="" && numeroCopias !="")

                                        {
                                            if(imprimePdf == "1")
                                            {
                                                try
                                                {
                                                    string name = TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".pdf";
                                                    string filenamepdfVal = dirXMLPDF + @"/" + name;

                                                    string uuid = arr.uuid.ToString();
                                                    string remoteUri = dirUR_p + uuid;
                                                    string myStringWebResource = null;
                                                    // Create a new WebClient instance.
                                                    WebClient myWebClient = new WebClient();
                                                    // Concatenate the domain with the Web resource filename.
                                                    myStringWebResource = remoteUri;

                                                    // Download the Web resource and save it into the current filesystem folder.
                                                    myWebClient.DownloadFile(myStringWebResource, filenamepdfVal);

                                                    try
                                                    {
                                                        //IPrinter printer = new Printer();

                                                        // Print the file
                                                        int numero = Int32.Parse(numeroCopias);
                                                        //for (int i = 0; i < numero; i++)
                                                        //{
                                                        //printer.PrintRawFile(impresora, filenamepdfVal, name);
                                                        using (var document = PdfDocument.Load(filenamepdfVal))
                                                        {
                                                            // Crear un PrintDocument
                                                            using (var printDocument = document.CreatePrintDocument())
                                                            {
                                                                // Mostrar el cuadro de diálogo de impresión
                                                                printDocument.PrinterSettings.PrinterName = impresora;
                                                                printDocument.PrinterSettings.Copies = (short)numero;
                                                                printDocument.Print();
                                                                //using (var printDialog = new PrintDialog())
                                                                //{
                                                                //    printDialog.Document = printDocument;

                                                                //    // Si el usuario acepta, imprimir el documento
                                                                //    if (printDialog.ShowDialog() == DialogResult.OK)
                                                                //    {
                                                                //        printDocument.Print();
                                                                //    }
                                                                //}
                                                            }
                                                        }
                                                        SBO_Application.SetStatusBarMessage("Documento enviado a impresora automáticamente....", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                                                        //}
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        SBO_Application.SetStatusBarMessage("Falla al intentar imprimir: " + ex.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                                                    }

                                                }
                                                catch (Exception ex)
                                                {
                                                    SBO_Application.SetStatusBarMessage("Falla en proceso global de impresión: " + ex.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        string data = res.ToString();
                                        filename = dirXMLerr + "/ErrorSistemaAprobada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                        Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "TrueError", "Error Interno Verifique portal web", "", "", "", "", "", "", "");
                                    }
                                }
                                else if (resultadojson == "False")
                                {
                                    string errores = "";
                                    for (var cont = 0; cont <= descripcionerrores.Count - 1; cont++)
                                    {
                                        errores += "Error: No." + cont + 1 + "Descripcion: " + descripcionerrores[cont] + "|..| ";
                                    }
                                    string data = res.ToString();
                                    filename = dirXMLerr + "/Error_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                    Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "False", errores.Replace("'", ""), "", "", "", "", "", "", "");
                                }
                            }
                            catch (Exception ex)
                            {
                                string data = res.ToString();
                                filename = dirXMLerr + "/ErrorSistema_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
                                SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " + ex.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                                return;
                            }
                        }
                        else
                        {
                            throw new Exception(resCert);
                        }
                    }
                    else
                    {
                        string data = "{\"Error de permisos\": " + DocNum.ToString() + "}";
                        filename = dirXMLerr + "/VerificarError_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                        Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "False", "No se pudo guardar el xml, verifique permisos", "", "", "", "", "", "", "");
                    }
                }
            }
            catch (Exception ex)
            {
                string data = "{\"No.Sap\": " + DocNum.ToString() + ",\"Error VB\": \"" + ex.ToString() + "\"}";
                filename = dirXMLerr + "/ErrorSistema_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".json";
                Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "False", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
            }
        }

        internal static string GetReceptorInfo(Company oCompany, SAPbouiCOM.Application SBO_Application, string uNit)
        {
            try
            {
                string dirUR_n;
                string passcertificad;
                string certificado;
                uNit = uNit.ToUpper().Replace("-", "").Replace("/", "");
                dirUR_n = Utils.ObtieneValorParametro(oCompany, SBO_Application, "UR_n");
                certificado = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLc");
                passcertificad = Utils.ObtieneValorParametro(oCompany, SBO_Application, "PATHXMLcp");

                string jsonstring = @"{""emisor_codigo"":""" + certificado + @""",
                                    ""emisor_clave"":""" + passcertificad + @""",
                                    ""nit_consulta"":""" + uNit + @""" }";

                Uri uri = new Uri(dirUR_n);
                var jsonDataBytes = Encoding.UTF8.GetBytes(jsonstring);

                WebRequest req = WebRequest.Create(uri);
                req.ContentType = "application/json";
                req.Method = "POST";
                req.ContentLength = jsonDataBytes.Length;
                Stream stream = req.GetRequestStream();
                stream.Write(jsonDataBytes, 0, jsonDataBytes.Length);
                stream.Close();
                Stream responseJSON = req.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(responseJSON);
                string res = reader.ReadToEnd();
                reader.Close();
                responseJSON.Close();

                InfileInfoClient JsonData = JsonConvert.DeserializeObject<InfileInfoClient>(res);
                if (JsonData.nombre == "")
                {
                    return JsonData.mensaje;
                }
                else
                {
                    return JsonData.nombre.Replace(',', ' ').Replace('\'', ' ').Replace("  ", ", ").Trim();
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        public static void EnviaDocumentoFELA(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string DocEntry)
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
            string email;
            string dirUR_a;
            string dirUR_t;
            string dirUR_r;
            string dirUR_p;
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string xmlResp = "";
            string xmlFile = "";
            string filename;
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
                    dirUR_a = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_a");
                    dirUR_r = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_r"); // Direccion URL Request
                    dirUR_t = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_t"); // Direccion URL Token
                    dirUR_p = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_p"); // Direccion URL Pdf
                    dirXMLerr = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr"); // Direccion XML error
                    Nit = Utils.ObtieneValorParametro(OCompany, SBO_Application, "NitEmi"); // Nit emisor para el token
                    email = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Correo"); // Correo cliente
                    string SerieAprobada;
                    string TipoDocFEL;


                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        SerieAprobada = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie);
                        xmlResp = Utils.TraeDatoH("CALL FELONE_INFILE_ANUL(" + DocEntry + ",'" + TipoDocFEL + "','" + Nit + "')");
                    }
                    else
                    {
                        SerieAprobada = Utils.TraeDato("SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        TipoDocFEL = Utils.TraeDato("SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie);
                        xmlResp = Utils.TraeDato("EXEC FELONE_INFILE_ANUL " + DocEntry + ",'" + TipoDocFEL + "','" + Nit + "'");
                    }

                    string rutasalidacertificado;


                    if (Utils.GrabarXml(OCompany, xmlResp, SerieAprobada, DocNum, Tipo, ref xmlFile))
                    {
                        rutasalidacertificado = xmlFile;
                        string base64EncodedCert;
                        byte[] dataCert;
                        dataCert = System.Text.ASCIIEncoding.ASCII.GetBytes(xmlResp);
                        base64EncodedCert = System.Convert.ToBase64String(dataCert);
                        string es_anulacion;

                        es_anulacion = "S";

                        var jsonstringCert = @"{ 
                                         ""llave"":""" + apikey + @""",   
                                         ""archivo"": """ + base64EncodedCert + @""", 
                                         ""codigo"": """ + apikey + @""",     
                                         ""alias"":  """ + certificado + @""",    
                                         ""es_anulacion"": """ + es_anulacion + @"""   	
                                          }";

                        Uri uriCert = new Uri(dirUR_t);
                        var jsonDataBytesCert = Encoding.UTF8.GetBytes(jsonstringCert);

                        WebRequest reqCert = WebRequest.Create(uriCert);
                        reqCert.ContentType = "application/json";
                        reqCert.Method = "POST";
                        reqCert.ContentLength = jsonDataBytesCert.Length;

                        var streamCert = reqCert.GetRequestStream();
                        streamCert.Write(jsonDataBytesCert, 0, jsonDataBytesCert.Length);
                        streamCert.Close();

                        var responseJSONCert = reqCert.GetResponse().GetResponseStream();

                        StreamReader readerCert = new StreamReader(responseJSONCert);
                        var resCert = readerCert.ReadToEnd();
                        readerCert.Close();
                        responseJSONCert.Close();

                        var arrCert = JsonConvert.DeserializeObject<firma>(resCert.ToString());



                        string resultadojsonCert;
                        resultadojsonCert = arrCert.resultado.ToString();


                        if (resultadojsonCert == "true")
                        {
                            string archivo;
                            archivo = arrCert.archivo.ToString();
                            var jsonstring = "{\"nit_emisor\": \"" + Nit + @""",
                                        ""correo_copia"": """ + email + @""",
                                        ""xml_dte"": """ + archivo + @"""
                                        }";

                            Uri uri = new Uri(dirUR_a);
                            var jsonDataBytes = Encoding.UTF8.GetBytes(jsonstring);

                            WebRequest req = WebRequest.Create(uri);
                            req.ContentType = "application/json";
                            req.Method = "POST";
                            req.Headers.Add("usuario", certificado);
                            req.Headers.Add("llave", passcertificad);
                            req.ContentLength = jsonDataBytes.Length;

                            var stream = req.GetRequestStream();
                            stream.Write(jsonDataBytes, 0, jsonDataBytes.Length);
                            stream.Close();

                            var responseJSON = req.GetResponse().GetResponseStream();

                            StreamReader reader = new StreamReader(responseJSON);
                            var res = reader.ReadToEnd();
                            reader.Close();
                            responseJSON.Close();
                            string resultadojson;
                            List<string> descripcionerrores = new List<string>();
                            List<string> cantidaderrores = new List<string>();
                            int rescount;
                            rescount = res.IndexOf("\"resultado\":false");
                            //var descrip;
                            DataTable erroresjson = new DataTable();
                            encabezadoerrores arr2 = null;
                            encabezado arr = null;
                            if (rescount > 0)
                            {
                                arr2 = JsonConvert.DeserializeObject<encabezadoerrores>(res.ToString());
                                resultadojson = arr2.resultado.ToString();
                                string Json1 = "{ 'root': " + res + " }";
                                XmlDocument doc = JsonConvert.DeserializeXmlNode(Json1);
                                string result = doc.ChildNodes[0].InnerXml;

                                XmlNodeList CatNodesList = doc.SelectNodes("root");
                                foreach (XmlNode xnDet in CatNodesList)
                                    cantidaderrores.Add(xnDet.SelectSingleNode("cantidad_errores").InnerText);

                                XmlNodeList CatNodesList2 = doc.SelectNodes("root/descripcion_errores");
                                foreach (XmlNode xnDet in CatNodesList2)
                                    descripcionerrores.Add(xnDet.SelectSingleNode("mensaje_error").InnerText);
                            }
                            else
                            {
                                arr = JsonConvert.DeserializeObject<encabezado>(res.ToString());
                                resultadojson = arr.resultado.ToString();
                            }

                            try
                            {
                                if (resultadojson == "true")
                                {
                                    try
                                    {
                                        string data = res.ToString();
                                        filename = dirXMLres + "/Anulada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                        Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "TrueA", arr.uuid.ToString(), arr.numero.ToString(), arr.serie, dirUR_p + arr.uuid.ToString(), arr.fecha.ToString(), "", "", "");
                                    }
                                    catch (Exception)
                                    {
                                        string data = res.ToString();
                                        filename = dirXMLerr + "/ErrorSistemaAnulada_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                        Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "TrueErrorA", "Error Interno Verifique portal web", "", "", "", "", "", "", "");
                                    }
                                }
                                else if (resultadojson == "False")
                                {
                                    string errores = "";
                                    for (var cont = 0; cont <= descripcionerrores.Count - 1; cont++)
                                    {
                                        errores += "Error: No." + cont + 1 + "Descripcion: " + descripcionerrores[cont] + "|..| ";
                                    }
                                    string data = res.ToString();
                                    filename = dirXMLerr + "/ErrorAnulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                    Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "FalseA", errores.Replace("'", ""), "", "", "", "", "", "", "");
                                }
                            }
                            catch (Exception ex)
                            {
                                string data = res.ToString();
                                filename = dirXMLerr + "/ErrorSistemaAnulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                                Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "FalseA", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
                                SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la fálla: " + ex.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                                return;
                            }
                        }
                    }
                    else
                    {
                        string data = "{\"Documento se encuentra firmado Numero interno Sap\": " + DocNum.ToString() + "}";
                        filename = dirXMLerr + "/VerificarErrorAnulacion_" + TipoDocFEL + "_" + CurrSerieName + "_" + DocNum + ".json";
                        Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "FalseA", "No se pudo guardar el xml, verifique permisos", "", "", "", "", "", "", "");
                    }
                }
            }
            catch (Exception ex)
            {
                string data = "{\"No.Sap\": " + DocNum.ToString() + ",\"Error VB\": \"" + ex.ToString() + "\"}";
                filename = dirXMLerr + "/ErrorSistemaAnulacion_" + Tipo + "_" + CurrSerieName + "_" + DocNum + ".json";
                Utils.GrabarArchivo(OCompany, Tipo, DocEntry, data, filename, "FalseA", ex.Message.ToString().Replace("'", ""), "", "", "", "", "", "", "");
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
                        case "UR_p":
                            oItem = oForm.Items.Item("UR_p");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UR_a":
                            oItem = oForm.Items.Item("UR_a");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UR_n":
                            oItem = oForm.Items.Item("UR_n");
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
                        case "VALFEL":
                            oItem = oForm.Items.Item("FEELVal");
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

                oItem = oForm.Items.Item("UR_n");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_n", oEdit.Value);
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

                oItem = oForm.Items.Item("FEELVal");
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
                ParametrosForm.GuardaParametro(oUsrTbl, "VALFEL", oComboBox.Value.ToString().Trim());
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
