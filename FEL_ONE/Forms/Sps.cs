using FEL_ONE.Clases;
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FEL_ONE.Forms
{
    class Sps
    {
        private string XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\Sps.srf").Replace(@"\\", @"\");
        private string StoresProcedures = (System.Windows.Forms.Application.StartupPath + @"\SPS").Replace(@"\\", @"\");
        SAPbouiCOM.Application SBO_Application;
        private SAPbouiCOM.Form oForm;
        private SAPbobsCOM.Company oCompany;


        public Sps()
        {
            try
            {
                SBO_Application = Utils.SBOApplication;
                oCompany = Utils.Company;
                if (Utils.ActivateFormIsOpen(SBO_Application, "SBOSps") == false)
                {
                    LoadFromXML(XmlForm);
                    oForm = SBO_Application.Forms.Item("SBOSps");
                    oForm.Visible = true;
                    oForm.PaneLevel = 1;

                    SAPbouiCOM.Grid oGrid;
                    oGrid = oForm.Items.Item("grdSps").Specific;
                    oForm.DataSources.DataTables.Add("MyDataTableSps");
                    oGrid.DataTable = oForm.DataSources.DataTables.Item("MyDataTableSps");
                    oForm.Freeze(true);
                    llenaGridSpsInstall();
                    oForm.Freeze(false);


                    //LINEAS DESARROLLO UNICAMENTE DEBUG
                    //InstalaTodosLosSps();
                    //LINEAS DESARROLLO UNICAMENTE DEBUG

                    setEvents();
                }
            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
            }
        }

        private void setEvents()
        {
            SBO_Application
                 .ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent_Sps);
        }

        private void removeEvents()
        {
            SBO_Application
                 .ItemEvent -= new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent_Sps);
        }

        private void llena_grid()
        {
            try
            {
                string sql = "";
                SAPbobsCOM.Recordset RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                string Certi = getCertificador();
                SAPbouiCOM.Grid oGrid;
                SAPbouiCOM.Item oitem;
                string queryListSps = "";

                ////SQL
                //string queryUTILS = @"SELECT * FROM sysobjects WHERE ""name"" = 'FELONE_UTILS'";
                //string queryLLENAGRID = @"SELECT * FROM sysobjects WHERE ""name"" = 'FELONE_LLENAGRID'";
                //string queryDOCS = @"SELECT * FROM sysobjects WHERE ""name"" LIKE 'FELONE_"+Certi+"%'";
                //RecSet.DoQuery(queryUTILS);

                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    queryListSps = @"SELECT * FROM PROCEDURES WHERE ""PROCEDURE_NAME"" LIKE 'FELONE%' AND ""SCHEMA_NAME"" = '" + oCompany.CompanyDB + "' ";
                }
                else
                {
                    //queryListSps = "EXEC [dbo].[FELONE_UTILS] 'TipoDocumentos','','','','','','','','','','' ";
                }

                oitem = oForm.Items.Item("grdSps");
                oGrid = oitem.Specific;
                oForm.DataSources.DataTables.Add("MyDataTableSps");
                oForm.DataSources.DataTables.Item(0).ExecuteQuery(queryListSps);
                oGrid.DataTable = oForm.DataSources.DataTables.Item("MyDataTableSps");
                oGrid.AutoResizeColumns();


                //if (RecSet.RecordCount > 0)
                //{

                //}


            }
            catch (Exception) { }
        }

        private string getCertificador()
        {
            switch (Utils.FEL)
            {
                case Utils.TipoFEL.MEGAPRINT:
                    return "MEGA";
                case Utils.TipoFEL.INFILE:
                    return "INFILE";
                case Utils.TipoFEL.G4S:
                    return "G4S";
                case Utils.TipoFEL.DIGIFACT:
                    return "DIGIFACT";
                case Utils.TipoFEL.ECOFACTURAS:
                    return "ECOFACTURAS";
                case Utils.TipoFEL.GUATEFACTURAS:
                    return "GUATE";
                case Utils.TipoFEL.TEKRA:
                    return "TEKRA";
                default:
                    return "NA";
            }
        }

        private void LoadFromXML(string FileName)
        {
            System.Xml.XmlDocument oXmlDoc;
            oXmlDoc = new System.Xml.XmlDocument();
            oXmlDoc.Load(FileName);
            SBO_Application.LoadBatchActions(oXmlDoc.InnerXml);
        }

        private void SBO_Application_ItemEvent_Sps(string FormUID, ref ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if (pVal.ItemUID == "consult" && pVal.FormType == 60004 && pVal.EventType == SAPbouiCOM.BoEventTypes.et_CLICK && pVal.Before_Action == false)
                {

                    try
                    {
                        oForm.Freeze(true);
                        llenaGridSpsInstall();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        oForm.Freeze(false);
                    }
                    BubbleEvent = false;
                }
                if (pVal.ItemUID == "utils" && pVal.FormType == 60004 && pVal.EventType == SAPbouiCOM.BoEventTypes.et_CLICK && pVal.Before_Action == false)
                {

                    try
                    {
                        oForm.Freeze(true);
                        installUtils();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        oForm.Freeze(false);
                    }
                    BubbleEvent = false;
                }
                if (pVal.ItemUID == "install" && pVal.FormType == 60004 && pVal.EventType == SAPbouiCOM.BoEventTypes.et_CLICK && pVal.Before_Action == false)
                {

                    try
                    {
                        oForm.Freeze(true);
                        InstallSps();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        oForm.Freeze(false);
                    }
                    BubbleEvent = false;
                }
                if (pVal.ItemUID == "cancel" & pVal.FormType == 60004 & pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED & pVal.Before_Action == false)
                {
                    oForm.Close();
                    oForm = null;
                    removeEvents();
                }
                if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_CLOSE && pVal.BeforeAction == true && pVal.FormType == 60004)
                {
                    oForm = null;
                    removeEvents();
                }

            }
            catch (Exception ex)
            {
                SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
                //SBO_Application.MessageBox(ex.Message);
            }
        }

        private void InstallSps()
        {
            string certi = getCertificador();
            string dbtype = "";
            if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
            {
                dbtype = "HANA";
            }
            else
            {
                dbtype = "SQL";
            }
            SAPbouiCOM.Grid oGrid;
            oGrid = oForm.Items.Item("grdSps").Specific;
            SAPbobsCOM.Recordset RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            string mensaje = "";
            bool error = false;

            for (int i = 0; i < oGrid.Rows.Count; i++)
            {

                string instalar = oGrid.DataTable.Columns.Item("INSTALAR").Cells.Item(i).Value;
                string documento = oGrid.DataTable.Columns.Item("DOCUMENTO").Cells.Item(i).Value;
                string estado = oGrid.DataTable.Columns.Item("ESTADO").Cells.Item(i).Value;
                if (instalar == "N") { continue; }
                if (estado == "INSTALADO")
                {
                    // MESSAGE ERROR    
                    error = true;
                    mensaje += " ERROR No. 1: FELONE_" + certi + "_" + documento + " YA SE ENCUENTRA INSTALADO EN ESTA BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
                    continue;
                }

                DirectoryInfo d = new DirectoryInfo(StoresProcedures + @"\" + dbtype + @"\" + certi + @"\");
                FileInfo file = d.GetFiles("FELONE_" + certi + "_" + documento + ".txt")[0]; //Getting Text files
                string texto = File.ReadAllText(file.FullName);
                try
                {
                    RecSet.DoQuery(texto);
                    mensaje += " MESSAGE No. 1: FELONE_UTILS INSTALADO CON EXITO EN LA BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
                }
                catch (Exception ex)
                {
                    mensaje += " ERROR No. 1: FELONE_" + certi + "_" + documento + ":  " + ex.Message + "; -- BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
                }

            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
            RecSet = null;
            GC.Collect();

            SBO_Application.MessageBox(mensaje);

            if (error)
            {
                throw new Exception(mensaje);
            }

        }

        private void installUtils()
        {
            SAPbobsCOM.Recordset RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            string sqlUtilsExist = @"SELECT count(1) FROM PROCEDURES WHERE ""PROCEDURE_NAME"" LIKE 'FELONE_UTILS' AND ""SCHEMA_NAME"" = '" + oCompany.CompanyDB + "'";
            string sqlLlenaExist = @"SELECT count(1) FROM PROCEDURES WHERE ""PROCEDURE_NAME"" LIKE 'FELONE_LLENAGRID' AND ""SCHEMA_NAME"" = '" + oCompany.CompanyDB + "'";
            RecSet.DoQuery(sqlUtilsExist);
            bool error = false;
            string erroresDescription = "";
            string dbtype;
            string certi = getCertificador();
            if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
            {
                dbtype = "HANA";
            }
            else
            {
                dbtype = "SQL";
            }

            if (RecSet.Fields.Item(0).Value == 1)
            {
                error = true;
                erroresDescription += " ERROR No. 1: FELONE_UTILS YA SE ENCUENTRA INSTALADO EN ESTA BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
            }
            else
            {
                DirectoryInfo d = new DirectoryInfo(StoresProcedures + @"\" + dbtype + @"\UTILS\");
                FileInfo[] Files = d.GetFiles("FELONE_UTILS.txt");
                string texto = File.ReadAllText(Files[0].FullName);
                RecSet.DoQuery(texto);
                erroresDescription += " MESSAGE No. 1: FELONE_UTILS INSTALADO CON EXITO EN LA BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
            }



            RecSet.DoQuery(sqlLlenaExist);
            if (RecSet.Fields.Item(0).Value == 1)
            {
                error = true;
                erroresDescription += " ERROR No. 1: FELONE_LLENAGRID YA SE ENCUENTRA INSTALADO EN ESTA BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
            }
            else
            {
                DirectoryInfo d = new DirectoryInfo(StoresProcedures + @"\" + dbtype + @"\UTILS\");
                FileInfo[] Files = d.GetFiles("FELONE_LLENAGRID.txt");
                string texto = File.ReadAllText(Files[0].FullName);
                RecSet.DoQuery(texto);
                erroresDescription += " MESSAGE No. 1: FELONE_LLENAGRID INSTALADO CON EXITO EN LA BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
            RecSet = null;
            GC.Collect();

            SBO_Application.MessageBox(erroresDescription);

            if (error)
            {
                throw new Exception(erroresDescription);
            }

        }

        private void llenaGridSpsInstall()
        {
            if (!SystemForm.ExistCerti()) { throw new Exception("Debe parametrizar un certificador antes"); }
            List<string> SpsListDB = new List<string>();
            SAPbobsCOM.Recordset RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            string queryListSps = @"SELECT ""PROCEDURE_NAME"" FROM PROCEDURES WHERE ""PROCEDURE_NAME"" LIKE 'FELONE%' AND ""SCHEMA_NAME"" = '" + oCompany.CompanyDB + "' ";
            RecSet.DoQuery(queryListSps);

            for (int i = 0; i < RecSet.RecordCount; i++)
            {
                SpsListDB.Add(RecSet.Fields.Item(0).Value);
                RecSet.MoveNext();
            }

            SAPbouiCOM.Grid oGrid;
            oGrid = oForm.Items.Item("grdSps").Specific;
            oGrid.DataTable = null;
            oForm.DataSources.DataTables.Item(0).Clear();
            oForm.DataSources.DataTables.Item(0).Columns.Add("INSTALAR", BoFieldsType.ft_AlphaNumeric, 32);
            oForm.DataSources.DataTables.Item(0).Columns.Add("CERTIFICADOR", BoFieldsType.ft_AlphaNumeric, 254);
            oForm.DataSources.DataTables.Item(0).Columns.Add("DOCUMENTO", BoFieldsType.ft_AlphaNumeric, 254);
            oForm.DataSources.DataTables.Item(0).Columns.Add("ESTADO", BoFieldsType.ft_AlphaNumeric, 254);
            oGrid.DataTable = oForm.DataSources.DataTables.Item("MyDataTableSps");
            oGrid.Columns.Item(0).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox;
            oGrid.Columns.Item(0).Editable = true;
            oGrid.Columns.Item(1).Editable = false;
            oGrid.Columns.Item(2).Editable = false;
            oGrid.Columns.Item(3).Editable = false;
            oGrid.AutoResizeColumns();

            string dbtype;
            string certi = getCertificador();
            if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
            {
                dbtype = "HANA";
            }
            else
            {
                dbtype = "SQL";
            }
            DirectoryInfo d = new DirectoryInfo(StoresProcedures + @"\" + dbtype + @"\" + certi + @"\");
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            string fileName;
            foreach (FileInfo File in Files)
            {
                fileName = File.Name.Split('.')[0];
                if (fileName == "FELONE_UTILS" || fileName == "FELONE_LLENAGRID") { continue; }

                string estado = "NO INSTALADO";
                string valor = "Y";
                if (SpsListDB.Contains(fileName))
                {
                    valor = "N";
                    estado = "INSTALADO";
                }
                oForm.DataSources.DataTables.Item(0).Rows.Add();
                oForm.DataSources.DataTables.Item(0).Columns.Item("INSTALAR")
                    .Cells.Item(oForm.DataSources.DataTables.Item(0).Columns.Item("INSTALAR").Cells.Count - 1).Value = valor;
                oForm.DataSources.DataTables.Item(0).Columns.Item("CERTIFICADOR")
                    .Cells.Item(oForm.DataSources.DataTables.Item(0).Columns.Item("CERTIFICADOR").Cells.Count - 1).Value = certi;
                oForm.DataSources.DataTables.Item(0).Columns.Item("DOCUMENTO")
                    .Cells.Item(oForm.DataSources.DataTables.Item(0).Columns.Item("DOCUMENTO").Cells.Count - 1).Value = fileName.Split('_')[2];
                oForm.DataSources.DataTables.Item(0).Columns.Item("ESTADO")
                    .Cells.Item(oForm.DataSources.DataTables.Item(0).Columns.Item("ESTADO").Cells.Count - 1).Value = estado;
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
            RecSet = null;
            GC.Collect();
        }


        private void InstalaTodosLosSps()
        {
            string[] lista = { "MEGA", "INFILE","DIGIFACT","ECOFACTURAS","GUATE", "G4S","TEKRA"};

            foreach (string certi in lista)
            {



                string dbtype = "";
                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    dbtype = "HANA";
                }
                else
                {
                    dbtype = "SQL";
                }
                SAPbouiCOM.Grid oGrid;
                oGrid = oForm.Items.Item("grdSps").Specific;
                SAPbobsCOM.Recordset RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                string mensaje = "";
                bool error = false;

                for (int i = 0; i < oGrid.Rows.Count; i++)
                {

                    string instalar = "Y";
                    string documento = oGrid.DataTable.Columns.Item("DOCUMENTO").Cells.Item(i).Value;
                    string estado = "NO INSTALADO";
                    if (instalar == "N") { continue; }
                    if (estado == "INSTALADO")
                    {
                        // MESSAGE ERROR    
                        error = true;
                        mensaje += " ERROR No. 1: FELONE_" + certi + "_" + documento + " YA SE ENCUENTRA INSTALADO EN ESTA BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
                        continue;
                    }

                    DirectoryInfo d = new DirectoryInfo(StoresProcedures + @"\" + dbtype + @"\" + certi + @"\");
                    FileInfo file = d.GetFiles("FELONE_" + certi + "_" + documento + ".txt")[0]; //Getting Text files
                    string texto = File.ReadAllText(file.FullName);
                    try
                    {
                        RecSet.DoQuery(texto);
                        mensaje += " MESSAGE No. 1: FELONE_" + certi + "_" + documento + " INSTALADO CON EXITO EN LA BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
                    }
                    catch (Exception ex)
                    {
                        mensaje += " ERROR No. 1: FELONE_" + certi + "_" + documento + ":  " + ex.Message + "; -- BASE DE DATOS [" + oCompany.CompanyDB + "] \n";
                    }

                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                RecSet = null;
                GC.Collect();

                SBO_Application.MessageBox(mensaje);

                if (error)
                {
                    SBO_Application.StatusBar.SetText("Error en Cartificador: " + certi);
                }

            }
        }

    }
}
