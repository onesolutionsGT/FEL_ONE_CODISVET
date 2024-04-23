using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using FEL_ONE.Forms;
using Microsoft.VisualBasic;
using SAPbobsCOM;
using SAPbouiCOM;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FEL_ONE.Clases
{
    public class SystemForm
    {

        #region variables form
        ParametrosForm paramForm;
        BatchForm batchForm;
        BatchFormA batchFormA;
        Certificador certiForm;
        Sps spsForm;
        #endregion

        #region VARIABLES DE ENTORNO
        private static SAPbouiCOM.Application SBO_Application { get; set; }
        private static SAPbobsCOM.Company oCompany { get; set; }
        private SAPbouiCOM.EventFilters oFilters { get; set; }
        private SAPbouiCOM.EventFilter oFilter { get; set; }
        private Boolean esFactura { get; set; }
        private string IdForm { get; set; }
        private string IdItem { get; set; }
        private int IdEvent { get; set; }
        public static string ConnectionString { get; set; }
        public static string SBOError { get { return "Error (" + oCompany.GetLastErrorCode() + "): " + oCompany.GetLastErrorDescription(); } }
        #endregion

        #region CONEXION A SAP

        [Obsolete]
        public SystemForm()
        {
            try
            {
                SetApplication();
                SBO_Application.StatusBar.SetText("Iniciando add-on facturaci\x00f3n electr\x00f3nica (FEL ONE standard version)...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);

                Utils.SBOApplication = SBO_Application;
                Utils.Company = oCompany;

                //METODOS DE INICIALIZACION
                AddUserTables();
                if (ExistCerti())
                {
                    AddUserTables();
                    AddMenuItems();
                }
                else
                {
                    AddConfigurationMenuItems();
                }
                SetEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\nSBO application no encontrada");
                System.Windows.Forms.Application.Exit();
            }
        }

        public static void SetApplication()
        {
            //Se obtiene string de conexion de Cliente SAP B1
            if (Environment.GetCommandLineArgs().Count() == 1) { throw new Exception("No se agregaron los parametros de conexión...", new Exception("No se encontro string de conexión SAP B1")); }
            ConnectionString = Environment.GetCommandLineArgs().GetValue(1).ToString();

            //Se realiza conexion 
            SAPbouiCOM.SboGuiApi client = new SAPbouiCOM.SboGuiApi();
            client.Connect(ConnectionString);
            SBO_Application = client.GetApplication(-1);

            //Se carga <<Company>> de aplicacion   
            oCompany = new SAPbobsCOM.Company();
            string cookies = oCompany.GetContextCookie();
            string connectionContext = SBO_Application.Company.GetConnectionContext(oCompany.GetContextCookie());
            oCompany.SetSboLoginContext(connectionContext);

            //Conexion con sociedad
            if (oCompany.Connect() != 0) { throw new Exception(SBOError); }
        }

        [Obsolete]
        private void SetEvents()
        {
            SBO_Application
                .MenuEvent += new SAPbouiCOM._IApplicationEvents_MenuEventEventHandler(SBOApp_MenuEvent);

            SBO_Application
                .FormDataEvent += new SAPbouiCOM._IApplicationEvents_FormDataEventEventHandler(SBOApp_FormDataEvent);

            SBO_Application
                .AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBOAppn_AppEvent);

            SBO_Application
                .ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBOApp_ItemEvent);
        }

        #endregion

        #region CODIGO GENERAL

        #region VARIABLES
        private SAPbouiCOM.Form oOrderForm;
        private const string Pais = "GT";
        #endregion

        [Obsolete]
        private void SBOApp_FormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, out bool BubbleEvent)
        {
            BubbleEvent = true;
            SAPbouiCOM.BoEventTypes[] boEventTypes = { SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD, SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE };
            string[] FormsTypeEx = { "133", "65300", "60090", "179", "65303", "60091", "141" };
            try
            {
                if (boEventTypes.Contains(BusinessObjectInfo.EventType) && BusinessObjectInfo.ActionSuccess)
                {
                    if (FormsTypeEx.Contains(BusinessObjectInfo.FormTypeEx))
                    {
                        string tabla = "";
                        string tipoDoc = "";
                        string queryStatus;
                        string status;
                        string queryDocNum;
                        SAPbouiCOM.ComboBox Serie;
                        string DocNum;
                        System.Xml.XmlNodeList DocEntry;

                        switch (BusinessObjectInfo.FormTypeEx)
                        {
                            case "133":     // FACTURA
                                tabla = "OINV"; tipoDoc = "FACT"; break;
                            case "65300":   // FACTURA ANTICIPO
                                tabla = "ODPI"; tipoDoc = "FACTA"; break;
                            case "60090":   // FACTURA + PAGO
                                tabla = "OINV"; tipoDoc = "FACT"; break;
                            case "179":     // NOTA DE CREDITO
                                tabla = "ORIN"; tipoDoc = "NCRE"; break;
                            case "65303":   // NOTA DE DEBITO
                                tabla = "OINV"; tipoDoc = "NDEB"; break;
                            case "60091":   // FACTURA DE RESERVA
                                tabla = "OINV"; tipoDoc = "FACT"; break;
                            case "141":     // FACTURA DE PROVEEDORES
                                tabla = "OPCH"; tipoDoc = "FESP"; break;
                        }

                        System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
                        xml.LoadXml(BusinessObjectInfo.ObjectKey);
                        Serie = oOrderForm.Items.Item("88").Specific;
                        DocEntry = xml.GetElementsByTagName("DocEntry");
                        queryDocNum = @"select ""DocNum"" from " + tabla + @" where ""DocEntry"" = '" + DocEntry[0].InnerText + "'";
                        queryStatus = @"select ""U_ESTADO_FACE"" from " + tabla + @" where ""DocEntry"" = '" + DocEntry[0].InnerText + "'";
                        if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            DocNum = Utils.TraeDatoH(queryDocNum);
                            status = Utils.TraeDatoH(queryStatus);
                        }
                        else
                        {
                            DocNum = Utils.TraeDato(queryDocNum);
                            status = Utils.TraeDato(queryStatus);
                        }


                        if (Utils.SerieEsBatch(oCompany, SBO_Application, Serie.Value.ToString().Trim()) == false)
                        {
                            if (status == null)
                            {
                                Utils.EnviaDocumento(oCompany, SBO_Application, tipoDoc, Serie.Selected.Value, DocNum, DocEntry[0].InnerText, Serie.Selected.Description, "GT", true);
                            }
                            else
                            {
                                if (status == "ANULAR")
                                {
                                    Utils.EnviaDocumentoA(oCompany, SBO_Application, tipoDoc, Serie.Selected.Value, DocNum, DocEntry[0].InnerText, Serie.Selected.Description, "GT", true);
                                }
                                else
                                {
                                    Utils.EnviaDocumento(oCompany, SBO_Application, tipoDoc, Serie.Selected.Value, DocNum, DocEntry[0].InnerText, Serie.Selected.Description, "GT", true);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void SBOApp_ItemEvent(string FormUID, ref ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {

                if (pVal.FormType == 133 || pVal.FormType == 141 || pVal.FormType == 179 || pVal.FormType == 65303 || pVal.FormType == 60091 || pVal.FormType == 60090 || pVal.FormType == 65300)
                {
                    if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_LOAD && pVal.BeforeAction == false)
                    {
                        SAPbouiCOM.Form oForm = SBO_Application.Forms.Item(FormUID);
                        SAPbouiCOM.Item oItem;
                        SAPbouiCOM.Button oButton;
                        oItem = oForm.Items.Add("PdfFEL", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        SAPbouiCOM.Item oItem1 = oForm.Items.Item("2");
                        oItem.Top = oItem1.Top;
                        oItem.Left = oItem1.Left + oItem1.Width + 6;
                        oItem.Width = 100;
                        oItem.Height = oItem1.Height;
                        oButton = oItem.Specific;
                        oButton.Caption = "Visualizar PDF FEL";

                        SAPbouiCOM.Item oItem2;
                        SAPbouiCOM.Button oButton2;
                        oItem2 = oForm.Items.Add("BtnNit", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        SAPbouiCOM.Item oItem3 = oForm.Items.Item("16");
                        oItem2.Top = oItem3.Top + (oItem3.Height - oItem.Height);
                        oItem2.Left = oItem3.Left + oItem3.Width + 6;
                        oItem2.Width = 91;
                        oItem2.Height = oItem.Height;
                        oButton2 = oItem2.Specific;
                        oButton2.Caption = "Consultar NIT";
                    }
                    if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_CLICK && pVal.BeforeAction == true && pVal.ItemUID == "PdfFEL")
                    {
                        try
                        {
                            SAPbouiCOM.Form oFormLink;
                            string LinkPDFINFILE = Utils.ObtieneValorParametro(oCompany, SBO_Application, "UR_p");
                            oFormLink = SBO_Application.Forms.Item(SBO_Application.Forms.ActiveForm.UDFFormUID);
                            SAPbouiCOM.EditText oEditLink = oFormLink.Items.Item("U_FACE_PDFFILE").Specific;
                            string url = oEditLink.Value;

                            if (string.IsNullOrEmpty(url))
                            {
                                SAPbouiCOM.EditText oEditFirma = oFormLink.Items.Item("U_FIRMA_ELETRONICA").Specific;
                                string firmax = oEditFirma.Value;

                                if (string.IsNullOrEmpty(firmax))
                                {
                                    SBO_Application.MessageBox("Este documento no tiene informacion de FEEL");
                                }
                                else
                                {
                                    if (Utils.FEL == Utils.TipoFEL.INFILE)
                                    {
                                        GoToSite(LinkPDFINFILE + firmax);
                                    }
                                    else
                                    {
                                        SBO_Application.MessageBox("Este documento no tiene informacion de FEEL");
                                    }
                                }
                            }
                            else
                            {
                                GoToSite(url);
                            }
                        }
                        catch (Exception ex)
                        {
                            SBO_Application.MessageBox(ex.Message);
                        }
                    }

                    if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_CLICK && pVal.BeforeAction == true && pVal.ItemUID == "BtnNit")
                    {
                        try
                        {
                            try
                            {
                                SAPbouiCOM.Form oFormLink;
                                oFormLink = SBO_Application.Forms.Item(SBO_Application.Forms.ActiveForm.UDFFormUID);
                                SAPbouiCOM.EditText uNit = oFormLink.Items.Item("U_NIT_SN_FEL").Specific;
                                SAPbouiCOM.EditText uNombre = oFormLink.Items.Item("U_NOMBRE_SN_FEL").Specific;
                                string nit = uNit.Value;
                                string name = Certificadores.Infile.GetReceptorInfo(oCompany, SBO_Application, nit);
                                uNombre.Value = name;
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    SAPbouiCOM.Form oFormLink;
                                    oFormLink = SBO_Application.Forms.Item(pVal.FormUID);
                                    SAPbouiCOM.EditText uNit = oFormLink.Items.Item("U_NIT_SN_FEL").Specific;
                                    SAPbouiCOM.EditText uNombre = oFormLink.Items.Item("U_NOMBRE_SN_FEL").Specific;
                                    string nit = uNit.Value;
                                    string name = Certificadores.Infile.GetReceptorInfo(oCompany, SBO_Application, nit);
                                    uNombre.Value = name;
                                }
                                catch (Exception)
                                {
                                    SBO_Application.MessageBox("No es posible ubicar los campos de NIT y NOMBRE");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SBO_Application.MessageBox(ex.Message);
                        }
                    }

                }
                if ((pVal.FormType == 133 || pVal.FormType == 179 || pVal.FormType == 65303 || pVal.FormType == 60091 || pVal.FormType == 141 || pVal.FormType == 60090 || pVal.FormType == 65300) && ((pVal.ItemUID == "1") && (pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) && (pVal.Before_Action == true)))
                {
                    oOrderForm = SBO_Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount);
                }

                if ((pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_CLOSE) && pVal.BeforeAction == false)
                {
                    if (FormUID.Equals("SBOParametrosFEL")) paramForm = null;
                    if (FormUID.Equals("SBOBatch")) batchForm = null;
                    if (FormUID.Equals("SBOBatchA")) batchFormA = null;
                    if (FormUID.Equals("SBOCerti")) certiForm = null;
                    if (FormUID.Equals("SBOSps")) spsForm = null;

                }
                GC.Collect();
            }
            catch { }
        }

        public static void GoToSite(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        private void SBOApp_MenuEvent(ref MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            if ((pVal.MenuUID == "paramFEL") & (pVal.BeforeAction == false))
            {
                if (paramForm == null)
                {
                    paramForm = new ParametrosForm();
                    BubbleEvent = false;
                }
            }

            if ((pVal.MenuUID == "procBatchFEL") & (pVal.BeforeAction == false))
            {
                if (batchForm == null)
                {
                    batchForm = new BatchForm();
                    BubbleEvent = false;
                }
            }

            if ((pVal.MenuUID == "procBatchFELA") & (pVal.BeforeAction == false))
            {
                if (batchFormA == null)
                {
                    batchFormA = new BatchFormA();
                    BubbleEvent = false;
                }
            }

            if ((pVal.MenuUID == "ConfigCert") & (pVal.BeforeAction == false))
            {
                if (certiForm == null)
                {
                    certiForm = new Certificador();
                    BubbleEvent = false;
                }
            }

            if ((pVal.MenuUID == "ConfigSPS") & (pVal.BeforeAction == false))
            {
                if (spsForm == null)
                {
                    spsForm = new Sps();
                    BubbleEvent = false;
                }
            }
        }

        public static Boolean ExistCerti()
        {
            SAPbobsCOM.Recordset RecSet;
            string sql;
            if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
            {
                sql = @" Select ""U_VALOR"" from ""@FEL_PARAMETROS"" where ""U_PARAMETRO"" = 'Certi'";
            }
            else
            {
                sql = @" Select ""U_VALOR"" from ""@FEL_PARAMETROS"" where ""U_PARAMETRO"" = 'Certi'";
            }
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            try
            {
                RecSet.DoQuery(sql);
                Utils.FEL = (Utils.TipoFEL)Convert.ToInt32(RecSet.Fields.Item(0).Value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                RecSet = null;
                GC.Collect();
            }
        }

        public static void AddConfigurationMenuItems()
        {

            try
            {
                SAPbouiCOM.Menus oMenus;
                SAPbouiCOM.MenuItem oMenuItem;
                oMenus = SBO_Application.Menus;
                SAPbouiCOM.MenuCreationParams oCreationPackage;
                oCreationPackage = SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams);
                oMenuItem = SBO_Application.Menus.Item("43520"); //moudles
                string sPath;
                sPath = System.Windows.Forms.Application.StartupPath;
                sPath = sPath.Remove(sPath.Length - 3, 3);

                if (SBO_Application.Menus.Exists("FEL"))
                {
                    SBO_Application.Menus.RemoveEx("FEL");
                }
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_POPUP;
                oCreationPackage.UniqueID = "FEL";
                oCreationPackage.String = "Configuraciones FEL";
                oCreationPackage.Enabled = true;
                oCreationPackage.Image = (System.Windows.Forms.Application.StartupPath + @"\SRFS\config.png").Replace(@"\\", @"\");
                oCreationPackage.Position = 1;

                oMenus = oMenuItem.SubMenus;

                try
                {
                    oMenus.AddEx(oCreationPackage);

                    oMenuItem = SBO_Application.Menus.Item("FEL");
                    oMenus = oMenuItem.SubMenus;

                    oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                    oCreationPackage.UniqueID = "ConfigCert";
                    oCreationPackage.String = "Configurar certificador";
                    oMenus.AddEx(oCreationPackage);

                    oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                    oCreationPackage.UniqueID = "ConfigSPS";
                    oCreationPackage.String = "Instalar Store procedures";
                    oMenus.AddEx(oCreationPackage);
                }
                catch (Exception)
                {
                    //NO EXISTE NADA EN ESTE CATCH, NO ES MALA IDEA AGREGAR ALGO
                }

            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
            }
        }


        private void AddMenuItems()
        {

            try
            {
                SAPbouiCOM.Menus oMenus;
                SAPbouiCOM.MenuItem oMenuItem;
                oMenus = SBO_Application.Menus;
                SAPbouiCOM.MenuCreationParams oCreationPackage;
                oCreationPackage = SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams);
                oMenuItem = SBO_Application.Menus.Item("43520"); //moudles
                string sPath;
                sPath = System.Windows.Forms.Application.StartupPath;
                sPath = sPath.Remove(sPath.Length - 3, 3);

                if (SBO_Application.Menus.Exists("FEL"))
                {
                    SBO_Application.Menus.RemoveEx("FEL");
                }
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_POPUP;
                oCreationPackage.UniqueID = "FEL";
                oCreationPackage.String = "Factura Electrónica en Línea";
                oCreationPackage.Enabled = true;
                oCreationPackage.Image = (System.Windows.Forms.Application.StartupPath + @"\SRFS\invoice.png").Replace(@"\\", @"\");
                oCreationPackage.Position = 1;

                oMenus = oMenuItem.SubMenus;

                try
                {
                    oMenus.AddEx(oCreationPackage);

                    oMenuItem = SBO_Application.Menus.Item("FEL");
                    oMenus = oMenuItem.SubMenus;


                    oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                    oCreationPackage.UniqueID = "paramFEL";
                    oCreationPackage.String = "Parámetros FEL";
                    oMenus.AddEx(oCreationPackage);


                    oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                    oCreationPackage.UniqueID = "procBatchFEL";
                    oCreationPackage.String = "Envio por Lote";
                    oMenus.AddEx(oCreationPackage);

                    oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                    oCreationPackage.UniqueID = "procBatchFELA";
                    oCreationPackage.String = "Anulacion por Lote";
                    oMenus.AddEx(oCreationPackage);

                }
                catch (Exception)
                {
                    //NO EXISTE NADA EN ESTE CATCH, NO ES MALA IDEA AGREGAR ALGO
                }

            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
            }
        }

        private void AddUserTables()
        {
            try
            {
                Utils.AddUserTable(oCompany, "FEL_PARAMETROS", "PARAMETROS FEL", BoUTBTableType.bott_MasterDataLines);
                Utils.AddUserTable(oCompany, "FEL_RESOLUCION", "SERIES FEL", BoUTBTableType.bott_MasterDataLines);

                Utils.AddUserTable(oCompany, "FEL_TIPODOC", "TIPOS DE DOC FACE", BoUTBTableType.bott_MasterDataLines);
                Utils.AddUserField(oCompany, "FEL_TIPODOC", "CODIGO", "CODIGO DOCUMENTO", BoFieldTypes.db_Alpha, 15);
                Utils.AddUserField(oCompany, "FEL_TIPODOC", "DESCRIPCION", "DESC TIPO DE DOC", BoFieldTypes.db_Alpha, 250);

                Utils.AddUserField(oCompany, "FEL_PARAMETROS", "PARAMETRO", "PARAMETRO FACE", BoFieldTypes.db_Alpha, 10);
                Utils.AddUserField(oCompany, "FEL_PARAMETROS", "VALOR", "VAL. PARAMETRO FACE", BoFieldTypes.db_Alpha, 254);

                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "SERIE", "SERIE FACTURA", BoFieldTypes.db_Numeric, 11);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "NOMBRECOMERCIAL", "NOMBRE COMERCIAL", BoFieldTypes.db_Alpha, 50);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "TIPO_DOC", "TIPO DE DOCUMENTO", BoFieldTypes.db_Alpha, 30);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "ES_BATCH", "PROESO EN LINEA O BATCH", BoFieldTypes.db_Alpha, 1);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "NIT", "NIT", BoFieldTypes.db_Alpha, 15);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "DISPOSITIVO", "DISPOSITIVO ELECTRONICO", BoFieldTypes.db_Alpha, 10);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "DIR", "DIRECCION", BoFieldTypes.db_Memo, 8000);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "MUNI", "MUNICIPIO", BoFieldTypes.db_Memo, 8000);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "DEPTO", "DEPARTAMENTO", BoFieldTypes.db_Memo, 8000);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "PAIS", "PAIS", BoFieldTypes.db_Memo, 8000);
                Utils.AddUserField(oCompany, "FEL_RESOLUCION", "CODP", "CODIGO POSTAL", BoFieldTypes.db_Memo, 8000);

                Utils.AddUserField(oCompany, "OINV", "ESTADO_FACE", "ESTADO FEL", BoFieldTypes.db_Alpha, 10, false);
                Utils.AddUserField(oCompany, "OINV", "MOTIVO_RECHAZO", "RECHAZO FEL", BoFieldTypes.db_Memo, 254, false);
                Utils.AddUserField(oCompany, "OINV", "FACE_PDFFILE", "PDF FEL", BoFieldTypes.db_Memo, 254, false, BoFldSubTypes.st_Link);
                Utils.AddUserField(oCompany, "OINV", "FIRMA_ELETRONICA", "FIRMA ELECTRONICA FEL", BoFieldTypes.db_Memo, 254, false);
                Utils.AddUserField(oCompany, "OINV", "NUMERO_DOCUMENTO", "NUMERO DOC FEL", BoFieldTypes.db_Alpha, 150, false);
                Utils.AddUserField(oCompany, "OINV", "SERIE_FACE", "NUMERO DE SERIE FEL", BoFieldTypes.db_Alpha, 20, false);
                Utils.AddUserField(oCompany, "OINV", "FECHA_ENVIO_FACE", "FECHA ENVIO FEL", BoFieldTypes.db_Alpha, 30, false);
                Utils.AddUserField(oCompany, "OINV", "FECHA_CERT_FACE", "FECHA CERTIFICACION", BoFieldTypes.db_Alpha, 30, false);
                Utils.AddUserField(oCompany, "OINV", "NUMERO_DOCUMENTO_NC", "FIRMA NC/ND", BoFieldTypes.db_Alpha, 150, false);
                Utils.AddUserField(oCompany, "OINV", "FECHA_NC", "FECHA NC/ND", BoFieldTypes.db_Alpha, 30, false);
                Utils.AddUserField(oCompany, "OINV", "MOTIVO_NC", "MOTIVO NC/ND", BoFieldTypes.db_Alpha, 30, false);
                Utils.AddUserField(oCompany, "OINV", "FRASE_EXENTO", "FRASE NO AFECTO A IVA", SAPbobsCOM.BoFieldTypes.db_Alpha, 254, false);
                Utils.AddUserField(oCompany, "OINV", "INCOTERM", "INCOTERM", SAPbobsCOM.BoFieldTypes.db_Alpha, 100, false);
                Utils.AddUserField(oCompany, "OINV", "TIPO_DOCUMENTO_FEL", "TIPO DOCUMENTO FEL", SAPbobsCOM.BoFieldTypes.db_Alpha, 100, false);

                if (Utils.FEL == Utils.TipoFEL.INFILE)
                {
                    Utils.AddUserField(oCompany, "OINV", "NOMBRE_SN_FEL", "NOMBRE - SOLICITA NIT", BoFieldTypes.db_Alpha, 254, false);
                    Utils.AddUserField(oCompany, "OINV", "NIT_SN_FEL", "NIT - SOLICITA NIT", BoFieldTypes.db_Alpha, 150, false);
                    Utils.AddUserField(oCompany, "OINV", "DIRECCION_SN_FEL", "DIRECCION - SOLICITA NIT", BoFieldTypes.db_Alpha, 254, false);
                }

                if (Utils.FEL == Utils.TipoFEL.GUATEFACTURAS)
                {
                    Utils.AddUserField(oCompany, "OINV", "DOCUMENTO_NC", "DOCUMENTO NC/ND", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, false);
                    Utils.AddUserField(oCompany, "OINV", "SERIE_NC", "SERIE NC/ND", SAPbobsCOM.BoFieldTypes.db_Alpha, 30, false);
                    Utils.AddUserField(oCompany, "OINV", "NOMBRE_FEL", "NOMBRE FEL", SAPbobsCOM.BoFieldTypes.db_Alpha, 254, false);
                    Utils.AddUserField(oCompany, "OINV", "DIRECCION_FEL", "DIRECCION FEL", SAPbobsCOM.BoFieldTypes.db_Alpha, 254, false);
                    Utils.AddUserField(oCompany, "OINV", "TELEFONO_FEL", "TELEFONO FEL", SAPbobsCOM.BoFieldTypes.db_Alpha, 254, false);
                }

                


                SAPbobsCOM.Recordset RecSetDocumentos;
                string sqlDocumentos = "";
                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    sqlDocumentos = "CALL FELONE_UTILS ('TipoDocumentos','','','','','','','','','','') ";
                }
                else
                {
                    sqlDocumentos = "EXEC [dbo].[FELONE_UTILS] 'TipoDocumentos','','','','','','','','','','' ";
                }

                RecSetDocumentos = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSetDocumentos.DoQuery(sqlDocumentos);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSetDocumentos);
                RecSetDocumentos = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
                System.Windows.Forms.Application.Exit();
            }
        }

        private void SBOAppn_AppEvent(BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    SBO_Application.SetStatusBarMessage("Finalizando add-on facturación electrónica One Solutions...", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    System.Environment.Exit(0);
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    SBO_Application.SetStatusBarMessage("Finalizando add-on facturación electrónica One Solutions...", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    System.Environment.Exit(0);
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                    SBO_Application.SetStatusBarMessage("Finalizando add-on facturación electrónica One Solutions...", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    System.Environment.Exit(0);
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                    SBO_Application.SetStatusBarMessage("Finalizando add-on facturación electrónica One Solutions...", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    System.Environment.Exit(0);
                    break;
            }
        }

        #endregion

    }
}
