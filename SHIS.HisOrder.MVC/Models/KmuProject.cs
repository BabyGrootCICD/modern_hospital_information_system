using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.MVC.Models
{
    /// <summary>
    /// Auth Setting reference Project File(main function node)
    /// </summary>
    public partial class ShisProject
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Url { get; set; }
        public string Creator { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
