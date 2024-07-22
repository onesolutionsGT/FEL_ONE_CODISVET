using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Xml;
using FEL_ONE.Certificadores;
using SAPbobsCOM;


namespace FEL_ONE.Clases
{

    class Utils
    {
        public static SAPbouiCOM.Application SBOApplication { get; set; }
        public static SAPbobsCOM.Company Company { get; set; }
        public enum TipoFEL
        {
            /*0*/
            MEGAPRINT,
            /*1*/
            INFILE,
            /*2*/
            G4S,
            /*3*/
            DIGIFACT,
            /*4*/
            ECOFACTURAS,
            /*5*/
            GUATEFACTURAS,
            /*6*/
            TEKRA
        }
        public static TipoFEL FEL { get; set; }

        internal static void AddUserTable(SAPbobsCOM.Company oCompany, string TableName, string TableDescription, SAPbobsCOM.BoUTBTableType typeTable)
        {
            SAPbobsCOM.UserTablesMD oUserTablesMD;

            try
            {
                oUserTablesMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables);
                if (!oUserTablesMD.GetByKey(TableName))
                {
                    oUserTablesMD.TableName = TableName;
                    oUserTablesMD.TableDescription = TableDescription;
                    oUserTablesMD.TableType = typeTable;
                    oUserTablesMD.Add();

                    oUserTablesMD.Update();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserTablesMD);
                oUserTablesMD = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        internal static bool ActivateFormIsOpen(SAPbouiCOM.Application SboApplication, string FormID)
        {
            try
            {
                Boolean result = false;
                for (int x = 0; x <= SboApplication.Forms.Count - 1; x++)
                {
                    if (SboApplication.Forms.Item(x).UniqueID == FormID)
                    {
                        SboApplication.Forms.Item(x).Select();
                        result = true;
                        break;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        internal static void AddUserField(SAPbobsCOM.Company oCompany, string TableName, string FieldName, string FieldDescription, SAPbobsCOM.BoFieldTypes FieldType, int Size, Boolean addSymbol = true, SAPbobsCOM.BoFldSubTypes SubType = default)
        {
            int lerrcode;
            string serrmsg;

            string[,] TipoDoc = {
                { "NIT", "NIT" },
                { "CUI", "Código Único de Identificación" },
                { "EXT", "Numero de Documento Extranjero" },
                { "CF", "Consumidor Final" },
            };

            string[,] Incoterms = {
                { "EXW", "En fábrica" },
                { "FCA", "Libre transportista" },
                { "FAS", "Libre al costado del buque" } ,
                { "FOB", "Libre a bordo" },
                { "CFR", "Costo y flete" },
                { "CIF", "Costo, seguro y flete" },
                { "CPT", "Flete pagado hasta" },
                { "CIP", "Flete y seguro pagado hasta" },
                { "DDP", "Entregado en destino con derechos pagados" },
                { "DAP", "Entregada en lugar" },
                { "DPU", "Entregada en el lugar de la descarga" },
                { "ZZZ", "Otros" }
            };

            string[,] Frases = {
                { "1",  "Exenta del IVA (art. 7 num. 2 Ley del IVA)" },
                { "2",  "Exenta del IVA (art. 7 num. 4 Ley del IVA)" },
                { "3",  "Exenta del IVA (art. 7 num. 5 Ley del IVA)" },
                { "4",  "Exenta del IVA (art. 7 num. 9 Ley del IVA)" },
                { "5",  "Exenta del IVA (art. 7 num. 10 Ley del IVA)" },
                { "6",  "Exenta del IVA (art. 7 num. 13 Ley del IVA)" },
                { "7",  "Exenta del IVA (art. 7 num. 14 Ley del IVA)" },
                { "8",  "Exenta del IVA (art. 8 num. 1 Ley del IVA)" },
                { "9",  "Exenta del IVA (art. 7 num. 15 Ley del IVA)" },
                { "10", "Esta factura no incluye IVA (art. 55 Ley del IVA)" },
                { "11", "No afecta al IVA (Decreto 29-89 Ley de Maquila)" },
                { "12", "No afecta al IVA (Decreto 65-89 Ley de Zonas Francas)" },
                { "13", "Exenta del IVA (art. 7 num. 12, Ley del IVA)" },
                { "14", "Exenta del IVA (art. 7 num. 6 Ley del IVA)" },
                { "15", "Exenta del IVA (art. 7 num. 11 Ley del IVA)" },
                { "16", "Exenta del IVA (art. 8 num. 2 Ley del IVA)" },
                { "17", "Exenta del IVA (art. 32 literal c, Ley Orgánica Zolic)" },
                { "18", "(Contribuyentes con disposiciones específicas de exención al IVA)" },
                { "19", "Exenta del IVA (art. 3 num. 7 Ley del IVA)" },
                { "20", "Aportes (art. 35 Ley de Fortalecimiento al Emprendimiento)" },
                { "21", "Cargos e impuestos no sujetos a IVA (Aerolíneas)" },
                { "22", "Factura origen no incluye IVA" },
                { "23", "Exenta del IVA (art. 7, numeral 3, literal c, Ley del IVA)" },
                { "24", "No afecto al IVA (Fuera del hecho generador art. 3, 7 y 8, Ley del IVA)" },
                { "25", "Exenta del IVA (art. 31 Dec. 22-73 Ley Orgánica Zolic)" },
                { "26", "Exenta del IVA (art. 4 Dec. 31-2022 Ley de Fom. del Trab. Temp. en el Extranjero)" },
                { "27", "Exenta del IVA (art. 7 literal “a” Dec. 40-2022 Ley Inc. Mov. Eléctrica)" },
                { "28", "Exenta del IVA (art. 7 literal “c” Dec. 40-2022 Ley Inc. Mov. Eléctrica)" },
                { "29", "Exenta del IVA (art. 7 literal “d” Dec. 40-2022 Ley Inc. Mov. Eléctrica)" },
                { "30", "Exenta del IVA (art. 7 literal “g” Dec. 40-2022 Ley Inc. Mov. Eléctrica)" },
                { "31", "Exenta del IVA (art. 7 literal “h” o “i” Dec. 40- 2022 Ley Inc. Mov. Eléctrica)" },
                { "32", "No afecto al IVA (Fuera del hecho generador art. 3, Ley del IVA)" },
            };

            string[,] EstadosFEL = {
                { "P", "PENDIENTE" },
                { "A", "AUTORIZADO" },
                { "R", "RECHAZADO" },
                { "ANULAR", "ANULAR" },
                { "ANULADO", "ANULADO" }
            };

            try
            {
                if (!ExistField(oCompany, TableName, FieldName, addSymbol))
                {
                    SAPbobsCOM.UserFieldsMD oUserFieldsMD;
                    oUserFieldsMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                    oUserFieldsMD.TableName = TableName;
                    oUserFieldsMD.Name = FieldName;
                    oUserFieldsMD.Description = FieldDescription;
                    oUserFieldsMD.Type = FieldType;

                    switch (FieldName)
                    {
                        case "INCOTERM":
                            for (int i = 0; i < Incoterms.GetLength(0); i++)
                            {
                                oUserFieldsMD.ValidValues.Description = Incoterms[i, 1];
                                oUserFieldsMD.ValidValues.Value = Incoterms[i, 0];
                                oUserFieldsMD.ValidValues.Add();
                            }
                            oUserFieldsMD.Mandatory = SAPbobsCOM.BoYesNoEnum.tNO;
                            break;
                        case "FRASE_EXENTO":
                            for (int i = 0; i < Frases.GetLength(0); i++)
                            {
                                oUserFieldsMD.ValidValues.Description = Frases[i, 1];
                                oUserFieldsMD.ValidValues.Value = Frases[i, 0];
                                oUserFieldsMD.ValidValues.Add();
                            }
                            oUserFieldsMD.Mandatory = SAPbobsCOM.BoYesNoEnum.tNO;
                            break;
                        case "ESTADO_FACE":
                            for (int i = 0; i < EstadosFEL.GetLength(0); i++)
                            {
                                oUserFieldsMD.ValidValues.Description = EstadosFEL[i, 1];
                                oUserFieldsMD.ValidValues.Value = EstadosFEL[i, 0];
                                oUserFieldsMD.DefaultValue = "P";
                                oUserFieldsMD.ValidValues.Add();
                            }
                            oUserFieldsMD.Mandatory = SAPbobsCOM.BoYesNoEnum.tNO;
                            break;
                        case "TIPO_DOCUMENTO_FEL":
                            for (int i = 0; i < TipoDoc.GetLength(0); i++)
                            {
                                oUserFieldsMD.ValidValues.Description = TipoDoc[i, 1];
                                oUserFieldsMD.ValidValues.Value = TipoDoc[i, 0];
                                oUserFieldsMD.DefaultValue = "NIT";
                                oUserFieldsMD.Mandatory = BoYesNoEnum.tYES;
                                oUserFieldsMD.ValidValues.Add();
                            }
                            oUserFieldsMD.Mandatory = SAPbobsCOM.BoYesNoEnum.tNO;
                            break;
                        default:
                            break;
                    }

                    oUserFieldsMD.SubType = SubType;
                    if (FieldType == SAPbobsCOM.BoFieldTypes.db_Alpha || FieldType == SAPbobsCOM.BoFieldTypes.db_Numeric)
                    {
                        oUserFieldsMD.EditSize = Size;
                    }

                    if (oUserFieldsMD.Add() != 0)
                    {
                        oCompany.GetLastError(out lerrcode, out serrmsg);
                        throw new Exception(serrmsg);
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserFieldsMD);
                    oUserFieldsMD = null;
                    GC.Collect();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        internal static Boolean ExistField(SAPbobsCOM.Company oCompany, string TableName, string FieldName, Boolean addSymbol)
        {
            SAPbobsCOM.Recordset RecSet;
            string QryStr = "";
            Boolean result = false;
            try
            {
                if (addSymbol)
                {
                    TableName = "@" + TableName;
                }
                QryStr = @"select ""TableID"",""FieldID"",""AliasID"" from CUFD WHERE ""TableID""='" + TableName + @"' and ""AliasID""  ='" + FieldName + "'";
                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(QryStr);
                if (RecSet.RecordCount > 0)
                {
                    result = true;
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                RecSet = null;
                GC.Collect();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static string TraeDato(string Sql)
        {
            try
            {
                SqlCommand cm;
                SqlDataAdapter da;
                DataSet ds;
                SqlConnection Cnn = new SqlConnection();
                string UsuarioDB = ObtieneValorParametro(Company, SBOApplication, "USRDB");
                string PassDB = ObtieneValorParametro(Company, SBOApplication, "PASSDB");

                Cnn.ConnectionString = "Data Source=" + Company.Server + ";initial Catalog=" + Company.CompanyDB + ";Persist Security Info=True;User ID=" + UsuarioDB + ";Password=" + PassDB;
                //Cnn.ConnectionString = "Data Source=" + Company.Server + ";initial Catalog=" + Company.CompanyDB + ";Persist Security Info=True;Trusted_Connection=True; ";
                Cnn.Open();
                cm = new SqlCommand();
                cm.CommandText = Sql;
                cm.CommandType = CommandType.Text;
                cm.Connection = Cnn;
                da = new SqlDataAdapter(cm);
                ds = new DataSet();
                da.Fill(ds);
                Cnn.Close();

                if (ds.Tables[0].Rows.Count > 0)
                {
                    return (ds.Tables[0].Rows[0][0]).ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static string TraeDatoH(string Sql)
        {
            try
            {
                SqlConnection Cnn = new SqlConnection();
                string UsuarioDB = ObtieneValorParametro(Company, SBOApplication, "USRDB");
                DataTable dtTable = new DataTable();
                SAPbobsCOM.Recordset RecSet;
                DataColumn NewCol;
                DataRow NewRow;
                int ColCount;
                RecSet = Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(Sql);

                for (ColCount = 0; ColCount <= RecSet.Fields.Count - 1; ColCount++)
                {
                    NewCol = new DataColumn(RecSet.Fields.Item(ColCount).Name);
                    dtTable.Columns.Add(NewCol);
                }

                while (!RecSet.EoF)
                {
                    NewRow = dtTable.NewRow();
                    for (ColCount = 0; ColCount <= RecSet.Fields.Count - 1; ColCount++)
                    {
                        NewRow[RecSet.Fields.Item(ColCount).Name] = RecSet.Fields.Item(ColCount).Value;
                        dtTable.Rows.Add(NewRow);
                        RecSet.MoveNext();
                    }
                }
                GC.Collect();
                return dtTable.Rows[0][0].ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string ObtieneValorParametro(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Parametro)
        {
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string resp;

            try
            {
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                QryStr = @"select * from ""@FEL_PARAMETROS"" where ""U_PARAMETRO""='" + Parametro + "'";
                RecSet.DoQuery(QryStr);
                RecSet.MoveFirst();
                resp = (RecSet.Fields.Item("U_VALOR").Value).ToString();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                RecSet = null;
                GC.Collect();
                return resp;
            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
                return null;
            }
        }
        public static bool SerieEsBatch(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string CodeSerie)
        {
            string sql;
            SAPbobsCOM.Recordset RecSet;

            try
            {
                if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    sql = @"SELECT ifnull(""U_ES_BATCH"",'N') FROM ""@FEL_RESOLUCION"" WHERE ""Code"" = " + CodeSerie + "";
                }
                else
                {
                    sql = @"SELECT isnull(U_ES_BATCH,'N') FROM [@FEL_RESOLUCION] WHERE CODE=" + CodeSerie;
                }
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(sql);
                if (RecSet.RecordCount > 0)
                {
                    if (RecSet.Fields.Item(0).Value == "Y")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
                return true;
            }
        }

        [Obsolete]
        internal static void EnviaDocumento(SAPbobsCOM.Company OCompany, string tabla, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string DocEntry, string CurrSerieName, string Pais, bool ProcesarBatch = false, string Linea = "")
        {
            ProcesarBatch = SerieEsBatch(OCompany, SBO_Application, CurrSerie);
            switch (Utils.FEL)
            {
                case TipoFEL.MEGAPRINT:
                    Megaprint.EnviaDocumentoFEL(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, DocEntry);
                    break;
                case TipoFEL.INFILE:
                    new Infile().EnviaDocumentoFEL(OCompany, tabla, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, DocEntry);
                    break;
                case TipoFEL.G4S:
                    G4s.EnviaDocumentoFEL(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, Pais, DocEntry, ProcesarBatch);
                    break;
                case TipoFEL.DIGIFACT:
                    Digifact.EnviaDocumentoFEL(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, Pais, DocEntry, ProcesarBatch);
                    break;
                case TipoFEL.ECOFACTURAS:
                    EcoFacturas.EnviaDocumentoFEL(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, Pais, DocEntry, ProcesarBatch);
                    break;
                case TipoFEL.GUATEFACTURAS:
                    Guatefacturas.EnviaDocumentoFEL(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, Pais, DocEntry, ProcesarBatch);
                    break;
                case TipoFEL.TEKRA:
                    Tekra.EnviaDocumentoFEL(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, DocEntry, "SI");
                    break;
            }
        }

        [Obsolete]
        internal static void EnviaDocumentoA(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Tipo, string CurrSerie, string DocNum, string DocEntry, string CurrSerieName, string Pais, bool ProcesarBatch = false, string Linea = "")
        {
            switch (Utils.FEL)
            {
                case TipoFEL.MEGAPRINT:
                    Megaprint.EnviaDocumentoFELA(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, DocEntry);
                    break;
                case TipoFEL.INFILE:
                    Infile.EnviaDocumentoFELA(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, DocEntry);
                    break;
                case TipoFEL.G4S:
                    G4s.EnviaDocumentoFELA(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, Pais, DocEntry, ProcesarBatch);
                    break;
                case TipoFEL.DIGIFACT:
                    Digifact.EnviaDocumentoFELA(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, Pais, DocEntry, ProcesarBatch);
                    break;
                case TipoFEL.ECOFACTURAS:
                    EcoFacturas.EnviaDocumentoFELA(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, Pais, DocEntry, ProcesarBatch);
                    break;
                case TipoFEL.GUATEFACTURAS:
                    Guatefacturas.EnviaDocumentoFELA(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, Pais, DocEntry, ProcesarBatch);
                    break;
                case TipoFEL.TEKRA:
                    Tekra.EnviaDocumentoFELA(OCompany, SBO_Application, Tipo, CurrSerie, DocNum, CurrSerieName, DocEntry, "SI");
                    break;
            }
        }
        public static bool ExisteDocumento(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Serie, string docentry, string TypeDoc, bool esBatch)
        {
            bool flag;
            bool flag2 = false;
            if (!esBatch)
            {
                try
                {
                    Recordset businessObject;
                    if (TypeDoc == "FACT" || TypeDoc == "RDON" || TypeDoc == "FCAM" || TypeDoc == "RECI" || TypeDoc == "FEXP" || TypeDoc == "FEXPM" || TypeDoc == "FRES")
                    {
                        businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        businessObject.DoQuery(("select \"U_ESTADO_FACE\" from OINV WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                        if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "A" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULADO" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "R")
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (TypeDoc == "FACTA")
                    {
                        businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        businessObject.DoQuery(("select \"U_ESTADO_FACE\" from ODPI WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                        if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "A" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULADO" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "R")
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (TypeDoc == "NDEB")
                    {
                        businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        string[] textArray1 = new string[] { "select \"U_ESTADO_FACE\" from OINV WHERE  \"Series\" =", Serie, " and \"DocEntry\" =", docentry.ToString(), " and \"DocSubType\" ='DN'" };
                        businessObject.DoQuery(string.Concat(textArray1));
                        if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "A" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULADO" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "R")
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (TypeDoc == "NCRE" || TypeDoc == "NABN")
                    {
                        businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        businessObject.DoQuery(("select \"U_ESTADO_FACE\" from ORIN WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                        if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "A" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULADO" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "R")
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (TypeDoc == "FESP")
                    {
                        businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        businessObject.DoQuery(("select \"U_ESTADO_FACE\" from OPCH  WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                        if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "A" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULADO" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "R")
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    flag = flag2;
                }
                catch (Exception exception1)
                {
                    SBO_Application.MessageBox(exception1.Message, 1, "Ok", "", "");
                    flag = false;
                }
            }
            else
            {
                try
                {
                    Recordset businessObject;
                    if (TypeDoc == "FACT" || TypeDoc == "RDON" || TypeDoc == "FCAM" || TypeDoc == "RECI" || TypeDoc == "FEXP" || TypeDoc == "FEXPM" || TypeDoc == "FRES")
                    {
                        businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        businessObject.DoQuery(("select \"U_ESTADO_FACE\" from OINV WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                        if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "A" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULADO")
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (TypeDoc == "NDEB")
                    {
                        businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        string[] textArray1 = new string[] { "select \"U_ESTADO_FACE\" from OINV WHERE  \"Series\" =", Serie, " and \"DocEntry\" =", docentry.ToString(), " and \"DocSubType\" ='DN'" };
                        businessObject.DoQuery(string.Concat(textArray1));
                        if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "A" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULADO")
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (TypeDoc == "NCRE" || TypeDoc == "NABN")
                    {
                        businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        businessObject.DoQuery(("select \"U_ESTADO_FACE\" from ORIN WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                        if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "A" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULADO")
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (TypeDoc == "FESP")
                    {
                        businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        businessObject.DoQuery(("select \"U_ESTADO_FACE\" from OPCH  WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                        if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "A" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR" || businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULADO")
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    flag = flag2;
                }
                catch (Exception exception1)
                {
                    SBO_Application.MessageBox(exception1.Message, 1, "Ok", "", "");
                    flag = false;
                }
            }
            return flag;
        }
        public static bool ExisteDocumentoANULAR(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string Serie, string docentry, string TypeDoc)
        {
            bool flag;
            bool flag2 = false;
            try
            {
                Recordset businessObject;
                if (TypeDoc == "FACT" || TypeDoc == "RDON" || TypeDoc == "FCAM" || TypeDoc == "RECI" || TypeDoc == "FEXP" || TypeDoc == "FEXPM" || TypeDoc == "FRES")
                {
                    businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    businessObject.DoQuery(("select \"U_ESTADO_FACE\" from OINV WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                    if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR")
                    {
                        flag2 = true;
                    }
                    else
                    {
                        flag2 = false;
                    }
                }
                if (TypeDoc == "FACTA")
                {
                    businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    businessObject.DoQuery(("select \"U_ESTADO_FACE\" from ODPI WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                    if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR")
                    {
                        flag2 = true;
                    }
                    else
                    {
                        flag2 = false;
                    }
                }
                if (TypeDoc == "NDEB")
                {
                    businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    string[] textArray1 = new string[] { "select \"U_ESTADO_FACE\" from OINV WHERE  \"Series\" =", Serie, " and \"DocEntry\" =", docentry.ToString() };
                    businessObject.DoQuery(string.Concat(textArray1));
                    if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR")
                    {
                        flag2 = true;
                    }
                    else
                    {
                        flag2 = false;
                    }
                }
                if (TypeDoc == "NCRE" || TypeDoc == "NABN")
                {
                    businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    businessObject.DoQuery(("select \"U_ESTADO_FACE\" from ORIN WHERE  \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                    if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR")
                    {
                        flag2 = true;
                    }
                    else
                    {
                        flag2 = false;
                    }
                }
                if (TypeDoc == "FESP")
                {
                    businessObject = (Recordset)OCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    businessObject.DoQuery(("select \"U_ESTADO_FACE\" from OPCH WHERE \"Series\" =" + Serie + " and \"DocEntry\" =" + docentry.ToString()) ?? "");
                    if (businessObject.Fields.Item("U_ESTADO_FACE").Value == "ANULAR")
                    {
                        flag2 = true;
                    }
                    else
                    {
                        flag2 = false;
                    }
                }
                flag = flag2;
            }
            catch (Exception exception1)
            {
                SBO_Application.MessageBox(exception1.Message, 1, "Ok", "", "");
                flag = false;
            }
            return flag;
        }
        public static bool GrabarXml(SAPbobsCOM.Company OCompany, string sXML, string sSerie, string sNumDoc, string TipoDoc, ref string fileName)
        {
            bool flag;
            string str = (OCompany.DbServerType != BoDataServerTypes.dst_HANADB) ? TraeDato("SELECT \"U_VALOR\" FROM \"@FEL_PARAMETROS\"  WHERE \"U_PARAMETRO\" = 'PATHXML'").ToString() : TraeDatoH("SELECT \"U_VALOR\" FROM \"@FEL_PARAMETROS\"  WHERE \"U_PARAMETRO\" = 'PATHXML'").ToString();
            try
            {
                new XmlDocument().LoadXml(sXML.ToString());
                string path = string.Format(@"{0}\{3}_{1}_{2}.xml", str, sSerie, sNumDoc, TipoDoc);
                StreamWriter writer = new StreamWriter(path);
                writer.Write(sXML);
                writer.Close();
                fileName = path;
                flag = true;
            }
            catch (Exception exception1)
            {
                flag = false;
                throw new Exception(exception1.Message);
            }
            return flag;
        }
        public static bool ValidaSerie(SAPbobsCOM.Company OCompany, SAPbouiCOM.Application SBO_Application, string codeSerie, bool ProcesarBatch = false)
        {
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            bool result = false;
            try
            {
                RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                if (ProcesarBatch == false)
                {
                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        QryStr = ("CALL FELONE_UTILS('ValidaSerie','" + codeSerie + "','','','','','','','','','')");
                    }
                    else
                    {
                        QryStr = ("EXEC FELONE_UTILS 'ValidaSerie','" + codeSerie + "','','','','','','','','',''");
                    }
                }
                else
                {
                    if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        QryStr = ("CALL FELONE_UTILS('ValidaSerieBATCH','" + codeSerie + "','','','','','','','','','')");
                    }
                    else
                    {
                        QryStr = ("EXEC FELONE_UTILS 'ValidaSerieBATCH','" + codeSerie + "','','','','','','','','',''");
                    }
                }
                RecSet.DoQuery(QryStr);
                if (RecSet.RecordCount > 0)
                {
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
                return result;
            }
        }

        public static bool IsValidPDF(string filePath)
        {
            try
            {
                // Leer los primeros bytes del archivo
                byte[] buffer = new byte[5];
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }

                // Convertir los bytes leídos a una cadena
                string header = Encoding.ASCII.GetString(buffer);

                // Verificar si la cadena contiene "%PDF-"
                return header.StartsWith("%PDF-");
            }
            catch
            {
                return false;
            }
        }
        internal static string getCertificador()
        {
            switch (Utils.FEL)
            {
                case Utils.TipoFEL.MEGAPRINT:
                    return "MEGAPRINT";
                case Utils.TipoFEL.INFILE:
                    return "INFILE";
                case Utils.TipoFEL.G4S:
                    return "G4S";
                case Utils.TipoFEL.DIGIFACT:
                    return "DIGIFACT";
                case Utils.TipoFEL.ECOFACTURAS:
                    return "ECOFACTURAS";
                case Utils.TipoFEL.GUATEFACTURAS:
                    return "GUATEFACTURAS";
                case Utils.TipoFEL.TEKRA:
                    return "TEKRA";
                default:
                    return "N/A";
            }
        }
        public static void GrabarArchivo(Company OCompany, string Tipo, string DocEntry, string data, string filename, string param1, string param4, string param5, string param6, string param7, string param8, string param9, string param10, string param11)
        {
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            if (OCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
            {
                QryStr = "CALL FELONE_UTILS ('" + param1 + "','" + DocEntry + "','" + Tipo + "','" + param4 + "','" + param5 + "','" + param6 + "','" + param7 + "','" + param8 + "','" + param9 + "','" + param10 + "','" + param11 + "' )";

            }
            else
            {
                QryStr = "EXEC FELONE_UTILS '" + param1 + "','" + DocEntry + "','" + Tipo + "','" + param4 + "','" + param5 + "','" + param6 + "','" + param7 + "','" + param8 + "','" + param9 + "','" + param10 + "','" + param11 + "' ";

            }
            RecSet = OCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            RecSet.DoQuery(QryStr);
            RecSet = null;

            StreamWriter escritor;
            escritor = File.AppendText(filename);
            escritor.Write(data);
            escritor.Flush();
            escritor.Close();
        }
        public static void ActualizaCamposDocumento(Company oCompany, string TipoDocFEL, string DocEntry, string documentoFel, string serieFel)
        {
            //ESTANDAR
            SAPbobsCOM.Documents oDoc;
            if (TipoDocFEL == "FESP")
            {
                oDoc = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices);
            }
            else if (TipoDocFEL == "NCRE" || TipoDocFEL == "NABN")
            {
                oDoc = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);
            }
            else
            {
                oDoc = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);
            }
            oDoc.GetByKey(Convert.ToInt32(DocEntry));
            oDoc.NumAtCard = serieFel + "-" + documentoFel;
            oDoc.Update();
        }
    }
}