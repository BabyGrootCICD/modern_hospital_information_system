using System;
using System.Collections.Generic;

namespace KMU.HisOrder.Console.Gateway.Models_MOHD;

public partial class MohdRegistration
{
    public string HospCode { get; set; } = null!;

    public DateOnly RegDate { get; set; }

    public string Inhospid { get; set; } = null!;

    public string? DeptCode { get; set; }

    public string? DeptName { get; set; }

    public string? Noon { get; set; }

    public short? SeqNo { get; set; }

    public string? HealthId { get; set; }

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

    public DateTime? UploadTime { get; set; }
}
