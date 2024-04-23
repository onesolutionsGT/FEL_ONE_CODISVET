﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FEL_ONE.Documento {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="FEL", ConfigurationName="Documento.DocumentoSoapPort")]
    public interface DocumentoSoapPort {
        
        // CODEGEN: Generating message contract since the wrapper name (Documento.Execute) of message ExecuteRequest does not match the default value (Execute)
        [System.ServiceModel.OperationContractAttribute(Action="FELaction/ADOCUMENTO.Execute", ReplyAction="*")]
        FEL_ONE.Documento.ExecuteResponse Execute(FEL_ONE.Documento.ExecuteRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="FELaction/ADOCUMENTO.Execute", ReplyAction="*")]
        System.Threading.Tasks.Task<FEL_ONE.Documento.ExecuteResponse> ExecuteAsync(FEL_ONE.Documento.ExecuteRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="Documento.Execute", WrapperNamespace="FEL", IsWrapped=true)]
    public partial class ExecuteRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="FEL", Order=0)]
        public string Cliente;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="FEL", Order=1)]
        public string Usuario;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="FEL", Order=2)]
        public string Clave;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="FEL", Order=3)]
        public string Nitemisor;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="FEL", Order=4)]
        public string Xmldoc;
        
        public ExecuteRequest() {
        }
        
        public ExecuteRequest(string Cliente, string Usuario, string Clave, string Nitemisor, string Xmldoc) {
            this.Cliente = Cliente;
            this.Usuario = Usuario;
            this.Clave = Clave;
            this.Nitemisor = Nitemisor;
            this.Xmldoc = Xmldoc;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="Documento.ExecuteResponse", WrapperNamespace="FEL", IsWrapped=true)]
    public partial class ExecuteResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="FEL", Order=0)]
        public string Respuesta;
        
        public ExecuteResponse() {
        }
        
        public ExecuteResponse(string Respuesta) {
            this.Respuesta = Respuesta;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface DocumentoSoapPortChannel : FEL_ONE.Documento.DocumentoSoapPort, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DocumentoSoapPortClient : System.ServiceModel.ClientBase<FEL_ONE.Documento.DocumentoSoapPort>, FEL_ONE.Documento.DocumentoSoapPort {
        
        public DocumentoSoapPortClient() {
        }
        
        public DocumentoSoapPortClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DocumentoSoapPortClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DocumentoSoapPortClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DocumentoSoapPortClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        FEL_ONE.Documento.ExecuteResponse FEL_ONE.Documento.DocumentoSoapPort.Execute(FEL_ONE.Documento.ExecuteRequest request) {
            return base.Channel.Execute(request);
        }
        
        public string Execute(string Cliente, string Usuario, string Clave, string Nitemisor, string Xmldoc) {
            FEL_ONE.Documento.ExecuteRequest inValue = new FEL_ONE.Documento.ExecuteRequest();
            inValue.Cliente = Cliente;
            inValue.Usuario = Usuario;
            inValue.Clave = Clave;
            inValue.Nitemisor = Nitemisor;
            inValue.Xmldoc = Xmldoc;
            FEL_ONE.Documento.ExecuteResponse retVal = ((FEL_ONE.Documento.DocumentoSoapPort)(this)).Execute(inValue);
            return retVal.Respuesta;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<FEL_ONE.Documento.ExecuteResponse> FEL_ONE.Documento.DocumentoSoapPort.ExecuteAsync(FEL_ONE.Documento.ExecuteRequest request) {
            return base.Channel.ExecuteAsync(request);
        }
        
        public System.Threading.Tasks.Task<FEL_ONE.Documento.ExecuteResponse> ExecuteAsync(string Cliente, string Usuario, string Clave, string Nitemisor, string Xmldoc) {
            FEL_ONE.Documento.ExecuteRequest inValue = new FEL_ONE.Documento.ExecuteRequest();
            inValue.Cliente = Cliente;
            inValue.Usuario = Usuario;
            inValue.Clave = Clave;
            inValue.Nitemisor = Nitemisor;
            inValue.Xmldoc = Xmldoc;
            return ((FEL_ONE.Documento.DocumentoSoapPort)(this)).ExecuteAsync(inValue);
        }
    }
}
