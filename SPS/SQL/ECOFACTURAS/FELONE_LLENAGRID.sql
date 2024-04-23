USE [SBO_DENARIUM_GUATEMALA]
GO

/****** Object:  StoredProcedure [dbo].[FELONE_LLENAGRID]    Script Date: 8/4/2022 5:01:41 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE procedure [dbo].[FELONE_LLENAGRID]
AS
select  
                     a.Series,
                     a.SeriesName,
                     Case objectcode WHEN 13 THEN CASE DocSubType WHEN 'DN' THEN 'Nota de Debito' ELSE 'Factura' END ELSE CASE objectcode WHEN 14 THEN 'Nota de Credito' ELSE 'Factura Proveedor' end End 'Tipo Serie',
                     'Es documento electrónico' = Case isnull(b.U_SERIE, '100') WHEN '100' THEN '0' ELSE 'Y' End,
					 b.U_TIPO_DOC 'Tipo Documento',
					 'Es batch' = Case isnull(b.U_ES_BATCH, '0') WHEN '0' THEN '0' ELSE 'Y' End ,
					 b.U_DISPOSITIVO 'Dispositivo',
                     b.U_DIR 'Direccion',
					 b.U_MUNI Municipio, 
					 b.U_DEPTO Departamento,
					 B.U_PAIS Pais,
					 B.U_CODP 'Codigo Postal',
					 B.U_NOMBRECOMERCIAL 'Nombre Comercial'
					-- b.U_mail Email,
					 --b.U_Telefono Telefono
					 
                     from NNM1 a left outer join [dbo].[@FEL_RESOLUCION] b
                     on  a.Series =b.U_SERIE 
                      where a.objectcode in ('13','14','18','2','4') and a.Locked = 'N'
                      order by a.objectcode,a.docsubtype  



					  select * from [@FEL_RESOLUCION]
GO

