USE [SBO_DENARIUM_GUATEMALA]
GO

/****** Object:  StoredProcedure [dbo].[FELONE_ECOFACTURAS_FEXP]    Script Date: 8/4/2022 5:00:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[FELONE_ECOFACTURAS_FEXP] (@DOCENTRY INT,@DECIMAL INT)
AS

DECLARE @DTEENCA VARCHAR(MAX)
DECLARE @DTEDETA VARCHAR(MAX)
DECLARE @DTECOMPLEMENTO VARCHAR(MAX)
DECLARE @DTEADENDA VARCHAR(MAX)
DECLARE @RESULT AS VARCHAR(MAX)
DECLARE @ENCODIGN AS VARCHAR(MAX)

SET @ENCODIGN='<?xml version="1.0"?><stdTWS xmlns="FEL">'

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
DECLARE @CodigoComercial VARCHAR(25)
DECLARE @Incoterm VARCHAR(20)

SELECT DISTINCT			
			@CodigoEstablecimiento = T1.U_DISPOSITIVO,	
			@Tipo = 'FACT',
			@DocNum = t0.DocNum,
			@FechaHoraEmision = convert(varchar, getdate(), 23),
			@CodigoMoneda = CASE t0.DOCCUR WHEN 'QTZ' THEN 'GTQ' ELSE 'USD' END,
			@IDReceptor = CASE 
				when t3.Password = 'C/F' then 'CF'
				when t3.Password = 'cf' then 'CF'
				else isnull(replace(t3.Password,'-',''),'CF') END,
			@RDireccion = isnull(t3.Address,'CIUDAD'),
			@CardCode = t0.CardCode,
			@NombreReceptor = t0.CardName,
			@Comentarios = isnull(t0.Comments,''),
			@Email = isnull(t3.E_Mail,''),
			@Incoterm = t0.U_INCOTERM,
			@Total = t0.DocTotal,
			@Valor1 = '',
			@Valor2 = '',
			@Valor3 = '',
			@Valor4 = '',
			@Valor5 = '',
			@Valor6 = ''
			from OINV t0 
			left outer join [@FEL_RESOLUCION] T1
				on T1.U_SERIE=t0.series
				left outer join [@FEL_PARAMETROS] T2 on t2.LineId = 0
				left outer join  OCRD t3
				on t3.CardCode =t0.CardCode
				INNER JOIN NNM1 on t0.Series =NNM1.Series 
				LEFT OUTER JOIN CRD1
				ON t3.CARDCODE=CRD1.CARDCODE
				left join inv12 t4 on t4.DocEntry = t0.DocEntry
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
SET @DTEENCA = ''
SET @DTEENCA=@DTEENCA+ '<TrnEstNum>' + @CodigoEstablecimiento + '</TrnEstNum>'
SET @DTEENCA=@DTEENCA+ '<TipTrnCod>'+@Tipo+'</TipTrnCod>'
SET @DTEENCA=@DTEENCA+ '<TrnNum>' + @TrnNum + '</TrnNum>'
SET @DTEENCA=@DTEENCA+ '<TrnFec>' + @FechaHoraEmision + '</TrnFec>'
SET @DTEENCA=@DTEENCA+ '<MonCod>' + @CodigoMoneda + '</MonCod>'
SET @DTEENCA=@DTEENCA+ '<TrnBenConNIT>CF</TrnBenConNIT>'
SET @DTEENCA=@DTEENCA+ '<TrnExp>1</TrnExp>'



IF @ICodigoUnidadGravable = 'EXE'
	BEGIN
		SET @Exento = 1
		SET @DTEENCA=@DTEENCA+ '<TrnExento>' + @Exento + '</TrnExento>'
		SET @DTEENCA=@DTEENCA+ '<TrnFraseTipo>4</TrnFraseTipo>'
		SET @DTEENCA=@DTEENCA+ '<TrnEscCod>1</TrnEscCod>'
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
SET @DTEENCA=@DTEENCA+ '<TrnCampAd01>ABCD 01</TrnCampAd01>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd02>ABCD 02</TrnCampAd02>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd03>ABCD 03</TrnCampAd03>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd04>ABCD 04</TrnCampAd04>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd05>ABCD 05</TrnCampAd05>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd06>ABCD 06</TrnCampAd06>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd07>ABCD 07</TrnCampAd07>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd08>ABCD 08</TrnCampAd08>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd09>ABCD 09</TrnCampAd09>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd10>ABCD 10</TrnCampAd10>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd11>ABCD 11</TrnCampAd11>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd12>ABCD 12</TrnCampAd12>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd13>ABCD 13</TrnCampAd13>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd14>ABCD 14</TrnCampAd14>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd15>ABCD 15</TrnCampAd15>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd16>ABCD 16</TrnCampAd16>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd17>ABCD 17</TrnCampAd17>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd18>ABCD 18</TrnCampAd18>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd19>ABCD 19</TrnCampAd19>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd20>ABCD 20</TrnCampAd20>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd21>ABCD 21</TrnCampAd21>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd22>ABCD 22</TrnCampAd22>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd23>ABCD 23</TrnCampAd23>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd24>ABCD 24</TrnCampAd24>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd25>ABCD 25</TrnCampAd25>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd26>ABCD 26</TrnCampAd26>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd27>ABCD 27</TrnCampAd27>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd28>ABCD 28</TrnCampAd28>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd29>ABCD 29</TrnCampAd29>'
SET @DTEENCA=@DTEENCA+ '<TrnCampAd30>ABCD 30</TrnCampAd30>'


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
	@ItemCode = A.ItemCode,
	@Cantidad = CASE WHEN (CASE WHEN A.Quantity <= 0 then 1 else   A.Quantity end) < 0 then 1 else  (CASE WHEN A.Quantity <= 0 then 1 else A.Quantity end) end,
	@PrecioUnitario = CASE WHEN a."TaxCode" = 'EXE' then case when a."Currency" = 'USD' then (("TotalFrgn") / (CASE WHEN a."Quantity" <= 0 then 1 else   a."Quantity" end)) else (("LineTotal") / (CASE WHEN a."Quantity" <= 0 then 1 else   a."Quantity" end)) end else case when a."Currency" = 'USD' then (("TotalFrgn"+ a."VatSumFrgn") / (CASE WHEN a."Quantity" <= 0 then 1 else   a."Quantity" end)) else (("LineTotal"+ a."VatSum") / (CASE WHEN a."Quantity" <= 0 then 1 else   a."Quantity" end)) end end,
	@BienOServicio = isnull(A.U_Tipo,'S')
FROM INV1 A
	INNER JOIN OINV C
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

select @CodigoComercial = U_VALOR from [@FEL_PARAMETROS] where U_PARAMETRO = 'CodCom'

SET @DTEDETA=@DTEDETA+'<stdTWSExp>'
SET @DTEDETA=@DTEDETA+'<stdTWS.stdTWSExp.stdTWSExpIt>'
SET @DTEDETA=@DTEDETA+'<NomConsigODest>' + @NombreReceptor + '</NomConsigODest>' 
SET @DTEDETA=@DTEDETA+'<DirConsigODest>' + @RDireccion + '</DirConsigODest>' 
SET @DTEDETA=@DTEDETA+'<CodConsigODest>' + @CardCode + '</CodConsigODest>' 
SET @DTEDETA=@DTEDETA+'<OtraRef>NA</OtraRef>' 
SET @DTEDETA=@DTEDETA+'<INCOTERM>' + @Incoterm + '</INCOTERM>' 
SET @DTEDETA=@DTEDETA+'<ExpNom>' + @NombreReceptor + '</ExpNom>' 
SET @DTEDETA=@DTEDETA+'<ExpCod>' + @CodigoComercial + '</ExpCod>' 
SET @DTEDETA=@DTEDETA+'</stdTWS.stdTWSExp.stdTWSExpIt>' 
SET @DTEDETA=@DTEDETA+'</stdTWSExp>'


SET @DTEDETA=@DTEDETA+'</stdTWS>'


SET @DTEENCA =@DTEENCA+@DTEDETA
SET @RESULT=@ENCODIGN+@DTEENCA
SELECT  @RESULT XML_GENERADO
GO
