#HIS Gateway Table Schema


-- Create Table ###############################################################################################

-- SEQUENCE: public.shis_upload_log_logid_seq

-- DROP SEQUENCE IF EXISTS public.shis_upload_log_logid_seq;

CREATE SEQUENCE IF NOT EXISTS public.shis_upload_log_logid_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.shis_upload_log_logid_seq
    OWNER TO hisuser;

-- 2023.12.12 update by 1050325 
-- FUNCTION: public.tr_shis_upload_log()

-- DROP FUNCTION IF EXISTS public.tr_shis_upload_log();

CREATE OR REPLACE FUNCTION public.tr_shis_upload_log()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
DECLARE
	logid shis_upload_log.logid%TYPE;
BEGIN	
	IF NEW.logid IS null OR NEW.logid =-1
	THEN
	NEW.logid :=  nextval('shis_upload_log_logid_seq'::regclass);
	END IF;
	RETURN NEW;
END;
$BODY$;

ALTER FUNCTION public.tr_shis_upload_log()
    OWNER TO hisuser;


-- Table: public.shis_upload_log

-- DROP TABLE IF EXISTS public.shis_upload_log;

CREATE TABLE IF NOT EXISTS public.shis_upload_log
(
    logid bigint NOT NULL,
    reg_date date,
    inhospid character varying COLLATE pg_catalog."default",
    exec_datetime timestamp without time zone,
    option "char",
    result_success boolean,
    result_message text COLLATE pg_catalog."default",
    result_status_code character varying COLLATE pg_catalog."default",
    result_status_desc character varying COLLATE pg_catalog."default",
    target_url character varying COLLATE pg_catalog."default",
    target_agency character varying COLLATE pg_catalog."default",
    local_ip character varying COLLATE pg_catalog."default",
    local_login_user character varying COLLATE pg_catalog."default",
    exec_batch_seq_no smallint,
    CONSTRAINT shis_upload_log_pkey PRIMARY KEY (logid)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.shis_upload_log
    OWNER to hisuser;
-- Index: idx_inhospid

-- DROP INDEX IF EXISTS public.idx_inhospid;

CREATE INDEX IF NOT EXISTS idx_inhospid
    ON public.shis_upload_log USING btree
    (inhospid COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_result_other_cols1

-- DROP INDEX IF EXISTS public.idx_result_other_cols1;

CREATE INDEX IF NOT EXISTS idx_result_other_cols1
    ON public.shis_upload_log USING btree
    (exec_datetime ASC NULLS LAST, result_success ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_result_other_cols2

-- DROP INDEX IF EXISTS public.idx_result_other_cols2;

CREATE INDEX IF NOT EXISTS idx_result_other_cols2
    ON public.shis_upload_log USING btree
    (reg_date ASC NULLS LAST, result_success ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_result_success

-- DROP INDEX IF EXISTS public.idx_result_success;

CREATE INDEX IF NOT EXISTS idx_result_success
    ON public.shis_upload_log USING btree
    (result_success ASC NULLS LAST)
    TABLESPACE pg_default;

-- Trigger: tr_shis_upload_log

-- DROP TRIGGER IF EXISTS tr_shis_upload_log ON public.shis_upload_log;

CREATE TRIGGER tr_shis_upload_log
    BEFORE INSERT
    ON public.shis_upload_log
    FOR EACH ROW
    EXECUTE FUNCTION public.tr_shis_upload_log();


-- Alter Table ###############################################################################################
--## 注意: 病歷主檔與掛號檔新增上傳欄位 ##
--## Upload_Status :   Y已上傳 、N待上傳

ALTER TABLE shis_chart
ADD upload_status character varying(1) COLLATE pg_catalog."default" DEFAULT 'N',
ADD upload_time timestamp without time zone;


ALTER TABLE registration
ADD upload_status character varying(1) COLLATE pg_catalog."default" DEFAULT 'N',
ADD upload_time timestamp without time zone;

ALTER TABLE public."shis_chart_MergeHistory" 
ADD COLUMN upload_status CHARACTER VARYING(1) DEFAULT 'N',
ALTER TABLE public."shis_chart_MergeHistory" 
ADD COLUMN upload_time TIMESTAMP WITHOUT TIME ZONE;

-- Update Data ###############################################################################################
--## 注意: 初次使用才須調整 ##
UPDATE public.registration
	SET  upload_status='N', upload_time= null

UPDATE public.shis_chart
	SET  upload_status='N', upload_time= null


-- Insert Data ###############################################################################################
--## 注意: ref_name 請於部屬後改入正式Server Url ##

--JWT驗證--JWT Auth--
INSERT INTO public.shis_coderef(
	ref_codetype, ref_code, ref_name, ref_des, ref_id, ref_casetype, ref_showseq, ref_des2, modify_id, modify_time, ref_default_flag)
	VALUES ('MOHD_TK','JWT','https://localhost:7287/api/auth/jwtlogin','JWT Auth','00000000000000236','Y',1,',
https://localhost:7287/api/auth/jwtlogin','1050325','2023-11-16',' ');
--門急診每日上傳--OPD and ER daily upload--
INSERT INTO public.shis_coderef(
	ref_codetype, ref_code, ref_name, ref_des, ref_id, ref_casetype, ref_showseq, ref_des2, modify_id, modify_time, ref_default_flag)
	VALUES ('MOHD_TK','TK_01','https://localhost:7287/api/UploadTask/MOHD_TK_01','OPD and ER daily upload','00000000000000235','Y',1,',
https://localhost:7287/api/UploadTask/MOHD_TK_01','1050325','2023-11-16',' ');
--病人基本資訊每日上傳--SHIS_CHART daily upload--
INSERT INTO public.shis_coderef(
	ref_codetype, ref_code, ref_name, ref_des, ref_id, ref_casetype, ref_showseq, ref_des2, modify_id, modify_time, ref_default_flag)
	VALUES ('MOHD_TK','TK_02','https://localhost:7287/api/UploadTask/MOHD_TK_02','SHIS_CHART daily upload','00000000000000234','Y',1,',
https://localhost:7287/api/UploadTask/MOHD_TK_02','1050325','2023-11-16',' ');
--病歷合併每日上傳--Merged or Undo SHIS_CHART daily upload--
INSERT INTO public.shis_coderef(
	ref_codetype, ref_code, ref_name, ref_des, ref_id, ref_casetype, ref_showseq, ref_des2, modify_id, modify_time, ref_default_flag)
	VALUES ('MOHD_TK','TK_03','https://localhost:7287/api/UploadTask/MOHD_TK_03','Merged or Undo SHIS_CHART daily upload','00000000000000233','Y',1,',
https://localhost:7287/api/UploadTask/MOHD_TK_03','1050325','2023-11-16',' ');

--## 注意: gateway帳號密碼是否與MOHD建立一致.(以HGH做範例，各院區依照需求自行建立)

INSERT INTO public.shis_users(
	user_idno, user_password, user_name_midname, user_birth_date, user_sex, start_date, end_date, user_mobile_phone, user_email, creator, create_time, user_name_firstname, user_name_lastname, user_category, account_status)
	VALUES ('HGH2023','HGHGW!246','Hargeisa Hospital ','2023-11-13 00:00:00','X',NULL,NULL,'+252 2 523114','HGH@smll.gov.tw','MOHDGW','2023-11-13 00:00:00','Somaliland','Gateway System','2','1');




