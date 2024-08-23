using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KMU.HisOrder.Console.Gateway.Models;

public partial class GatewayContext : DbContext
{
    public GatewayContext()
    {
    }

    public GatewayContext(DbContextOptions<GatewayContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ClinicSchedule> ClinicSchedules { get; set; }

    public virtual DbSet<Hisorderplan> Hisorderplans { get; set; }

    public virtual DbSet<HisorderplanAttr> HisorderplanAttrs { get; set; }

    public virtual DbSet<Hisordersoa> Hisordersoas { get; set; }

    public virtual DbSet<KmuAttribute> KmuAttributes { get; set; }

    public virtual DbSet<KmuChart> KmuCharts { get; set; }

    public virtual DbSet<KmuChartLog> KmuChartLogs { get; set; }

    public virtual DbSet<KmuChartMergeHistory> KmuChartMergeHistories { get; set; }

    public virtual DbSet<KmuCoderef> KmuCoderefs { get; set; }

    public virtual DbSet<KmuCondition> KmuConditions { get; set; }

    public virtual DbSet<KmuDepartment> KmuDepartments { get; set; }

    public virtual DbSet<KmuIcd> KmuIcds { get; set; }

    public virtual DbSet<KmuMedfrequency> KmuMedfrequencies { get; set; }

    public virtual DbSet<KmuMedfrequencyInd> KmuMedfrequencyInds { get; set; }

    public virtual DbSet<KmuMedicine> KmuMedicines { get; set; }

    public virtual DbSet<KmuMedpathway> KmuMedpathways { get; set; }

    public virtual DbSet<KmuNonMedicine> KmuNonMedicines { get; set; }


    public virtual DbSet<KmuUploadLog> KmuUploadLogs { get; set; }

    public virtual DbSet<KmuUser> KmuUsers { get; set; }


    public virtual DbSet<PhysicalSign> PhysicalSigns { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.

        var configuration = new ConfigurationBuilder().
          SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
         .AddJsonFile("appsettings.json")
         .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        

        //=> optionsBuilder.UseNpgsql("Server=172.18.18.78;Database=KMU;Port=5432;User ID=hisuser;Password=Kmuh@$^");
        //=> optionsBuilder.UseNpgsql("Server=172.18.18.78;Database=BORH;Port=5432;User ID=hisuser;Password=Kmuh@$^");
        //=> optionsBuilder.UseNpgsql("Server=172.18.18.78;Database=KMU;Port=5432;User ID=hisuser;Password=Kmuh@$^");
        //=> optionsBuilder.UseNpgsql("Server=localhost;Database=KMU; Port=5432; User ID=postgres;Password=abdi;");

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClinicSchedule>(entity =>
        {
            entity.HasKey(e => new { e.ScheWeek, e.ScheNoon, e.ScheRoom }).HasName("clinic_schedule_pkey");

            entity.ToTable("clinic_schedule");

            entity.Property(e => e.ScheWeek)
                .HasMaxLength(1)
                .HasComment("星期別")
                .HasColumnName("sche_week");
            entity.Property(e => e.ScheNoon)
                .HasMaxLength(5)
                .HasComment("午別")
                .HasColumnName("sche_noon");
            entity.Property(e => e.ScheRoom)
                .HasMaxLength(3)
                .HasComment("診間號碼")
                .HasColumnName("sche_room");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_time");
            entity.Property(e => e.ModifyUser)
                .HasMaxLength(7)
                .HasColumnName("modify_user");
            entity.Property(e => e.ScheCallNo)
                .HasComment("叫號號碼")
                .HasColumnName("sche_call_no");
            entity.Property(e => e.ScheCallTime)
                .HasComment("Calling Time Update")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sche_call_time");
            entity.Property(e => e.ScheDoctor)
                .HasMaxLength(7)
                .HasComment("醫師職編")
                .HasColumnName("sche_doctor");
            entity.Property(e => e.ScheDoctorName)
                .HasMaxLength(200)
                .HasComment("醫師姓名")
                .HasColumnName("sche_doctor_name");
            entity.Property(e => e.ScheDptCode)
                .HasMaxLength(6)
                .HasComment("科別代碼")
                .HasColumnName("sche_dpt_code");
            entity.Property(e => e.ScheDptName)
                .HasMaxLength(200)
                .HasComment("科別名稱")
                .HasColumnName("sche_dpt_name");
            entity.Property(e => e.ScheOpenFlag)
                .HasMaxLength(1)
                .HasComment("診次是否開放")
                .HasColumnName("sche_open_flag");
            entity.Property(e => e.ScheRemark)
                .HasMaxLength(1000)
                .HasColumnName("sche_remark");
        });


        modelBuilder.Entity<Hisorderplan>(entity =>
        {
            entity.HasKey(e => e.Orderplanid).HasName("hisorderplan_pkey");

            entity.ToTable("hisorderplan");

            entity.HasIndex(e => e.HealthId, "idx_hisorderplan_01").UseCollation(new[] { "C" });

            entity.HasIndex(e => e.Inhospid, "idx_hisorderplan_02")
                .HasOperators(new[] { "bpchar_pattern_ops" })
                .UseCollation(new[] { "C" });

            entity.Property(e => e.Orderplanid)
                .ValueGeneratedNever()
                .HasColumnName("orderplanid");
            entity.Property(e => e.AddFlag)
                .HasMaxLength(1)
                .HasDefaultValueSql("'N'::bpchar")
                .HasColumnName("add_flag");
            entity.Property(e => e.ChargeStatus)
                .HasMaxLength(1)
                .HasColumnName("charge_status");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_date");
            entity.Property(e => e.CreateUser)
                .HasMaxLength(7)
                .HasColumnName("create_user");
            entity.Property(e => e.DcDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dc_date");
            entity.Property(e => e.DcStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'0'::bpchar")
                .HasColumnName("dc_status");
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
                .HasDefaultValueSql("'N'::bpchar")
                .HasColumnName("free_charge");
            entity.Property(e => e.FreqCode)
                .HasMaxLength(20)
                .HasColumnName("freq_code");
            entity.Property(e => e.HealthId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("health_id");
            entity.Property(e => e.HplanType)
                .HasMaxLength(20)
                .HasColumnName("hplan_type");
            entity.Property(e => e.Inhospid)
                .HasColumnType("character varying")
                .HasColumnName("inhospid");
            entity.Property(e => e.KeepspecFlag)
                .HasMaxLength(1)
                .HasDefaultValueSql("'N'::bpchar")
                .HasColumnName("keepspec_flag");
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
                .IsFixedLength()
                .HasColumnName("order_dept");
            entity.Property(e => e.OrderDr)
                .HasMaxLength(7)
                .IsFixedLength()
                .HasColumnName("order_dr");
            entity.Property(e => e.PlanCode)
                .HasMaxLength(20)
                .HasColumnName("plan_code");
            entity.Property(e => e.PlanDays)
                .HasDefaultValueSql("0")
                .HasColumnName("plan_days");
            entity.Property(e => e.PlanDes)
                .HasMaxLength(150)
                .HasColumnName("plan_des");
            entity.Property(e => e.PreopFlag)
                .HasMaxLength(1)
                .HasDefaultValueSql("'N'::bpchar")
                .HasColumnName("preop_flag");
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
                .HasDefaultValueSql("'0'::bpchar")
                .HasColumnName("status");
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
                .HasDefaultValueSql("'N'::bpchar")
                .HasColumnName("urg_flag");
        });

        modelBuilder.Entity<HisorderplanAttr>(entity =>
        {
            entity.HasKey(e => e.Orderplanatrrid).HasName("hisorderplan_attr_pkey");

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

            entity.HasOne(d => d.Orderplan).WithMany(p => p.HisorderplanAttrs)
                .HasForeignKey(d => d.Orderplanid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hisorderplan_attr_fkey");
        });

        modelBuilder.Entity<Hisordersoa>(entity =>
        {
            entity.HasKey(e => e.Soaid).HasName("hisorderplan_soa_pkey");

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
                .HasMaxLength(7)
                .HasColumnName("create_user");
            entity.Property(e => e.DcDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dc_date");
            entity.Property(e => e.DcUser)
                .HasMaxLength(7)
                .HasColumnName("dc_user");
            entity.Property(e => e.HealthId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("health_id");
            entity.Property(e => e.Inhospid)
                .HasColumnType("character varying")
                .HasColumnName("inhospid");
            entity.Property(e => e.Kind)
                .HasMaxLength(20)
                .HasColumnName("kind");
            entity.Property(e => e.ModifyDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_date");
            entity.Property(e => e.ModifyUser)
                .HasColumnType("character varying")
                .HasColumnName("modify_user");
            entity.Property(e => e.SourceType)
                .HasMaxLength(10)
                .HasColumnName("source_type");
            entity.Property(e => e.Status)
                .HasColumnType("char")
                .HasColumnName("status");
            entity.Property(e => e.VersionCode).HasColumnName("version_code");
        });

        modelBuilder.Entity<KmuAttribute>(entity =>
        {
            entity.HasKey(e => e.AttrCode).HasName("kmu_attribute_pkey");

            entity.ToTable("kmu_attribute");

            entity.Property(e => e.AttrCode)
                .HasMaxLength(3)
                .HasComment("身分代碼")
                .HasColumnName("attr_code");
            entity.Property(e => e.AttrName)
                .HasMaxLength(100)
                .HasComment("身分說明")
                .HasColumnName("attr_name");
            entity.Property(e => e.AttrRegFee)
                .HasComment("該身分預收的掛號費用")
                .HasColumnName("attr_reg_fee");
            entity.Property(e => e.AttrStatus)
                .HasMaxLength(1)
                .HasComment("啟用狀態")
                .HasColumnName("attr_status");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_time");
            entity.Property(e => e.ModifyUser)
                .HasMaxLength(7)
                .HasColumnName("modify_user");
        });

        modelBuilder.Entity<KmuChart>(entity =>
        {
            entity.HasKey(e => e.ChrHealthId).HasName("KMUCHART_pkey");

            entity.ToTable("kmu_chart");

            entity.HasIndex(e => new { e.ChrPatientFirstname, e.ChrPatientMidname, e.ChrPatientLastname }, "idx_kmu_chart_01");

            entity.HasIndex(e => e.ChrMobilePhone, "idx_kmu_chart_02");

            entity.Property(e => e.ChrHealthId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("chr_health_id");
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
                .IsFixedLength()
                .HasColumnName("chr_contact_relation");
            entity.Property(e => e.ChrEmgContact)
                .HasMaxLength(650)
                .HasColumnName("chr_emg_contact");
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
                .HasDefaultValueSql("'N'::bpchar")
                .HasComment("refugee: Y")
                .HasColumnName("chr_refugee_flag");
            entity.Property(e => e.ChrRemark)
                .HasMaxLength(1000)
                .HasColumnName("chr_remark");
            entity.Property(e => e.ChrSex)
                .HasMaxLength(1)
                .HasColumnName("chr_sex");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_time");
            entity.Property(e => e.ModifyUser)
                .HasMaxLength(7)
                .IsFixedLength()
                .HasColumnName("modify_user");
            entity.Property(e => e.UploadStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'N'::character varying")
                .HasColumnName("upload_status");
            entity.Property(e => e.UploadTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("upload_time");
        });

        modelBuilder.Entity<KmuChartLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("KMUCHARTLOG_pkey");

            entity.ToTable("kmu_chart_log");

            entity.HasIndex(e => e.ChrHealthId, "idx_kmu_chart_log_01");

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
                .IsFixedLength()
                .HasColumnName("chr_contact_relation");
            entity.Property(e => e.ChrEmgContact)
                .HasMaxLength(650)
                .HasColumnName("chr_emg_contact");
            entity.Property(e => e.ChrHealthId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("chr_health_id");
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
                .HasComment("Command Mode: Insert, Update, Delete")
                .HasColumnName("log_mode");
            entity.Property(e => e.LogTime)
                .HasComment("Modify Time")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("log_time");
            entity.Property(e => e.LogUser)
                .HasMaxLength(7)
                .IsFixedLength()
                .HasComment("Modify by User")
                .HasColumnName("log_user");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_time");
            entity.Property(e => e.ModifyUser)
                .HasMaxLength(7)
                .IsFixedLength()
                .HasColumnName("modify_user");
        });

        modelBuilder.Entity<KmuChartMergeHistory>(entity =>
        {
            entity.ToTable("kmu_chart_MergeHistory");

            entity.Property(e => e.ChrHalthId).HasColumnName("chr_halth_id");
            //entity.Property(e => e.CreateTime)
            //    .HasColumnType("timestamp without time zone")
            //    .HasColumnName("create_time");
            entity.Property(e => e.MergedTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("merged_time");
            entity.Property(e => e.MergerUser).HasColumnName("merger_user");
            entity.Property(e => e.MhHealthId).HasColumnName("mh_health_id");
            entity.Property(e => e.UploadStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'N'::character varying")
                .HasColumnName("upload_status");
            entity.Property(e => e.UploadTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("upload_time");
        });

        modelBuilder.Entity<KmuCoderef>(entity =>
        {
            entity.HasKey(e => e.RefId).HasName("kmu_coderef_pkey");

            entity.ToTable("kmu_coderef");

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
                .HasComment("是否預設啟用")
                .HasColumnName("ref_default_flag");
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

        modelBuilder.Entity<KmuCondition>(entity =>
        {
            entity.HasKey(e => new { e.CndCodetype, e.CndCode }).HasName("kmu_condition_pkey");

            entity.ToTable("kmu_condition");

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
                .IsFixedLength()
                .HasColumnName("cnd_symbol1");
            entity.Property(e => e.CndSymbol2)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("cnd_symbol2");
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

        modelBuilder.Entity<KmuDepartment>(entity =>
        {
            entity.HasKey(e => e.DptCode).HasName("kmu_department_pkey");

            entity.ToTable("kmu_department");

            entity.Property(e => e.DptCode)
                .HasMaxLength(6)
                .HasColumnName("dpt_code");
            entity.Property(e => e.DptCategory)
                .HasMaxLength(3)
                .HasColumnName("dpt_category");
            entity.Property(e => e.DptDefaultAttr)
                .HasMaxLength(3)
                .HasComment("預設身分別")
                .HasColumnName("dpt_default_attr");
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

        modelBuilder.Entity<KmuIcd>(entity =>
        {
            entity.HasKey(e => e.IcdCode).HasName("KMUICD_pkey");

            entity.ToTable("kmu_icd", tb => tb.HasComment("Diagnosis data."));

            entity.HasIndex(e => e.ParentCode, "idx_icd_01").HasOperators(new[] { "varchar_ops" });

            entity.Property(e => e.IcdCode)
                .HasMaxLength(8)
                .HasColumnName("icd_code");
            entity.Property(e => e.Dhis2Code).HasColumnName("dhis2_code");
            entity.Property(e => e.IcdCodeUndot)
                .HasMaxLength(7)
                .HasComment("ICD Code without decimal point.")
                .HasColumnName("icd_code_undot");
            entity.Property(e => e.IcdEnglishName)
                .HasMaxLength(400)
                .HasColumnName("icd_english_name");
            entity.Property(e => e.IcdType)
                .HasMaxLength(10)
                .HasComment("CM/PCS")
                .HasColumnName("icd_type");
            entity.Property(e => e.ModifyDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_date");
            entity.Property(e => e.ModifyUser)
                .HasMaxLength(7)
                .HasColumnName("modify_user");
            entity.Property(e => e.ParentCode)
                .HasMaxLength(8)
                .HasComment("Parent ICD Code for HisOrder UI Design.")
                .HasColumnName("parent_code");
            entity.Property(e => e.ShowMode)
                .HasMaxLength(10)
                .HasComment("Show position for HisOrder UI Design.")
                .HasColumnName("show_mode");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .HasComment("ICD Code.")
                .HasColumnName("status");
            entity.Property(e => e.Versioncode)
                .HasMaxLength(2)
                .HasComment("ICD 9 / ICD 10 ...")
                .HasColumnName("versioncode");
        });

        modelBuilder.Entity<KmuMedfrequency>(entity =>
        {
            entity.HasKey(e => e.FrqCode).HasName("kmu_medfrequency_pkey");

            entity.ToTable("kmu_medfrequency");

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
                .HasMaxLength(100)
                .HasColumnName("freq_desc");
            entity.Property(e => e.FrqForDays).HasColumnName("frq_for_days");
            entity.Property(e => e.FrqForTimes).HasColumnName("frq_for_times");
            entity.Property(e => e.FrqOneDayTimes).HasColumnName("frq_one_day_times");
            entity.Property(e => e.FrqSeqNo)
                .HasDefaultValueSql("999")
                .HasColumnName("frq_seq_no");
            entity.Property(e => e.ModifyUser)
                .HasMaxLength(7)
                .HasColumnName("modify_user");
        });

        modelBuilder.Entity<KmuMedfrequencyInd>(entity =>
        {
            entity.HasKey(e => new { e.FrqCode, e.IndCode }).HasName("kmu_medfrequency_ind_pkey");

            entity.ToTable("kmu_medfrequency_ind");

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

        modelBuilder.Entity<KmuMedicine>(entity =>
        {
            entity.HasKey(e => e.MedId).HasName("KMUMEDICINE_pkey");

            entity.ToTable("kmu_medicine");

            entity.Property(e => e.MedId)
                .HasMaxLength(10)
                .HasColumnName("med_id");
            entity.Property(e => e.BrandName)
                .HasMaxLength(150)
                .HasColumnName("brand_name");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_date");
            entity.Property(e => e.CreateUser)
                .HasMaxLength(7)
                .HasColumnName("create_user");
            entity.Property(e => e.DefaultFreq)
                .HasMaxLength(10)
                .HasComment("開立時預設頻次(可空白)")
                .HasColumnName("default_freq");
            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_date");
            entity.Property(e => e.GenericName)
                .HasMaxLength(150)
                .HasColumnName("generic_name");
            entity.Property(e => e.MedType)
                .HasMaxLength(2)
                .HasComment("1-口服\n2-針劑\n3-外用")
                .HasColumnName("med_type");
            entity.Property(e => e.ModifyDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_date");
            entity.Property(e => e.ModifyUser)
                .HasMaxLength(7)
                .HasColumnName("modify_user");
            entity.Property(e => e.PackSpec)
                .HasMaxLength(20)
                .HasComment("包裝單位(藥局發藥)")
                .HasColumnName("pack_spec");
            entity.Property(e => e.RefDuration)
                .HasMaxLength(3)
                .HasComment("建議的用藥天數(不用預設)")
                .HasColumnName("ref_duration");
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .HasComment("其他備註說明")
                .HasColumnName("remarks");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .HasDefaultValueSql("'0'::bpchar")
                .HasComment("醫囑系統是否顯示")
                .HasColumnName("status");
            entity.Property(e => e.UnitSpec)
                .HasMaxLength(20)
                .HasComment("醫囑單位")
                .HasColumnName("unit_spec");
        });

        modelBuilder.Entity<KmuMedpathway>(entity =>
        {
            entity.HasKey(e => new { e.MedType, e.PathCode }).HasName("kmu_medpathway_pkey");

            entity.ToTable("kmu_medpathway");

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
                .HasMaxLength(100)
                .HasColumnName("path_desc");
            entity.Property(e => e.Showseq)
                .HasPrecision(3)
                .HasColumnName("showseq");
        });

        modelBuilder.Entity<KmuNonMedicine>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("kmu_non_medicine_pkey");

            entity.ToTable("kmu_non_medicine");

            entity.Property(e => e.ItemId)
                .HasMaxLength(10)
                .HasColumnName("item_id");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_date");
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
                .HasMaxLength(150)
                .HasColumnName("item_name");
            entity.Property(e => e.ItemSpec)
                .HasMaxLength(20)
                .HasColumnName("item_spec");
            entity.Property(e => e.ItemType)
                .HasMaxLength(10)
                .HasComment("5.Laboratory 6.Radiology 7.Pathology 8.Material")
                .HasColumnName("item_type");
            entity.Property(e => e.ModifyDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_date");
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
                .HasDefaultValueSql("'0'::bpchar")
                .HasColumnName("status");
        });

        modelBuilder.Entity<KmuUploadLog>(entity =>
        {
            entity.HasKey(e => e.Logid).HasName("kmu_upload_log_pkey");

            entity.ToTable("kmu_upload_log");

            entity.Property(e => e.Logid)
                .ValueGeneratedNever()
                .HasColumnName("logid");
            entity.Property(e => e.ExecBatchSeqNo).HasColumnName("exec_batch_seq_no");
            entity.Property(e => e.ExecDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("exec_datetime");
            entity.Property(e => e.Inhospid)
                .HasColumnType("character varying")
                .HasColumnName("inhospid");
            entity.Property(e => e.LocalIp)
                .HasColumnType("character varying")
                .HasColumnName("local_ip");
            entity.Property(e => e.LocalLoginUser)
                .HasColumnType("character varying")
                .HasColumnName("local_login_user");
            entity.Property(e => e.Option)
                .HasColumnType("char")
                .HasColumnName("option");
            entity.Property(e => e.RegDate).HasColumnName("reg_date");
            entity.Property(e => e.ResultMessage).HasColumnName("result_message");
            entity.Property(e => e.ResultStatusCode)
                .HasColumnType("character varying")
                .HasColumnName("result_status_code");
            entity.Property(e => e.ResultStatusDesc)
                .HasColumnType("character varying")
                .HasColumnName("result_status_desc");
            entity.Property(e => e.ResultSuccess).HasColumnName("result_success");
            entity.Property(e => e.TargetAgency)
                .HasColumnType("character varying")
                .HasColumnName("target_agency");
            entity.Property(e => e.TargetUrl)
                .HasColumnType("character varying")
                .HasColumnName("target_url");
        });

        modelBuilder.Entity<KmuUser>(entity =>
        {
            entity.HasKey(e => e.UserIdno).HasName("kmu_users_pkey");

            entity.ToTable("kmu_users", tb => tb.HasComment("Account Basic File(user account )"));

            entity.Property(e => e.UserIdno)
                .HasMaxLength(7)
                .HasColumnName("user_idno");
            entity.Property(e => e.AccountStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("1")
                .HasColumnName("account_status");
            entity.Property(e => e.CreateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_time");
            entity.Property(e => e.Creator)
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
                .HasMaxLength(1)
                .HasComment("分類(1:Doctor,2:Nurse,3.Staff")
                .HasColumnName("user_category");
            entity.Property(e => e.UserEmail).HasColumnName("user_email");
            entity.Property(e => e.UserMobilePhone)
                .HasMaxLength(30)
                .HasColumnName("user_mobile_phone");
            entity.Property(e => e.UserNameFirstname).HasColumnName("user_name_firstname");
            entity.Property(e => e.UserNameLastname).HasColumnName("user_name_lastname");
            entity.Property(e => e.UserNameMidname).HasColumnName("user_name_midname");
            entity.Property(e => e.UserPassword).HasColumnName("user_password");
            entity.Property(e => e.UserSex).HasColumnName("user_sex");
        });

        modelBuilder.Entity<PhysicalSign>(entity =>
        {
            entity.HasKey(e => e.PhyId).HasName("physical_sign_pkey");

            entity.ToTable("physical_sign");

            entity.HasIndex(e => new { e.Inhospid, e.PhyType }, "idx_physical_sign_01");

            entity.Property(e => e.PhyId)
                .HasMaxLength(50)
                .HasColumnName("phy_id");
            entity.Property(e => e.Inhospid)
                .HasMaxLength(20)
                .HasColumnName("inhospid");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_time");
            entity.Property(e => e.ModifyUser)
                .HasMaxLength(7)
                .HasColumnName("modify_user");
            entity.Property(e => e.PhyType)
                .HasMaxLength(30)
                .HasColumnName("phy_type");
            entity.Property(e => e.PhyValue)
                .HasMaxLength(500)
                .HasColumnName("phy_value");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => new { e.RegDate, e.RegDepartment, e.RegNoon, e.RegSeqNo }).HasName("registration_pkey");

            entity.ToTable("registration");

            entity.HasIndex(e => e.Inhospid, "idx_registration_01");

            entity.Property(e => e.RegDate)
                .HasComment("看診日")
                .HasColumnName("reg_date");
            entity.Property(e => e.RegDepartment)
                .HasMaxLength(6)
                .HasComment("看診科別")
                .HasColumnName("reg_department");
            entity.Property(e => e.RegNoon)
                .HasMaxLength(5)
                .HasComment("午別")
                .HasColumnName("reg_noon");
            entity.Property(e => e.RegSeqNo)
                .HasComment("門診:看診號\n急診:檢傷序號")
                .HasColumnName("reg_seq_no");
            entity.Property(e => e.Inhospid)
                .HasMaxLength(20)
                .HasComment("就醫序號")
                .HasColumnName("inhospid");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_time");
            entity.Property(e => e.ModifyUser)
                .HasMaxLength(7)
                .HasColumnName("modify_user");
            entity.Property(e => e.RegAttrDesc)
                .HasMaxLength(200)
                .HasComment("身分備註")
                .HasColumnName("reg_attr_desc");
            entity.Property(e => e.RegAttribute)
                .HasMaxLength(3)
                .HasComment("特殊身分->參考kmu_attribute")
                .HasColumnName("reg_attribute");
            entity.Property(e => e.RegBedNo)
                .HasMaxLength(3)
                .HasComment("急診床號")
                .HasColumnName("reg_bed_no");
            entity.Property(e => e.RegCallTime)
                .HasComment("叫號時間")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reg_call_time");
            entity.Property(e => e.RegCreateTime)
                .HasComment("create datetime for registration ")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reg_create_time");
            entity.Property(e => e.RegDoctorId)
                .HasMaxLength(7)
                .HasComment("醫師職邊")
                .HasColumnName("reg_doctor_id");
            entity.Property(e => e.RegEndTime)
                .HasComment("結束看診時間")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reg_end_time");
            entity.Property(e => e.RegExamEndTime)
                .HasComment("finish examining return time")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reg_exam_end_time");
            entity.Property(e => e.RegExamStartTime)
                .HasComment("click examining start time")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reg_exam_start_time");
            entity.Property(e => e.RegFollowCode)
                .HasMaxLength(20)
                .HasColumnName("reg_follow_code");
            entity.Property(e => e.RegFollowDesc)
                .HasMaxLength(500)
                .HasColumnName("reg_follow_desc");
            entity.Property(e => e.RegHealthId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasComment("病歷號")
                .HasColumnName("reg_health_id");
            entity.Property(e => e.RegRoomNo)
                .HasMaxLength(3)
                .HasComment("診間號")
                .HasColumnName("reg_room_no");
            entity.Property(e => e.RegStartTime)
                .HasComment("開始看診時間")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reg_start_time");
            entity.Property(e => e.RegStatus)
                .HasMaxLength(1)
                .HasComment("看診狀態\nN:未看診\nT :暫存\n* :已看診\nC:取消掛號")
                .HasColumnName("reg_status");
            entity.Property(e => e.RegTriage)
                .HasMaxLength(1)
                .HasComment("檢傷分類\n0：一般門診(白燈)\n1：急診分類(綠燈)\n2：急診分類(黃燈)\n3：急診分類(紅燈)\n4：急診分類(黑燈)")
                .HasColumnName("reg_triage");
            entity.Property(e => e.Score)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("score");
            entity.Property(e => e.UploadStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'N'::character varying")
                .HasColumnName("upload_status");
            entity.Property(e => e.UploadTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("upload_time");
        });

        modelBuilder.HasSequence("hisorderplan_soa_soaid_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
