using FEL_ONE.Clases;
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FEL_ONE.Forms
{
    class Certificador
    {
        string XmlForm = (System.Windows.Forms.Application.StartupPath + @"\SRFS\Certificador.srf").Replace(@"\\", @"\");
        private SAPbouiCOM.Application SBO_Application;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.EventFilters oFilters;
        private SAPbouiCOM.EventFilter oFilter;
        private SAPbouiCOM.ComboBox oCertificador;
        private SAPbouiCOM.Button oBtnOk;
        private SAPbouiCOM.Button oBtnQl;
        private SAPbobsCOM.Company oCompany;

        public Certificador()
        {
            SBO_Application = Utils.SBOApplication;
            oCompany = Utils.Company;

            if (Utils.ActivateFormIsOpen(SBO_Application, "SBOCerti") == false)
            {
                setEvents();
                LoadFromXML(XmlForm);
                oForm = SBO_Application.Forms.Item("SBOCerti");
                oForm.Left = 400;
                oForm.Top = 200;
                oForm.Visible = true;
                oForm.PaneLevel = 1;
                oCertificador = oForm.Items.Item("certi").Specific;
                oBtnOk = oForm.Items.Item("Item_3").Specific;
                oBtnQl = oForm.Items.Item("Item_4").Specific;

                SAPbobsCOM.Recordset RecSet;
                RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                string Sql;
                if (oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    Sql = @" Select ""U_VALOR"" from ""@FEL_PARAMETROS"" where ""U_PARAMETRO"" = 'Certi'";
                    oForm.Title = "Certificador FEL ! HANA";
                }
                else
                {
                    oForm.Title = "Certificador FEL ! SQL";
                    Sql = " SELECT \"U_VALOR\" FROM \"@FEL_PARAMETROS\" WHERE \"U_PARAMETRO\" = 'Certi'  ";
                }               
                try 
                {
                    RecSet.DoQuery(Sql);
                    if(RecSet.RecordCount != 0)
                    {
                        oCertificador.Select(RecSet.Fields.Item(0).Value);
                        oBtnOk.Item.Enabled = false;
                        oBtnQl.Item.Enabled = false;
                    }
                }
                catch (Exception) { }
            }

        }

        private void setEvents()
        {
            SBO_Application
                .ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent_CERTI);
        }

        private void removeEvents()
        {
            SBO_Application
                .ItemEvent -= new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent_CERTI);
        }

        private void LoadFromXML(string FileName)
        {
            System.Xml.XmlDocument oXmlDoc;
            oXmlDoc = new System.Xml.XmlDocument();
            oXmlDoc.Load(FileName);
            SBO_Application.LoadBatchActions(oXmlDoc.InnerXml);
        }

        private void SetFilters()
        {
            oFilters = new SAPbouiCOM.EventFilters();
            oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED);
            oFilter.AddEx("60006"); 
            oFilter.Add(60006);
            SBO_Application.SetFilter(oFilters);
        }

        private bool addCertificador(string certificador)
        {
            SAPbobsCOM.Recordset RecSet;
            RecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            string Sql;
            if ((oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB))
            {
                Sql = @" insert into ""@FEL_PARAMETROS"" values('Certi',0,-3,0,'Certi'," + certificador + ")";

            }
            else
            {
                Sql = @" insert into ""@FEL_PARAMETROS"" values('Certi',0,-3,0,'Certi'," + certificador + ")";

            }            
            RecSet.DoQuery(Sql);
            return true;
        }

        private void SBO_Application_ItemEvent_CERTI(string FormUID, ref ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if (pVal.ItemUID == "Item_3" & pVal.FormType == 60004 & pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED & pVal.Before_Action == true)
                {
                    if (addCertificador(oCertificador.Value))
                    {
                        SBO_Application.SetStatusBarMessage("Certificador ingresado con exito", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                        oForm.Close();
                        SystemForm.ExistCerti();
                    }
                    else
                    {
                        SBO_Application.SetStatusBarMessage("Fallo al intentar ingresar certificador", SAPbouiCOM.BoMessageTime.bmt_Short, true);
                    }                      
                    BubbleEvent = false;
                }

                if (pVal.ItemUID == "Item_4" & pVal.FormType == 60004 & pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED & pVal.Before_Action == true)
                {
                    removeEvents();
                    oForm.Close();
                    SBO_Application.SetStatusBarMessage("Finalizando add-on facturación electrónica...", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                    Utils.Company.Disconnect();
                    System.Environment.Exit(0);
                    BubbleEvent = false;
                }

                if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_CLOSE & pVal.BeforeAction == true & pVal.FormType == 60004)
                {
                    removeEvents();
                    oForm = null;
                }
            }
            catch (Exception ex)
            {
                SBO_Application.MessageBox(ex.Message);
            }
        }

        //~Certificador() { }
    }


}
