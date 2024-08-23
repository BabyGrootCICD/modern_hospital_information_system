using KMU.HisOrder.MVC.Areas.HisOrder.Models;
using KMU.HisOrder.MVC.Areas.Maintenance.Models;
using KMU.HisOrder.MVC.Areas.Maintenance.ViewModels;
using KMU.HisOrder.MVC.Areas.MedicalRecord.Models;
using KMU.HisOrder.MVC.Areas.MedicalRecord.ViewModels;
using KMU.HisOrder.MVC.Areas.Reservation.Models;
using KMU.HisOrder.MVC.Areas.Reservation.ViewModels;
using KMU.HisOrder.MVC.Extesion;
using KMU.HisOrder.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace KMU.HisOrder.MVC.Areas.Reservation.Conrollers
{
    [Area("Reservation")]
    [Authorize(Roles = "Reservation")]//登入後可依據設定的 專案名稱 project_id 作為判斷依據
    public class AjaxController : Controller
    {
        private readonly KMUContext _context;


        public AjaxController(KMUContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        #region View Action

        public PartialViewResult PatientSearch(string? PatientID, string? MobilePhone, string? PatientName)
        {
            #region Variable Setting

            PatientListReturn PatientList = new PatientListReturn();

            #endregion

            using (MedicalRecordService service = new MedicalRecordService(_context))
            {
                PatientList = service.getPatientData(PatientID, MobilePhone, PatientName, EnumClass.DisplayLanguage.English);
            }

            return PartialView("../Reserve/PartialViews/_PatientDataPartialView", PatientList);
        }

        public PartialViewResult ShowReserveContent(string reserveType, string? PatientID)
        {
            #region Variable Setting

            List<ScheduleData> scheduleList = new List<ScheduleData>();
            #endregion

            using (ClinicScheduleService scheService = new ClinicScheduleService(_context))
            {
                scheduleList = scheService.getCanReserveClinicByDate(DateTime.Today, reserveType);
            }

            #region 到時候要搬進去身分檔維護區

            List<KmuAttribute> attrList = new List<KmuAttribute>();
            attrList = _context.KmuAttributes.ToList();

            #endregion


            ViewData["reserveType"] = reserveType;
            ViewData["scheduleList"] = scheduleList;
            ViewData["attrList"] = attrList;
            ViewData["PatientID"] = PatientID;

            return PartialView("../Reserve/PartialViews/_ReservePartialView");
        }

        public PartialViewResult ShowHistory(string? PatientID)
        {
            #region Variable Setting

            ReserveHistoryList HistoryList = new ReserveHistoryList();

            #endregion

            using (ReservationService service = new ReservationService(_context))
            {
                HistoryList = service.getReserveHistory(PatientID, EnumClass.DisplayLanguage.English);
            }

            return PartialView("../Reserve/PartialViews/_ReserveHistoryPartialView", HistoryList);
        }

        public PartialViewResult ShowPhysicalSignContent(string reserveType, string? PatientID)
        {
            #region Variable Setting

            List<PhysicalSignItem> physicalList = new List<PhysicalSignItem>();

            #endregion

            #region 生理資訊清單

            using (PhysicalSignService phyService = new PhysicalSignService(_context))
            {
                physicalList = phyService.GetPhysicalSignItems(reserveType);
            }

            #endregion
            ViewData["physicalList"] = physicalList;
            return PartialView("../Reserve/PartialViews/_PhysicalSignPartialView");
        }

        #endregion

        /// <summary>
        /// 執行掛號
        /// </summary>
        /// <param name="reserveJsonData"></param>
        /// <param name="physicalJsonData"></param>
        /// <returns></returns>
        public string MakeAppointment(string reserveType,string reserveJsonData, string physicalJsonData)
        {
            #region Variable Setting
            AppointmentReturnCollect Msg = new AppointmentReturnCollect();
            AppointmentStructure objReserveDto = new AppointmentStructure();
            List<PhysicalStructure> objPhysicalDto = new List<PhysicalStructure>();

            #endregion

            #region Login Session Check

            var checklogin = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");

            #endregion

            AppointmentStructure[]? objArray = JsonConvert.DeserializeObject(reserveJsonData, typeof(AppointmentStructure[])) as AppointmentStructure[];
            objReserveDto = objArray.Any() ? objArray.FirstOrDefault() : null;

            
            PhysicalStructure[]? phyArray;

            if (!string.IsNullOrEmpty(physicalJsonData))
            {
                phyArray = JsonConvert.DeserializeObject(physicalJsonData, typeof(PhysicalStructure[])) as PhysicalStructure[];
                objPhysicalDto = phyArray.ToList();
            }
            else
            {
                objPhysicalDto = null;
            }

            if (objReserveDto != null)
            {
                using (ReservationService service = new ReservationService(_context))
                {

                    Msg = service.MakeAppointment(reserveType, objReserveDto, EnumClass.DisplayLanguage.English, checklogin.EMPCODE);

                    if (Msg.ReturnT.isSuccess && objPhysicalDto != null)
                    {
                        EnumClass.ReplyNoCode replyNo = service.CompletePhysical(objPhysicalDto, Msg.Appointment, checklogin.EMPCODE);
                    }
                }
            }
            else
            {
                Msg.ReturnT = MessageFunction.GetFullReplyNoMsg(EnumClass.ReplyNoCode.WrongParam, EnumClass.DisplayLanguage.English);
            }

            return JsonConvert.SerializeObject(Msg);
        }

        /// <summary>
        /// 取消掛號
        /// </summary>
        /// <param name="InHospID"></param>
        /// <returns></returns>
        public string CancelReserve(string InHospID)
        {
            CancelAppointReturnCollect Msg = new CancelAppointReturnCollect();

            #region Login Session Check

            var checklogin = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");

            #endregion


            using (ReservationService service = new ReservationService(_context))
            {
                Msg = service.CancelAppointment(InHospID, EnumClass.DisplayLanguage.English, checklogin.EMPCODE);
            }

            return JsonConvert.SerializeObject(Msg);
        }
       

    }
}
