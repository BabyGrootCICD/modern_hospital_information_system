using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KMU.Lab.WebAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace KMU.Lab.WebAPI.Data
{
    public partial class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }


        public DbSet<KmuChart> KmuChart { get; set; }
        public virtual DbSet<Hisorderplan> Hisorderplans { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin<string>>().HasNoKey();
            modelBuilder.Entity<IdentityUserRole<string>>().HasNoKey();
            modelBuilder.Entity<IdentityUserToken<string>>().HasNoKey();
            modelBuilder.Entity<IdentityUserClaim<string>>().HasNoKey();
            modelBuilder.Entity<KmuChart>(entity =>
            {
                entity.HasKey(e => e.HealthId).HasName("KMUCHART_pkey");

                entity.ToTable("kmu_chart");

                entity.HasIndex(e => new { e.PatientFirstname, e.PatientMidname, e.PatientLastname }, "idx_kmu_chart_01");

                entity.HasIndex(e => e.MobilePhone, "idx_kmu_chart_02");

                entity.Property(e => e.HealthId)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .HasColumnName("chr_health_id");
                entity.Property(e => e.Address).HasColumnName("chr_address");
        
                entity.Property(e => e.BirthDate).HasColumnName("chr_birth_date");
           
         
                entity.Property(e => e.MobilePhone)
                    .HasMaxLength(30)
                    .HasColumnName("chr_mobile_phone");
   
                entity.Property(e => e.PatientFirstname)
                    .HasMaxLength(200)
                    .HasColumnName("chr_patient_firstname");
                entity.Property(e => e.PatientLastname)
                    .HasMaxLength(200)
                    .HasColumnName("chr_patient_lastname");
                entity.Property(e => e.PatientMidname)
                    .HasMaxLength(200)
                    .HasColumnName("chr_patient_midname");
       
                entity.Property(e => e.Sex)
                    .HasMaxLength(1)
                    .HasColumnName("chr_sex");
                entity.Property(e => e.ModifyTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modify_time");      

            });

            modelBuilder.Entity<Hisorderplan>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("hisorderplan_pkey");

                entity.ToTable("hisorderplan");              

                entity.HasIndex(e => e.TestOrderId, "idx_hisorderplan_02")
                    .HasOperators(new[] { "bpchar_pattern_ops" })
                    .UseCollation(new[] { "C" });

                entity.HasIndex(e => e.PatientId, "idx_hisorderplan_01")
                    .UseCollation(new[] { "C" });

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("orderplanid");

                entity.Property(e => e.PatientId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("health_id")
                    .IsFixedLength();

                entity.Property(e => e.CreateDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.CreateUser)
                    .HasMaxLength(7)
                    .HasColumnName("create_user");         


                entity.Property(e => e.TestType)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("hplan_type");

                entity.Property(e => e.TestOrderId)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("inhospid");

                entity.Property(e => e.TestCode)
                    .HasMaxLength(20)
                    .HasColumnName("plan_code");

                entity.Property(e => e.TestName)
                    .HasMaxLength(150)
                    .HasColumnName("plan_des");

                entity.Property(e => e.Remark)
                    .HasMaxLength(300)
                    .HasColumnName("remark");  

                entity.Property(e => e.Status)
                    .HasMaxLength(1)
                    .HasColumnName("status")
                    .HasDefaultValueSql("'0'::bpchar");

            });

            OnModelCreatingPartial(modelBuilder);
        }



        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}