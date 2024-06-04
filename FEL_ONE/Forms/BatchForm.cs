using FEL_ONE.Certificadores;
using FEL_ONE.Clases;
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FEL_ONE.Forms
{

    class BatchForm
    {
        private string XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\Batch.srf").Replace(@"\\", @"\");
        SAPbouiCOM.Application SBO_Application;
        private SAPbouiCOM.Form oForm;
        private SAPbobsCOM.Company oCompany;


        public BatchForm()
        {

            try
            {
                SBO_Application = Utils.SBOApplication;
                oCompany = Utils.Company;

                if (Utils.ActivateFormIsOpen(SBO_Application, "SBOBatch") == false)
                {
                    LoadFromXML(XmlForm);
                    oForm = SBO_Application.Forms.Item("SBOBatch");
                    oForm.Visible = true;
                    oForm.PaneLevel = 1;

                    oForm.DataSources.DataTables.Add("MyDataTable");
                    oForm.DataSources.UserDataSources.Add("CheckDS1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 15);

                    SAPbouiCOM.CheckBox chkRecha;
                    chkRecha = oForm.Items.Item("chkRecha").Specific;
                    chkRecha.DataBind.SetBound(true, "", "CheckDS1");

                    SAPbouiCOM.EditText del;
                    SAPbouiCOM.EditText Al;
                    SAPbouiCOM.Item cmdResult;
                    SAPbouiCOM.Button cmdenviar;

                    oForm.DataSources.UserDataSources.Add("UDDate", SAPbouiCOM.BoDataType.dt_DATE);
                    oForm.DataSources.UserDataSources.Add("UDDate2", SAPbouiCOM.BoDataType.dt_DATE);
                    del = oForm.Items.Item("txtDel").Specific;
                    Al = oForm.Items.Item("txtAl").Specific;
                    cmdResult = oForm.Items.Item("cmdResult");
                    cmdenviar = oForm.Items.Item("cmdEnviar").Specific;
                    del.DataBind.SetBound(true, "", "UDDate");
                    Al.DataBind.SetBound(true, "", "UDDate2");

                    cmdenviar.Caption = "Enviar";
                    cmdResult.Enabled = true;

                    oForm.Title = "One Solutions - Envio por Lote";
                    LlenaSeries();
                    setEvents();
                }
                else
                {
                    oForm = SBO_Application.Forms.Item("SBOBatch");
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
                .ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent_SBOBATCH);
        }

        private void removeEvents()
        {
            SBO_Application
                .ItemEvent -= new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent_SBOBATCH);
        }

        private void SBO_Application_ItemEvent_SBOBATCH(string FormUID, ref ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if (oForm == null)
                {
                    return;
                }


                if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_CLOSE && pVal.BeforeAction == true && pVal.FormType == 60006)
                {
                    removeEvents();
                    oForm = null;
                }

                if (pVal.FormType == 60006)
                {
                    if (pVal.ItemUID == "cmdConsul" & pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED & pVal.Before_Action == true)
                    {
                        Consulta();
                        BubbleEvent = false;
                    }

                    if (pVal.ItemUID == "chkRecha" & pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED & pVal.Before_Action == false)
                    {
                        Consulta();
                        BubbleEvent = false;
                    }

                    if (BubbleEvent == true & pVal.ItemUID == "cmdEnviar" & pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED & pVal.Before_Action == false)
                    {
                        SAPbouiCOM.ComboBox serie;
                        serie = oForm.Items.Item("cmbSerie").Specific;
                        Enviar(serie.Value);
                        BubbleEvent = false;
                    }
                    if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_CLOSE)
                    {
                        removeEvents();
                        oForm = null;
                    }
                }
            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message.ToString());
            }
        }

        private int SetConnectionContext()
        {
            string sCookie;
            string sConnectionContext;

            oCompany = new SAPbobsCOM.Company();
            sCookie = oCompany.GetContextCookie();
            sConnectionContext = SBO_Application.Company.GetConnectionContext(sCookie);

            if (oCompany.Connected == true)
            {
                oCompany.Disconnect();
            }
            return oCompany.SetSboLoginContext(sConnectionContext);
        }

        private int ConnectToCompany()
        {
            if (oCompany.Connected == true)
            {
                oCompany.Disconnect();
            }
            return oCompany.Connect();
        }

        private void LlenaSeries()
        {
            string sql = "";
            SAPbobsCOM.Recordset RecSet;
            var sUser = oCompany.UserSignature;
            try
            {
                var user = SBO_Application.Company.UserName.ToString();
                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    sql += "CALL FELONE_UTILS ('LLENASERIES','" + user + "','','','','','','','','','') ";
                }
                else
                {
                    sql += "EXEC FELONE_UTILS 'LLENASERIES','" + user + "','','','','','','','','','' ";
                }

                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(sql);
                if (RecSet.RecordCount > 0)
                {
                    SAPbouiCOM.ComboBox cmbSerie;
                    for (var i = 0; i <= RecSet.RecordCount - 1; i++)
                    {
                        cmbSerie = oForm.Items.Item("cmbSerie").Specific;
                        cmbSerie.ValidValues.Add(RecSet.Fields.Item("Series").Value, RecSet.Fields.Item("SeriesName").Value);
                        RecSet.MoveNext();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void LlengaGrid(string Serie, bool incluirRechazadas, string del, string al)
        {
            string sql;
            SAPbouiCOM.Grid oGrid;
            SAPbouiCOM.Item oitem;
            SAPbobsCOM.Recordset RecSet;
            SAPbouiCOM.Item cmdEnviar;
            SAPbouiCOM.Item chkRecha;
            SAPbouiCOM.StaticText lblReg;
            SAPbouiCOM.StaticText lblProc;

            try
            {
                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    sql = "CALL FELONE_UTILS ('LISTADOBATCH','" + Serie + "','" + del + "','" + al + "','','','','','','','') ";
                }
                else
                {
                    sql = "EXEC FELONE_UTILS 'LISTADOBATCH','" + Serie + "','" + del + "','" + al + "','','','','','','','' ";
                }

                cmdEnviar = oForm.Items.Item("cmdEnviar");
                cmdEnviar.Enabled = false;
                chkRecha = oForm.Items.Item("chkRecha");
                chkRecha.Enabled = false;
                lblReg = oForm.Items.Item("lblReg").Specific;
                lblProc = oForm.Items.Item("lblProc").Specific;
                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                RecSet.DoQuery(sql);
                string sql2;

                sql2 = "select \"ObjectCode\" from nnm1 where \"Series\" = '" + Serie + "'";


                string objetoresult;

                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    objetoresult = Utils.TraeDatoH(sql2);
                }
                else
                {
                    objetoresult = Utils.TraeDato(sql2);
                }

                if (RecSet.RecordCount > 0)
                {
                    oitem = oForm.Items.Item("grdDatos");
                    oGrid = oitem.Specific;
                    oForm.DataSources.DataTables.Item(0).ExecuteQuery(sql);
                    oGrid.DataTable = oForm.DataSources.DataTables.Item("MyDataTable");
                    oGrid.AutoResizeColumns();
                    oGrid.Columns.Item(1).RightJustified = true;
                    oGrid.Columns.Item(4).RightJustified = true;
                    oGrid.Columns.Item(5).RightJustified = true;
                    oGrid.Columns.Item(0).Editable = false;
                    oGrid.Columns.Item(1).Editable = false;
                    oGrid.Columns.Item(2).Editable = false;
                    oGrid.Columns.Item(3).Editable = false;
                    oGrid.Columns.Item(4).Editable = false;
                    oGrid.Columns.Item(5).Editable = false;
                    ((SAPbouiCOM.EditTextColumn)oGrid.Columns.Item(5)).LinkedObjectType = objetoresult;
                    cmdEnviar.Enabled = true;
                    chkRecha.Enabled = true;
                    lblReg.Caption = "Total registros (" + oGrid.Rows.Count + ")";
                    lblProc.Caption = "Registros procesados (0 de " + oGrid.Rows.Count + ")";
                }
                else
                {
                    oForm.DataSources.DataTables.Item(0).Clear();
                    SBO_Application.SetStatusBarMessage("La información solicitada no ha sido encontrada", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                }
            }
            catch (Exception ex)
            {
                SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
            }
        }

        private void Consulta()
        {
            SAPbouiCOM.ComboBox cmbItem;
            SAPbouiCOM.CheckBox chkRecha;
            SAPbouiCOM.EditText del;
            SAPbouiCOM.EditText al;
            try
            {
                cmbItem = oForm.Items.Item("cmbSerie").Specific;
                chkRecha = oForm.Items.Item("chkRecha").Specific;
                del = oForm.Items.Item("txtDel").Specific;
                al = oForm.Items.Item("txtAl").Specific;
                if (cmbItem.Value == "")
                {
                    throw (new Exception("Debe de seleccionar la serie a enviar"));
                }
                if (del.Value == "")
                {
                    throw new Exception("Debe de ingresar la fecha inicial");
                }
                if (al.Value == "")
                {
                    throw new Exception("Debe de ingresar la fecha final");
                }
                if (DateTime.ParseExact(del.Value, "yyyyMMdd", CultureInfo.CurrentCulture) > DateTime.ParseExact(al.Value, "yyyyMMdd", CultureInfo.CurrentCulture))
                {
                    throw new Exception("El rango de fechas es inválido");
                }
                LlengaGrid(cmbItem.Value, chkRecha.Checked, del.Value, al.Value);
            }
            catch (Exception ex)
            {
                SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
            }
        }

        private void Enviar(string mySerie)
        {
            SAPbouiCOM.Grid oGrid;
            string Tipo = "";
            string Serie = "";
            string SerieName = "";
            string sql = "";
            SAPbouiCOM.Item cmdEnvio;
            SAPbobsCOM.Recordset RecSet;
            int TotalDocs;
            int cont = 0;
            string certificador;
            SAPbouiCOM.StaticText lblProc;
            try
            {
                oGrid = oForm.Items.Item("grdDatos").Specific;
                if (oGrid.Rows.Count > 0)
                {
                    cmdEnvio = oForm.Items.Item("cmdEnviar");
                    cmdEnvio.Enabled = false;
                    TotalDocs = oGrid.Rows.Count;
                    lblProc = oForm.Items.Item("lblProc").Specific;
                    SAPbouiCOM.ComboBox cmbSerie;
                    cmbSerie = oForm.Items.Item("cmbSerie").Specific;
                    Serie = cmbSerie.Selected.Value;
                    SerieName = cmbSerie.Selected.Description;
                    sql = "select \"U_TIPO_DOC\" from \"@FEL_RESOLUCION\" where \"U_SERIE\" = '" + Serie + "' ";
                    RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    RecSet.DoQuery(sql);
                    if (RecSet.RecordCount > 0)
                    {
                        Tipo = RecSet.Fields.Item("U_TIPO_DOC").Value;
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(RecSet);
                    RecSet = null;
                    GC.Collect();

                    certificador = Utils.getCertificador();

                    string pais = "Guatemala";
                    
                    try
                    {
                        for (var i = 0; i <= oGrid.Rows.Count - 1; i++)
                        {
                            switch (Utils.FEL)
                            {
                                case Utils.TipoFEL.MEGAPRINT:
                                    Megaprint.EnviaDocumentoFEL(oCompany, SBO_Application, Tipo, Serie, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i).ToString(), SerieName, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i).ToString(), true);
                                    break;
                                case Utils.TipoFEL.INFILE:
                                    new Infile().EnviaDocumentoFEL(oCompany, "", SBO_Application, Tipo, Serie, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i).ToString(), SerieName, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i).ToString(), true);
                                    break;
                                case Utils.TipoFEL.G4S:
                                    G4s.EnviaDocumentoFEL(oCompany, SBO_Application, Tipo, Serie, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i).ToString(), SerieName, pais, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i).ToString(), true);
                                    break;
                                case Utils.TipoFEL.DIGIFACT:
                                    Digifact.EnviaDocumentoFEL(oCompany, SBO_Application, Tipo, Serie, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i).ToString(), SerieName, pais, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i).ToString(), true);
                                    break;
                                case Utils.TipoFEL.TEKRA:
                                    Tekra.EnviaDocumentoFEL(oCompany, SBO_Application, Tipo, Serie, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i).ToString(), SerieName, oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i).ToString(), "SI", true);
                                    break;
                                case Utils.TipoFEL.ECOFACTURAS:
                                    EcoFacturas.EnviaDocumentoFEL(oCompany, SBO_Application, Tipo, Serie, (oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i)).ToString(), SerieName, pais, (oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i)).ToString(), true);
                                    break;
                                case Utils.TipoFEL.GUATEFACTURAS:
                                    Guatefacturas.EnviaDocumentoFEL(oCompany, SBO_Application, Tipo, Serie, (oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(1).Name, i)).ToString(), SerieName, pais, (oGrid.DataTable.GetValue(oGrid.DataTable.Columns.Item(5).Name, i)).ToString(), true);
                                    break;
                            }
                            cont += 1;
                            SBO_Application.SetStatusBarMessage("Documentos Procesados (" + cont.ToString() + " de " + TotalDocs.ToString() + ")", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                            lblProc.Caption = "Registros procesados (" + cont.ToString() + " de " + TotalDocs.ToString() + ")";
                        }
                        oForm.DataSources.DataTables.Item(0).Clear();
                        SBO_Application.SetStatusBarMessage("Proceso finalizado...", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    }
                    catch (Exception ex)
                    {
                        SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short);
                    }
                }
            }
            catch (Exception ex)
            {
                SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
            }
        }

        private void LoadFromXML(string FileName)
        {
            System.Xml.XmlDocument oXmlDoc;
            oXmlDoc = new System.Xml.XmlDocument();
            oXmlDoc.Load(FileName);
            SBO_Application.LoadBatchActions(oXmlDoc.InnerXml);
        }
    }
}
