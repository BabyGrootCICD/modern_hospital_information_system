
--Inital Borama DB--
--Step 1 -- Users and Auth --
DELETE FROM shis_auths WHERE user_idno NOT IN ('admin','850079');
TRUNCATE shis_auths_log;

DELETE FROM shis_users WHERE user_idno NOT IN ('admin','850079');
TRUNCATE shis_users_log;

--Step 2 -- Medical Records Related --
TRUNCATE public.shis_chart;
TRUNCATE public.shis_chart_log; 
DELETE FROM public.hisorderplan;
TRUNCATE public.hisordersoa;
TRUNCATE public.physical_sign;

--Step 3 --. Registration Records --
TRUNCATE public.registration;

-- Step 4 -- Prefix of the Medicial ID  --
-- HG: Hargeisa Group Hospital --
-- BO: Borama Regional Hospital --
-- BU: Burao Regional Hospital --
-- BE: Berbera Regional Hospital --
-- GA: Gabiley Gereral Hospital --
SELECT * FROM public.shis_coderef WHERE ref_codetype IN ('ChartNoSerialNo','TestChartNoSerialNo');
UPDATE public.shis_coderef SET ref_code='BO' WHERE ref_codetype ='ChartNoSerialNo';

-- Step 5 --Set sequential number to zero but not include :CodeRef --
SELECT * FROM public.shis_serialpool;
UPDATE public.shis_serialpool  SET serial_no=0 where serial_owner <> 'CodeRef';
SELECT * FROM public.shis_serialpool;

-- Step 6 --Set the mininum number of the CodeRef --
SELECT * FROM public.shis_serialpool;
UPDATE public.shis_serialpool  SET serial_no=300 where serial_owner = 'CodeRef';
SELECT * FROM public.shis_serialpool;

-- Setup the following from HIS System--
-- The clinic_schedule --
TRUNCATE public.clinic_schedule;
-- the Departments --
--TRUNCATE shis_department;
-- SELECT * FROM shis_department;
-- Calling Group, Clinic Room No --
DELETE FROM  shis_coderef WHERE ref_codetype IN ('call_group');
--SELECT * FROM shis_coderef WHERE ref_codetype IN ('call_group');

-- Clear HGH DB，delete unused table:  
DROP TABLE public.transaction_call;
DROP TABLE public.transaction_fee;
-- Clear HGH DB，delete unused trigger function:
DROP FUNCTION public.tr_calllog();
DROP FUNCTION public.tr_registertocall();
-- Clear HGH DB，delete unused sequence: 
DROP SEQUENCE hisorderplan_attr_orderplanid_seq CASCADE;
DROP SEQUENCE hisorderplan_inhospid_seq CASCADE;
DROP SEQUENCE hisorderplan_plan_days_seq CASCADE;
DROP SEQUENCE hisorderplan_seq_no_seq CASCADE;
DROP SEQUENCE hisorderplan_soa_inhospid_seq CASCADE;