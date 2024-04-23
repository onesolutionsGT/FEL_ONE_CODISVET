USE [SBO_DENARIUM_GUATEMALA]
GO

/****** Object:  StoredProcedure [dbo].[FELONE_ECOFACTURAS_FESP]    Script Date: 8/4/2022 5:00:40 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[FELONE_ECOFACTURAS_FESP] (@DOCENTRY INT,@DECIMAL INT)
AS

DECLARE @DTEENCA VARCHAR(MAX)
DECLARE @DTEDETA VARCHAR(MAX)
DECLARE @DTECOMPLEMENTO VARCHAR(MAX)
DECLARE @DTEADENDA VARCHAR(MAX)
DECLARE @RESULT AS VARCHAR(MAX)
DECLARE @ENCODIGN AS VARCHAR(MAX)

SET @ENCODIGN='<?xml version="1.0"?>'


--VARIABLES DE ENCABEZADO
DECLARE @CodigoMoneda VARCHAR(20)
DECLARE @FechaHoraEmision VARCHAR(50)
DECLARE @Tipo VARCHAR(4)
DECLARE @AfiliacionIVA VARCHAR(50)
DECLARE @CodigoEstablecimiento VARCHAR(20)
DECLARE @NITEmisor VARCHAR(20)
DECLARE @NombreComercial VARCHAR(150)
DECLARE @NombreEmisor VARCHAR(150)
DECLARE @CorreoEmisor VARCHAR(150)
DECLARE @EDireccion VARCHAR(100)
DECLARE @ECodigoPostal VARCHAR(20)
DECLARE @EMunicipio VARCHAR(100)
DECLARE @EDepartamento VARCHAR(100)
DECLARE @EPais VARCHAR(15)
DECLARE @IDReceptor VARCHAR(15)
DECLARE @CardCode VARCHAR(50)
DECLARE @NombreReceptor VARCHAR(500)
DECLARE @CorreoReceptor VARCHAR(150)
DECLARE @RDireccion VARCHAR(100)
DECLARE @RCodigoPostal VARCHAR(20)
DECLARE @RMunicipio VARCHAR(100)
DECLARE @RDepartamento VARCHAR(100)
DECLARE @RPais VARCHAR(15)
DECLARE @Email VARCHAR(250)
DECLARE @DocNum VARCHAR(25)
DECLARE @Comentarios VARCHAR(500)
DECLARE @Exento VARCHAR(25)
DECLARE @TrnNum VARCHAR(25)
----------------IMPUESTOS-------------------
DECLARE @TINombreCorto VARCHAR(10)
DECLARE @TITotalMontoImpuesto numeric(19,6)
DECLARE @GranTotal numeric(19,6)
DECLARE @BienOServicio VARCHAR(15)
-----------------TOTALES-------------------
DECLARE @TotalFinal VARCHAR(100)
DECLARE @Ivatotal VARCHAR(100)

--VARIABLES DE ADENDA
DECLARE @Adendaid VARCHAR(10)
DECLARE @Valor1 VARCHAR(20)
DECLARE @Valor2 VARCHAR(20)
DECLARE @Valor3 VARCHAR(20)
DECLARE @Valor4 VARCHAR(20)
DECLARE @Valor5 VARCHAR(20)
DECLARE @Valor6 VARCHAR(20)


--VARIABLES DE DETALLE
DECLARE @LINEAS INTEGER
DECLARE @i INTEGER
DECLARE @Num Varchar(10)
DECLARE @NumeroLinea VARCHAR(10)
DECLARE @ItemCode VARCHAR(200)
DECLARE @Cantidad numeric(19,6)
DECLARE @UnidadMedida VARCHAR(500)
DECLARE @Descripcion VARCHAR(MAX)
DECLARE @PrecioUnitario numeric(19,6)
DECLARE	@Precio numeric(19,6)
DECLARE @Descuento numeric(19,6)--VARCHAR(10)
DECLARE @INombreCorto VARCHAR(15)
DECLARE @ICodigoUnidadGravable VARCHAR(500)
DECLARE @IMontoGravable numeric(19,6)
DECLARE @IMontoImpuesto numeric(19,6)
DECLARE @Total numeric(19,6)

SELECT DISTINCT			
			@CodigoEstablecimiento = T1.U_DISPOSITIVO,
			@Tipo = 'FESP',
			@DocNum = t0.DocNum,
			@FechaHoraEmision = convert(varchar, getdate(), 23),
			@CodigoMoneda = CASE t0.DOCCUR WHEN 'QTZ' THEN 'GTQ' ELSE 'USD' END,
			@IDReceptor = CASE 
				when t3."Password" = 'C/F' then 'CF'
				when t3."Password" = 'cf' then 'CF'
				else isnull(replace(t3."Password",'-',''),'CF') END,
			@RDireccion = isnull(t3.Address,'CIUDAD'),
			@CardCode = t0.CardCode,
			@NombreReceptor = t0.CardName,
			@Comentarios = isnull(t0.Comments,''),
			@Email = isnull(t3.E_Mail,''),
			@Total = t0.DocTotal,

			--BienOServicio = case when t0.doctype = 'I' then 'B' else 'S' end,
			@Valor1 = '',
			@Valor2 = '',
			@Valor3 = '',
			@Valor4 = '',
			@Valor5 = '',
			@Valor6 = ''
			from OPCH t0 
			left outer join [@FEL_RESOLUCION] T1
				on T1.U_SERIE=t0.series
				left outer join [@FEL_PARAMETROS] T2 on t2.LineId = 0
				left outer join  OCRD t3
				on t3.CardCode =t0.CardCode
				INNER JOIN NNM1 on t0.Series =NNM1.Series 
				LEFT OUTER JOIN CRD1
				ON t3.CARDCODE=CRD1.CARDCODE
			where t0.DocEntry =@Docentry;

SELECT 
	@ICodigoUnidadGravable = A."TaxCode" 
FROM INV1 A WHERE A.DocEntry =@Docentry AND  A."VisOrder" = 0;

 select @TrnNum = U_VALOR 
 from "@FEL_PARAMETROS" 
 where U_PARAMETRO = 'TrnNum'

 UPDATE 
 "@FEL_PARAMETROS" SET U_VALOR = @TrnNum + 1
 where U_PARAMETRO = 'TrnNum'

----------------------------ENCABEZADO
SET @DTEENCA = '<stdTWS xmlns="FEL">'
SET @DTEENCA=@DTEENCA+ '<TrnEstNum>' + @CodigoEstablecimiento + '</TrnEstNum>'
SET @DTEENCA=@DTEENCA+ '<TipTrnCod>'+@Tipo+'</TipTrnCod>'
SET @DTEENCA=@DTEENCA+ '<TrnNum>' + @TrnNum + '</TrnNum>'
SET @DTEENCA=@DTEENCA+ '<TrnFec>' + @FechaHoraEmision + '</TrnFec>'
SET @DTEENCA=@DTEENCA+ '<MonCod>' + @CodigoMoneda + '</MonCod>'



SET @DTEENCA=@DTEENCA+ '<TrnBenConNIT>' + @IDReceptor + '</TrnBenConNIT>'
SET @DTEENCA=@DTEENCA+ '<TrnBenConEspecial>1</TrnBenConEspecial>'
SET @DTEENCA=@DTEENCA+ '<TrnExp>0</TrnExp>'


IF @ICodigoUnidadGravable = 'EXE'
	BEGIN
		SET @Exento = 1
		SET @DTEENCA=@DTEENCA+ '<TrnExento>' + @Exento + '</TrnExento>'
		SET @DTEENCA=@DTEENCA+ '<TrnFraseTipo>4</TrnFraseTipo>'
		SET @DTEENCA=@DTEENCA+ '<TrnEscCod>2</TrnEscCod>'
	END
ELSE
	BEGIN
		SET @Exento = 0
		SET @DTEENCA=@DTEENCA+ '<TrnExento>' + @Exento + '</TrnExento>'
		SET @DTEENCA=@DTEENCA+ '<TrnFraseTipo>0</TrnFraseTipo>'
		SET @DTEENCA=@DTEENCA+ '<TrnEscCod>0</TrnEscCod>'
	END


SET @DTEENCA=@DTEENCA+ '<TrnEFACECliCod>' + @CardCode + '</TrnEFACECliCod>'
SET @DTEENCA=@DTEENCA+ '<TrnEFACECliNom>' + @NombreReceptor + '</TrnEFACECliNom>'
SET @DTEENCA=@DTEENCA+ '<TrnEFACECliDir>' + @RDireccion + '</TrnEFACECliDir>'
SET @DTEENCA=@DTEENCA+ '<TrnObs>' + @Comentarios + '</TrnObs>'
SET @DTEENCA=@DTEENCA+ '<TrnEmail>' + @Email + '</TrnEmail>'

--ADENDAS
SET @DTEENCA=@DTEENCA+ '<TrnCampAd01>0</TrnCampAd01>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd02>0</TrnCampAd02>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd03>0</TrnCampAd03>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd04>0</TrnCampAd04>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd05>0</TrnCampAd05>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd06>0</TrnCampAd06>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd07>0</TrnCampAd07>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd08>0</TrnCampAd08>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd09>0</TrnCampAd09>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd10>0</TrnCampAd10>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd11>0</TrnCampAd11>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd12>0</TrnCampAd12>' 
SET @DTEENCA=@DTEENCA+ '<TrnCampAd13>0</TrnCampAd13>'


----------------------DETALLE

SET @DTEDETA= ''
SET @DTEDETA=@DTEDETA+ '<stdTWSD>'
SET @i = 0
SET @NumeroLinea = 1
SELECT @LINEAS = count(A."VisOrder") 
	FROM inv1 A
	WHERE A.DocEntry = @Docentry;

set @NumeroLinea = 1
set @Ivatotal = 0
set @TotalFinal = 0


--LOOP LINEAS
WHILE @i < @LINEAS 
	BEGIN


select
	@Descripcion = A.Dscription,
	@ItemCode = isnull(A.ItemCode,''),
	@Cantidad = CASE WHEN (CASE WHEN A.Quantity <= 0 then 1 else   A.Quantity end) < 0 then 1 else  (CASE WHEN A.Quantity <= 0 then 1 else A.Quantity end) end,
	@PrecioUnitario = CASE WHEN a."TaxCode" = 'EXE' then case when @CodigoMoneda = 'USD' then (("TotalFrgn") / (CASE WHEN a."Quantity" <= 0 then 1 else   a."Quantity" end)) else (("LineTotal") / (CASE WHEN a."Quantity" <= 0 then 1 else   a."Quantity" end)) end else case when @CodigoMoneda = 'USD' then (("TotalFrgn"+ a."VatSumFrgn") / (CASE WHEN a."Quantity" <= 0 then 1 else   a."Quantity" end)) else (("LineTotal"+ a."VatSum") / (CASE WHEN a."Quantity" <= 0 then 1 else   a."Quantity" end)) end end,
	@BienOServicio = isnull(A.U_Tipo,'S')
FROM PCH1 A
	INNER JOIN OPCH C
	ON A."DocEntry"=C."DocEntry" 
	WHERE A."DocEntry" = @Docentry AND A."VisOrder" = @i
	group by a."ItemCode",a."LineTotal",a."VatSum",a."Currency",a."TotalFrgn",a."VatSumFrgn",a."Quantity",a."Dscription",a."TaxCode",a."PriceBefDi","INMPrice", a.U_Tipo;

	
		SET @DTEDETA=@DTEDETA+ '<stdTWS.stdTWSCIt.stdTWSDIt>' 
		SET @DTEDETA=@DTEDETA+ '<TrnLiNum>' + @NumeroLinea + '</TrnLiNum>' 
		SET @DTEDETA=@DTEDETA+ '<TrnArtCod>' + @ItemCode + '</TrnArtCod>' 
		SET @DTEDETA=@DTEDETA+ '<TrnArtNom>' + @Descripcion + '</TrnArtNom>' 
		SET @DTEDETA=@DTEDETA+ '<TrnCan>' + convert(varchar,@Cantidad) + '</TrnCan>' 
		SET @DTEDETA=@DTEDETA+ '<TrnVUn>' + convert(varchar,@PrecioUnitario) + '</TrnVUn>' 
		SET @DTEDETA=@DTEDETA+ '<TrnUniMed>UNI</TrnUniMed>' 
		SET @DTEDETA=@DTEDETA+ '<TrnVDes>0</TrnVDes>' 
		SET @DTEDETA=@DTEDETA+ '<TrnArtBienSer>' + @BienOServicio + '</TrnArtBienSer>' 
		SET @DTEDETA=@DTEDETA+ '<TrnArtImpAdiCod>0</TrnArtImpAdiCod>' 
		SET @DTEDETA=@DTEDETA+ '<TrnArtImpAdiUniGrav>0</TrnArtImpAdiUniGrav>' 
		SET @DTEDETA=@DTEDETA+ '</stdTWS.stdTWSCIt.stdTWSDIt>'
		
		SET @NumeroLinea = @NumeroLinea + 1
		SET @Ivatotal = @Ivatotal + @IMontoImpuesto;
		SET @TotalFinal = @TotalFinal + @Total;
		SET @i = @i+1;

	END

SET @DTEDETA=@DTEDETA+'</stdTWSD>'

--SET @DTEDETA=@DTEDETA+'<stdTWSCam>'
--SET @DTEDETA=@DTEDETA+'<stdTWS.stdTWSCam.stdTWSCamIt>'
--SET @DTEDETA=@DTEDETA+'<TrnAbonoNum>1</TrnAbonoNum>'
--SET @DTEDETA=@DTEDETA+'<TrnAbonoFecVen>' + @FechaHoraEmision + '</TrnAbonoFecVen>'
--SET @DTEDETA=@DTEDETA+'<TrnAbonoMonto>' + convert(varchar,@Total) + '</TrnAbonoMonto>'
--SET @DTEDETA=@DTEDETA+'</stdTWS.stdTWSCam.stdTWSCamIt>'
--SET @DTEDETA=@DTEDETA+'</stdTWSCam>'

SET @DTEDETA=@DTEDETA+'</stdTWS>';


SET @DTEENCA =@DTEENCA+@DTEDETA
SET @RESULT=@ENCODIGN+@DTEENCA
SELECT  @RESULT XML_GENERADO
GO

