using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FEL_ONE.Clases;
using System.Net;
using System.Collections;
using RestSharp;
using SAPbouiCOM;
using FEL_ONE.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using RestSharp.Extensions.MonoHttp;
using Newtonsoft.Json.Linq;

namespace FEL_ONE.Certificadores
{
    class Tekra
    {

        public static void EnviaDocumentoFEL(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string DocEntry, string conti, bool esBatch = false)
        {
            string str14;
            string str4 = "";
            string str6 = "";
            string str7 = "";
            string str8 = "";
            string str9 = "";
            string sXML = "";
            string fileName = "";
            try
            {
                string str19;
                StreamWriter writer;
                XmlDocument document;
                string str21 = null;
                string str22 = null;
                string str23 = null;
                string str25 = null;
                string str26 = null;
                string str27 = null;
                bool flag6 = false;
                string str29 = null;
                if (Utils.ExisteDocumento(OCompany, SBO_Application, CurrSerie, DocEntry, Tipo, esBatch))
                {
                    string str18;
                    str4 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXML");
                    str9 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHPDF");
                    str6 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLaut");
                    str7 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLres");
                    string baseUrl = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_t");
                    str8 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr");
                    string str10 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "NitEmi");

                    str19 = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie).ToString();

                    string tabla = "";
                    string nit = "";

                    if (str19 == "NCRE" || str19 == "NABN")
                    {
                        tabla = "orin";
                    }
                    else if (str19 == "FESP")
                    {
                        tabla = "opch";
                    }
                    else
                    {
                        tabla = "oinv";
                    }



                    if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                    {
                        nit = Utils.TraeDatoH(@"select ""U_NIT"" from " + tabla + @" where ""DocEntry"" = '" + DocEntry + "'");
                    }
                    else
                    {
                        nit = Utils.TraeDato(@"select ""U_NIT"" from " + tabla + @" where ""DocEntry"" = '" + DocEntry + "'");
                    }

                    string nombreClinteApi = getNameApiNit(OCompany, SBO_Application, nit);
                    string nombre = "";
                    if (nombreClinteApi != "")
                    {
                        //string[] lista = nombreClinteApi.Split(',');
                        //nombre = lista[3] + " " + lista[4] + " " + lista[0] + " " + lista[1];
                        nombre = nombreClinteApi.Replace(',', ' ').Replace('\'', ' ').Trim();
                    }


                    if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                    {
                        str18 = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie).ToString();
                        str19 = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie).ToString();
                        if (conti != "SI")
                        {
                            string textArray2 = "CALL FELONE_TEKRA_" + str19 + " (" + DocEntry + ",'','" + nombre + "')";
                            sXML = Utils.TraeDatoH(textArray2).ToString();
                        }
                        else
                        {
                            string textArray1 = "CALL FELONE_TEKRA_" + str19 + " (" + DocEntry + ",'" + DocNum + "','" + nombre + "')";
                            sXML = Utils.TraeDatoH(textArray1).ToString();
                        }
                    }
                    else
                    {
                        str18 = Utils.TraeDato("SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie).ToString();
                        str19 = Utils.TraeDato("SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie).ToString();
                        if (conti != "SI")
                        {
                            string textArray4 = "EXEC FELONE_TEKRA_" + str19 + " " + DocEntry + ",'', '" + nombre + "'";
                            sXML = Utils.TraeDato(textArray4).ToString();
                        }
                        else
                        {
                            string textArray3 = "EXEC FELONE_TEKRA_" + str19 + " " + DocEntry + ",'" + DocNum + "', '" + nombre + "'";
                            sXML = Utils.TraeDato(textArray3).ToString();
                        }
                    }
                    if (Utils.GrabarXml(OCompany, sXML, str18, DocNum, Tipo, ref fileName))
                    {
                        IEnumerator enumerator = null;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        RestClient client = new RestClient(baseUrl);
                        RestRequest request = new RestRequest(RestSharp.Method.POST);
                        request.AddHeader("Content-Type", "application/xml");
                        string str20 = sXML;
                        request.AddParameter("application/xml", str20, ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);
                        Console.WriteLine(response.Content);
                        document = new XmlDocument();
                        document.LoadXml(response.Content);
                        flag6 = false;
                        XmlNodeList elementsByTagName = document.GetElementsByTagName("SOAP-ENV:Envelope");
                        try
                        {
                            enumerator = elementsByTagName.GetEnumerator();
                            while (true)
                            {
                                IEnumerator enumerator2 = null;
                                if (!enumerator.MoveNext())
                                {
                                    break;
                                }
                                XmlNode current = (XmlNode)enumerator.Current;
                                string xml = current.InnerXml.ToString();
                                XmlDocument document2 = new XmlDocument();
                                document2.LoadXml(xml);
                                XmlNodeList list2 = document2.GetElementsByTagName("SOAP-ENV:Body");
                                try
                                {
                                    enumerator2 = list2.GetEnumerator();
                                    while (true)
                                    {
                                        if (!enumerator2.MoveNext())
                                        {
                                            break;
                                        }
                                        XmlNode node2 = (XmlNode)enumerator2.Current;
                                        string str31 = node2.InnerXml.ToString();
                                        XmlDocument document3 = new XmlDocument();
                                        document3.LoadXml(str31);
                                        XmlNodeList list3 = document3.GetElementsByTagName("ns1:CertificacionDocumentoResponse");
                                        str27 = list3[0].ChildNodes.Item(0).InnerText.Trim();
                                        if (str27.Contains("\"error\":0"))
                                        {
                                            IEnumerator enumerator3 = null;
                                            flag6 = true;
                                            str26 = list3[0].ChildNodes.Item(2).InnerText.Trim();
                                            str23 = list3[0].ChildNodes.Item(6).InnerText.Trim();
                                            str22 = list3[0].ChildNodes.Item(7).InnerText.Trim();
                                            str21 = list3[0].ChildNodes.Item(8).InnerText.Trim();
                                            str25 = list3[0].ChildNodes.Item(9).InnerText.Trim();
                                            XmlDocument document4 = new XmlDocument();
                                            document4.LoadXml(list3[0].ChildNodes.Item(1).InnerText.Trim().Replace("dte:", ""));
                                            XmlNodeList list4 = document4.SelectNodes("GTDocumento/SAT/DTE/DatosEmision");
                                            try
                                            {
                                                enumerator3 = list4.GetEnumerator();
                                                while (true)
                                                {
                                                    if (!enumerator3.MoveNext())
                                                    {
                                                        break;
                                                    }
                                                    XmlNode node3 = (XmlNode)enumerator3.Current;
                                                    bool flag8 = (str19 == "FEXP") | (str19 == "FEXPM");
                                                    str29 = !flag8 ? node3.SelectSingleNode("DatosGenerales").Attributes[1].Value.ToString() : node3.SelectSingleNode("DatosGenerales").Attributes[2].Value.ToString();
                                                }
                                            }
                                            finally
                                            {
                                                if (enumerator3 is IDisposable)
                                                {
                                                    (enumerator3 as IDisposable).Dispose();
                                                }
                                            }
                                        }
                                    }
                                    continue;
                                }
                                finally
                                {
                                    if (enumerator2 is IDisposable)
                                    {
                                        (enumerator2 as IDisposable).Dispose();
                                    }
                                }
                            }
                            goto TR_0020;
                        }
                        finally
                        {
                            if (enumerator is IDisposable)
                            {
                                (enumerator as IDisposable).Dispose();
                            }
                        }
                    }
                    string[] textArray19 = new string[] { str8, "/VerificarError_", CurrSerieName, "_", DocNum, ".xml" };
                    StreamWriter writer3 = System.IO.File.AppendText(string.Concat(textArray19));
                    writer3.Write("Error de permisos: " + DocNum.ToString());
                    writer3.Flush();
                    writer3.Close();
                    if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                    {
                        string[] textArray20 = new string[] { "CALL FELONE_UTILS ('False','", DocEntry, "','", Tipo, "','No se pudo guardar el xml, verifique permisos','','','','','','','') " };
                        str14 = string.Concat(textArray20);
                    }
                    else
                    {
                        string[] textArray21 = new string[] { "EXEC FELONE_UTILS 'False','", DocEntry, "','", Tipo, "','No se pudo guardar el xml, verifique permisos','','','','','','','' " };
                        str14 = string.Concat(textArray21);
                    }
                    ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str14);
                }
                return;
            TR_0020:
                try
                {
                    if (!flag6)
                    {
                        if (!flag6)
                        {
                            string[] textArray13 = new string[] { str8, "/Error_", str19, "_", CurrSerieName, "_", DocNum, ".xml" };
                            writer = System.IO.File.AppendText(string.Concat(textArray13));
                            writer.Write(str27.ToString());
                            writer.Flush();
                            writer.Close();
                            if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                            {
                                string[] textArray14 = new string[] { "CALL FELONE_UTILS ('False','", DocEntry, "','", Tipo, "','", str27.ToString(), "','','','','','','','') " };
                                str14 = string.Concat(textArray14);
                            }
                            else
                            {
                                string[] textArray15 = new string[] { "EXEC FELONE_UTILS 'False','", DocEntry, "','", Tipo, "','", str27.ToString(), "','','','','','','','' " };
                                str14 = string.Concat(textArray15);
                            }
                            ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str14);
                        }
                    }
                    else
                    {
                        try
                        {
                            string[] textArray5 = new string[] { str7, "/Aprobada_", str19, "_", DateTime.Now.ToString("ddMMyyyHHmmss"), "_", DocEntry, "_", CurrSerieName, "_", DocNum, ".xml" };
                            string str17 = string.Concat(textArray5);
                            string[] textArray6 = new string[] { str7, "/Aprobada_", str19, "_", DateTime.Now.ToString("ddMMyyyHHmmss"), "_", DocEntry, "_", CurrSerieName, "_", DocNum, ".xml" };
                            writer = System.IO.File.AppendText(string.Concat(textArray6));
                            writer.Write(document.InnerXml.ToString());
                            writer.Flush();
                            writer.Close();
                            string[] textArray7 = new string[] { str9, "/", str19, "_", DateTime.Now.ToString("ddMMyyyHHmmss"), "_", DocEntry, "_", CurrSerieName, "_", DocNum, ".pdf" };
                            string path = string.Concat(textArray7);
                            BinaryWriter writer2 = new BinaryWriter(System.IO.File.Open(path, FileMode.Create));
                            writer2.Write(Convert.FromBase64String(str26));
                            writer2.Close();
                            if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                            {
                                string[] textArray8 = new string[0x11];
                                textArray8[0] = "CALL FELONE_UTILS ('True','";
                                textArray8[1] = DocEntry;
                                textArray8[2] = "','";
                                textArray8[3] = Tipo;
                                textArray8[4] = "','";
                                textArray8[5] = str23.ToString();
                                textArray8[6] = "','";
                                textArray8[7] = str22;
                                textArray8[8] = "','";
                                textArray8[9] = str21;
                                textArray8[10] = "','";
                                textArray8[11] = path;
                                textArray8[12] = "','";
                                textArray8[13] = str25;
                                textArray8[14] = "','";
                                textArray8[15] = str29;
                                textArray8[0x10] = "','','') ";
                                str14 = string.Concat(textArray8);
                            }
                            else
                            {
                                string[] textArray9 = new string[0x11];
                                textArray9[0] = "EXEC FELONE_UTILS 'True','";
                                textArray9[1] = DocEntry;
                                textArray9[2] = "','";
                                textArray9[3] = Tipo;
                                textArray9[4] = "','";
                                textArray9[5] = str23.ToString();
                                textArray9[6] = "','";
                                textArray9[7] = str22;
                                textArray9[8] = "','";
                                textArray9[9] = str21;
                                textArray9[10] = "','";
                                textArray9[11] = path;
                                textArray9[12] = "','";
                                textArray9[13] = str25;
                                textArray9[14] = "','";
                                textArray9[15] = str29;
                                textArray9[0x10] = "','','' ";
                                str14 = string.Concat(textArray9);
                            }
                            ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str14);
                        }
                        catch (Exception)
                        {
                            string[] textArray10 = new string[] { str8, "/ErrorSistemaAprobada_", str19, "_", CurrSerieName, "_", DocNum, ".xml" };
                            writer = System.IO.File.AppendText(string.Concat(textArray10));
                            writer.Write(document.InnerXml.ToString());
                            writer.Flush();
                            writer.Close();
                            if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                            {
                                string[] textArray11 = new string[] { "CALL FELONE_UTILS ('TrueError','", DocEntry, "','", Tipo, "','", document.InnerXml.ToString(), "','','','','','','','') " };
                                str14 = string.Concat(textArray11);
                            }
                            else
                            {
                                string[] textArray12 = new string[] { "EXEC FELONE_UTILS 'TrueError','", DocEntry, "','", Tipo, "','", document.InnerXml.ToString(), "','','','','','','','' " };
                                str14 = string.Concat(textArray12);
                            }
                            ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str14);
                        }
                    }
                }
                catch (Exception exception4)
                {
                    string[] textArray16 = new string[] { str8, "/ErrorSistema_", str19, "_", CurrSerieName, "_", DocNum, ".xml" };
                    writer = System.IO.File.AppendText(string.Concat(textArray16));
                    writer.Write(str27.ToString());
                    writer.Flush();
                    writer.Close();
                    if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                    {
                        string[] textArray17 = new string[] { "CALL FELONE_UTILS ('False','", DocEntry, "','", Tipo, "','", exception4.Message.ToString(), "','','','','','','','') " };
                        str14 = string.Concat(textArray17);
                    }
                    else
                    {
                        string[] textArray18 = new string[] { "EXEC FELONE_UTILS 'False','", DocEntry, "','", Tipo, "','", exception4.Message.ToString(), "','','','','','','','' " };
                        str14 = string.Concat(textArray18);
                    }
                    ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str14);
                    SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la f\x00e1lla: " + exception4.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                }
            }
            catch (Exception exception5)
            {
                string[] textArray22 = new string[] { str8, "/ErrorSistema_", Tipo, "_", CurrSerieName, "_", DocNum, ".xml" };
                StreamWriter writer4 = System.IO.File.AppendText(string.Concat(textArray22));
                writer4.Write("No.Sap: " + DocNum.ToString() + "Error VB: " + exception5.ToString());
                writer4.Flush();
                writer4.Close();
                if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                {
                    string[] textArray23 = new string[] { "CALL FELONE_UTILS ('False','", DocEntry, "','", Tipo, "','", exception5.Message.ToString(), "','','','','','','','') " };
                    str14 = string.Concat(textArray23);
                }
                else
                {
                    string[] textArray24 = new string[] { "EXEC FELONE_UTILS 'False','", DocEntry, "','", Tipo, "','", exception5.Message.ToString(), "','','','','','','','' " };
                    str14 = string.Concat(textArray24);
                }
                ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str14);
            }
        }

        public static void EnviaDocumentoFELA(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string CurrSerieName, string DocEntry, string conti)
        {
            Recordset recordset = null;
            string str15;
            string str4 = "";
            string str6 = "";
            string str7 = "";
            string str8 = "";
            string str9 = "";
            string sXML = "";
            string fileName = "";
            try
            {
                string str20;
                StreamWriter writer;
                XmlDocument document;
                string str22;
                string str23;
                string str24;
                string str25;
                string str26;
                string str27;
                string str28;
                bool flag5;
                if (Utils.ExisteDocumentoANULAR(OCompany, SBO_Application, CurrSerie, DocEntry, Tipo))
                {
                    string str19;
                    str4 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXML");
                    str9 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHPDF");
                    str6 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLaut");
                    str7 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLres");
                    string baseUrl = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_t");
                    str8 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "PATHXMLerr");
                    string str10 = Utils.ObtieneValorParametro(OCompany, SBO_Application, "NitEmi");
                    if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                    {
                        str19 = Utils.TraeDatoH("SELECT \"U_SERIE\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie).ToString();
                        str20 = Utils.TraeDatoH("SELECT \"U_TIPO_DOC\" FROM \"@FEL_RESOLUCION\" WHERE \"U_SERIE\" = " + CurrSerie).ToString();
                        string[] textArray1 = new string[] { "CALL FELONE_TEKRA_ANUL (", DocEntry, ",'", str20, "','", str10, "')" };
                        sXML = Utils.TraeDatoH(string.Concat(textArray1)).ToString();
                    }
                    else
                    {
                        str19 = Utils.TraeDato("SELECT U_SERIE FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie).ToString();
                        str20 = Utils.TraeDato("SELECT U_TIPO_DOC FROM [@FEL_RESOLUCION] WHERE U_SERIE = " + CurrSerie).ToString();
                        string[] textArray2 = new string[] { "EXEC FELONE_TEKRA_ANUL ", DocEntry, ",'", str20, "','", str10, "'" };
                        sXML = Utils.TraeDato(string.Concat(textArray2)).ToString();
                    }
                    if (!Utils.GrabarXml(OCompany, sXML, str19, DocNum, "Anulacion_", ref fileName))
                    {
                        string[] textArray18 = new string[] { str8, "/VerificarErrorAnulada_", CurrSerieName, "_", DocNum, ".xml" };
                        StreamWriter writer3 = System.IO.File.AppendText(string.Concat(textArray18));
                        writer3.Write("Documento se encuentra firmado No.Sap: " + DocNum.ToString());
                        writer3.Flush();
                        writer3.Close();
                        if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                        {
                            string[] textArray19 = new string[] { "CALL FELONE_UTILS ('False','", DocEntry, "','", Tipo, "','No se pudo guardar el xml, verifique permisos','','','','','','','') " };
                            str15 = string.Concat(textArray19);
                        }
                        else
                        {
                            string[] textArray20 = new string[] { "EXEC FELONE_UTILS 'False','", DocEntry, "','", Tipo, "','No se pudo guardar el xml, verifique permisos','','','','','','','' " };
                            str15 = string.Concat(textArray20);
                        }
                        recordset.DoQuery(str15);
                    }
                    else
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        RestClient client = new RestClient(baseUrl);
                        RestRequest request = new RestRequest(RestSharp.Method.POST);
                        request.AddHeader("Content-Type", "application/xml");
                        string str21 = sXML;
                        request.AddParameter("application/xml", str21, ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);

                        if (response.Content.Length > 0)
                        {
                            IEnumerator enumerator = null;
                            document = new XmlDocument();
                            document.LoadXml(response.Content);
                            str22 = "";
                            str23 = "";
                            str24 = "";
                            str25 = "";
                            str26 = "";
                            str27 = "";
                            str28 = "";
                            flag5 = false;
                            XmlNodeList elementsByTagName = document.GetElementsByTagName("SOAP-ENV:Envelope");
                            try
                            {
                                enumerator = elementsByTagName.GetEnumerator();
                                while (true)
                                {
                                    IEnumerator enumerator2 = null;
                                    if (!enumerator.MoveNext())
                                    {
                                        break;
                                    }
                                    XmlNode current = (XmlNode)enumerator.Current;
                                    string xml = current.InnerXml.ToString();
                                    XmlDocument document2 = new XmlDocument();
                                    document2.LoadXml(xml);
                                    XmlNodeList list2 = document2.GetElementsByTagName("SOAP-ENV:Body");
                                    try
                                    {
                                        enumerator2 = list2.GetEnumerator();
                                        while (true)
                                        {
                                            if (!enumerator2.MoveNext())
                                            {
                                                break;
                                            }
                                            XmlNode node2 = (XmlNode)enumerator2.Current;
                                            string str30 = node2.InnerXml.ToString();
                                            XmlDocument document3 = new XmlDocument();
                                            document3.LoadXml(str30);
                                            XmlNodeList list3 = document3.GetElementsByTagName("ns1:CertificacionDocumentoResponse");
                                            str28 = list3[0].ChildNodes.Item(0).InnerText.Trim();
                                            if (!str28.Contains("\"error\":0"))
                                            {
                                                if (!str28.Contains("El documento ya se encuentra anulado"))
                                                {
                                                    continue;
                                                }
                                                flag5 = true;
                                                continue;
                                            }
                                            flag5 = true;
                                            str27 = list3[0].ChildNodes.Item(2).InnerText.Trim();
                                            str24 = list3[0].ChildNodes.Item(6).InnerText.Trim();
                                            str23 = list3[0].ChildNodes.Item(7).InnerText.Trim();
                                            str22 = list3[0].ChildNodes.Item(8).InnerText.Trim();
                                            str26 = list3[0].ChildNodes.Item(9).InnerText.Trim();
                                        }
                                    }
                                    finally
                                    {
                                        if (enumerator2 is IDisposable)
                                        {
                                            (enumerator2 as IDisposable).Dispose();
                                        }
                                    }
                                }
                                goto TR_0022;
                            }
                            finally
                            {
                                if (enumerator is IDisposable)
                                {
                                    (enumerator as IDisposable).Dispose();
                                }
                            }
                        }
                        if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                        {
                            string[] textArray16 = new string[] { "CALL FELONE_UTILS ('FalseA','", DocEntry, "','", Tipo, "','Envie a anular nuevamente','','','','','','','') " };
                            str15 = string.Concat(textArray16);
                        }
                        else
                        {
                            string[] textArray17 = new string[] { "EXEC FELONE_UTILS 'FalseA','", DocEntry, "','", Tipo, "','Envie a anular nuevamente','','','','','','','' " };
                            str15 = string.Concat(textArray17);
                        }
                        ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str15);
                    }
                }
                return;
            TR_0022:
                try
                {
                    if (!flag5)
                    {
                        if (!flag5)
                        {
                            string[] textArray10 = new string[] { str7, "/Error_Anulada_", CurrSerieName, "_", DocNum, ".xml" };
                            writer = System.IO.File.AppendText(string.Concat(textArray10));
                            writer.Write(str28.ToString());
                            writer.Flush();
                            writer.Close();
                            if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                            {
                                string[] textArray11 = new string[] { "CALL FELONE_UTILS ('FalseA','", DocEntry, "','", Tipo, "','", str28.ToString(), "','','','','','','','') " };
                                str15 = string.Concat(textArray11);
                            }
                            else
                            {
                                string[] textArray12 = new string[] { "EXEC FELONE_UTILS 'FalseA','", DocEntry, "','", Tipo, "','", str28.ToString(), "','','','','','','','' " };
                                str15 = string.Concat(textArray12);
                            }
                            ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str15);
                        }
                    }
                    else
                    {
                        try
                        {
                            string[] textArray3 = new string[] { str7, "/AprobadaAnulada_", CurrSerieName, "_", DocNum, ".xml" };
                            writer = System.IO.File.AppendText(string.Concat(textArray3));
                            writer.Write(document.InnerXml.ToString());
                            writer.Flush();
                            writer.Close();
                            if (str27.Length > 0)
                            {
                                string[] textArray4 = new string[] { str9, "/", str20, "_", CurrSerieName, "_", DocNum, ".pdf" };
                                str25 = string.Concat(textArray4);
                                BinaryWriter writer2 = new BinaryWriter(System.IO.File.Open(str25, FileMode.Create));
                                writer2.Write(Convert.FromBase64String(str27));
                                writer2.Close();
                            }
                            if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                            {
                                string[] textArray5 = new string[0x11];
                                textArray5[0] = "CALL FELONE_UTILS ('TrueA','";
                                textArray5[1] = DocEntry;
                                textArray5[2] = "','";
                                textArray5[3] = Tipo;
                                textArray5[4] = "','";
                                textArray5[5] = str24.ToString();
                                textArray5[6] = "','";
                                textArray5[7] = str23;
                                textArray5[8] = "','";
                                textArray5[9] = str22;
                                textArray5[10] = "','";
                                textArray5[11] = str25;
                                textArray5[12] = "','";
                                textArray5[13] = str26;
                                textArray5[14] = "','";
                                textArray5[15] = str26;
                                textArray5[0x10] = "','','') ";
                                str15 = string.Concat(textArray5);
                            }
                            else
                            {
                                string[] textArray6 = new string[0x11];
                                textArray6[0] = "EXEC FELONE_UTILS 'TrueA','";
                                textArray6[1] = DocEntry;
                                textArray6[2] = "','";
                                textArray6[3] = Tipo;
                                textArray6[4] = "','";
                                textArray6[5] = str24.ToString();
                                textArray6[6] = "','";
                                textArray6[7] = str23;
                                textArray6[8] = "','";
                                textArray6[9] = str22;
                                textArray6[10] = "','";
                                textArray6[11] = str25;
                                textArray6[12] = "','";
                                textArray6[13] = str26;
                                textArray6[14] = "','";
                                textArray6[15] = str26;
                                textArray6[0x10] = "','','' ";
                                str15 = string.Concat(textArray6);
                            }
                            ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str15);
                        }
                        catch (Exception)
                        {
                            string[] textArray7 = new string[] { str8, "/ErrorSistemaAprobadaAnulada_", CurrSerieName, "_", DocNum, ".xml" };
                            writer = System.IO.File.AppendText(string.Concat(textArray7));
                            writer.Write(document.InnerXml.ToString());
                            writer.Flush();
                            writer.Close();
                            if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                            {
                                string[] textArray8 = new string[] { "CALL FELONE_UTILS ('TrueErrorA','", DocEntry, "','", Tipo, "','", document.InnerXml.ToString(), "','','','','','','','') " };
                                str15 = string.Concat(textArray8);
                            }
                            else
                            {
                                string[] textArray9 = new string[] { "EXEC FELONE_UTILS 'TrueErrorA','", DocEntry, "','", Tipo, "','", document.InnerXml.ToString(), "','','','','','','','' " };
                                str15 = string.Concat(textArray9);
                            }
                            ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str15);
                        }
                    }
                }
                catch (Exception exception2)
                {
                    string[] textArray13 = new string[] { str8, "/ErrorSistemaAnulada_", CurrSerieName, "_", DocNum, ".xml" };
                    writer = System.IO.File.AppendText(string.Concat(textArray13));
                    writer.Write(str28.ToString());
                    writer.Flush();
                    writer.Close();
                    if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                    {
                        string[] textArray14 = new string[] { "CALL FELONE_UTILS ('False','", DocEntry, "','", Tipo, "','", exception2.Message.ToString(), "','','','','','','','') " };
                        str15 = string.Concat(textArray14);
                    }
                    else
                    {
                        string[] textArray15 = new string[] { "EXEC FELONE_UTILS 'False','", DocEntry, "','", Tipo, "','", exception2.Message.ToString(), "','','','','','','','' " };
                        str15 = string.Concat(textArray15);
                    }
                    ((Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset)).DoQuery(str15);
                    SBO_Application.SetStatusBarMessage("Falla al intentar registrar el documento , motivo de la f\x00e1lla: " + exception2.Message.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, true);
                }
            }
            catch (Exception exception3)
            {
                string[] textArray21 = new string[] { str8, "/ErrorSistema_", CurrSerieName, "_", DocNum, ".xml" };
                StreamWriter writer4 = System.IO.File.AppendText(string.Concat(textArray21));
                writer4.Write("No.Sap: " + DocNum.ToString() + "Error VB: " + exception3.ToString());
                writer4.Flush();
                writer4.Close();
                if (OCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                {
                    string[] textArray22 = new string[] { "CALL FELONE_UTILS ('False','", DocEntry, "','", Tipo, "','", exception3.Message.ToString(), "','','','','','','','') " };
                    str15 = string.Concat(textArray22);
                }
                else
                {
                    string[] textArray23 = new string[] { "EXEC FELONE_UTILS 'False','", DocEntry, "','", Tipo, "','", exception3.Message.ToString(), "','','','','','','','' " };
                    str15 = string.Concat(textArray23);
                }
                recordset.DoQuery(str15);
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
                        case "TEKRAuser":
                            oItem = oForm.Items.Item("usuario");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "TEKRApass":
                            oItem = oForm.Items.Item("clave");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Tclient":
                            oItem = oForm.Items.Item("cliente");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "Tbusiness":
                            oItem = oForm.Items.Item("contrato");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
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
                        case "UR_tci":
                            oItem = oForm.Items.Item("UR_tci");
                            Valor = RecSet.Fields.Item("U_VALOR").Value;
                            break;
                        case "UR_wsdl":
                            oItem = oForm.Items.Item("UR_wsdl");
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
            SAPbouiCOM.EditText text;
            SAPbouiCOM.ProgressBar bar = null;
            SAPbouiCOM.ProgressBar bar2 = null;
            try
            {
                bar = SBO_Application.StatusBar.CreateProgressBar("Guardando parámetros por favor espere...", 18, false);
                oUsrTbl = oCompany.UserTables.Item("FEL_PARAMETROS");

                text = (EditText)oForm.Items.Item("Nemi").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Nombre del Emisor");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "Nemi", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("NitEmi").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el numero de NIT");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "NitEmi", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("Tafilia").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Tipo de Afiliacion");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "Tafilia", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("Correo").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Nombre del Emisor");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "Correo", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("UR_t").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Path del token");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_t", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("UR_wsdl").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Path del token");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_wsdl", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("UR_tci").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Path del XML Sin Autorizacion");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "UR_tci", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("Pathsaut").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Path del XML Sin Autorizacion");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXML", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("Pathaut").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Path del XML de Autorizacion");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXMLaut", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("Pathres").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Path  respuesta");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXMLres", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("Patherr").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Path del XML Error");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHXMLerr", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("Pathpdf").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 1;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabFACE")).Select();
                    throw new Exception("Debe de Ingresar el Path del XML PDF");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PATHPDF", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("txtUsuario").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabCNN")).Select();
                    throw new Exception("Debe de Ingresar el usuario de la base de datos");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "USRDB", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("txtPass").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabCNN")).Select();
                    throw new Exception("Debe de Ingresar el password de la base de datos");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "PASSDB", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;

                text = (EditText)oForm.Items.Item("usuario").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabCNN")).Select();
                    throw new Exception("Debe de Ingresar el password de la base de datos");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "TEKRAuser", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("clave").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabCNN")).Select();
                    throw new Exception("Debe de Ingresar el password de la base de datos");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "TEKRApass", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("cliente").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabCNN")).Select();
                    throw new Exception("Debe de Ingresar el password de la base de datos");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "Tclient", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;
                text = (EditText)oForm.Items.Item("contrato").Specific;
                if (text.Value.Trim() == "")
                {
                    oForm.PaneLevel = 3;
                    text.Active = true;
                    ((Folder)oForm.Items.Item("tabCNN")).Select();
                    throw new Exception("Debe de Ingresar el password de la base de datos");
                }
                ParametrosForm.GuardaParametro(oUsrTbl, "Tbusiness", text.Value);
                (bar2 = bar).Value = bar2.Value + 1;

                ParametrosForm.GuardaDatosSeries();
                bar2.Value += 1;
                bar2.Stop();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(bar2);
                bar2 = null;
                GC.Collect();
                SBO_Application.SetStatusBarMessage("Parámetros guardados exítosamente", SAPbouiCOM.BoMessageTime.bmt_Short, false);
            }
            catch (Exception ex)
            {
                bar2.Stop();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(bar2);
                bar2 = null/* TODO Change to default(_) if this is not a reference type */;
                GC.Collect();
                SBO_Application.MessageBox(ex.Message);
            }
        }



        public static string getNameApiNit(SAPbobsCOM.Company OCompany, Application SBO_Application, string nit)
        {
            try
            {
                string usuario = Utils.ObtieneValorParametro(OCompany, SBO_Application, "TEKRAuser");
                string clave = Utils.ObtieneValorParametro(OCompany, SBO_Application, "TEKRApass");
                string cliente = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Tclient");
                string contrato = Utils.ObtieneValorParametro(OCompany, SBO_Application, "Tbusiness");
                string urlCI = Utils.ObtieneValorParametro(OCompany, SBO_Application, "UR_tci");

                //usuario = "toj_api_user";
                //clave = "F2wnDe9Y5e4O0qmA";
                //cliente = "2221080008";
                //contrato = "2222080008";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient(urlCI);
                RestRequest request = new RestRequest(RestSharp.Method.POST);
                request.AddHeader("Content-Type", "application/xml");
                string json = @"{
                   ""autenticacion"":
                   {
                        ""pn_usuario"": """ + usuario + @""",
                        ""pn_clave"": """ + clave + @"""
                   },
                   ""parametros"":
                   {
                        ""pn_empresa"": 1,
                               ""pn_cliente"": """ + cliente + @""",
                               ""pn_contrato"": """ + contrato + @""",
                               ""pn_nit"": """ + nit + @"""
                   }
                }";
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                JObject h = JObject.Parse(response.Content);
                tekraMoedelInfoClient m = JsonConvert.DeserializeObject<tekraMoedelInfoClient>(response.Content);
                string nombre = m.datos[0].nombre;
                return nombre;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
