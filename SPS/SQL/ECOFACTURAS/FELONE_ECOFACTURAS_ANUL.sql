USE [SBO_DENARIUM_GUATEMALA]
GO

/****** Object:  StoredProcedure [dbo].[FELONE_ECOFACTURAS_ANUL]    Script Date: 8/4/2022 5:00:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE PROCEDURE [dbo].[FELONE_ECOFACTURAS_ANUL] (@DOCENTRY INT,@TIPO char(9))
AS

DECLARE @DTEENCA VARCHAR(MAX)
DECLARE @RESULT AS VARCHAR(MAX)

--SET @ENCODIGN='<?xml version="1.0" encoding="UTF-8" standalone="no"?>'

--VARIABLES DE ENCABEZADO
DECLARE @U_FIRMA_ELETRONICA VARCHAR(50)
DECLARE @IDReceptor VARCHAR(50)
DECLARE @FechaEmisionDocumentoAnular VARCHAR(50)
DECLARE @FechaHoraAnulacion VARCHAR(50)
DECLARE @U_MOTIVO_NC VARCHAR(50)


--CURSORES

IF @Tipo='FACT' OR @Tipo='NDEB' OR @Tipo='FCAM' OR @Tipo='RDON' OR @Tipo='FEXP'
	BEGIN
			Select 
			@U_FIRMA_ELETRONICA = isnull(t0.U_NUMERO_DOCUMENTO_NC,'')
			from OINV t0 
			where t0.DocEntry =@Docentry;
END
	IF @Tipo='NCRE' or @Tipo='NABN'
	begin
			Select 
			@U_FIRMA_ELETRONICA = isnull(t0.U_NUMERO_DOCUMENTO_NC,'')
			from ORIN t0 
			where t0.DocEntry =@Docentry;
	end

	IF @Tipo='FESP' 
	begin
			Select 
			@U_FIRMA_ELETRONICA = isnull(t0.U_NUMERO_DOCUMENTO_NC,'')
			from opch t0 
			where t0.DocEntry =@Docentry;
	end

SELECT  @U_FIRMA_ELETRONICA XML_GENERADO
GO

