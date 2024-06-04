using FEL_ONE.Certificadores;
using FEL_ONE.Clases;
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEL_ONE.Forms
{
    class ParametrosForm
    {

        private string XmlForm;
        private static SAPbouiCOM.Application SBO_Application;
        private static SAPbobsCOM.Company oCompany;
        private static SAPbouiCOM.Form oForm;

        public ParametrosForm()
        {

            SAPbouiCOM.Folder oTab;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Item oItemN;
            SAPbouiCOM.Item oItemPass;

            try
            {
                switch (Utils.FEL)
                {
                    case Utils.TipoFEL.INFILE:
                        XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\ParametrosINFILE.srf").Replace(@"\\", @"\");
                        break;
                    case Utils.TipoFEL.MEGAPRINT:
                        XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\ParametrosMEGA.srf").Replace(@"\\", @"\");
                        break;
                    case Utils.TipoFEL.G4S:
                        XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\ParametrosG4S.srf").Replace(@"\\", @"\");
                        break;
                    case Utils.TipoFEL.TEKRA:
                        XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\ParametrosTEKRA.srf").Replace(@"\\", @"\");
                        break;
                    case Utils.TipoFEL.ECOFACTURAS:
                        XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\ParametrosECO.srf").Replace(@"\\", @"\");
                        break;
                    case Utils.TipoFEL.GUATEFACTURAS:
                        XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\ParametrosGUATE.srf").Replace(@"\\", @"\");
                        break;
                    case Utils.TipoFEL.DIGIFACT:
                        XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\ParametrosDIGI.srf").Replace(@"\\", @"\");
                        break;
                    default:
                        XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\Parametros.srf").Replace(@"\\", @"\");
                        break;
                }
                SBO_Application = Utils.SBOApplication;
                oCompany = Utils.Company;

                if (Utils.ActivateFormIsOpen(SBO_Application, "SBOParametrosFEL") == false)
                {
                    LoadFromXML(XmlForm);
                    oForm = SBO_Application.Forms.Item("SBOParametrosFEL");
                    oForm.Visible = true;
                    oForm.PaneLevel = 1;

                    LlenaGrid();
                    oForm.Freeze(true);
                    switch (Utils.FEL)
                    {
                        case Utils.TipoFEL.INFILE:
                            Infile.LLenaParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.MEGAPRINT:
                            Megaprint.LLenaParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.G4S:
                            G4s.LLenaParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.ECOFACTURAS:
                            EcoFacturas.LLenaParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.TEKRA:
                            Tekra.LLenaParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.DIGIFACT:
                            Digifact.LLenaParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.GUATEFACTURAS:
                            Guatefacturas.LLenaParametros(oCompany, oForm, SBO_Application);
                            break;
                        default:
                            LLenaParametros();
                            break;
                    }
                    oForm.Freeze(false);
                    oItem = oForm.Items.Item("tabSeries");
                    oTab = oItem.Specific;
                    oTab.Select();

                    SAPbouiCOM.StaticText oEditCert;
                    SAPbouiCOM.EditText oEditNam;
                    SAPbouiCOM.EditText oEditPass = null;
                    oItem = oForm.Items.Item("Certifi");
                    oItemN = oForm.Items.Item("NCom");

                    switch (Utils.FEL)
                    {
                        case Utils.TipoFEL.G4S:
                            break;
                        case Utils.TipoFEL.TEKRA:
                            break;
                        case Utils.TipoFEL.ECOFACTURAS:
                            break;
                        case Utils.TipoFEL.GUATEFACTURAS:
                            break;
                        default:
                            oItemPass = oForm.Items.Item("certpass");
                            oEditPass = oItemPass.Specific;
                            break;
                    }

                    oEditCert = oItem.Specific;
                    oEditNam = oItemN.Specific;
                    string Sql;

                    if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        Sql = "CALL FELONE_UTILS ('Empresa','','','','','','','','','','') ";
                        oForm.Title = "One Solutions - Parametrizacion FEL ! HANA";
                    }
                    else
                    {
                        Sql = "EXEC [dbo].[FELONE_UTILS] 'Empresa','','','','','','','','','','' ";
                        oForm.Title = "One Solutions - Parametrizacion FEL ! SQL";
                    }
                    SAPbobsCOM.Recordset RecSet;
                    RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    RecSet.DoQuery(Sql);

                    if (RecSet.RecordCount > 0)
                    {
                        oEditNam.Value = RecSet.Fields.Item("Empresa").Value;
                    }

                    oEditCert.Caption = "Certificador: " + Utils.getCertificador();
                    if (Utils.FEL == Utils.TipoFEL.MEGAPRINT)
                    {
                        oEditPass.IsPassword = true;
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                    RecSet = null;
                    GC.Collect();
                }
                else
                {
                    oForm = SBO_Application.Forms.Item("SBOParametrosFEL");
                }
                setEvents();
            }
            catch (Exception ex)
            {
                oForm.Freeze(false);
                SBO_Application.MessageBox(ex.Message);
            }

        }

        private void setEvents()
        {
            SBO_Application
                 .ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent_PARAM);
        }

        private void removeEvents()
        {
            SBO_Application
                 .ItemEvent -= new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent_PARAM);
        }

        private void LoadFromXML(string FileName)
        {
            System.Xml.XmlDocument oXmlDoc;
            oXmlDoc = new System.Xml.XmlDocument();
            oXmlDoc.Load(FileName);
            SBO_Application.LoadBatchActions(oXmlDoc.InnerXml);
        }

        private void SBO_Application_ItemEvent_PARAM(string FormUID, ref ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if ((pVal.FormType == 60006) && (pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) && (pVal.Before_Action == false))
                {
                    switch (pVal.ItemUID)
                    {
                        case "tabSeries":
                            oForm.PaneLevel = 2;
                            break;
                        case "tabCNN":
                            oForm.PaneLevel = 3;
                            break;
                    }
                }

                if (pVal.ItemUID == "cmdOk" && pVal.FormType == 60006 && pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED && pVal.Before_Action == true)
                {
                    switch (Utils.FEL)
                    {
                        case Utils.TipoFEL.INFILE:
                            Infile.GuardarParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.MEGAPRINT:
                            Megaprint.GuardarParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.DIGIFACT:
                            Digifact.GuardarParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.G4S:
                            G4s.GuardarParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.TEKRA:
                            Tekra.GuardarParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.ECOFACTURAS:
                            EcoFacturas.GuardarParametros(oCompany, oForm, SBO_Application);
                            break;
                        case Utils.TipoFEL.GUATEFACTURAS:
                            Guatefacturas.GuardarParametros(oCompany, oForm, SBO_Application);
                            break;
                        default:
                            GuardarParametros();
                            break;
                    }
                    BubbleEvent = false;
                }

                if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_CLOSE && pVal.BeforeAction == true && pVal.FormType == 60006)
                {
                    oForm = null;
                    removeEvents();
                }
            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
            }
        }

        private void LlenaGrid()
        {
            string QryStr;
            SAPbouiCOM.Grid oGrid;
            SAPbouiCOM.Item oItem;

            try
            {
                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    QryStr = "CALL FELONE_LLENAGRID";
                }
                else
                {
                    QryStr = "EXEC [dbo].[FELONE_LLENAGRID]";
                }

                oForm.DataSources.DataTables.Add("MyDataTable");

                oForm.DataSources.DataTables.Item(0).ExecuteQuery(QryStr);

                oItem = oForm.Items.Item("grdDatos");
                oGrid = oItem.Specific;
                oGrid.DataTable = oForm.DataSources.DataTables.Item("MyDataTable");
                oGrid.Columns.Item(0).Editable = false;  // series
                oGrid.Columns.Item(1).Editable = false;  // seriesname
                oGrid.Columns.Item(2).Editable = false;  // tiposerie
                oGrid.Columns.Item(2).Width = 100; // tiposerie
                oGrid.Columns.Item(3).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox; // esdocelectronico
                oGrid.Columns.Item(4).Width = 100; // tipodocumento
                oGrid.Columns.Item(5).Width = 100; // esbatch
                oGrid.Columns.Item(6).Width = 100; // dispositivo
                oGrid.Columns.Item(7).Width = 100; // direccion
                oGrid.Columns.Item(8).Width = 100; // municipio
                oGrid.Columns.Item(9).Width = 100;  // departamento
                oGrid.Columns.Item(10).Width = 100; // pais
                oGrid.Columns.Item(11).Width = 100; // codpostal
                oGrid.Columns.Item(13).Width = 100; // IMPRIME SERIE DIRECTAMENTE
                oGrid.Columns.Item(14).Width = 100; // IMPRIME SERIE DIRECTAMENTE
                oGrid.Columns.Item(13).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox;
                oGrid.Columns.Item(4).Type = SAPbouiCOM.BoGridColumnType.gct_ComboBox;
                oGrid.Columns.Item(5).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox;

                SAPbouiCOM.ComboBoxColumn oGridColumn = (SAPbouiCOM.ComboBoxColumn)oGrid.Columns.Item(4);

                SAPbobsCOM.Recordset RecSet;
                string Sql;
                if ((oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
                    Sql = @"select ""U_CODIGO"",""U_DESCRIPCION"" from ""@FEL_TIPODOC"";";
                else
                    Sql = @"select u_codigo,u_descripcion from [@FEL_TIPODOC]";
                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(Sql);
                oGridColumn.DisplayType = SAPbouiCOM.BoComboDisplayType.cdt_Description;
                for (var index = 0; index <= RecSet.RecordCount - 1; index++)
                {
                    oGridColumn.ValidValues.Add(RecSet.Fields.Item("U_CODIGO").Value, RecSet.Fields.Item("U_DESCRIPCION").Value);
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

        private void LLenaParametros()
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

        public static void GuardaParametro(SAPbobsCOM.UserTable oUsrTbl, string IDParametro, string ValorParmetro)
        {
            int Res;
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            try
            {
                oUsrTbl.Code = IDParametro;
                oUsrTbl.Name = "PARAM";
                oUsrTbl.UserFields.Fields.Item("U_PARAMETRO").Value = IDParametro;
                oUsrTbl.UserFields.Fields.Item("U_VALOR").Value = ValorParmetro;
                if (ExisteParametro(IDParametro) == false)
                {
                    Res = oUsrTbl.Add();
                    if (Res != 0)
                    {
                        throw new Exception("Hubo un error al intentar guardar los parametros");
                    }
                }
                else
                {
                    RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        QryStr = "update \"@FEL_PARAMETROS\" set \"U_VALOR\"= '" + ValorParmetro + "' where \"U_PARAMETRO\"= '" + IDParametro + "';";
                    }
                    else
                    {
                        QryStr = "update [@FEL_PARAMETROS] set U_VALOR='" + ValorParmetro + "' where U_PARAMETRO='" + IDParametro + "'";
                    }

                    RecSet.DoQuery(QryStr);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                    RecSet = null;
                    GC.Collect();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static bool ExisteParametro(string IDParametro)
        {
            bool result = false;
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            try
            {
                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    QryStr = "select * from \"@FEL_PARAMETROS\" where \"U_PARAMETRO\" ='" + IDParametro + "'";
                }
                else
                {
                    QryStr = "select * from [@FEL_PARAMETROS] where U_PARAMETRO='" + IDParametro + "'";
                }

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
                SBO_Application.MessageBox(ex.Message);
                return false;
            }
        }

        public static void GuardaDatosSeries()
        {
            string esFACe;
            int serie;
            string NombreComercial;
            string Pais;
            SAPbouiCOM.Grid oGrid;
            SAPbouiCOM.Item oItem;
            string Sql;
            SAPbobsCOM.Recordset RecSet;
            string QryStr;
            string TipoDoc;
            string EsBatch;
            string CodP;
            string Dispositivo;
            string Direccion;
            string Municipio;
            string Departamento;
            string impresora;
            string imprimePdf;
            try
            {
                oItem = oForm.Items.Item("grdDatos");
                oGrid = oItem.Specific;
                for (var i = 0; i <= oGrid.Rows.Count - 1; i++)
                {
                    if (oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i) == "Y")
                    {
                        serie = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(0).Name, i);
                        esFACe = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i);
                        TipoDoc = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(4).Name, i);
                        EsBatch = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i);
                        Dispositivo = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(6).Name, i);
                        Direccion = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(7).Name, i);
                        Municipio = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(8).Name, i);
                        Departamento = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(9).Name, i);
                        Pais = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(10).Name, i);
                        CodP = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(11).Name, i);
                        NombreComercial = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(12).Name, i);
                        imprimePdf = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(13).Name, i);
                        impresora = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(14).Name, i);

                        if (esFACe == "Y")
                        {
                            if (TipoDoc == "")
                            {
                                throw new Exception("El tipo de documento de la serie debe ser definido");
                            }
                            if (Dispositivo == "")
                            {
                                throw new Exception("El codigo de dispositivo debe ser definido");
                            }
                        }
                    }
                }

                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    QryStr = "DELETE FROM \"@FEL_RESOLUCION\";";
                }
                else
                {
                    QryStr = "delete  [@FEL_RESOLUCION]";
                }
                RecSet.DoQuery(QryStr);

                for (var i = 0; i <= oGrid.Rows.Count - 1; i++)
                {
                    if (oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i) == "Y")
                    {
                        serie = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(0).Name, i);
                        esFACe = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(3).Name, i);
                        TipoDoc = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(4).Name, i);
                        EsBatch = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i);
                        Dispositivo = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(6).Name, i);
                        Direccion = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(7).Name, i);
                        Municipio = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(8).Name, i);
                        Departamento = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(9).Name, i);
                        Pais = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(10).Name, i);
                        CodP = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(11).Name, i);
                        NombreComercial = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(12).Name, i);
                        //imprimePdf = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(13).Name, i);
                        impresora = oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(14).Name, i);

                        if (oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i) == "0" | oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i) == "N")
                        {
                            EsBatch = "0";
                        }
                        else
                        {
                            EsBatch = "'" + oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i) + "'";
                        }

                        if (oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(13).Name, i) == "0" | oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(13).Name, i) == "N")
                        {
                            imprimePdf = "0";
                        }
                        else
                        {
                            imprimePdf = "1";
                        }

                        if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            Sql = "insert into \"@FEL_RESOLUCION\" (\"Code\",\"LineId\",\"Object\",\"LogInst\",\"U_SERIE\",\"U_TIPO_DOC\",\"U_ES_BATCH\",\"U_DISPOSITIVO\",\"U_DIR\",\"U_MUNI\",\"U_DEPTO\",\"U_PAIS\",\"U_CODP\",\"U_NOMBRECOMERCIAL\",\"U_IMPRIME_PDF\",\"U_IMPRESORA\") " + "values ('" + serie + "'," + serie + ",null,null,'" + serie + "','" + TipoDoc + "'," + EsBatch + ",'" + Dispositivo + "','" + Direccion + "','" + Municipio + "','" + Departamento + "','" + Pais + "','" + CodP + "','" + NombreComercial + "'," + imprimePdf + ",'" + impresora + "')";
                        }
                        else
                        {
                            Sql = "insert into [@FEL_RESOLUCION] (Code,LineId,Object,LogInst,U_SERIE,U_TIPO_DOC,U_ES_BATCH,U_DISPOSITIVO,U_DIR,U_MUNI,U_DEPTO,U_PAIS,U_CODP,U_NOMBRECOMERCIAL,U_IMPRIME_PDF,U_IMPRESORA) " + "values ('" + serie + "'," + serie + ",null,null,'" + serie + "','" + TipoDoc + "'," + EsBatch + ",'" + Dispositivo + "','" + Direccion + "','" + Municipio + "','" + Departamento + "','" + Pais + "','" + CodP + "','" + NombreComercial + "'," + imprimePdf + ",'" + impresora + "')";
                        }
                        RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        RecSet.DoQuery(Sql);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void GuardarParametros()
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
                GuardaParametro(oUsrTbl, "Nemi", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "NitEmi", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "Tafilia", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "Correo", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "UR_t", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "UR_r", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "UR_p", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "UR_a", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "PATHXML", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "PATHXMLaut", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "PATHXMLres", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "PATHXMLerr", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "PATHPDF", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "ApiKey", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "PATHXMLc", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "PATHXMLcp", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "USRDB", oEdit.Value);
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
                GuardaParametro(oUsrTbl, "PASSDB", oEdit.Value);
                ProgressBar.Value += 1;

                GuardaDatosSeries();
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

        ~ParametrosForm() { }
    }
}
