using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.Console.Gateway.Models_MOHD;

/// <summary>
/// Account Basic File(user account )
/// </summary>
public partial class MohdUser
{
    public string UserIdno { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public string UserNameMidname { get; set; } = null!;

    public DateTime? UserBirthDate { get; set; }

    public string? UserSex { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string UserMobilePhone { get; set; } = null!;

    public string? UserEmail { get; set; }

    public string Creator { get; set; } = null!;

    public DateTime CreateTime { get; set; }

    public string UserNameFirstname { get; set; } = null!;

    public string UserNameLastname { get; set; } = null!;

    /// <summary>
    /// 分類(1:Personal,2:System
    /// </summary>
    public string UserCategory { get; set; } = null!;

    public string AccountStatus { get; set; } = null!;
}
