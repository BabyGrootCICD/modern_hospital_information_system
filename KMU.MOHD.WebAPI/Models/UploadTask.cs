
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU.MOHD.WebAPI.Models
{
    public class UploadTask
    {

    }




    //public class UploadTask
    //{
    public class SenderInfo
    {
        public string? IP { get; set; }
        public string? Name { get; set; }
    }

    public class HospInfo : MohdHosp
    {
        //public string? HospCode { get; set; }
        //public string? HospName { get; set; }
        //public string? HospAddress { get; set; }
        //public string? HospTel { get; set; }
        //public string? HKey { get; set; }
    }

    public class RegInfo : MohdRegistration
    {
        //    public string? Date { get; set; }
        //    public string? DeptCode { get; set; }
        //    public string? DeptName { get; set; }
        //    public string? Noon { get; set; }
        //    public string? Seq { get; set; }
        //    public string? HealthID { get; set; }
        //    public string? InhospID { get; set; }
        //    public string? Triage { get; set; }
        //    public string? Bed { get; set; }
        //    public string? Attr { get; set; }
        //    public string? AttrDes { get; set; }
        //    public string? DrID { get; set; }
        //    public string? DrName { get; set; }
        //    public string? Room { get; set; }
        //    public string? Status { get; set; }
        //    public string? RegCall { get; set; }
        //    public string? RegStart { get; set; }
        //    public string? RegEnd { get; set; }
        //    public string? ExamStart { get; set; }
        //    public string? ExamEnd { get; set; }
        //    public string? Create { get; set; }
        //    public string? FollowCode { get; set; }
        //    public string? FollowDes { get; set; }
    }

    public class MergePatient
    {
        public int Id { get; set; }

        public string? ChrHalthId { get; set; }

        public string? MhHealthId { get; set; }

        //public DateTime CreateTime { get; set; }

        public DateTime MergedTime { get; set; }

        public string? MergerUser { get; set; }
        public string? upload_status { get; set; }
        public DateTime? upload_time { get; set; }
        public string? FromHosp { get; set; }
    }


    public class PatientInfo : MohdChart
    {
        //public string? HealthID { get; set; }
        //public string? NationalID { get; set; }
        //public string? FName { get; set; }
        //public string? MName { get; set; }
        //public string? LName { get; set; }
        //public string? Sex { get; set; }
        //public string? BirthDate { get; set; }
        //public string? Mobile { get; set; }
        //public string? Address { get; set; }
        //public string? EC { get; set; }
        //public string? CR { get; set; }
        //public string? CPhone { get; set; }
        //public string? Combine { get; set; }
        //public string? OldHealthID { get; set; }
        //public string? Remark { get; set; }
        //public string? Modify { get; set; }
        //public string? Area { get; set; }
        //public string? Refuee { get; set; }
    }

    public class SOAP : MohdHisordersoa
    {
        //public long SoaID { get; set; }
        //public string? InhospID { get; set; }
        //public string? HealthID { get; set; }
        //public string? Kind { get; set; }
        //public string? Context { get; set; }
        //public string? Create { get; set; }
        //public string? Modify { get; set; }
        //public string? Source { get; set; }
        //public int Version { get; set; }
        //public string? Status { get; set; }
    }
    //public class OrderPlan : MohdHisorderplan
    //{
        //public string? HospCode { get; set; }
        //public long PlanID { get; set; }
        //public string? InhospID { get; set; }
        //public string? HealthID { get; set; }
        //public string? PlanType { get; set; }
        //public int SeqNo { get; set; }
        //public string? PlanCode { get; set; }
        //public string? PlanDes { get; set; }
        //public string? Free { get; set; }
        //public string? ExecDateFrom { get; set; }
        //public string? ExecDateTo { get; set; }
        //public string? OrderDept { get; set; }
        //public string? OrderDr { get; set; }
        //public decimal? PlanDays { get; set; }
        //public decimal? QtyDose { get; set; }
        //public decimal? QtyDaily { get; set; }
        //public string? UnitDose { get; set; }
        //public string? FreqCode { get; set; }
        //public string? DoseIndi { get; set; }
        //public string? DosePath { get; set; }
        //public string? MadeType { get; set; }
        //public decimal? TotalQty { get; set; }
        //public string? ExamLoc { get; set; }
        //public string? Urg { get; set; }
        //public string? Preop { get; set; }
        //public string? Add { get; set; }
        //public string? KeepSpec { get; set; }
        //public string? Location { get; set; }
        //public string? TrigTable { get; set; }
        //public string? TrigRecID { get; set; }
        //public string? Status { get; set; }
        //public string? ExStauts { get; set; }
        //public string? ChgStatus { get; set; }
        //public string? Create { get; set; }
        //public string? Modify { get; set; }
        //public string? Remark { get; set; }
        //public short? MedBag { get; set; }
    //}

    public partial class Dx : MohdHisorderplan { };

    public partial class Med : MohdHisorderplan { };

    public partial class Others : MohdHisorderplan { };

    public class Root
    {
        public SenderInfo SenderInfo { get; set; }
        public HospInfo HospInfo { get; set; }
        public RegInfo RegInfo { get; set; }
        public PatientInfo PatientInfo { get; set; }
        public List<SOAP> SOAP { get; set; }
        public List<MohdHisorderplan> Dx { get; set; }
        public List<MohdHisorderplan> Med { get; set; }
        public List<MohdHisorderplan> Others { get; set; }
    }
    //}
}
