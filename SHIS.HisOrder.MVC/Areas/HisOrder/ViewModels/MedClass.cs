using SHIS.HisOrder.MVC.Models;
using Microsoft.CodeAnalysis.Recommendations;
using System.ComponentModel.DataAnnotations;

namespace SHIS.HisOrder.MVC.Areas.HisOrder.ViewModels
{
    public partial class VShisMedicine
    {
        private ShisMedicine _ShisMedicine;

        public VShisMedicine(ShisMedicine inShisMedicine)
        {
            _ShisMedicine = inShisMedicine;
        }

        [Display(Name = "Code")]
        public string MedId { get { return _ShisMedicine.MedId; } set { _ShisMedicine.MedId = value; } }
        [Display(Name = "Type")]
        public string MedType { get { return _ShisMedicine.MedType; } set { _ShisMedicine.MedType = value; } }
        public string GenericName { get { return _ShisMedicine.GenericName; } set { _ShisMedicine.GenericName = value; } }
        public string? BrandName { get { return _ShisMedicine.BrandName; } set { _ShisMedicine.BrandName = value; } }
        public string? UnitSpec { get { return _ShisMedicine.UnitSpec; } set { _ShisMedicine.UnitSpec = value; } }
        public string? PackSpec { get { return _ShisMedicine.PackSpec; } set { _ShisMedicine.PackSpec = value; } }
        [Display(Name = "Default")]
        public string? DefaultFreq { get { return _ShisMedicine.DefaultFreq; } set { _ShisMedicine.DefaultFreq = value; } }
        [Display(Name = "Dura")]
        public string? RefDuration { get { return _ShisMedicine.RefDuration; } set { _ShisMedicine.RefDuration = value; } }
        public string? Remarks { get { return _ShisMedicine.Remarks; } set { _ShisMedicine.Remarks = value; } }
    }
}
