using SHIS.HisOrder.MVC.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SHIS.HisOrder.MVC.Areas.HisOrder.ViewModels
{
    public partial class DiagnosisClass
    {

    }

    public partial class VShisIcd
    {
        private ShisIcd _ShisIcd;

        public VShisIcd(ShisIcd inShisIcd)
        {
            _ShisIcd = inShisIcd;
        }

        [Display(Name = "ICD10")]
        public string IcdCode { get { return _ShisIcd.IcdCode; } set { _ShisIcd.IcdCode = value; } }
        [Display(Name = "Name")]
        public string IcdEnglishName { get { return _ShisIcd.IcdEnglishName; } set { _ShisIcd.IcdEnglishName = value; } }
        public string? ParentCode { get { return _ShisIcd.ParentCode; } set { _ShisIcd.ParentCode = value; } }
        public bool IsParent { get; set; }

    }
}
