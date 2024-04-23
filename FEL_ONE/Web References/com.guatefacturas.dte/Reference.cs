﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace FEL_ONE.com.guatefacturas.dte {
    using System.Diagnostics;
    using System;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System.Web.Services;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3761.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="Guatefac", Namespace="http://dbguatefac/Guatefac.wsdl")]
    public partial class Guatefac : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback anulaDocumentoOperationCompleted;
        
        private System.Threading.SendOrPostCallback generaDocumentoOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Guatefac() {
            this.Url = global::FEL_ONE.Properties.Settings.Default.FEL_ONE_com_guatefacturas_dte_Guatefac;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event anulaDocumentoCompletedEventHandler anulaDocumentoCompleted;
        
        /// <remarks/>
        public event generaDocumentoCompletedEventHandler generaDocumentoCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://dbguatefac/Guatefac.wsdl/anulaDocumento", RequestNamespace="http://dbguatefac/Guatefac.wsdl", ResponseNamespace="http://dbguatefac/Guatefac.wsdl", Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        [return: System.Xml.Serialization.XmlElementAttribute("result")]
        public string anulaDocumento(string pUsuario, string pPassword, string pNitEmisor, string pSerie, string pPreimpreso, string pNitComprador, string pFechaAnulacion, string pMotivoAnulacion) {
            object[] results = this.Invoke("anulaDocumento", new object[] {
                        pUsuario,
                        pPassword,
                        pNitEmisor,
                        pSerie,
                        pPreimpreso,
                        pNitComprador,
                        pFechaAnulacion,
                        pMotivoAnulacion});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void anulaDocumentoAsync(string pUsuario, string pPassword, string pNitEmisor, string pSerie, string pPreimpreso, string pNitComprador, string pFechaAnulacion, string pMotivoAnulacion) {
            this.anulaDocumentoAsync(pUsuario, pPassword, pNitEmisor, pSerie, pPreimpreso, pNitComprador, pFechaAnulacion, pMotivoAnulacion, null);
        }
        
        /// <remarks/>
        public void anulaDocumentoAsync(string pUsuario, string pPassword, string pNitEmisor, string pSerie, string pPreimpreso, string pNitComprador, string pFechaAnulacion, string pMotivoAnulacion, object userState) {
            if ((this.anulaDocumentoOperationCompleted == null)) {
                this.anulaDocumentoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnanulaDocumentoOperationCompleted);
            }
            this.InvokeAsync("anulaDocumento", new object[] {
                        pUsuario,
                        pPassword,
                        pNitEmisor,
                        pSerie,
                        pPreimpreso,
                        pNitComprador,
                        pFechaAnulacion,
                        pMotivoAnulacion}, this.anulaDocumentoOperationCompleted, userState);
        }
        
        private void OnanulaDocumentoOperationCompleted(object arg) {
            if ((this.anulaDocumentoCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.anulaDocumentoCompleted(this, new anulaDocumentoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://dbguatefac/Guatefac.wsdl/generaDocumento", RequestNamespace="http://dbguatefac/Guatefac.wsdl", ResponseNamespace="http://dbguatefac/Guatefac.wsdl", Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        [return: System.Xml.Serialization.XmlElementAttribute("result")]
        public string generaDocumento(string pUsuario, string pPassword, string pNitEmisor, decimal pEstablecimiento, decimal pTipoDoc, string pIdMaquina, string pTipoRespuesta, string pXml) {
            object[] results = this.Invoke("generaDocumento", new object[] {
                        pUsuario,
                        pPassword,
                        pNitEmisor,
                        pEstablecimiento,
                        pTipoDoc,
                        pIdMaquina,
                        pTipoRespuesta,
                        pXml});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void generaDocumentoAsync(string pUsuario, string pPassword, string pNitEmisor, decimal pEstablecimiento, decimal pTipoDoc, string pIdMaquina, string pTipoRespuesta, string pXml) {
            this.generaDocumentoAsync(pUsuario, pPassword, pNitEmisor, pEstablecimiento, pTipoDoc, pIdMaquina, pTipoRespuesta, pXml, null);
        }
        
        /// <remarks/>
        public void generaDocumentoAsync(string pUsuario, string pPassword, string pNitEmisor, decimal pEstablecimiento, decimal pTipoDoc, string pIdMaquina, string pTipoRespuesta, string pXml, object userState) {
            if ((this.generaDocumentoOperationCompleted == null)) {
                this.generaDocumentoOperationCompleted = new System.Threading.SendOrPostCallback(this.OngeneraDocumentoOperationCompleted);
            }
            this.InvokeAsync("generaDocumento", new object[] {
                        pUsuario,
                        pPassword,
                        pNitEmisor,
                        pEstablecimiento,
                        pTipoDoc,
                        pIdMaquina,
                        pTipoRespuesta,
                        pXml}, this.generaDocumentoOperationCompleted, userState);
        }
        
        private void OngeneraDocumentoOperationCompleted(object arg) {
            if ((this.generaDocumentoCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.generaDocumentoCompleted(this, new generaDocumentoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3761.0")]
    public delegate void anulaDocumentoCompletedEventHandler(object sender, anulaDocumentoCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3761.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class anulaDocumentoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal anulaDocumentoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3761.0")]
    public delegate void generaDocumentoCompletedEventHandler(object sender, generaDocumentoCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3761.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class generaDocumentoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal generaDocumentoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591