
using SHIS.HisOrder.MVC.Models;

namespace SHIS.HisOrder.MVC.Areas.MedicalRecord.ViewModels
{
    public partial class PrintClass
    {
        public string HospName { get; set; }
        public string FormTitle { get; set; }

        public PersonalCardClass PersonalInfo { get; set; }

        public string QrCodeStr { get; set; }

        public string ModifyID { get; set; }

        public string ModifyName { get; set; }
    }
}
