using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHIS.HisOrder.Console.Gateway.Models
{
    public class UploadTask
    {

    }


    public class ExportData 
    {
        public List<Root> roots = new List<Root>(); 
        public List<PatientInfo> patientinfos  = new List<PatientInfo>();
        public List<MergePatient> mergerecords = new List<MergePatient>();

        //// 屬性來存儲清單屬性的項目數量
        //public int RootsCount { get { return roots.Count; } }
        //public int PatientInfosCount { get { return patientinfos.Count; } }
        //public int MergeRecordsCount { get { return mergerecords.Count; } }

        //// 屬性來計算清單項目的總數量
        //public int TotalCount
        //{
        //    get
        //    {
        //        return RootsCount + PatientInfosCount + MergeRecordsCount;
        //    }
        //}
    }



    //public class UploadTask
    //{
    public class SenderInfo
    {
        public string? IP { get; set; }
        public string? Name { get; set; }
    }

    public class HospInfo
    {
        public string HospCode { get; set; } = null!;

        public string? HospName { get; set; }

        public string? HospAddress { get; set; }

        public string? HospTel { get; set; }

        public string? HospUploadKey { get; set; }
    }


    public class RegInfo
    {
        public DateOnly RegDate { get; set; }

        public string? DeptCode { get; set; }

        public string? DeptName { get; set; }

        public string? Noon { get; set; }

        public short? SeqNo { get; set; }

        public string? HealthId { get; set; }

        public string? Inhospid { get; set; }

        public string? Triage { get; set; }

        public string? BedNo { get; set; }

        public string? RegAttribute { get; set; }

        public string? DoctorId { get; set; }

        public string? DoctorName { get; set; }

        public string? RoomNo { get; set; }

        public string? Status { get; set; }

        public DateTime? CallTime { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? FollowCode { get; set; }

        public string? FollowDesc { get; set; }

        public string? AttrDesc { get; set; }

        public DateTime? ExamStartTime { get; set; }

        public DateTime? ExamEndTime { get; set; }

        public DateTime? CreateTime { get; set; }

        public string? HospCode { get; set; }

        public DateTime? UploadTime { get; set; }
        public string? dept_parent { get; set; }
        public string? dept_parent_name { get; set; }
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

    public class patientLog 
    {
        public string HospCode { get; set; }
        public string PatientNumber { get; set; }
    }


    public class PatientInfo
    {
        public string HealthId { get; set; } = null!;

        public string? NationalId { get; set; }

        public string? FName { get; set; }

        public string? MName { get; set; }

        public string? LName { get; set; }

        public string? Sex { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? Mobile { get; set; }

        public string? Address { get; set; }

        public string? EmgCont { get; set; }

        public string? ContRel { get; set; }

        public string? ContPhone { get; set; }

        public char? CombineFlag { get; set; }

        public string? Remark { get; set; }

        public DateTime? ModifyTime { get; set; }

        public string? AreaCode { get; set; }

        public char? RefugeeFlag { get; set; }

        public string? FromHosp { get; set; }

        public DateTime? UploadTime { get; set; }
    }

    public class SOAP
    {
        public string HospCode { get; set; } = null!;

        public long Soaid { get; set; }

        public string? Inhospid { get; set; }

        public string? HealthId { get; set; }

        public string? Kind { get; set; }

        public string? Context { get; set; }

        public DateTime? CreateTime { get; set; }

        public string? SourceType { get; set; }

        public int? VersionCode { get; set; }

        public char? Status { get; set; }

        public DateTime? ModifyTime { get; set; }
    }
    public class OrderPlan
    {
        public string HospCode { get; set; } = null!;

        public long Orderplanid { get; set; }

        public string? Inhospid { get; set; }

        public string? HealthId { get; set; }

        public string? HplanType { get; set; }

        public short? SeqNo { get; set; }

        public string? PlanCode { get; set; }

        public string? PlanDes { get; set; }

        public char? FreeCharge { get; set; }

        public DateTime? ExecDateFrom { get; set; }

        public DateTime? ExecDateTo { get; set; }

        public string? OrderDept { get; set; }

        public string? OrderDr { get; set; }

        public short? PlanDays { get; set; }

        public decimal? QtyDose { get; set; }

        public decimal? QtyDaily { get; set; }

        public string? UnitDose { get; set; }

        public string? FreqCode { get; set; }

        public string? DoseIndi { get; set; }

        public string? DosePath { get; set; }

        public string? MadeType { get; set; }

        public decimal? TotalQty { get; set; }

        public short? MedBag { get; set; }

        public string? ExamLoc { get; set; }

        public char? UrgFlag { get; set; }

        public char? PreopFlag { get; set; }

        public char? AddFlag { get; set; }

        public char? KeepspecFlag { get; set; }

        public string? LocationCode { get; set; }

        public string? TriggerTablecode { get; set; }

        public long? TriggerRecid { get; set; }

        public char? Status { get; set; }

        public char? ExecStatus { get; set; }

        public char? ChargeStatus { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? ModifyDate { get; set; }

        public string? Remark { get; set; }
    }

    public partial class Dx : OrderPlan { };

    public partial class Med : OrderPlan { };

    public partial class Others : OrderPlan { };

    public class Root
    {
        public SenderInfo SenderInfo { get; set; }
        public HospInfo HospInfo { get; set; }
        public RegInfo RegInfo { get; set; }
        public PatientInfo PatientInfo { get; set; }
        public List<SOAP> SOAP { get; set; }
        public List<OrderPlan> Dx { get; set; }
        public List<OrderPlan> Med { get; set; }
        public List<OrderPlan> Others { get; set; }
    }
    //}
}
