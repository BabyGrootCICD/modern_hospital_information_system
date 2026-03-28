using SHIS.HisOrder.MVC.Areas.HisOrder.Models;
using SHIS.HisOrder.MVC.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SHIS.HisOrder.MVC.Areas.HisOrder.ViewModels
{
    public partial class VShisNonMedicine
    {
        private ExtendShisNonMedicine _ShisNonMedicine;

        public VShisNonMedicine(ExtendShisNonMedicine inShisNonMedicine)
        {
            _ShisNonMedicine = inShisNonMedicine;
        }

        // [Display(Name = "Id")]
        public string ItemId { get { return _ShisNonMedicine.ItemId; } set { _ShisNonMedicine.ItemId = value; } }
        
        public string ItemName { get { return _ShisNonMedicine.ItemName; } set { _ShisNonMedicine.ItemName = value; } }

        /// <summary>
        /// 5. Lab
        /// 6. Exam
        /// 7. Path
        /// </summary>
        public string ItemType { get { return _ShisNonMedicine.ItemType; } set { _ShisNonMedicine.ItemType = value; } }
        public string? ItemSpec { get { return _ShisNonMedicine.ItemSpec; } set { _ShisNonMedicine.ItemSpec = value; } }
        public string Remark { get { return _ShisNonMedicine.Remark; } set { _ShisNonMedicine.Remark = value; } }
        public decimal? ShowSeq { get { return _ShisNonMedicine.ShowSeq; } set { _ShisNonMedicine.ShowSeq = value; } }
        public string? GroupCode { get { return _ShisNonMedicine.GroupCode; } set { _ShisNonMedicine.GroupCode = value; } }
        public bool enabled { get { return _ShisNonMedicine.enabled; } set { _ShisNonMedicine.enabled = value; } }

        // Extend join columns
        public string? RefName { get { return _ShisNonMedicine.RefName; } set { _ShisNonMedicine.RefName = value; } }
        public int? RefShowseq { get { return _ShisNonMedicine.RefShowseq; } set { _ShisNonMedicine.RefShowseq = value; } }
        public string PlanDes { get { return _ShisNonMedicine.PlanDes; } set { _ShisNonMedicine.PlanDes = value; } }
    }
}
