using SHIS.HisOrder.MVC.Areas.Maintenance.Models;
using SHIS.HisOrder.MVC.Areas.Maintenance.ViewModels;
using SHIS.HisOrder.MVC.Areas.Reservation.ViewModels;
using SHIS.HisOrder.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Controllers
{
    [Area("Maintenance")]
    public class PhysicalSignController : Controller
    {

        private readonly SHISContext _context;

        public PhysicalSignController(SHISContext context)
        {
            _context = context;
        }

        #region View Action

        #endregion



        public string CaculateTriage(string strTriage)
        {
            #region Variable Setting
            
            TriageReturnClass triageMsg = new TriageReturnClass();
            List<PhysicalConditionItem> objConditionDto = new List<PhysicalConditionItem>();

            #endregion

            PhysicalConditionItem[]? objArray = JsonConvert.DeserializeObject(strTriage, typeof(PhysicalConditionItem[])) as PhysicalConditionItem[];
            objConditionDto = objArray.ToList();

            using (PhysicalSignService service = new PhysicalSignService(_context))
            {
                triageMsg = service.CaculateTriage(objConditionDto);
            }

                


            return JsonConvert.SerializeObject(triageMsg);
        }
    }
}
