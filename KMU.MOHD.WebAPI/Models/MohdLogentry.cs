using System;
using System.Collections.Generic;

namespace KMU.MOHD.WebAPI.Models;

public partial class MohdLogentry
{
    public long Id { get; set; }

    public string Loglevel { get; set; }

    public bool? ResultSuccess { get; set; }

    public string ResultMessage { get; set; }

    public string Ipaddress { get; set; }

    public string Endpoint { get; set; }

    public string Requestdata { get; set; }

    public string Exception { get; set; }

    public DateTime? Createtime { get; set; }
}
