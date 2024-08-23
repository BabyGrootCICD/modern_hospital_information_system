using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;



namespace KMU.HisOrder.Console.Gateway.Models_MOHD;

public partial class MOHDGWContext : DbContext
{
    public MOHDGWContext()
    {
    }

    public MOHDGWContext(DbContextOptions<MOHDGWContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MohdChart> MohdCharts { get; set; }

    public virtual DbSet<MohdHisorderplan> MohdHisorderplans { get; set; }

    public virtual DbSet<MohdHisordersoa> MohdHisordersoas { get; set; }

    public virtual DbSet<MohdHosp> MohdHosps { get; set; }

    public virtual DbSet<MohdLogentry> MohdLogentries { get; set; }

    public virtual DbSet<MohdRegistration> MohdRegistrations { get; set; }

    public virtual DbSet<MohdUser> MohdUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.

        var configuration = new ConfigurationBuilder().
            SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
           .AddJsonFile("appsettings.json")
           .Build();

#if DEBUG

        var mohdGWConnectionString = configuration.GetSection("ConnectionStrings:MOHDGW_Connection")["Debug"];
#else
   
        var mohdGWConnectionString = configuration.GetSection("ConnectionStrings:MOHDGW_Connection")["Release"];
#endif

        optionsBuilder.UseNpgsql(mohdGWConnectionString);


   

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MohdChart>(entity =>
        {
            entity.HasKey(e => new { e.HealthId, e.FromHosp }).HasName("mohd_chart_pkey");

            entity.ToTable("mohd_chart");

            entity.Property(e => e.HealthId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("health_id");
            entity.Property(e => e.FromHosp)
                .HasMaxLength(20)
                .HasColumnName("from_hosp");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.AreaCode)
                .HasMaxLength(20)
                .HasColumnName("area_code");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CombineFlag)
                .HasMaxLength(1)
                .HasColumnName("combine_flag");
            entity.Property(e => e.ContPhone)
                .HasMaxLength(30)
                .HasColumnName("cont_phone");
            entity.Property(e => e.ContRel)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("cont_rel");
            entity.Property(e => e.EmgCont)
                .HasMaxLength(650)
                .HasColumnName("emg_cont");
            entity.Property(e => e.FName)
                .HasMaxLength(200)
                .HasColumnName("f_name");
            entity.Property(e => e.LName)
                .HasMaxLength(200)
                .HasColumnName("l_name");
            entity.Property(e => e.MName)
                .HasMaxLength(200)
                .HasColumnName("m_name");
            entity.Property(e => e.Mobile)
                .HasMaxLength(30)
                .HasColumnName("mobile");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_time");
            entity.Property(e => e.NationalId)
                .HasMaxLength(10)
                .HasColumnName("national_id");
            entity.Property(e => e.RefugeeFlag)
                .HasMaxLength(1)
                .HasColumnName("refugee_flag");
            entity.Property(e => e.Remark)
                .HasMaxLength(1000)
                .HasColumnName("remark");
            entity.Property(e => e.Sex)
                .HasMaxLength(1)
                .HasColumnName("sex");
            entity.Property(e => e.UploadTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("upload_time");
        });

        modelBuilder.Entity<MohdHisorderplan>(entity =>
        {
            entity.HasKey(e => new { e.HospCode, e.Orderplanid }).HasName("mohd_hisorderplan _pkey");

            entity.ToTable("mohd_hisorderplan");

            entity.Property(e => e.HospCode)
                .HasMaxLength(20)
                .HasColumnName("hosp_code");
            entity.Property(e => e.Orderplanid).HasColumnName("orderplanid");
            entity.Property(e => e.AddFlag)
                .HasMaxLength(1)
                .HasColumnName("add_flag");
            entity.Property(e => e.ChargeStatus)
                .HasMaxLength(1)
                .HasColumnName("charge_status");
            entity.Property(e => e.CreateDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_date");
            entity.Property(e => e.DoseIndi)
                .HasMaxLength(20)
                .HasColumnName("dose_indi");
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
            entity.Property(e => e.PlanDays).HasColumnName("plan_days");
            entity.Property(e => e.PlanDes)
                .HasMaxLength(150)
                .HasColumnName("plan_des");
            entity.Property(e => e.PreopFlag)
                .HasMaxLength(1)
                .HasColumnName("preop_flag");
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
                .HasColumnName("urg_flag");
        });

        modelBuilder.Entity<MohdHisordersoa>(entity =>
        {
            entity.HasKey(e => new { e.HospCode, e.Soaid }).HasName("mohd_hisordersoa _pkey");

            entity.ToTable("mohd_hisordersoa");

            entity.Property(e => e.HospCode)
                .HasMaxLength(20)
                .HasColumnName("hosp_code");
            entity.Property(e => e.Soaid).HasColumnName("soaid");
            entity.Property(e => e.Context).HasColumnName("context");
            entity.Property(e => e.CreateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_time");
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
            entity.Property(e => e.ModifyTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modify_time");
            entity.Property(e => e.SourceType)
                .HasMaxLength(10)
                .HasColumnName("source_type");
            entity.Property(e => e.Status)
                .HasColumnType("char")
                .HasColumnName("status");
            entity.Property(e => e.VersionCode).HasColumnName("version_code");
        });

        modelBuilder.Entity<MohdHosp>(entity =>
        {
            entity.HasKey(e => e.HospCode).HasName("mohd_hosp_pkey");

            entity.ToTable("mohd_hosp");

            entity.Property(e => e.HospCode)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("hosp_code");
            entity.Property(e => e.HospAddress).HasColumnName("hosp_address");
            entity.Property(e => e.HospName)
                .HasColumnType("character varying")
                .HasColumnName("hosp_name");
            entity.Property(e => e.HospTel)
                .HasMaxLength(30)
                .HasColumnName("hosp_tel");
            entity.Property(e => e.HospUploadKey)
                .HasColumnType("character varying")
                .HasColumnName("hosp_upload_key");
        });

        modelBuilder.Entity<MohdLogentry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mohd_logentries_pkey");

            entity.ToTable("mohd_logentries");

            entity.HasIndex(e => new { e.Createtime, e.ResultSuccess }, "idx_createtime_other_cols1");

            entity.HasIndex(e => e.Ipaddress, "idx_ipaddress");

            entity.HasIndex(e => e.ResultSuccess, "idx_result_success");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createtime");
            entity.Property(e => e.Endpoint)
                .HasColumnType("character varying")
                .HasColumnName("endpoint");
            entity.Property(e => e.Exception).HasColumnName("exception");
            entity.Property(e => e.Ipaddress)
                .HasColumnType("character varying")
                .HasColumnName("ipaddress");
            entity.Property(e => e.Loglevel)
                .HasColumnType("character varying")
                .HasColumnName("loglevel");
            entity.Property(e => e.Requestdata)
                .HasColumnType("jsonb")
                .HasColumnName("requestdata");
            entity.Property(e => e.ResultMessage).HasColumnName("result_message");
            entity.Property(e => e.ResultSuccess).HasColumnName("result_success");
        });

        modelBuilder.Entity<MohdRegistration>(entity =>
        {
            entity.HasKey(e => new { e.HospCode, e.RegDate, e.Inhospid }).HasName("mohd_registration _pkey");

            entity.ToTable("mohd_registration");

            entity.Property(e => e.HospCode)
                .HasMaxLength(20)
                .HasColumnName("hosp_code");
            entity.Property(e => e.RegDate).HasColumnName("reg_date");
            entity.Property(e => e.Inhospid)
                .HasMaxLength(20)
                .HasColumnName("inhospid");
            entity.Property(e => e.AttrDesc)
                .HasMaxLength(200)
                .HasColumnName("attr_desc");
            entity.Property(e => e.BedNo)
                .HasMaxLength(3)
                .HasColumnName("bed_no");
            entity.Property(e => e.CallTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("call_time");
            entity.Property(e => e.CreateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_time");
            entity.Property(e => e.DeptCode)
                .HasMaxLength(6)
                .HasColumnName("dept_code");
            entity.Property(e => e.DeptName)
                .HasMaxLength(150)
                .HasColumnName("dept_name");
            entity.Property(e => e.DoctorId)
                .HasMaxLength(7)
                .HasColumnName("doctor_id");
            entity.Property(e => e.DoctorName)
                .HasColumnType("character varying")
                .HasColumnName("doctor_name");
            entity.Property(e => e.EndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_time");
            entity.Property(e => e.ExamEndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("exam_end_time");
            entity.Property(e => e.ExamStartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("exam_start_time");
            entity.Property(e => e.FollowCode)
                .HasMaxLength(20)
                .HasColumnName("follow_code");
            entity.Property(e => e.FollowDesc)
                .HasMaxLength(500)
                .HasColumnName("follow_desc");
            entity.Property(e => e.HealthId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("health_id");
            entity.Property(e => e.Noon)
                .HasMaxLength(5)
                .HasColumnName("noon");
            entity.Property(e => e.RegAttribute)
                .HasMaxLength(3)
                .HasColumnName("reg_attribute");
            entity.Property(e => e.RoomNo)
                .HasMaxLength(3)
                .HasColumnName("room_no");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.StartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .HasColumnName("status");
            entity.Property(e => e.Triage)
                .HasMaxLength(1)
                .HasColumnName("triage");
            entity.Property(e => e.UploadTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("upload_time");
        });

        modelBuilder.Entity<MohdUser>(entity =>
        {
            entity.HasKey(e => e.UserIdno).HasName("mohd_users_pkey");

            entity.ToTable("mohd_users", tb => tb.HasComment("Account Basic File(user account )"));

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
                .HasComment("分類(1:Personal,2:System")
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
        modelBuilder.HasSequence<int>("mohd_logentries_id_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
