using System;
using System.Collections.Generic;

namespace SHIS.HisOrder.MVC.Models
{
    /// <summary>
    /// User Auth File(Account permissions)
    /// </summary>
    public partial class ShisAuth
    {
        public string UserIdno { get; set; }
        public string ProjectId { get; set; }
        public string Creator { get; set; }
        public DateTime CreateTime { get; set; }

        public virtual ShisUser UserIdnoNavigation { get; set; }
    }
}
