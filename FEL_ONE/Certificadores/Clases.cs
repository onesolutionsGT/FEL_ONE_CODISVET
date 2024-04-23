using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEL_ONE.Certificadores
{
    public class firma
    {
        public string resultado { get; set; }
        public string descripcion { get; set; }
        public string archivo { get; set; }
    }

    public class encabezado
    {
        public string resultado { get; set; }
        public string fecha { get; set; }
        public string origen { get; set; }
        public string descripcion { get; set; }
        public int cantidad_errores { get; set; }
        public ControlEmision control_emision { get; set; }
        public List<List<erroresinfile>> descripcion_errores { get; set; }
        public string uuid { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string xml_certificado { get; set; }
    }

    public class ControlEmision
    {
        public string Saldo { get; set; }
        public string Creditos { get; set; }
    }

    public class DescripcionErrore
    {
        public bool resultado { get; set; }
        public string fuente { get; set; }
        public string categoria { get; set; }
        public string numeral { get; set; }
        public string validacion { get; set; }
        public string mensaje_error { get; set; }
    }

    public class encabezadoerrores
    {
        public bool resultado { get; set; }
        public DateTime fecha { get; set; }
        public string origen { get; set; }
        public string descripcion { get; set; }
        public ControlEmision control_emision { get; set; }
        public bool alertas_infile { get; set; }
        public object[] descripcion_alertas_infile { get; set; }
        public bool alertas_sat { get; set; }
        public object[] descripcion_alertas_sat { get; set; }
        public int cantidad_errores { get; set; }
        public DescripcionErrore[] descripcion_errores { get; set; }
        public string informacion_adicional { get; set; }
        public string uuid { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string xml_certificado { get; set; }
    }

    public class erroresinfile
    {
        public string resultado { get; set; }
        public string fuente { get; set; }
        public string categoria { get; set; }
        public string numeral { get; set; }
        public string validacion { get; set; }
        public string mensaje_error { get; set; }
    }



    public class Dato
    {
        public string nit { get; set; }
        public string estado { get; set; }
        public string nombre { get; set; }
        public int es_emisor { get; set; }
        public string afiliacion_iva { get; set; }
        public string fecha_creacion { get; set; }
        public int cantidad_marcas { get; set; }
        public int tipo_personeria { get; set; }
        public string usuario_creacion { get; set; }
        public object direccion_completa { get; set; }
        public string estado_descripcion { get; set; }
        public object fecha_modificacion { get; set; }
        public object correo_notificacion { get; set; }
        public object usuario_modificacion { get; set; }
        public int cantidad_establecimientos { get; set; }
        public string afiliacion_iva_descripcion { get; set; }
        public int cantidad_frases_escenarios { get; set; }
        public string tipo_personeria_descripcion { get; set; }
    }

    public class Resultado
    {
        public int error { get; set; }
        public string mensaje { get; set; }
    }

    public class tekraMoedelInfoClient
    {
        public List<Resultado> resultado { get; set; }
        public List<Dato> datos { get; set; }
    }

    public class InfileInfoClient
    {
        public string nit { get; set; }

        public string nombre { get; set; }

        public string mensaje { get; set; }
    }
}
