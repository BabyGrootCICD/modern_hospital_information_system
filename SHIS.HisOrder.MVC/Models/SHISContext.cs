using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SHIS.HisOrder.MVC.Models;

namespace SHIS.HisOrder.MVC.Models
{
    public partial class SHISContext : DbContext
    {
        public SHISContext()
        {
        }

        public SHISContext(DbContextOptions<SHISContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ClinicSchedule> ClinicSchedules { get; set; }
        public virtual DbSet<Dhis2Disease> Dhis2Diseases { get; set; }
        public virtual DbSet<Hisorderplan> Hisorderplans { get; set; }
        public virtual DbSet<HisorderplanAttr> HisorderplanAttrs { get; set; }
        public virtual DbSet<Hisordersoa> Hisordersoas { get; set; }
        public virtual DbSet<ShisAttribute> ShisAttributes { get; set; }
        public virtual DbSet<ShisAuth> ShisAuths { get; set; }
        public virtual DbSet<ShisAuthsLog> ShisAuthsLogs { get; set; }
        public virtual DbSet<ShisChart> ShisCharts { get; set; }
        public virtual DbSet<ShisChartLog> ShisChartLogs { get; set; }
        public virtual DbSet<ShisCoderef> ShisCoderefs { get; set; }
        public virtual DbSet<ShisCondition> ShisConditions { get; set; }
        public virtual DbSet<ShisDepartment> ShisDepartments { get; set; }
        public virtual DbSet<ShisIcd> ShisIcds { get; set; }
        public virtual DbSet<ShisMedfrequency> ShisMedfrequencies { get; set; }
        public virtual DbSet<ShisMedfrequencyInd> ShisMedfrequencyInds { get; set; }
        public virtual DbSet<ShisMedicine> ShisMedicines { get; set; }
        public virtual DbSet<ShisMedpathway> ShisMedpathways { get; set; }
        public virtual DbSet<ShisNonMedicine> ShisNonMedicines { get; set; }
        public virtual DbSet<ShisProject> ShisProjects { get; set; }
        public virtual DbSet<ShisSerialpool> ShisSerialpools { get; set; }
        public virtual DbSet<ShisUser> ShisUsers { get; set; }
        public virtual DbSet<ShisUsersLog> ShisUsersLogs { get; set; }
        public virtual DbSet<PhysicalSign> PhysicalSigns { get; set; }
        public virtual DbSet<Registration> Registrations { get; set; }
        public virtual DbSet<TransactionCall> TransactionCalls { get; set; }
        public virtual DbSet<TransactionFee> TransactionFees { get; set; }
        public DbSet<SHIS.HisOrder.MVC.Models.shis_chart_MergeHistory> shis_chart_MergeHistory { get; set; }
        public DbSet<SHIS.HisOrder.MVC.Models.SHIS_MergeHistory> SHIS_MergeHistory { get; set; }
        public DbSet<SHIS.HisOrder.MVC.Models.shis_ncd> shis_ncd { get; set; }
        public DbSet<SHIS.HisOrder.MVC.Models.home_physicalsign> home_physicalsign { get; set; }
        public DbSet<SHIS.HisOrder.MVC.Models.shis_mental> shis_mental { get; set; }
        public virtual DbSet<TestResult> testresults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

           // modelBuilder.Entity<TestResult>(entity =>
           // {


           //     entity.HasKey(e => e.testresultid)
           //.HasName("test_result_pkey");

           //     entity.ToTable("test_result");

           //     entity.Property(e => e.testresultid)
           //         .HasColumnName("testresult_id")
           //         .HasComment("id")
           //         .ValueGeneratedOnAdd();


           //     entity.Property(e => e.refbillorder)
           //         .IsRequired()
           //         .HasMaxLength(20)
           //         .HasColumnName("inhospid");


           //     entity.Property(e => e.Billno)
           //         .HasMaxLength(30)
           //         .HasColumnName("billno");

           //     entity.Property(e => e.Billdate)
           //         .HasMaxLength(30)
           //         .HasColumnName("billdate");

           //     entity.Property(e => e.TestGroup)
           //         .IsRequired()
           //         .HasMaxLength(40)
           //         .HasColumnName("test_group");


           //     entity.Property(e => e.Reportedby)
           //         .IsRequired()
           //         .HasMaxLength(40)
           //         .HasColumnName("reported_by");

           //     entity.Property(e => e.Result)
           //        .IsRequired()
           //        .HasMaxLength(40)
           //        .HasColumnName("result");

           //     entity.Property(e => e.TestName)
           //      .IsRequired()
           //      .HasMaxLength(40)
           //      .HasColumnName("test_name");

           //     entity.Property(e => e.Contents)
           //    .IsRequired()
           //    .HasMaxLength(40)
           //    .HasColumnName("contents");

           //     entity.Property(e => e.NvalueMale)
           // .IsRequired()
           // .HasMaxLength(40)
           // .HasColumnName("nvaluemale");

           //     entity.Property(e => e.NvalueFemale)
           //         .IsRequired()
           //         .HasMaxLength(40)
           //         .HasColumnName("nvaluefemale");

           //     entity.Property(e => e.Sufix)
           //         .IsRequired()
           //         .HasMaxLength(40)
           //         .HasColumnName("sufix");

           //     entity.Property(e => e.OriOrder)
           //         .IsRequired()
           //         .HasMaxLength(40)
           //         .HasColumnName("oriorder");


           //     entity.Property(e => e.AttachFile)
           //         .IsRequired()
           //         .HasMaxLength(40)
           //         .HasColumnName("attachfile");

           //     entity.Property(e => e.code)
           //         .IsRequired()
           //         .HasMaxLength(40)
           //         .HasColumnName("code");

           // });



            modelBuilder.Entity<ClinicSchedule>(entity =>
            {
                entity.HasKey(e => new { e.ScheWeek, e.ScheNoon, e.ScheRoom, e.shift })
                    .HasName("clinic_schedule_pkey");

                entity.ToTable("clinic_schedule");

                entity.Property(e => e.ScheWeek)
                    .HasMaxLength(1)
                    .HasColumnName("sche_week")
                    .HasComment("星期別");

                entity.Property(e => e.shift)
               .HasMaxLength(1)
               .HasColumnName("shift")
               .HasComment("星期別");

                entity.Property(e => e.ScheNoon)
                    .HasMaxLength(5)
                    .HasColumnName("sche_noon")
                    .HasComment("午別");

                entity.Property(e => e.ScheRoom)
                    .HasMaxLength(3)
                    .HasColumnName("sche_room")
                    .HasComment("診間號碼");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.ScheCallNo)
                    .HasColumnName("sche_call_no")
                    .HasComment("叫號號碼");

                entity.Property(e => e.ScheCallTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("sche_call_time")
                    .HasComment("Calling Time Update");

                entity.Property(e => e.ScheDoctor)
                    .HasMaxLength(7)
                    .HasColumnName("sche_doctor")
                    .HasComment("醫師職編");

                entity.Property(e => e.ScheDoctorName)
                    .HasMaxLength(200)
                    .HasColumnName("sche_doctor_name")
                    .HasComment("醫師姓名");

                entity.Property(e => e.ScheDptCode)
                    .HasMaxLength(6)
                    .HasColumnName("sche_dpt_code")
                    .HasComment("科別代碼");

                entity.Property(e => e.ScheDptName)
                    .HasMaxLength(200)
                    .HasColumnName("sche_dpt_name")
                    .HasComment("科別名稱");

                entity.Property(e => e.ScheOpenFlag)
                    .HasMaxLength(1)
                    .HasColumnName("sche_open_flag")
                    .HasComment("診次是否開放");

                entity.Property(e => e.ScheRemark)
                    .HasMaxLength(1000)
                    .HasColumnName("sche_remark");
            });

            modelBuilder.Entity<Dhis2Disease>(entity =>
            {
                entity.HasKey(e => e.Dhis2Code)
                    .HasName("DHIS2DISEASES_pkey");

                entity.ToTable("dhis2_diseases");

                entity.Property(e => e.Dhis2Code)
                    .HasColumnName("dhis2_code")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Diseases)
                    .IsRequired()
                    .HasMaxLength(400)
                    .HasColumnName("diseases");

                entity.Property(e => e.ShowSeq).HasColumnName("show_seq");
            });

            modelBuilder.Entity<Hisorderplan>(entity =>
            {
                entity.HasKey(e => e.Orderplanid)
                    .HasName("hisorderplan_pkey");

                entity.ToTable("hisorderplan");

                entity.HasIndex(e => e.HealthId, "idx_hisorderplan_01")
                    .UseCollation(new[] { "C" });

                entity.HasIndex(e => e.Inhospid, "idx_hisorderplan_02")
                    .HasOperators(new[] { "bpchar_pattern_ops" })
                    .UseCollation(new[] { "C" });

                entity.Property(e => e.Orderplanid)
                    .ValueGeneratedNever()
                    .HasColumnName("orderplanid");

                entity.Property(e => e.AddFlag)
                    .HasMaxLength(1)
                    .HasColumnName("add_flag")
                    .HasDefaultValueSql("'N'::bpchar");

                entity.Property(e => e.ChargeStatus)
                    .HasMaxLength(1)
                    .HasColumnName("charge_status");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(7)
                    .HasColumnName("create_user");

                entity.Property(e => e.DcDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("dc_date");

                entity.Property(e => e.DcStatus)
                    .HasMaxLength(1)
                    .HasColumnName("dc_status")
                    .HasDefaultValueSql("'0'::bpchar");

                entity.Property(e => e.DcUser)
                    .HasMaxLength(7)
                    .HasColumnName("dc_user");

                entity.Property(e => e.DoseIndication)
                    .HasMaxLength(20)
                    .HasColumnName("dose_indication");

                entity.Property(e => e.DosePath)
                    .HasMaxLength(20)
                    .HasColumnName("dose_path");

                entity.Property(e => e.ExamLoc)
                    .HasMaxLength(20)
                    .HasColumnName("exam_loc");

                entity.Property(e => e.ExecDateFrom)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("exec_date_from");

                entity.Property(e => e.ExecDateTo)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("exec_date_to");

                entity.Property(e => e.ExecStatus)
                    .HasMaxLength(1)
                    .HasColumnName("exec_status");

                entity.Property(e => e.FreeCharge)
                    .HasMaxLength(1)
                    .HasColumnName("free_charge")
                    .HasDefaultValueSql("'N'::bpchar");

                entity.Property(e => e.FreqCode)
                    .HasMaxLength(20)
                    .HasColumnName("freq_code");

                entity.Property(e => e.HealthId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("health_id")
                    .IsFixedLength();

                entity.Property(e => e.HplanType)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("hplan_type");

                entity.Property(e => e.Inhospid)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("inhospid");

                entity.Property(e => e.KeepspecFlag)
                    .HasMaxLength(1)
                    .HasColumnName("keepspec_flag")
                    .HasDefaultValueSql("'N'::bpchar");

                entity.Property(e => e.LocationCode)
                    .HasMaxLength(10)
                    .HasColumnName("location_code");

                entity.Property(e => e.MadeType)
                    .HasMaxLength(20)
                    .HasColumnName("made_type");

                entity.Property(e => e.MedBag).HasColumnName("med_bag");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_date");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.OrderDept)
                    .HasMaxLength(6)
                    .HasColumnName("order_dept")
                    .IsFixedLength();

                entity.Property(e => e.OrderDr)
                    .HasMaxLength(7)
                    .HasColumnName("order_dr")
                    .IsFixedLength();

                entity.Property(e => e.PlanCode)
                    .HasMaxLength(20)
                    .HasColumnName("plan_code");

                entity.Property(e => e.PlanDays)
                    .HasColumnName("plan_days")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.PlanDes)
                    .HasMaxLength(150)
                    .HasColumnName("plan_des");

                entity.Property(e => e.PreopFlag)
                    .HasMaxLength(1)
                    .HasColumnName("preop_flag")
                    .HasDefaultValueSql("'N'::bpchar");

                entity.Property(e => e.PrintDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("print_date");

                entity.Property(e => e.PrintUser)
                    .HasMaxLength(7)
                    .HasColumnName("print_user");

                entity.Property(e => e.QtyDaily)
                    .HasPrecision(9, 2)
                    .HasColumnName("qty_daily");

                entity.Property(e => e.QtyDose)
                    .HasPrecision(9, 2)
                    .HasColumnName("qty_dose");

                entity.Property(e => e.Remark)
                    .HasMaxLength(300)
                    .HasColumnName("remark");

                entity.Property(e => e.SeqNo).HasColumnName("seq_no");

                entity.Property(e => e.Status)
                    .HasMaxLength(1)
                    .HasColumnName("status")
                    .HasDefaultValueSql("'0'::bpchar");

                entity.Property(e => e.TotalQty)
                    .HasPrecision(10, 2)
                    .HasColumnName("total_qty");

                entity.Property(e => e.TriggerRecid).HasColumnName("trigger_recid");

                entity.Property(e => e.TriggerTablecode)
                    .HasMaxLength(30)
                    .HasColumnName("trigger_tablecode");

                entity.Property(e => e.UnitDose)
                    .HasMaxLength(20)
                    .HasColumnName("unit_dose");

                entity.Property(e => e.UrgFlag)
                    .HasMaxLength(1)
                    .HasColumnName("urg_flag")
                    .HasDefaultValueSql("'N'::bpchar");
            });

            modelBuilder.Entity<HisorderplanAttr>(entity =>
            {
                entity.HasKey(e => e.Orderplanatrrid)
                    .HasName("hisorderplan_attr_pkey");

                entity.ToTable("hisorderplan_attr");

                entity.Property(e => e.Orderplanatrrid).HasColumnName("orderplanatrrid");

                entity.Property(e => e.AttrCode)
                    .HasMaxLength(30)
                    .HasColumnName("attr_code");

                entity.Property(e => e.Des)
                    .HasMaxLength(1000)
                    .HasColumnName("des");

                entity.Property(e => e.Orderplanid)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("orderplanid");

                entity.Property(e => e.Parameter1)
                    .HasMaxLength(50)
                    .HasColumnName("parameter_1");

                entity.Property(e => e.Parameter2)
                    .HasMaxLength(50)
                    .HasColumnName("parameter_2");

                entity.Property(e => e.Parameter3)
                    .HasMaxLength(50)
                    .HasColumnName("parameter_3");

                entity.Property(e => e.Parameter4)
                    .HasMaxLength(50)
                    .HasColumnName("parameter_4");

                entity.Property(e => e.Parameter5)
                    .HasMaxLength(50)
                    .HasColumnName("parameter_5");

                entity.Property(e => e.Parameter6)
                    .HasMaxLength(50)
                    .HasColumnName("parameter_6");

                entity.HasOne(d => d.Orderplan)
                    .WithMany(p => p.HisorderplanAttrs)
                    .HasForeignKey(d => d.Orderplanid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("hisorderplan_attr_fkey");
            });

            modelBuilder.Entity<Hisordersoa>(entity =>
            {
                entity.HasKey(e => e.Soaid)
                    .HasName("hisorderplan_soa_pkey");

                entity.ToTable("hisordersoa");

                entity.HasIndex(e => e.Inhospid, "idx_hisordersoa_01")
                    .HasOperators(new[] { "bpchar_pattern_ops" })
                    .UseCollation(new[] { "C" });

                entity.HasIndex(e => e.HealthId, "idx_hisordersoa_02")
                    .HasOperators(new[] { "bpchar_pattern_ops" })
                    .UseCollation(new[] { "C" });

                entity.Property(e => e.Soaid)
                    .ValueGeneratedNever()
                    .HasColumnName("soaid");

                entity.Property(e => e.Context).HasColumnName("context");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_date");

                entity.Property(e => e.CreateUser)
                    .IsRequired()
                    .HasMaxLength(7)
                    .HasColumnName("create_user");

                entity.Property(e => e.DcDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("dc_date");

                entity.Property(e => e.DcUser)
                    .HasMaxLength(7)
                    .HasColumnName("dc_user");

                entity.Property(e => e.HealthId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("health_id")
                    .IsFixedLength();

                entity.Property(e => e.Inhospid)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("inhospid");

                entity.Property(e => e.Kind)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("kind");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_date");

                entity.Property(e => e.ModifyUser)
                    .HasColumnType("character varying")
                    .HasColumnName("modify_user");

                entity.Property(e => e.SourceType)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("source_type");

                entity.Property(e => e.Status)
                    .HasColumnType("char")
                    .HasColumnName("status");

                entity.Property(e => e.VersionCode).HasColumnName("version_code");
            });

            modelBuilder.Entity<ShisAttribute>(entity =>
            {
                entity.HasKey(e => e.AttrCode)
                    .HasName("shis_attribute_pkey");

                entity.ToTable("shis_attribute");

                entity.Property(e => e.AttrCode)
                    .HasMaxLength(3)
                    .HasColumnName("attr_code")
                    .HasComment("身分代碼");

                entity.Property(e => e.AttrName)
                    .HasMaxLength(100)
                    .HasColumnName("attr_name")
                    .HasComment("身分說明");

                entity.Property(e => e.AttrRegFee)
                    .HasColumnName("attr_reg_fee")
                    .HasComment("該身分預收的掛號費用");

                entity.Property(e => e.AttrStatus)
                    .HasMaxLength(1)
                    .HasColumnName("attr_status")
                    .HasComment("啟用狀態");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");
            });

            modelBuilder.Entity<ShisAuth>(entity =>
            {
                entity.HasKey(e => new { e.UserIdno, e.ProjectId })
                    .HasName("shis_auths_pkey");

                entity.ToTable("shis_auths");

                entity.HasComment("User Auth File(Account permissions)");

                entity.Property(e => e.UserIdno)
                    .HasMaxLength(7)
                    .HasColumnName("user_idno");

                entity.Property(e => e.ProjectId)
                    .HasMaxLength(32)
                    .HasColumnName("project_id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_time");

                entity.Property(e => e.Creator)
                    .IsRequired()
                    .HasMaxLength(7)
                    .HasColumnName("creator");

                entity.HasOne(d => d.UserIdnoNavigation)
                    .WithMany(p => p.ShisAuths)
                    .HasForeignKey(d => d.UserIdno)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("shis_auths_fk01");
            });

            modelBuilder.Entity<ShisAuthsLog>(entity =>
            {
                entity.HasKey(e => new { e.EditUser, e.EditTime, e.UserIdno, e.ProjectId })
                    .HasName("shis_auths_log_pkey");

                entity.ToTable("shis_auths_log");

                entity.HasComment("Auth change log");

                entity.Property(e => e.EditUser)
                    .HasMaxLength(7)
                    .HasColumnName("edit_user");

                entity.Property(e => e.EditTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("edit_time")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.UserIdno)
                    .HasMaxLength(7)
                    .HasColumnName("user_idno");

                entity.Property(e => e.ProjectId)
                    .HasMaxLength(32)
                    .HasColumnName("project_id");

                entity.Property(e => e.EditType)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("edit_type");
            });

            modelBuilder.Entity<ShisChart>(entity =>
            {
                entity.HasKey(e => e.ChrHealthId)
                    .HasName("SHISCHART_pkey");

                entity.ToTable("shis_chart");

                entity.HasIndex(e => new { e.ChrPatientFirstname, e.ChrPatientMidname, e.ChrPatientLastname }, "idx_shis_chart_01");

                entity.HasIndex(e => e.ChrMobilePhone, "idx_shis_chart_02");

                entity.Property(e => e.ChrHealthId)
                    .HasMaxLength(10)
                    .HasColumnName("chr_health_id")
                    .IsFixedLength();

                entity.Property(e => e.ChrAddress).HasColumnName("chr_address");

                entity.Property(e => e.ChrAreaCode)
                    .HasMaxLength(20)
                    .HasColumnName("chr_area_code");

                entity.Property(e => e.ChrBirthDate).HasColumnName("chr_birth_date");

                entity.Property(e => e.ChrCombineFlag)
                    .HasMaxLength(1)
                    .HasColumnName("chr_combine_flag");

                entity.Property(e => e.ChrContactPhone)
                    .HasMaxLength(30)
                    .HasColumnName("chr_contact_phone");

                entity.Property(e => e.ChrContactRelation)
                    .HasMaxLength(2)
                    .HasColumnName("chr_contact_relation")
                    .IsFixedLength();

                entity.Property(e => e.ChrEmgContact)
                    .HasMaxLength(650)
                    .HasColumnName("chr_emg_contact");

                entity.Property(e => e.ChrMobilePhone)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("chr_mobile_phone");

                entity.Property(e => e.ChrNationalId)
                    .HasMaxLength(10)
                    .HasColumnName("chr_national_id");

                entity.Property(e => e.ChrPatientFirstname)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("chr_patient_firstname");

                entity.Property(e => e.ChrPatientLastname)
                    .HasMaxLength(200)
                    .HasColumnName("chr_patient_lastname");

                entity.Property(e => e.ChrPatientMidname)
                    .HasMaxLength(200)
                    .HasColumnName("chr_patient_midname");

                entity.Property(e => e.ChrRefugeeFlag)
                    .HasMaxLength(1)
                    .HasColumnName("chr_refugee_flag")
                    .HasDefaultValueSql("'N'::bpchar")
                    .HasComment("refugee: Y");

                entity.Property(e => e.ChrRemark)
                    .HasMaxLength(1000)
                    .HasColumnName("chr_remark");

                entity.Property(e => e.ChrSex)
                    .IsRequired()
                    .HasMaxLength(1)
                    .HasColumnName("chr_sex");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user")
                    .IsFixedLength();
            });

            modelBuilder.Entity<ShisChartLog>(entity =>
            {
                entity.HasKey(e => e.LogId)
                    .HasName("SHISCHARTLOG_pkey");

                entity.ToTable("shis_chart_log");

                entity.HasIndex(e => e.ChrHealthId, "idx_shis_chart_log_01");

                entity.Property(e => e.LogId)
                    .HasMaxLength(20)
                    .HasColumnName("log_id");

                entity.Property(e => e.ChrAddress).HasColumnName("chr_address");

                entity.Property(e => e.ChrAreaCode)
                    .HasMaxLength(20)
                    .HasColumnName("chr_area_code");

                entity.Property(e => e.ChrBirthDate).HasColumnName("chr_birth_date");

                entity.Property(e => e.ChrCombineFlag)
                    .HasMaxLength(1)
                    .HasColumnName("chr_combine_flag");

                entity.Property(e => e.ChrContactPhone)
                    .HasMaxLength(30)
                    .HasColumnName("chr_contact_phone");

                entity.Property(e => e.ChrContactRelation)
                    .HasMaxLength(2)
                    .HasColumnName("chr_contact_relation")
                    .IsFixedLength();

                entity.Property(e => e.ChrEmgContact)
                    .HasMaxLength(650)
                    .HasColumnName("chr_emg_contact");

                entity.Property(e => e.ChrHealthId)
                    .HasMaxLength(10)
                    .HasColumnName("chr_health_id")
                    .IsFixedLength();

                entity.Property(e => e.ChrMobilePhone)
                    .HasMaxLength(30)
                    .HasColumnName("chr_mobile_phone");

                entity.Property(e => e.ChrNationalId)
                    .HasMaxLength(10)
                    .HasColumnName("chr_national_id");

                entity.Property(e => e.ChrPatientFirstname)
                    .HasMaxLength(200)
                    .HasColumnName("chr_patient_firstname");

                entity.Property(e => e.ChrPatientLastname)
                    .HasMaxLength(200)
                    .HasColumnName("chr_patient_lastname");

                entity.Property(e => e.ChrPatientMidname)
                    .HasMaxLength(200)
                    .HasColumnName("chr_patient_midname");

                entity.Property(e => e.ChrRefugeeFlag)
                    .HasMaxLength(1)
                    .HasColumnName("chr_refugee_flag");

                entity.Property(e => e.ChrRemark)
                    .HasMaxLength(1000)
                    .HasColumnName("chr_remark");

                entity.Property(e => e.ChrSex)
                    .HasMaxLength(1)
                    .HasColumnName("chr_sex");

                entity.Property(e => e.LogMode)
                    .HasMaxLength(1)
                    .HasColumnName("log_mode")
                    .HasComment("Command Mode: Insert, Update, Delete");

                entity.Property(e => e.LogTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("log_time")
                    .HasComment("Modify Time");

                entity.Property(e => e.LogUser)
                    .HasMaxLength(7)
                    .HasColumnName("log_user")
                    .IsFixedLength()
                    .HasComment("Modify by User");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user")
                    .IsFixedLength();
            });

            modelBuilder.Entity<ShisCoderef>(entity =>
            {
                entity.HasKey(e => e.RefId)
                    .HasName("shis_coderef_pkey");

                entity.ToTable("shis_coderef");

                entity.Property(e => e.RefId)
                    .HasMaxLength(20)
                    .HasColumnName("ref_id");

                entity.Property(e => e.ModifyId)
                    .HasMaxLength(7)
                    .HasColumnName("modify_id");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");

                entity.Property(e => e.RefCasetype)
                    .HasMaxLength(5)
                    .HasColumnName("ref_casetype");

                entity.Property(e => e.RefCode)
                    .HasMaxLength(500)
                    .HasColumnName("ref_code");

                entity.Property(e => e.RefCodetype)
                    .HasMaxLength(100)
                    .HasColumnName("ref_codetype");

                entity.Property(e => e.RefDefaultFlag)
                    .HasMaxLength(5)
                    .HasColumnName("ref_default_flag")
                    .HasComment("是否預設啟用");

                entity.Property(e => e.RefDes)
                    .HasMaxLength(2000)
                    .HasColumnName("ref_des");

                entity.Property(e => e.RefDes2)
                    .HasMaxLength(2000)
                    .HasColumnName("ref_des2");

                entity.Property(e => e.RefName)
                    .HasMaxLength(1000)
                    .HasColumnName("ref_name");

                entity.Property(e => e.RefShowseq).HasColumnName("ref_showseq");
            });

            modelBuilder.Entity<ShisCondition>(entity =>
            {
                entity.HasKey(e => new { e.CndCodetype, e.CndCode })
                    .HasName("shis_condition_pkey");

                entity.ToTable("shis_condition");

                entity.Property(e => e.CndCodetype)
                    .HasMaxLength(100)
                    .HasColumnName("cnd_codetype");

                entity.Property(e => e.CndCode)
                    .HasMaxLength(500)
                    .HasColumnName("cnd_code");

                entity.Property(e => e.CndDesc)
                    .HasMaxLength(500)
                    .HasColumnName("cnd_desc");

                entity.Property(e => e.CndEnable)
                    .HasMaxLength(1)
                    .HasColumnName("cnd_enable");

                entity.Property(e => e.CndNoon)
                    .HasMaxLength(5)
                    .HasColumnName("cnd_noon");

                entity.Property(e => e.CndRoom)
                    .HasMaxLength(3)
                    .HasColumnName("cnd_room");

                entity.Property(e => e.CndSymbol1)
                    .HasMaxLength(2)
                    .HasColumnName("cnd_symbol1")
                    .IsFixedLength();

                entity.Property(e => e.CndSymbol2)
                    .HasMaxLength(2)
                    .HasColumnName("cnd_symbol2")
                    .IsFixedLength();

                entity.Property(e => e.CndValue1)
                    .HasMaxLength(100)
                    .HasColumnName("cnd_value1");

                entity.Property(e => e.CndValue2)
                    .HasMaxLength(100)
                    .HasColumnName("cnd_value2");

                entity.Property(e => e.CndWeek)
                    .HasMaxLength(1)
                    .HasColumnName("cnd_week");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");
            });

            modelBuilder.Entity<ShisDepartment>(entity =>
            {
                entity.HasKey(e => e.DptCode)
                    .HasName("shis_department_pkey");

                entity.ToTable("shis_department");

                entity.Property(e => e.DptCode)
                    .HasMaxLength(6)
                    .HasColumnName("dpt_code");

                entity.Property(e => e.DptCategory)
                    .HasMaxLength(3)
                    .HasColumnName("dpt_category");

                entity.Property(e => e.DptDefaultAttr)
                    .HasMaxLength(3)
                    .HasColumnName("dpt_default_attr")
                    .HasComment("預設身分別");

                entity.Property(e => e.DptDepth).HasColumnName("dpt_depth");

                entity.Property(e => e.DptName)
                    .HasMaxLength(200)
                    .HasColumnName("dpt_name");

                entity.Property(e => e.DptParent)
                    .HasMaxLength(6)
                    .HasColumnName("dpt_parent");

                entity.Property(e => e.DptRemark)
                    .HasMaxLength(1000)
                    .HasColumnName("dpt_remark");

                entity.Property(e => e.DptStatus)
                    .HasMaxLength(1)
                    .HasColumnName("dpt_status");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");
            });

            modelBuilder.Entity<ShisIcd>(entity =>
            {
                entity.HasKey(e => e.IcdCode)
                    .HasName("SHISICD_pkey");

                entity.ToTable("shis_icd");

                entity.HasComment("Diagnosis data.");

                entity.HasIndex(e => e.ParentCode, "idx_icd_01")
                    .HasOperators(new[] { "varchar_ops" });

                entity.Property(e => e.IcdCode)
                    .HasMaxLength(8)
                    .HasColumnName("icd_code");

                entity.Property(e => e.Dhis2Code).HasColumnName("dhis2_code");

                entity.Property(e => e.IcdCodeUndot)
                    .IsRequired()
                    .HasMaxLength(7)
                    .HasColumnName("icd_code_undot")
                    .HasComment("ICD Code without decimal point.");

                entity.Property(e => e.IcdEnglishName)
                    .IsRequired()
                    .HasMaxLength(400)
                    .HasColumnName("icd_english_name");

                entity.Property(e => e.IcdType)
                    .HasMaxLength(10)
                    .HasColumnName("icd_type")
                    .HasComment("CM/PCS");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_date");

                entity.Property(e => e.ModifyUser)
                    .IsRequired()
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.ParentCode)
                    .HasMaxLength(8)
                    .HasColumnName("parent_code")
                    .HasComment("Parent ICD Code for HisOrder UI Design.");

                entity.Property(e => e.ShowMode)
                    .HasMaxLength(10)
                    .HasColumnName("show_mode")
                    .HasComment("Show position for HisOrder UI Design.");

                entity.Property(e => e.Status)
                    .HasMaxLength(1)
                    .HasColumnName("status")
                    .HasComment("ICD Code.");

                entity.Property(e => e.Versioncode)
                    .HasMaxLength(2)
                    .HasColumnName("versioncode")
                    .HasComment("ICD 9 / ICD 10 ...");
            });

            modelBuilder.Entity<ShisMedfrequency>(entity =>
            {
                entity.HasKey(e => e.FrqCode)
                    .HasName("shis_medfrequency_pkey");

                entity.ToTable("shis_medfrequency");

                entity.Property(e => e.FrqCode)
                    .HasMaxLength(20)
                    .HasColumnName("frq_code");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(7)
                    .HasColumnName("create_user");

                entity.Property(e => e.EnableStatus)
                    .HasMaxLength(1)
                    .HasColumnName("enable_status");

                entity.Property(e => e.FreqDesc)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("freq_desc");

                entity.Property(e => e.FrqForDays).HasColumnName("frq_for_days");

                entity.Property(e => e.FrqForTimes).HasColumnName("frq_for_times");

                entity.Property(e => e.FrqOneDayTimes).HasColumnName("frq_one_day_times");

                entity.Property(e => e.FrqSeqNo)
                    .HasColumnName("frq_seq_no")
                    .HasDefaultValueSql("999");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");
            });

            modelBuilder.Entity<ShisMedfrequencyInd>(entity =>
            {
                entity.HasKey(e => new { e.FrqCode, e.IndCode })
                    .HasName("shis_medfrequency_ind_pkey");

                entity.ToTable("shis_medfrequency_ind");

                entity.Property(e => e.FrqCode)
                    .HasMaxLength(20)
                    .HasColumnName("frq_code");

                entity.Property(e => e.IndCode)
                    .HasMaxLength(20)
                    .HasColumnName("ind_code");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(7)
                    .HasColumnName("create_user");

                entity.Property(e => e.EnableStatus)
                    .HasMaxLength(1)
                    .HasColumnName("enable_status");

                entity.Property(e => e.IndDesc)
                    .HasMaxLength(100)
                    .HasColumnName("ind_desc");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.Showseq)
                    .HasPrecision(3)
                    .HasColumnName("showseq");
            });

            modelBuilder.Entity<ShisMedicine>(entity =>
            {
                entity.HasKey(e => e.MedId)
                    .HasName("SHISMEDICINE_pkey");

                entity.ToTable("shis_medicine");

                entity.Property(e => e.MedId)
                    .HasMaxLength(10)
                    .HasColumnName("med_id");

                entity.Property(e => e.BrandName)
                    .HasMaxLength(150)
                    .HasColumnName("brand_name");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(7)
                    .HasColumnName("create_user");

                entity.Property(e => e.DefaultFreq)
                    .HasMaxLength(10)
                    .HasColumnName("default_freq")
                    .HasComment("開立時預設頻次(可空白)");

                entity.Property(e => e.EndDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("end_date");

                entity.Property(e => e.GenericName)
                    .HasMaxLength(150)
                    .HasColumnName("generic_name");

                entity.Property(e => e.MedType)
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnName("med_type")
                    .HasComment("1-口服\n2-針劑\n3-外用");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_date")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.PackSpec)
                    .HasMaxLength(20)
                    .HasColumnName("pack_spec")
                    .HasComment("包裝單位(藥局發藥)");

                entity.Property(e => e.RefDuration)
                    .HasMaxLength(3)
                    .HasColumnName("ref_duration")
                    .HasComment("建議的用藥天數(不用預設)");

                entity.Property(e => e.Remarks)
                    .HasMaxLength(500)
                    .HasColumnName("remarks")
                    .HasComment("其他備註說明");

                entity.Property(e => e.StartDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("start_date")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Status)
                    .HasMaxLength(1)
                    .HasColumnName("status")
                    .HasDefaultValueSql("'0'::bpchar")
                    .HasComment("醫囑系統是否顯示");

                entity.Property(e => e.UnitSpec)
                    .HasMaxLength(20)
                    .HasColumnName("unit_spec")
                    .HasComment("醫囑單位");
            });

            modelBuilder.Entity<ShisMedpathway>(entity =>
            {
                entity.HasKey(e => new { e.MedType, e.PathCode })
                    .HasName("shis_medpathway_pkey");

                entity.ToTable("shis_medpathway");

                entity.Property(e => e.MedType)
                    .HasMaxLength(1)
                    .HasColumnName("med_type");

                entity.Property(e => e.PathCode)
                    .HasMaxLength(20)
                    .HasColumnName("path_code");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(7)
                    .HasColumnName("create_user");

                entity.Property(e => e.EnableStatus)
                    .HasMaxLength(1)
                    .HasColumnName("enable_status");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.PathDesc)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("path_desc");

                entity.Property(e => e.Showseq)
                    .HasPrecision(3)
                    .HasColumnName("showseq");
            });

            modelBuilder.Entity<ShisNonMedicine>(entity =>
            {
                entity.HasKey(e => e.ItemId)
                    .HasName("shis_non_medicine_pkey");

                entity.ToTable("shis_non_medicine");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(10)
                    .HasColumnName("item_id");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(7)
                    .HasColumnName("create_user");

                entity.Property(e => e.EndDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("end_date");

                entity.Property(e => e.GroupCode)
                    .HasMaxLength(10)
                    .HasColumnName("group_code");

                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("item_name");

                entity.Property(e => e.ItemSpec)
                    .HasMaxLength(20)
                    .HasColumnName("item_spec");

                entity.Property(e => e.ItemType)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("item_type")
                    .HasComment("5.Laboratory 6.Radiology 7.Pathology 8.Material");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_date")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.Remark)
                    .HasMaxLength(500)
                    .HasColumnName("remark");

                entity.Property(e => e.ShowSeq)
                    .HasPrecision(7, 2)
                    .HasColumnName("show_seq");

                entity.Property(e => e.StartDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("start_date");

                entity.Property(e => e.Status)
                    .HasMaxLength(1)
                    .HasColumnName("status")
                    .HasDefaultValueSql("'0'::bpchar");
            });

            modelBuilder.Entity<ShisProject>(entity =>
            {
                entity.HasKey(e => e.ProjectId)
                    .HasName("shis_projects_pkey");

                entity.ToTable("shis_projects");

                entity.HasComment("Auth Setting reference Project File(main function node)");

                entity.Property(e => e.ProjectId)
                    .HasMaxLength(32)
                    .HasColumnName("project_id");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_time")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Creator)
                    .IsRequired()
                    .HasMaxLength(7)
                    .HasColumnName("creator");

                entity.Property(e => e.ProjectName)
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnName("project_name");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("url");
            });

            modelBuilder.Entity<ShisSerialpool>(entity =>
            {
                entity.HasKey(e => e.SerialOwner)
                    .HasName("shis_serialpool_pkey");

                entity.ToTable("shis_serialpool");

                entity.Property(e => e.SerialOwner)
                    .HasMaxLength(50)
                    .HasColumnName("serial_owner");

                entity.Property(e => e.SerialMaxno)
                    .HasMaxLength(20)
                    .HasColumnName("serial_maxno");

                entity.Property(e => e.SerialNo).HasColumnName("serial_no");

                entity.Property(e => e.SerialPrefix)
                    .HasMaxLength(5)
                    .HasColumnName("serial_prefix");
            });

            modelBuilder.Entity<ShisUser>(entity =>
            {
                entity.HasKey(e => e.UserIdno)
                    .HasName("shis_users_pkey");

                entity.ToTable("shis_users");

                entity.HasComment("Account Basic File(user account )");

                entity.Property(e => e.UserIdno)
                    .HasMaxLength(7)
                    .HasColumnName("user_idno");

                entity.Property(e => e.AccountStatus)
                    .IsRequired()
                    .HasMaxLength(1)
                    .HasColumnName("account_status")
                    .HasDefaultValueSql("1");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_time");

                entity.Property(e => e.Creator)
                    .IsRequired()
                    .HasMaxLength(7)
                    .HasColumnName("creator");

                entity.Property(e => e.EndDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("end_date");

                entity.Property(e => e.StartDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("start_date");

                entity.Property(e => e.UserBirthDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("user_birth_date");

                entity.Property(e => e.UserCategory)
                    .IsRequired()
                    .HasMaxLength(1)
                    .HasColumnName("user_category")
                    .HasComment("分類(1:Doctor,2:Nurse,3.Staff");

                entity.Property(e => e.UserEmail).HasColumnName("user_email");

                entity.Property(e => e.UserMobilePhone)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("user_mobile_phone");

                entity.Property(e => e.UserNameFirstname)
                    .IsRequired()
                    .HasColumnName("user_name_firstname");

                entity.Property(e => e.UserNameLastname)
                    .IsRequired()
                    .HasColumnName("user_name_lastname");

                entity.Property(e => e.UserNameMidname)
                    .IsRequired()
                    .HasColumnName("user_name_midname");

                entity.Property(e => e.UserPassword)
                    .IsRequired()
                    .HasColumnName("user_password");

                entity.Property(e => e.UserSex).HasColumnName("user_sex");
            });

            modelBuilder.Entity<ShisUsersLog>(entity =>
            {
                entity.HasKey(e => new { e.UserIdno, e.EventType, e.EventTime, e.Ip })
                    .HasName("shis_users_log_pkey");

                entity.ToTable("shis_users_log");

                entity.HasComment("Account change log(帳號基本檔修改紀錄表2023.03.03)");

                entity.Property(e => e.UserIdno).HasColumnName("user_idno");

                entity.Property(e => e.EventType).HasColumnName("event_type");

                entity.Property(e => e.EventTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("event_time")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Ip)
                    .HasMaxLength(64)
                    .HasColumnName("ip");

                entity.Property(e => e.EventErrorInput).HasColumnName("event_error_input");

                entity.Property(e => e.EventLoggingUser)
                    .HasMaxLength(7)
                    .HasColumnName("event_logging_user");

                entity.Property(e => e.IsSuccess).HasColumnName("is_success");

                entity.Property(e => e.Message).HasColumnName("message");
            });

            modelBuilder.Entity<PhysicalSign>(entity =>
            {
                entity.HasKey(e => e.PhyId)
                    .HasName("physical_sign_pkey");

                entity.ToTable("physical_sign");

                entity.HasIndex(e => new { e.Inhospid, e.PhyType }, "idx_physical_sign_01");

                entity.Property(e => e.PhyId)
                    .HasMaxLength(50)
                    .HasColumnName("phy_id");

                entity.Property(e => e.Inhospid)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("inhospid");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.PhyType)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("phy_type");

                entity.Property(e => e.PhyValue)
                    .HasMaxLength(500)
                    .HasColumnName("phy_value");
            });

            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasKey(e => new { e.RegDate, e.RegDepartment, e.RegNoon, e.RegSeqNo })
                    .HasName("registration_pkey");

                entity.ToTable("registration");

                entity.HasIndex(e => e.Inhospid, "idx_registration_01");

                entity.Property(e => e.RegDate)
                    .HasColumnName("reg_date")
                    .HasComment("看診日");

                entity.Property(e => e.RegDepartment)
                    .HasMaxLength(6)
                    .HasColumnName("reg_department")
                    .HasComment("看診科別");

                entity.Property(e => e.RegNoon)
                    .HasMaxLength(5)
                    .HasColumnName("reg_noon")
                    .HasComment("午別");

                entity.Property(e => e.RegSeqNo)
                    .HasColumnName("reg_seq_no")
                    .HasComment("門診:看診號\n急診:檢傷序號");

                entity.Property(e => e.Inhospid)
                    .HasMaxLength(20)
                    .HasColumnName("inhospid")
                    .HasComment("就醫序號");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.RegAttrDesc)
                    .HasMaxLength(200)
                    .HasColumnName("reg_attr_desc")
                    .HasComment("身分備註");

                entity.Property(e => e.RegAttribute)
                    .HasMaxLength(3)
                    .HasColumnName("reg_attribute")
                    .HasComment("特殊身分->參考shis_attribute");

                entity.Property(e => e.RegBedNo)
                    .HasMaxLength(3)
                    .HasColumnName("reg_bed_no")
                    .HasComment("急診床號");

                entity.Property(e => e.RegCallTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("reg_call_time")
                    .HasComment("叫號時間");

                entity.Property(e => e.RegCreateTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("reg_create_time")
                    .HasComment("create datetime for registration ");

                entity.Property(e => e.RegDoctorId)
                    .HasMaxLength(7)
                    .HasColumnName("reg_doctor_id")
                    .HasComment("醫師職邊");

                entity.Property(e => e.RegEndTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("reg_end_time")
                    .HasComment("結束看診時間");

                entity.Property(e => e.RegExamEndTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("reg_exam_end_time")
                    .HasComment("finish examining return time");

                entity.Property(e => e.RegExamStartTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("reg_exam_start_time")
                    .HasComment("click examining start time");

                entity.Property(e => e.RegFollowCode)
                    .HasMaxLength(20)
                    .HasColumnName("reg_follow_code");

                entity.Property(e => e.RegFollowDesc)
                    .HasMaxLength(500)
                    .HasColumnName("reg_follow_desc");

                entity.Property(e => e.RegHealthId)
                    .HasMaxLength(10)
                    .HasColumnName("reg_health_id")
                    .IsFixedLength()
                    .HasComment("病歷號");

                entity.Property(e => e.RegRoomNo)
                    .HasMaxLength(3)
                    .HasColumnName("reg_room_no")
                    .HasComment("診間號");

                entity.Property(e => e.RegStartTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("reg_start_time")
                    .HasComment("開始看診時間");

                entity.Property(e => e.RegStatus)
                    .HasMaxLength(1)
                    .HasColumnName("reg_status")
                    .HasComment("看診狀態\nN:未看診\nT :暫存\n* :已看診\nC:取消掛號");

                entity.Property(e => e.RegTriage)
                    .HasMaxLength(1)
                    .HasColumnName("reg_triage")
                    .HasComment("檢傷分類\n0：一般門診(白燈)\n1：急診分類(綠燈)\n2：急診分類(黃燈)\n3：急診分類(紅燈)\n4：急診分類(黑燈)");
            });

            modelBuilder.Entity<TransactionCall>(entity =>
            {
                entity.HasKey(e => e.CallId)
                    .HasName("transaction_call_pkey");

                entity.ToTable("transaction_call");

                entity.Property(e => e.CallId)
                    .HasColumnName("call_id")
                    .HasComment("流水號")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CallPatientId)
                    .HasMaxLength(10)
                    .HasColumnName("call_patient_id")
                    .IsFixedLength()
                    .HasComment("病歷號");

                entity.Property(e => e.CallRegDate)
                    .HasColumnName("call_reg_date")
                    .HasComment("看診日");

                entity.Property(e => e.CallRegDepartment)
                    .HasMaxLength(6)
                    .HasColumnName("call_reg_department")
                    .HasComment("看診科");

                entity.Property(e => e.CallRegNoon)
                    .HasMaxLength(5)
                    .HasColumnName("call_reg_noon")
                    .HasComment("午別");

                entity.Property(e => e.CallRegSeqNo)
                    .HasColumnName("call_reg_seq_no")
                    .HasComment("看診號");

                entity.Property(e => e.CallTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("call_time")
                    .HasComment("叫號時間");

                entity.Property(e => e.Inhospid)
                    .HasColumnName("inhospid")
                    .HasComment("就醫序號");

                entity.Property(e => e.ModifySuer)
                    .HasMaxLength(7)
                    .HasColumnName("modify_suer");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time")
                    .HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<TransactionFee>(entity =>
            {
                entity.HasKey(e => e.TransationId)
                    .HasName("transaction_fee_pkey");

                entity.ToTable("transaction_fee");

                entity.Property(e => e.TransationId)
                    .HasColumnName("transation_id")
                    .HasComment("流水號")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.FeePaidFlag)
                    .HasMaxLength(1)
                    .HasColumnName("fee_paid_flag")
                    .HasComment("是否已收費");

                entity.Property(e => e.FeePaidMoney)
                    .HasColumnName("fee_paid_money")
                    .HasComment("收費金額");

                entity.Property(e => e.FeeType)
                    .HasMaxLength(10)
                    .HasColumnName("fee_type")
                    .HasComment("收費項目");

                entity.Property(e => e.Inhospid)
                    .HasColumnName("inhospid")
                    .HasComment("就醫序號");

                entity.Property(e => e.ModifyUser)
                    .HasMaxLength(7)
                    .HasColumnName("modify_user");

                entity.Property(e => e.TransactionTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("transaction_time")
                    .HasComment("交易時間");
            });

            modelBuilder.HasSequence("hisorderplan_soa_soaid_seq");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
