USE [SBO_DENARIUM_GUATEMALA]
GO

/****** Object:  StoredProcedure [dbo].[FELONE_UTILS]    Script Date: 8/4/2022 5:01:50 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








CREATE procedure [dbo].[FELONE_UTILS]

@opcion NVARCHAR(100), 
@param1 NVARCHAR(1000),
@param2 NVARCHAR(1000),
@param3 NVARCHAR(1000),
@param4 NVARCHAR(1000),
@param5 NVARCHAR(1000),
@param6 NVARCHAR(1000),
@param7 NVARCHAR(1000),
@param8 NVARCHAR(1000),
@param9 NVARCHAR(1000),
@param10 NVARCHAR(1000)

AS
--Forma de llamada
--"CALL FELONE_UTILS('OPCION','','','','','','','',''.'','')"
BEGIN

----------------------------------------------------------TIPO DE DOCUMENTOS A UTILIZAR
	IF (@opcion = 'TipoDocumentos') 
	BEGIN
		delete  [@FEL_TIPODOC];

		insert into [@FEL_TIPODOC] (code,lineid,u_codigo,u_descripcion) values(1,1,'FACT','Facturas');
		insert into [@FEL_TIPODOC] (code,lineid,u_codigo,u_descripcion) values(2,2,'FEXP','Factura Exportacion');
		insert into [@FEL_TIPODOC] (code,lineid,u_codigo,u_descripcion) values(3,3,'FCAM','Factura Cambiaria');
		insert into [@FEL_TIPODOC] (code,lineid,u_codigo,u_descripcion) values(4,4,'FESP','Factura Especial');
		insert into [@FEL_TIPODOC] (code,lineid,u_codigo,u_descripcion) values(5,5,'NCRE','Nota de Credito');
		insert into [@FEL_TIPODOC] (code,lineid,u_codigo,u_descripcion) values(6,6,'NDEB','Nota de Debito');
		insert into [@FEL_TIPODOC] (code,lineid,u_codigo,u_descripcion) values(7,7,'NABN','Nota de abono');

	END

IF (@opcion = 'Empresa') 
	BEGIN
		SELECT 'INFILE' as "Certificador", CompnyName as "Empresa" from OADM;
	END

/***** CONDICION 1 *****/
	IF (@opcion = 'ExistField') 
		SELECT "TableID","FieldID","AliasID" FROM "CUFD" WHERE "TableID"= @param1 AND "AliasID"  = @param2;

/***** CONDICION 2 *****/
	IF (@opcion = 'Delete') 
		DELETE FROM "@FEL_TIPODOC";
	
/***** CONDICION 3 y 4 *****/
	IF (@opcion = 'AddDocument1') 
		SELECT * FROM "@FEL_TIPODOC" WHERE "U_CODIGO"= @param1 AND "Code"= @param2;
	
	IF (@opcion = 'AddDocument2') 
		SELECT * FROM "@FEL_TIPODOC" WHERE "U_CODIGO"= @param1;
	
/***** CONDICION 5 *****/
	IF (@opcion = 'SELECTFEL') 
		SELECT * FROM "@FEL_PARAMETROS";
	
/***** CONDICION 6 *****/
	IF (@opcion = 'FELPARAMETROS') 
		IF(@param1 = 'G4S')
			Begin
				insert into "@FEL_PARAMETROS" values('Requestor',1,-3,1,'Requestor',null);
                insert into "@FEL_PARAMETROS" values('Correo',2,-3,2,'Correo',null);
                insert into "@FEL_PARAMETROS" values('Nemi',3,-3,3,'Nemi',null);
				insert into "@FEL_PARAMETROS" values('NitEmi',4,-3,4,'NitEmi',null);
                insert into "@FEL_PARAMETROS" values('Country',5,-3,5,'Country',null);
                insert into "@FEL_PARAMETROS" values('PATHPDF',6,-3,6,'PATHPDF',null);
				insert into "@FEL_PARAMETROS" values('PATHXML',7,-3,7,'PATHXML',null);
                insert into "@FEL_PARAMETROS" values('PATHXMLaut',8,-3,8,'PATHXMLaut',null);
                insert into "@FEL_PARAMETROS" values('Entity',9,-3,9,'Entity',null);
				insert into "@FEL_PARAMETROS" values('UserName',10,-3,10,'UserName',null);
                insert into "@FEL_PARAMETROS" values('PATHXMLerr',11,-3,11,'PATHXMLerr',null);
                insert into "@FEL_PARAMETROS" values('PATHXMLres',12,-3,12,'PATHXMLres',null);
				insert into "@FEL_PARAMETROS" values('Tafilia',13,-3,13,'Tafilia',null);
                insert into "@FEL_PARAMETROS" values('Data1',14,-3,14,'Data1',null);
                insert into "@FEL_PARAMETROS" values('Data3',15,-3,15,'Data3',null);
				insert into "@FEL_PARAMETROS" values('URL_WS',16,-3,16,'URL_WS',null);
                insert into "@FEL_PARAMETROS" values('Trans',17,-3,17,'Trans',null);
			END
		Else
			Begin
				insert into "@FEL_PARAMETROS" values('ApiKey',1,-3,1,'ApiKey',null);
                insert into "@FEL_PARAMETROS" values('Correo',2,-3,2,'Correo',null);
                insert into "@FEL_PARAMETROS" values('Nemi',3,-3,3,'Nemi',null);
				insert into "@FEL_PARAMETROS" values('NitEmi',4,-3,4,'NitEmi',null);
                insert into "@FEL_PARAMETROS" values('PASSDB',5,-3,5,'PASSDB',null);
                insert into "@FEL_PARAMETROS" values('PATHPDF',6,-3,6,'PATHPDF',null);
				insert into "@FEL_PARAMETROS" values('PATHXML',7,-3,7,'PATHXML',null);
                insert into "@FEL_PARAMETROS" values('PATHXMLaut',8,-3,8,'PATHXMLaut',null);
                insert into "@FEL_PARAMETROS" values('PATHXMLc',9,-3,9,'PATHXMLc',null);
				insert into "@FEL_PARAMETROS" values('PATHXMLcp',10,-3,10,'PATHXMLcp',null);
                insert into "@FEL_PARAMETROS" values('PATHXMLerr',11,-3,11,'PATHXMLerr',null);
                insert into "@FEL_PARAMETROS" values('PATHXMLres',12,-3,12,'PATHXMLres',null);
				insert into "@FEL_PARAMETROS" values('Tafilia',13,-3,13,'Tafilia',null);
                insert into "@FEL_PARAMETROS" values('UR_a',14,-3,14,'UR_a',null);
                insert into "@FEL_PARAMETROS" values('UR_p',15,-3,15,'UR_p',null);
				insert into "@FEL_PARAMETROS" values('UR_r',16,-3,16,'UR_r',null);
                insert into "@FEL_PARAMETROS" values('UR_t',17,-3,17,'UR_t',null);
                insert into "@FEL_PARAMETROS" values('USRDB',18,-3,18,'USRDB',null);
			END
	
	
	IF ( @opcion='TipoDoc') 
		insert into "@FEL_TIPODOC" ("Code","LineId","U_CODIGO","U_DESCRIPCION") values(@param1,@param2,@param3,@param4);
	
	
	/***** CONDICION 7 *****/
	IF (@opcion = 'DELETERESOLUCION') 
		DELETE FROM "@FEL_RESOLUCION";
	
	/***** CONDICION 8 *****/
	IF (@opcion = 'LLENAGRID') 
		select "U_CODIGO","U_DESCRIPCION" from "@FEL_TIPODOC";
	
	
	/***** CONDICION 10 *****/
	IF (@opcion = 'EXISTEPARAMETRO') 
		select * from "@FEL_PARAMETROS" where "U_PARAMETRO" = @param1;
	
	
	/***** CONDICION 9 *****/
	IF (@opcion = 'LLENASERIES') 
	BEGIN
		select A."Series", A."SeriesName" +' ('+ CASE WHEN A."ObjectCode" = 13 and A."DocSubType" != 'DN'  THEN  
		'Factura' WHEN A."ObjectCode" =  14   THEN
		'Nota Credito' WHEN A."ObjectCode" = 18  THEN
		'Factura Proveedor'WHEN A."ObjectCode" = 4   THEN
		'Manual' WHEN A."ObjectCode" = 2 THEN
		'Manual' WHEN  A."ObjectCode" = 13 and A."DocSubType" = 'DN'   THEN
		'Nota Debito'  END+')' as "SeriesName" 
		from NNM1 A  
		inner join "@FEL_RESOLUCION" B  on A."Series"=B."U_SERIE" 
		where isnull(B."U_ES_BATCH",'N')='Y';
	END
	
	
		IF (@opcion = 'LLENASERIESA') 
		BEGIN
		select A."Series", A."SeriesName" +' ('+ CASE WHEN A."ObjectCode" = 13 and A."DocSubType" != 'DN'    THEN
		'Factura' WHEN A."ObjectCode" =  14   THEN
		'Nota Credito' WHEN A."ObjectCode" = 18  THEN
		'Factura Proveedor'WHEN A."ObjectCode" = 4   THEN
		'Manual' WHEN A."ObjectCode" = 2 THEN
		'Manual' WHEN  A."ObjectCode" = 13 and A."DocSubType" = 'DN'   THEN
		'Nota Debito'  END+')' as "SeriesName" 
		from NNM1 A  
		inner join "@FEL_RESOLUCION" B  on A."Series"=B."U_SERIE" ;
		END
		--where isnull(B."U_ES_BATCH",'N')='Y';
		
	
	
	/***** CONDICION 11 *****/
	IF (@opcion = 'UPDATEFEL') 
		update "@FEL_PARAMETROS" set "U_VALOR"= @param1 where "U_PARAMETRO"= @param2;
	
	
	/***** OPCION 17 *****/
	IF (@opcion = 'ValidaDocumentoFCND') 
		select "U_ESTADO_FACE" from OINV WHERE  "DocEntry"=@param1 and isnull("U_ESTADO_FACE",'P') IN ('A','ANULAR','ANULADO');
	
/***** OPCION 18 *****/
	IF (@opcion = 'ValidaDocumentoNC') 
		select "U_ESTADO_FACE" from ORIN WHERE  "DocEntry"=@param1 and isnull("U_ESTADO_FACE",'P') IN ('A','ANULAR','ANULADO');
	
/***** OPCION 19 *****/
	IF (@opcion = 'ValidaDocumentoFACP') 
		select "U_ESTADO_FACE" from OPCH WHERE  "DocEntry"=@param1 and isnull("U_ESTADO_FACE",'P') IN ('A','ANULAR','ANULADO');
		
	
	/***** OPCION 20 *****/
	IF (@opcion = 'SerieEsBatch') 
		SELECT isnull("U_ES_BATCH",'N') FROM "@FEL_RESOLUCION" WHERE "Code" = @param1;
	
	
	/***** OPCION 56 *****/
	IF (@opcion = 'ValidaSerie') 
		select * from "@FEL_RESOLUCION" where "U_SERIE" = @param1 AND isnull("U_ES_BATCH",'0') = '0';
		
/***** OPCION 57 *****/
	IF (@opcion = 'ValidaSerieBATCH') 
		select * from "@FEL_RESOLUCION" where "U_SERIE" = @param1;		
		
	
/***** OPCION 59 *****/
	IF (@opcion = 'ExisteDocumentoND') 
		select COUNT("DocEntry") from "ORIN" WHERE  "DocEntry" =@param1 and "DocSubType" ='DN';
			
	/***** OPCION 59 *****/
	IF (@opcion = 'ExisteDocumentoNC') 
		select COUNT("DocEntry") from "ORIN" WHERE  "DocEntry" =@param1;
		
		----------------------------------------------------------LLENA LISTADO BATCH DE DOCUMENTOS
		IF (@opcion = 'LISTADOBATCH') 
		BEGIN
			select Estado=case isnull(a.U_ESTADO_FACE,'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end ,
			DocNum 'No. SAP',
			convert(char(10),DocDate,103)  'Fecha Documento' ,
			CardName  'Cliente',
			concat('QTZ  ',convert(numeric(18,2),DocTotal,1))  'Total Documento', 
			a.docentry '# DocEntry'
			from oinv a 
			inner join NNM1 b 
			on a.Series = b.Series 
			where isnull(U_ESTADO_FACE,'P') in ('P','R')  
			and a.docdate between @param2 and  @param3
			and   b.Series = @param1 
			union 
			select Estado=case isnull(a.U_ESTADO_FACE,'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end , 
			DocNum 'No. SAP',
			convert(char(10),DocDate,103)  'Fecha Documento' ,
			CardName  'Cliente',
			concat('QTZ  ',convert(numeric(18,2),DocTotal,1))  'Total Documento', 
			a.docentry '# DocEntry'   
			from ORIN  a 
			inner join NNM1 b 
			on a.Series = b.Series  
			where isnull(U_ESTADO_FACE,'P') in ('P','R')  
			and a.docdate between @param2 and  @param3
			and   b.Series = @param1 
			union 
			select Estado=case isnull(a.U_ESTADO_FACE,'P') when 'P' then 'Pendiente' when 'R' then 'Rechazado' when 'A' then 'Autorizado' end , 
			DocNum 'No. SAP',
			convert(char(10),DocDate,103)  'Fecha Documento' ,
			CardName  'Cliente',
			concat('QTZ  ',convert(numeric(18,2),DocTotal,1))  'Total Documento', 
			a.docentry '# DocEntry'   
			from OPCH  a 
			inner join NNM1 b 
			on a.Series = b.Series  
			where isnull(U_ESTADO_FACE,'P') in ('P','R')  
			and a.docdate between @param2 and  @param3
			and   b.Series = @param1 
			order by DocNum;
		END
		--where isnull(B."U_ES_BATCH",'N')='Y';
		
	
	----------------------------------------------------------LLENA LISTADO ANULACION BATCH DE DOCUMENTOS
		IF (@opcion = 'LISTADOBATCHA') 
		BEGIN
			select Estado=case isnull(a.U_ESTADO_FACE,'ANULAR') when 'ANULAR' then 'Pendiente'  when 'ANULADO' then 'Autorizado' end ,
	DocNum 'No. SAP',
	convert(char(10),DocDate,103)  'Fecha Documento' ,
	CardName  'Cliente',
	concat('QTZ  ',convert(numeric(18,2),DocTotal,1))  'Total Documento', 
	a.docentry '# DocEntry'
	from oinv a 
	inner join NNM1 b 
	on a.Series = b.Series 
	where U_ESTADO_FACE in ('ANULAR')  
	and a.docdate between @param2 and  @param3
			and   b.Series = @param1 
	union 
	select Estado=case isnull(a.U_ESTADO_FACE,'ANULAR') when 'ANULAR' then 'Pendiente'  when 'ANULADO' then 'Autorizado' end ,
	DocNum 'No. SAP',
	convert(char(10),DocDate,103)  'Fecha Documento' ,
	CardName  'Cliente',
	concat('QTZ  ',convert(numeric(18,2),DocTotal,1))  'Total Documento', 
	a.docentry '# DocEntry'  
	from ORIN  a 
	inner join NNM1 b 
	on a.Series = b.Series  
	where U_ESTADO_FACE in ('ANULAR')   
	and a.docdate between @param2 and  @param3
			and   b.Series = @param1 
		union 
			select Estado=case isnull(a.U_ESTADO_FACE,'ANULAR') when 'ANULAR' then 'Pendiente'  when 'ANULADO' then 'Autorizado' end ,
			DocNum 'No. SAP',
			convert(char(10),DocDate,103)  'Fecha Documento' ,
			CardName  'Cliente',
			concat('QTZ  ',convert(numeric(18,2),DocTotal,1))  'Total Documento', 
			a.docentry '# DocEntry'   
			from OPCH  a 
			inner join NNM1 b 
			on a.Series = b.Series  
			where U_ESTADO_FACE in ('ANULAR')    
			and a.docdate between @param2 and  @param3
			and   b.Series = @param1 
			order by DocNum;
		END
		--where isnull(B."U_ES_BATCH",'N')='Y';

----------------------------------------------------------- TRUE CERTIFICIACION

	IF (@opcion = 'True')
	BEGIN
		if @param2 = 'FACT' OR @param2 = 'RDON' OR @param2 = 'FCAM' OR @param2 = 'RECI' OR @param2 = 'FEXP' OR @param2 = 'NDEB'
		BEGIN
			update "OINV"  set "U_ESTADO_FACE" ='A', "U_FIRMA_ELETRONICA"= @param3, "U_NUMERO_DOCUMENTO"= @param4, "U_SERIE_FACE"= @param5, "U_FACE_PDFFILE"= @param6, "U_FECHA_CERT_FACE" = @param7, "U_FECHA_ENVIO_FACE" = @param8, "U_MOTIVO_RECHAZO"= '' WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'NCRE' OR @param2 = 'NABN'
		BEGIN
			update "ORIN"  set "U_ESTADO_FACE" ='A', "U_FIRMA_ELETRONICA"= @param3, "U_NUMERO_DOCUMENTO"= @param4, "U_SERIE_FACE"= @param5, "U_FACE_PDFFILE"= @param6, "U_FECHA_CERT_FACE" = @param7, "U_FECHA_ENVIO_FACE" = @param8, "U_MOTIVO_RECHAZO" = '' WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'FESP' 
		BEGIN
			update "OPCH"  set "U_ESTADO_FACE" ='A', "U_FIRMA_ELETRONICA"= @param3, "U_NUMERO_DOCUMENTO"= @param4, "U_SERIE_FACE"= @param5, "U_FACE_PDFFILE"= @param6, "U_FECHA_CERT_FACE" = @param7, "U_FECHA_ENVIO_FACE" = @param8, "U_MOTIVO_RECHAZO" = '' WHERE "DocEntry"=@param1;
		END
	END

	---------------------------------------------------------- ERROR WEB
	IF (@opcion = 'TrueError') 
	BEGIN
		if @param2 = 'FACT' OR @param2 = 'RDON' OR @param2 = 'FCAM' OR @param2 = 'RECI' OR @param2 = 'FEXP' OR @param2 = 'NDEB'
		BEGIN
			update "OINV"  set "U_ESTADO_FACE" ='A', "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'NCRE' OR @param2 = 'NABN'
		BEGIN
			update "ORIN"  set "U_ESTADO_FACE" ='A',  "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'FESP' 
		BEGIN
			update "OPCH"  set "U_ESTADO_FACE" ='A',  "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
	END
---------------------------------------------------------- FIRMA NO AUTORIZADA

	IF (@opcion = 'False') 
	BEGIN
		if @param2 = 'FACT' OR @param2 = 'RDON' OR @param2 = 'FCAM' OR @param2 = 'RECI' OR @param2 = 'FEXP' OR @param2 = 'NDEB'
		BEGIN
			update "OINV"  set "U_ESTADO_FACE" ='R', "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'NCRE' OR @param2 = 'NABN'
		BEGIN
			update "ORIN"  set "U_ESTADO_FACE" ='R',  "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'FESP' 
		BEGIN
			update "OPCH"  set "U_ESTADO_FACE" ='R',  "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
	END	


	---------------------------------------------------------- ANULACION ERROR

	IF (@opcion = 'FalseA') 
	BEGIN
		if @param2 = 'FACT' OR @param2 = 'RDON' OR @param2 = 'FCAM' OR @param2 = 'RECI' OR @param2 = 'FEXP' OR @param2 = 'NDEB'
		BEGIN
			update "OINV"  set "U_ESTADO_FACE" ='ANULAR', "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'NCRE' OR @param2 = 'NABN'
		BEGIN
			update "ORIN"  set "U_ESTADO_FACE" ='ANULAR',  "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'FESP' 
		BEGIN
			update "OPCH"  set "U_ESTADO_FACE" ='ANULAR',  "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
	END	

	---------------------------------------------------------- ERROR WEB ANULACION
	IF (@opcion = 'TrueErrorA') 
	BEGIN
		if @param2 = 'FACT' OR @param2 = 'RDON' OR @param2 = 'FCAM' OR @param2 = 'RECI' OR @param2 = 'FEXP' OR @param2 = 'NDEB'
		BEGIN
			update "OINV"  set "U_ESTADO_FACE" ='ANULADO', "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'NCRE' OR @param2 = 'NABN'
		BEGIN
			update "ORIN"  set "U_ESTADO_FACE" ='ANULADO',  "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'FESP' 
		BEGIN
			update "OPCH"  set "U_ESTADO_FACE" ='ANULADO',  "U_MOTIVO_RECHAZO" = @param3 WHERE "DocEntry"=@param1;
		END
	END

	---------------------------------------------------------- ANULADA AUTORIZADA
	IF (@opcion = 'TrueA')
	BEGIN
		if @param2 = 'FACT' OR @param2 = 'RDON' OR @param2 = 'FCAM' OR @param2 = 'RECI' OR @param2 = 'FEXP' OR @param2 = 'NDEB'
		BEGIN
			update "OINV"  set "U_ESTADO_FACE" ='ANULADO', "U_FIRMA_ELETRONICA"= @param3, "U_MOTIVO_RECHAZO" = '', "U_FACE_PDFFILE"= @param6 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'NCRE' OR @param2 = 'NABN'
		BEGIN
			update "ORIN"  set "U_ESTADO_FACE" ='ANULADO', "U_FIRMA_ELETRONICA"= @param3, "U_MOTIVO_RECHAZO" = '', "U_FACE_PDFFILE"= @param6 WHERE "DocEntry"=@param1;
		END
		ELSE if @param2 = 'FESP' 
		BEGIN
			update "OPCH"  set "U_ESTADO_FACE" ='ANULADO', "U_FIRMA_ELETRONICA"= @param3, "U_MOTIVO_RECHAZO" = '', "U_FACE_PDFFILE"= @param6 WHERE "DocEntry"=@param1;
		END
	END













	
	--IF (@opcion = 'TrueFACTA') 
	--	update "OINV"  set "U_ESTADO_FACE" ='ANULADO' WHERE "DocEntry"=@param1;
			
	--IF (@opcion = 'TrueFESPA') 
	--	update "OPCH"  set "U_ESTADO_FACE" ='ANULADO' WHERE "DocEntry"=@param1;
		
	--	IF (@opcion = 'TrueNDEBA') 
	--	update "OINV"  set "U_ESTADO_FACE" ='ANULADO' WHERE "DocEntry"=@param1;
		
	--	IF (@opcion = 'TrueNCREA') 
	--	update "ORIN"  set "U_ESTADO_FACE" ='ANULADO' WHERE "DocEntry"=@param1;
	
	--/***** OPCION 54 *****/
	--IF (@opcion = 'FalseFACT') 
	--	update OINV set "U_ESTADO_FACE" ='R', "U_MOTIVO_RECHAZO"=@param1 WHERE "DocEntry"=@param2;
		
	--IF (@opcion = 'FalseFESP') 
	--	update "OPCH" set "U_ESTADO_FACE" ='R', "U_MOTIVO_RECHAZO"=@param1 WHERE "DocEntry"=@param2;
			
	--	IF (@opcion = 'FalseNDEB') 
	--	update "OINV" set "U_ESTADO_FACE" ='R', "U_MOTIVO_RECHAZO"=@param1 WHERE "DocEntry"=@param2;
		
	--	IF (@opcion = 'FalseNCRE') 
	--	update "ORIN" set "U_ESTADO_FACE" ='R', "U_MOTIVO_RECHAZO"=@param1 WHERE "DocEntry"=@param2;
		
	
	--IF (@opcion = 'FalseFACTA') 
	--	update "OINV" set "U_ESTADO_FACE" ='ANULAR', "U_MOTIVO_RECHAZO"=@param1 WHERE "DocEntry"=@param2;
		
	--IF (@opcion = 'FalseFESPA') 
	--	update "OPCH" set "U_ESTADO_FACE" ='ANULAR', "U_MOTIVO_RECHAZO"=@param1 WHERE "DocEntry"=@param2;
			
	--	IF (@opcion = 'FalseNDEBA') 
	--	update "OINV" set "U_ESTADO_FACE" ='ANULAR', "U_MOTIVO_RECHAZO"=@param1 WHERE "DocEntry"=@param2;
		
	--	IF (@opcion = 'FalseNCREA') 
	--	update "ORIN" set "U_ESTADO_FACE" ='ANULAR', "U_MOTIVO_RECHAZO"=@param1 WHERE "DocEntry"=@param2;
	
END;
GO
