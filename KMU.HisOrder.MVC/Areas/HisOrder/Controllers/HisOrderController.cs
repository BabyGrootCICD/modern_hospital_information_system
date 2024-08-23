using KMU.HisOrder.MVC.Areas.HisOrder.Models;
using KMU.HisOrder.MVC.Areas.Maintenance.Models;
using KMU.HisOrder.MVC.Areas.Maintenance.ViewModels;
using KMU.HisOrder.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Web;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using Microsoft.AspNetCore.Cors;
using Microsoft.CodeAnalysis.Operations;


namespace KMU.HisOrder.MVC.Areas.HisOrder.Controllers
{
    public class CheckClinicSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext;
            if (context.Session.GetObject<ClinicDTO>("ClinicDTO") == null)
            {
                var request = context.Request;
                filterContext.Result = new RedirectResult("~/Login/Index");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }



    public class CheckSessionTimeOutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext;
            if (context.Session.GetObject<LoginDTO>("LoginDTO") == null)
            {
                var request = context.Request;
                filterContext.Result = new RedirectResult("~/Login/Logout?message=Your connection has expired, please login again");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
    [CheckSessionTimeOutAttribute]
    [Area("HisOrder")]
    public class HisOrderController : Controller
    {
        private readonly KMUContext _context;

        public HisOrderController(KMUContext context)
        {
            _context = context;
        }

        #region View Function



        public IActionResult Index(string sourceType = "OPD")
        {
            if (sourceType == "OPD")
            {
                return Enter_opd_clinic();
            }
            else
            {
                return Enter_emg_clinc();
            }

            //    var checklogin = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
            //    if (checklogin == null || string.IsNullOrWhiteSpace(checklogin.EMPCODE))
            //    {
            //        return RedirectToAction("Login/Index");
            //    }

            //    var gv = new GlobalVariableDTO()
            //    {
            //        Clinic = HttpContext.Session.GetObject<ClinicDTO>("ClinicDTO"),
            //        Login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO")
            //    };

            //    if (gv.Clinic == null)
            //    {
            //        IniatializeSessionClinicDto(
            //                gv.Clinic == null ? null : gv.Clinic.RegDate,
            //                gv.Clinic == null ? null : gv.Clinic.DeptCode,
            //                "A",
            //                gv.Login.EMPCODE,
            //                gv.Login.EMPCODE
            //                );
            //        gv.Clinic = HttpContext.Session.GetObject<ClinicDTO>("ClinicDTO");
            //    }
            //    if (gv.Clinic == null)
            //    {
            //        ViewBag.DisPlaySwithClinicModal = true;
            //        ViewBag.DisPlayDefaultRegDate = DateTime.Today;
            //    }
            //    else
            //    {
            //        ViewBag.DisPlaySwithClinicModal = false;
            //        ViewBag.DisPlayDefaultClinic = gv.Clinic;
            //        ViewBag.DisPlayDefaultRegDate = gv.Clinic.RegDate;
            //    }

            //    var patientData = InitializePatientList();
            //    ViewBag.clinicScheList = GetClinicScheList(gv.Login.EMPCODE, gv.Clinic == null ? DateTime.Today : gv.Clinic.RegDate);
            //    ViewBag.loginDTO = gv.Login;
            //    return View(patientData);

        }


        public IActionResult Enter_opd_clinic()
        {

            //var request = HttpContext.Request;
            //var checklogin = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
            //if (checklogin == null || string.IsNullOrWhiteSpace(checklogin.EMPCODE))
            //{
            //    return Redirect(request.Host + "/Login/Index");
            //}

            var gv = new GlobalVariableDTO()
            {
                Clinic = HttpContext.Session.GetObject<ClinicDTO>("ClinicDTO"),
                Login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO")
            };

            if (gv.Clinic != null && gv.Clinic.InhospType == "EMG")
            {
                gv.Clinic = null;
                HttpContext.Session.SetObject("ClinicDTO", null);
            }


            if (gv.Clinic == null)
            {
                var getDefaultSchDept = _context.ClinicSchedules
                    .Where(c => c.ScheDoctor == gv.Login.EMPCODE
                    && c.ScheWeek == DateTime.Today.DayOfWeek.ToString()
                    && c.ScheNoon == "AM"
                    && c.ScheDptCode.Substring(0, 2) != "16").FirstOrDefault();
                if (getDefaultSchDept != null)
                {

                    IniatializeSessionClinicDto(
                            DateTime.Today,
                           getDefaultSchDept.ScheDptCode,
                            getDefaultSchDept.ScheNoon,
                            gv.Login.EMPCODE,
                            gv.Login.EMPCODE
                            );
                    gv.Clinic = HttpContext.Session.GetObject<ClinicDTO>("ClinicDTO");
                }
            }
            if (gv.Clinic == null)
            {
                ViewBag.DisPlaySwithClinicModal = true;
                ViewBag.DisPlayDefaultRegDate = DateTime.Today;
            }
            else
            {
                ViewBag.DisPlaySwithClinicModal = false;
                ViewBag.DisPlayDefaultClinic = gv.Clinic;
                ViewBag.DisPlayDefaultRegDate = gv.Clinic.RegDate;
            }


            var patientData = InitializePatientList();
            ViewBag.SourceType = "OPD";
            ViewBag.clinicScheList = GetClinicScheList(
               gv.Login.EMPCODE,
               gv.Clinic == null ? DateTime.Today : gv.Clinic.RegDate,
               "OPD"
               );
            ViewBag.loginDTO = gv.Login;
            return View("Index", patientData);
        }


        public IActionResult Enter_emg_clinc()
        {
            var checklogin = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
            if (checklogin == null || string.IsNullOrWhiteSpace(checklogin.EMPCODE))
            {
                return RedirectToAction("Login/Index");
            }

            var gv = new GlobalVariableDTO()
            {
                Clinic = HttpContext.Session.GetObject<ClinicDTO>("ClinicDTO"),
                Login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO")
            };


            if (gv.Clinic != null && gv.Clinic.InhospType == "OPD")
            {
                gv.Clinic = null;
                HttpContext.Session.SetObject("ClinicDTO", null);
            }

            if (gv.Clinic == null)
            {
                IniatializeSessionClinicDto(
                        gv.Clinic == null ? null : gv.Clinic.RegDate,
                        gv.Clinic == null ? null : gv.Clinic.DeptCode,
                        "A",
                        gv.Login.EMPCODE,
                        gv.Login.EMPCODE
                        );
                gv.Clinic = HttpContext.Session.GetObject<ClinicDTO>("ClinicDTO");
            }
            if (gv.Clinic == null)
            {
                ViewBag.DisPlaySwithClinicModal = true;
                ViewBag.DisPlayDefaultRegDate = DateTime.Today;
            }
            else
            {
                ViewBag.DisPlaySwithClinicModal = false;
                ViewBag.DisPlayDefaultClinic = gv.Clinic;
                ViewBag.DisPlayDefaultRegDate = gv.Clinic.RegDate;
            }

            var patientData = InitializePatientList();
            ViewBag.SourceType = "EMG";
            ViewBag.clinicScheList = GetClinicScheList(gv.Login.EMPCODE, gv.Clinic == null ? DateTime.Today : gv.Clinic.RegDate, "EMG");
            ViewBag.loginDTO = gv.Login;
            return View("Index", patientData);

        }



        [HttpPost]
        public ActionResult SwitchClinic(string clinicDate, string clinicDeptCode, string clinicRoomNo, string clinicDoctorId, string loginId, string sourceType)
        {

            //    DateTime.Parse(clinicInfo["clinic-date"].Trim()),
            //    clinicInfo["clinic-dept-code"].Trim(),
            //    clinicInfo["clinic-noon-no"].Trim(),
            //    clinicInfo["clinic-doctor-id"].Trim(),
            //    clinicInfo["login-id"].Trim(),
            //    clinicInfo["clinic-doctor-id"].Trim()

            var checklogin = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
            if (checklogin == null || string.IsNullOrWhiteSpace(checklogin.EMPCODE))
            {
                return RedirectToAction("Login/Index");
            }

            IniatializeSessionClinicDto(
                    DateTime.ParseExact(clinicDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    clinicDeptCode,
                    "AM",
                    checklogin.EMPCODE,
                    checklogin.EMPCODE
                    );

            //var _clinicDTO = HttpContext.Session.GetObject<ClinicDTO>("ClinicDTO");

            //if (_clinicDTO == null)
            //{
            //    ViewBag.DisPlaySwithClinicModal = true;
            //    ViewBag.DisPlayDefaultRegDate = DateTime.Today;
            //}
            //else
            //{
            //    ViewBag.DisPlaySwithClinicModal = false;
            //    ViewBag.DisPlayDefaultClinic = _clinicDTO;
            //    ViewBag.DisPlayDefaultRegDate = _clinicDTO.RegDate;
            //}

            //var patientData = GetPatientList(DateTime.ParseExact(clinicDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), clinicDeptCode);
            //ViewBag.SourceType = sourceType;
            //ViewBag.clinicScheList = GetClinicScheList(checklogin.EMPCODE, _clinicDTO == null ? DateTime.Today : _clinicDTO.RegDate, sourceType);
            //ViewBag.loginDTO = checklogin;
            //return View("Index", patientData);

            //2023.08.22 Html.BeginForm ERR_CACHE_MISS  update by 1050325 
            if (sourceType == "OPD")
            {
                return RedirectToAction("Enter_opd_clinic");
            }
            else
            {
                return RedirectToAction("Enter_emg_clinc");
            }
        }


        /// <summary>
        /// ClinicOrder畫面(SOAP、藥品、檢驗、檢查...)
        /// </summary>
        /// <returns></returns>
        /// 
        [CheckClinicSessionAttribute]
        public ActionResult ClinicOrder()
        {
            //viewbag assign
            ViewBag.htmlbody = HttpContext.Session.GetString("htmlBody");
            //ViewBag.htmlbodyActive =  HttpContext.Session.GetString("htmlbodyActive");
            return View();
        }



        private bool CheckPatientVisitBool(DateOnly inRegDate)
        {
            try
            {
                // pass visit
                var passData = _context.KmuCoderefs.Where(c => c.RefCodetype == "PassClinicVisit").FirstOrDefault();
                if (passData != null && passData.RefCasetype == "Y")
                {
                    return true;
                }

                if (inRegDate <= DateOnly.FromDateTime(DateTime.Today))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        public string CheckPatientVisit(string patientInhospid, string patientPatientid)
        {
            try
            {
                var today = DateTime.Today;
                var result = new ResultDTO() { isSuccess = false };

                var regData = _context.Registrations.Where(c => c.Inhospid == patientInhospid && c.RegHealthId == patientPatientid).FirstOrDefault();
                var clinicCategory = _context.KmuDepartments.Where(d => d.DptCode == regData.RegDepartment).FirstOrDefault().DptCategory;

                if (regData != null)
                {
                    if (CheckPatientVisitBool(regData.RegDate) || clinicCategory == "EMG")
                    {
                        result.isSuccess = true;
                        return JsonConvert.SerializeObject(result);
                    }
                    else
                    {
                        result.isSuccess = false;
                        result.Message = "The patient of this visit is locked";
                        return JsonConvert.SerializeObject(result);
                    }
                }
                else
                {
                    result.isSuccess = false;
                    result.Message = "No such patient id found";
                    return JsonConvert.SerializeObject(result);
                }
            }
            catch
            {
                return JsonConvert.SerializeObject(new ResultDTO() { isSuccess = false, Message = "An exception occurred ( CheckPatientVisit )" });
            }
        }


        [HttpPost]
        public ActionResult PatientVisit(string patientInhospid, string patientPatientid, string patientVisitStatus, string htmlBody, string htmlBodyActive)
        {

            if (string.IsNullOrWhiteSpace(patientInhospid) || string.IsNullOrWhiteSpace(patientPatientid))
            {
                return RedirectToAction("Index");
            }
            else
            {
                //取得 patient session data
                var patientInfo = GetPatientInfo(patientInhospid, patientPatientid);

                IniatializeSessionPatientDto(patientInfo);


                //取得 hisorderplan
                var hplanList = GetHisOrderPlanList(patientInhospid, patientPatientid);

                if (hplanList != null)
                {
                    ViewBag.MedList = hplanList.Where(c => c.HplanType == "Med" && c.DcDate == null).OrderBy(c => c.SeqNo).ToList();
                    ViewBag.NonMedList = hplanList.Where(c => (c.HplanType != "Med" && c.HplanType != "ICD") && c.DcDate == null).OrderBy(c => c.SeqNo).ToList();
                }

                //取得 soap
                List<hisordersoa_version> soapVersion = null;
                using (SoapController _soapController = new SoapController(_context, null))
                {

                    //var regdata = _context.Registrations.Where(c => c.Inhospid == patientInhospid).FirstOrDefault();
                    //var souretype = (regdata != null && regdata.RegDepartment.Substring(0, 2) != "16") ? "OPD" : "EMG";

                    soapVersion = _soapController.querySoapVer(patientInhospid);
                    ViewBag.SoapVersion = soapVersion;
                    //2023.02.17 add by 1050325 MG_INFO
                    ViewBag.MG_INFO = _soapController.getMgInfo();
                }

                //取得藥品相關設定
                ViewBag.MedFreq = _context.KmuMedfrequencies.ToList();
                ViewBag.MedIndication = _context.KmuMedfrequencyInds.ToList();
                ViewBag.MedPathWay = _context.KmuMedpathways.ToList();
                ViewBag.MedItem = JsonConvert.SerializeObject(_context.KmuMedicines.ToList());

                //取得非藥相關
                if (_context.KmuCoderefs.Where(c => c.RefCodetype == "NonMedLocation").Count() > 0)
                {
                    ViewBag.NonMedLocation = _context.KmuCoderefs.Where(c => c.RefCodetype == "NonMedLocation").OrderBy(d => d.RefShowseq).ToList();
                }
                else
                {
                    ViewBag.NonMedLocation = new List<KmuCoderef>();
                }

                //取得 diagnosis 
                //to do...
                //取得 allergy
                //to do...
                //畫面左側bar設定
                HttpContext.Session.SetString("htmlBody", htmlBody);
                //取得歷史病歷頭檔
                ViewBag.HistoryRecordMaster = getHistoryRecordMaster(patientInfo.RegDate.Value.AddMonths(-6), patientInfo.RegDate, patientInfo.RegPatientId);
                //寫入看診時間
                modifyRegStartEndTime(patientInfo.Inhospid, "reg_start","");
                //開啟看診畫面
                //return RedirectToAction("ClinicOrder");
                return View("ClinicOrder", patientInfo);
            }

        }

        #endregion

        #region init Function

        private void IniatializeSessionClinicDto(DateTime? regDate, string deptCode, string noonNo, string doctorCode, string loginCode, string authdoctorCode = null)
        {

            DateTime _regDate = regDate == null ? DateTime.Now : (DateTime)regDate;

            if (string.IsNullOrWhiteSpace(deptCode))
            {
                HttpContext.Session.SetObject("ClinicDTO", null);
                return;
            }
            else
            {
                var clinicInfo = GetDefaultClinicInfo(_regDate, deptCode, "AM", doctorCode, loginCode, null);
                var deptInfo = _context.KmuDepartments.Where(c => c.DptCode == deptCode).FirstOrDefault();


                if (clinicInfo != null)
                {
                    HttpContext.Session.SetObject("ClinicDTO", new ClinicDTO()
                    {
                        DeptCode = clinicInfo.ScheDptCode,
                        DeptName = clinicInfo.ScheDptName,
                        RegDate = _regDate,
                        DoctorCode = clinicInfo.ScheDoctor,
                        DoctorName = clinicInfo.ScheDoctorName,
                        NoonNO = noonNo,
                        RoomNumber = clinicInfo.ScheRoom,
                        InhospType = deptInfo.DptCategory
                    }); ;
                }
            }

        }


        public void IniatializeSessionPatientDto(PatientDTO inPatientInfo)
        {
            HttpContext.Session.SetObject("PatientDTO", null);
            HttpContext.Session.SetObject("PatientDTO", inPatientInfo);
        }

        public void IniatializeSessionPatientDto(HttpContext _context, PatientDTO inPatientInfo)
        {
            _context.Session.SetObject("PatientDTO", null);
            _context.Session.SetObject("PatientDTO", inPatientInfo);
        }

        private List<PatientDTO> InitializePatientList()
        {
            var gv = new GlobalVariableDTO()
            {
                Clinic = HttpContext.Session.GetObject<ClinicDTO>("ClinicDTO"),
                Patient = HttpContext.Session.GetObject<PatientDTO>("PatientDTO"),
                Login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO")
            };


            if (gv.Login == null)
            {
                throw new Exception("看診資訊或登入者資訊有錯! 可能發呆太久 Sesssion Timeout...");
            }

            if (gv.Clinic == null)
            {
                return new List<PatientDTO>();
            }
            else
            {
                return GetPatientList(gv.Clinic.RegDate, gv.Clinic.DeptCode);
            }
        }



        #endregion

        #region get data Function
        /// <summary>
        /// 取得病人清單
        /// </summary>
        /// <param name="inRegDate"></param>
        /// <param name="inDeptCode"></param>
        /// <returns></returns>
        public List<PatientDTO> GetPatientList(DateTime inRegDate, string inDeptCode)
        {
            //Console.WriteLine(inDeptCode);

            var shift = "";

            var ShiftA = _context.KmuCoderefs.SingleOrDefault(sh => sh.RefCodetype == "Shift" && sh.RefCode == "Shift A").RefDes;
            var ShiftB = _context.KmuCoderefs.SingleOrDefault(sh => sh.RefCodetype == "Shift" && sh.RefCode == "Shift B").RefDes;
            var ShiftC = _context.KmuCoderefs.SingleOrDefault(sh => sh.RefCodetype == "Shift" && sh.RefCode == "Shift C").RefDes;

            //string ShiftA = "07:30:00";
            //string ShiftB = "13:30:00";
            //string ShiftC = "19:30:00";

            TimeSpan duration = DateTime.Now.TimeOfDay;


            if (duration > TimeSpan.Parse(ShiftA) && duration < TimeSpan.Parse(ShiftB))
            {
                shift = "Shift A";
            }
            else if (duration > TimeSpan.Parse(ShiftB) && duration < TimeSpan.Parse(ShiftC))
            {
                shift = "Shift B";
            }
            else
            {
                shift = "Shift C";
            }

            var ptData = from d in _context.Set<Registration>()
                         join chart in _context.Set<KmuChart>()
                         on d.RegHealthId equals chart.ChrHealthId
                         where d.RegDate == DateOnly.FromDateTime(inRegDate) && (d.shift == shift || d.shift == "All Time") && d.RegDepartment == inDeptCode && d.RegStatus != "C"
                         select new PatientDTO
                         {
                             Inhospid = d.Inhospid,
                             RegPatientId = d.RegHealthId,
                             RegSeqNo = d.RegSeqNo,
                             RegStatus = d.RegStatus,
                             NationalId = chart.ChrNationalId,
                             FirstName = chart.ChrPatientFirstname,
                             MidName = chart.ChrPatientMidname,
                             LastName = chart.ChrPatientLastname,
                             Sex = chart.ChrSex,
                             MobilePhone = chart.ChrMobilePhone,
                             BirthDate = chart.ChrBirthDate == null ? null : chart.ChrBirthDate.Value.ToDateTime(TimeOnly.Parse("00:00 AM")),
                             Age = chart.ChrBirthDate == null ? -1 : DateTime.Now.Year - chart.ChrBirthDate.Value.Year,
                             RegDate = d.RegDate.ToDateTime(TimeOnly.Parse("00:00 AM")),
                             RegDept = d.RegDepartment,
                             RegNoon = d.RegNoon,
                             transfer_code = d.RegFollowCode,
                             transfer_des = d.RegFollowDesc,
                             remark = chart.ChrRemark


                         };


            if (ptData.Any())
            {
                var list = ptData.ToList();
                var clinicCategory = _context.KmuDepartments.Where(c => c.DptCode == inDeptCode).FirstOrDefault().DptCategory;

                list.ForEach(c =>
                {
                    if (clinicCategory == "EMG")
                    {
                        c.canVisit = true;
                    }
                    else if (c.RegDate != null)
                    {
                        c.canVisit = CheckPatientVisitBool(DateOnly.FromDateTime(Convert.ToDateTime(c.RegDate)));
                    }
                    else
                    {
                        c.canVisit = false;
                    }
                });


                return list;
            }
            else
            {
                return new List<PatientDTO>();
            }
        }
        /// <summary>
        /// 取得診間單一病人資料
        /// </summary>
        /// <param name="inhospid"></param>
        /// <param name="inPatientid"></param>
        /// <returns></returns>
        public PatientDTO GetPatientInfo(string inhospid, string inPatientid)
        {
            if (string.IsNullOrWhiteSpace(inhospid) || string.IsNullOrWhiteSpace(inPatientid))
            {
                return null;
            }
            else
            {
                var ptData = from d in _context.Set<Registration>()
                             join chart in _context.Set<KmuChart>()
                             on d.RegHealthId equals chart.ChrHealthId
                             where d.Inhospid == inhospid && d.RegHealthId == inPatientid

                             select new PatientDTO
                             {
                                 Inhospid = d.Inhospid,
                                 RegPatientId = d.RegHealthId,
                                 RegSeqNo = d.RegSeqNo,
                                 RegStatus = d.RegStatus,
                                 NationalId = chart.ChrNationalId,
                                 FirstName = chart.ChrPatientFirstname,
                                 MidName = chart.ChrPatientMidname,
                                 LastName = chart.ChrPatientLastname,
                                 Sex = chart.ChrSex,
                                 MobilePhone = chart.ChrMobilePhone,
                                 BirthDate = chart.ChrBirthDate == null ? null : chart.ChrBirthDate.Value.ToDateTime(TimeOnly.Parse("00:00 AM")),
                                 Age = chart.ChrBirthDate == null ? -1 : DateTime.Now.Year - chart.ChrBirthDate.Value.Year,
                                 Address = chart.ChrAddress,
                                 RegDate = d.RegDate.ToDateTime(TimeOnly.Parse("00:00 AM")),
                                 RegDept = d.RegDepartment,
                                 RegNoon = d.RegNoon,
                                 transfer_code = d.RegFollowCode,
                                 transfer_des = d.RegFollowDesc,
                                 remark = chart.ChrRemark
                             };

                if (ptData.Any())
                {
                    var list = ptData.ToList();

                    list.ForEach(c =>
                    {
                        var clinicCategory = _context.KmuDepartments.Where(d => d.DptCode == c.RegDept).FirstOrDefault().DptCategory;
                        if (clinicCategory == "EMG")
                        {
                            c.canVisit = true;
                        }
                        else
                        if (c.RegDate != null)
                        {
                            c.canVisit = CheckPatientVisitBool(DateOnly.FromDateTime(Convert.ToDateTime(c.RegDate)));
                        }
                        else
                        {
                            c.canVisit = false;
                        }
                    });

                    return list.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        public PatientDTO GetPatientInfo(string inPatientid)
        {
            if (string.IsNullOrWhiteSpace(inPatientid))
            {
                return null;
            }
            else
            {
                var ptData = from d in _context.Set<Registration>()
                             join chart in _context.Set<KmuChart>()
                             on d.RegHealthId equals chart.ChrHealthId
                             where d.RegHealthId == inPatientid

                             select new PatientDTO
                             {
                                 Inhospid = d.Inhospid,
                                 RegPatientId = d.RegHealthId,
                                 RegSeqNo = d.RegSeqNo,
                                 RegStatus = d.RegStatus,
                                 NationalId = chart.ChrNationalId,
                                 FirstName = chart.ChrPatientFirstname,
                                 MidName = chart.ChrPatientMidname,
                                 LastName = chart.ChrPatientLastname,
                                 Sex = chart.ChrSex,
                                 MobilePhone = chart.ChrMobilePhone,
                                 BirthDate = chart.ChrBirthDate == null ? null : chart.ChrBirthDate.Value.ToDateTime(TimeOnly.Parse("00:00 AM")),
                                 Age = chart.ChrBirthDate == null ? -1 : DateTime.Now.Year - chart.ChrBirthDate.Value.Year,
                                 Address = chart.ChrAddress,
                                 RegDate =  d.RegDate.ToDateTime(TimeOnly.Parse("00:00 AM")),
                                 RegDept = d.RegDepartment,
                                 RegNoon = d.RegNoon,
                                 transfer_code = d.RegFollowCode,
                                 transfer_des = d.RegFollowDesc,
                                 remark = chart.ChrRemark

                             };

                if (ptData.Any())
                {
                    var list = ptData.ToList();

                    list.ForEach(c =>
                    {

                        if (c.RegDate != null)
                        {
                            c.canVisit = CheckPatientVisitBool(DateOnly.FromDateTime(Convert.ToDateTime(c.RegDate)));
                        }
                        else
                        {
                            c.canVisit = false;
                        }
                    });

                    return list.OrderBy(c => c.RegDate).LastOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        private ClinicSchedule GetDefaultClinicInfo(System.DateTime inREGISTER_DATE, string inDeptCode, string inNOON_NO, string inDoctorCode, string inEmpcode, string inAuthDoctorCode)
        {
            var scheWeek = (int)inREGISTER_DATE.DayOfWeek;
            var clinicData = _context.ClinicSchedules.Where(c => c.ScheWeek == scheWeek.ToString() && c.ScheDptCode == inDeptCode).FirstOrDefault();

            return clinicData;

        }


        private List<ClinicScheduleItem> GetClinicScheList(string inDoctorCode, DateTime inRegDate, string inSourceType)
        {
            if (string.IsNullOrWhiteSpace(inDoctorCode))
            {
                throw new Exception();
            }
            else
            {
                //var scheData = new List<ClinicSchedule>();
                var scheWeek = (int)inRegDate.DayOfWeek;

                //var data_opd = _context.ClinicSchedules.Where(c => c.ScheDoctor == inDoctorCode && c.ScheWeek == scheWeek.ToString() && c.ScheNoon == "AM");
                //var data_er = _context.ClinicSchedules.Where(c => c.ScheWeek == scheWeek.ToString() && c.ScheDptCode.Substring(0, 2) == "16");

                using (ClinicScheduleService service = new ClinicScheduleService(_context))
                {
                    var data = service.GetScheduleDataForHisOrder(new string[] { inSourceType }, DateOnly.FromDateTime(inRegDate), inDoctorCode);


                    if (data.Any())
                    {
                        return data;
                    }
                    else
                    {
                        return new List<ClinicScheduleItem>();
                    }
                }

            }
        }



        private List<ClinicScheduleItem> GetClinicScheList(string inDoctorCode, DateTime inRegDate)
        {
            if (string.IsNullOrWhiteSpace(inDoctorCode))
            {
                throw new Exception();
            }
            else
            {
                //var scheData = new List<ClinicSchedule>();
                var scheWeek = (int)inRegDate.DayOfWeek;

                var data_opd = _context.ClinicSchedules.Where(c => c.ScheDoctor == inDoctorCode && c.ScheWeek == scheWeek.ToString() && c.ScheNoon == "AM");
                var data_er = _context.ClinicSchedules.Where(c => c.ScheWeek == scheWeek.ToString() && c.ScheDptCode.Substring(0, 2) == "16");

                using (ClinicScheduleService service = new ClinicScheduleService(_context))
                {
                    var data = service.GetScheduleDataForHisOrder(new string[] { "OPD", "EMG" }, DateOnly.FromDateTime(inRegDate), inDoctorCode);

                    if (data.Any())
                    {
                        return data;
                    }
                    else
                    {
                        return new List<ClinicScheduleItem>();
                    }
                }

            }
        }


        private List<Hisorderplan> GetHisOrderPlanList(string inhospid, string inPatientid)
        {
            if (string.IsNullOrWhiteSpace(inhospid) || string.IsNullOrWhiteSpace(inPatientid))
            {
                return null;
            }
            else
            {
                var hplan = _context.Hisorderplans.Where(c => c.Inhospid == inhospid && c.HealthId == inPatientid && c.DcStatus != '2').OrderBy(d => d.SeqNo);
                if (hplan.Any())
                {
                    return hplan.ToList();
                }
                else
                {
                    return null;
                }
            }

        }


        #endregion

        #region getGlobalVariableDTO
        public GlobalVariableDTO getGlobalVariablDTO(HttpContext httpContext)
        {
            var grv = new GlobalVariableDTO();

            if (httpContext.Session.GetObject<ClinicDTO>("ClinicDTO") != null)
            {
                grv.Clinic = httpContext.Session.GetObject<ClinicDTO>("ClinicDTO");
            }

            if (httpContext.Session.GetObject<LoginDTO>("LoginDTO") != null)
            {
                grv.Login = httpContext.Session.GetObject<LoginDTO>("LoginDTO");
            }


            if (httpContext.Session.GetObject<PatientDTO>("PatientDTO") != null)
            {
                grv.Patient = httpContext.Session.GetObject<PatientDTO>("PatientDTO");
            }

            return grv;

        }
        #endregion


        #region History Record 相關

        public List<HistoryRecordDto> getHistoryRecordMaster(DateTime? inBeginDate, DateTime? inEndDate, string inHealthId)
        {
            try
            {
                DateOnly _inBeginDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
                DateOnly _inEndDate = DateOnly.FromDateTime(DateTime.Today);


                if (inBeginDate is not null)
                {
                    _inBeginDate = DateOnly.FromDateTime(Convert.ToDateTime(inBeginDate));
                }

                if (inEndDate is not null)
                {
                    _inEndDate = DateOnly.FromDateTime(Convert.ToDateTime(inEndDate));
                }

                //var historyData = _context.Registrations.Where(c => c.RegPatientId == inChartNo && (c.RegDate >= _inBeginDate && c.RegDate <= _inEndDate));

                //2022.04.11 update by 1050325 排除取消掛號的病人
                var historyData = from reg in _context.Set<Registration>()
                                  join dept in _context.Set<KmuDepartment>()
                                  on reg.RegDepartment equals dept.DptCode into DptCodeGroup
                                  from D in DptCodeGroup.DefaultIfEmpty()
                                  join user in _context.Set<KmuUser>()
                                  on reg.RegDoctorId equals user.UserIdno into UserIndoGroup
                                  from U in UserIndoGroup.DefaultIfEmpty()
                                  where reg.RegHealthId == inHealthId && (reg.RegDate >= _inBeginDate && reg.RegDate <= _inEndDate) && reg.RegStatus != "C"
                                  select new HistoryRecordDto
                                  {
                                      inhospid = reg.Inhospid,
                                      regDate = reg.RegDate.ToDateTime(TimeOnly.Parse("00:00 AM")),
                                      clinicCode = D.DptCode,
                                      clinicName = D.DptName,
                                      doctorCode = reg.RegDoctorId,
                                      doctorName = String.Format("{0} {1} {2}", U.UserNameFirstname, U.UserNameMidname, U.UserNameLastname),
                                      sourceType = D.DptCategory,
                                      regFollowCode = reg.RegFollowCode

                                  };

                //測試用記得ban
                //getHistoryRecordDetail(historyData.Select(c => c.inhospid).ToList());

                if (historyData.Any())
                {
                    return historyData.OrderByDescending(c => c.regDate).ToList();
                }
                else
                {
                    return new List<HistoryRecordDto>();
                }
            }
            catch
            {
                return new List<HistoryRecordDto>();
            }
        }


        public List<HistoryRecordDetail> getHistoryRecordDetail(List<string> inhospidList)
        {
            try
            {
                if (inhospidList == null || inhospidList.Count == 0)
                {
                    return new List<HistoryRecordDetail>();
                }
                else
                {
                    using (SoapService service = new SoapService(_context))
                    {
                        List<HistoryRecordDetail> detailList = new List<HistoryRecordDetail>();
                        using (SoapController _soapController = new SoapController(_context, null))
                        {
                            //2023.04.28 add by 1050325 MG_INFO
                            var mg_info_list = _soapController.getMgInfo();

                            foreach (var inhospid in inhospidList)
                            {
                                //2023.04.28 add by 1050325 transCode
                                var regFollowCode = "";
                                var reg_data =_context.Registrations.Where(c => c.Inhospid == inhospid).FirstOrDefault();
                                if(reg_data != null)
                                {
                                    regFollowCode = reg_data.RegFollowCode;
                                }

                                List<ExtendHisrdersoa> soapData_ex = new List<ExtendHisrdersoa>();
                                var soapData = _context.Hisordersoas.Where(c => c.Inhospid == inhospid && c.Status == 'V').ToList();
                                if (soapData != null)
                                {
                                    foreach (var obj in soapData)
                                    {
                                        soapData_ex.Add(new ExtendHisrdersoa()
                                        {
                                            Inhospid = obj.Inhospid,
                                            Soaid = obj.Soaid,
                                            HealthId = obj.HealthId,
                                            Context = obj.Context,
                                            Kind = obj.Kind,
                                            SourceType = obj.SourceType,
                                            Status = obj.Status,
                                            CreateUser = obj.CreateUser,
                                            CreateDate = obj.CreateDate,
                                            ModifyDate = obj.ModifyDate,
                                            ModifyUser = obj.ModifyUser,
                                            VersionCode = obj.VersionCode,
                                            pure_context = service.StripHTML(obj.Context)
                                        });

                                    }
                                }

                                var diagnosisData = _context.Hisorderplans.Where(c => c.Inhospid == inhospid && c.HplanType == "ICD" && c.DcDate == null).ToList();
                                var medData = _context.Hisorderplans.Where(c => c.Inhospid == inhospid && c.HplanType == "Med" && c.DcDate == null).ToList();
                                var nonmedData = _context.Hisorderplans.Where(c => c.Inhospid == inhospid && (c.HplanType != "Med" && c.HplanType != "ICD") && c.DcDate == null).ToList();

                                detailList.Add(new HistoryRecordDetail()
                                {
                                    inhospid = inhospid,
                                    soapContext = soapData_ex,
                                    DiagnosisContext = diagnosisData,
                                    MedicineContext = medData,
                                    NonMedContext = nonmedData,
                                    MG_INFO = mg_info_list,
                                    RegFollowCode = regFollowCode,
                                }); ;

                            }

                            return detailList;
                        }
                    }
                }
            }
            catch
            {
                return new List<HistoryRecordDetail>();
            }
        }


        public IActionResult HistoryRecordMastrView(string patientInhospid, string patientPatientid, bool showClinic = true , bool showPrint = false)
        {
            PatientDTO patientInfo = null;
            if (string.IsNullOrWhiteSpace(patientInhospid))
            {
                patientInfo = GetPatientInfo(patientPatientid);
            }
            else
            {
                patientInfo = GetPatientInfo(patientInhospid, patientPatientid);
            }

            if (patientInfo != null)
            {
                //取得歷史病歷頭檔，預設6個月
                var HistoryRecordMaster = getHistoryRecordMaster(patientInfo.RegDate.Value.AddMonths(-6), patientInfo.RegDate, patientInfo.RegPatientId);


                List<HistoryRecordDetail> HistoryRecordDetail = null;
                if (HistoryRecordMaster != null && HistoryRecordMaster.Count() > 0)
                {
                    HistoryRecordDetail = getHistoryRecordDetail(HistoryRecordMaster.Select(c => c.inhospid).ToList());

                }

                ViewData["patientInfo"] = patientInfo;
                ViewData["HistoryRecordMaster"] = HistoryRecordMaster;
                ViewData["HistoryRecordDetail"] = HistoryRecordDetail;
                ViewData["ShowClinic"] = showClinic;
                ViewData["ShowPrint"] = showPrint;


                return PartialView("~/Areas/HisOrder/Views/HisOrder/PartialViews/_HistoryRecordMasterPartialView.cshtml", new ViewDataDictionary(ViewData));
            }
            else
            {
                return Content("null", "text/html");
            }
        }

        /// <summary>
        /// 紀錄看診相關時間
        /// </summary>
        /// <param name="inhospid"></param>
        /// <param name="inType"></param>
        public void modifyRegStartEndTime(string inhospid, string inType , string inEmpCode)
        {
            if (string.IsNullOrWhiteSpace(inhospid) || string.IsNullOrWhiteSpace(inType))
            {
                return;
            }
            else
            {
                var regData = _context.Registrations.Where(c => c.Inhospid == inhospid).FirstOrDefault();
                if (regData != null)
                {
                    if (inType == "reg_start")
                    {
                        if(regData.RegStatus == "N")
                        {
                            regData.RegStartTime = DateTime.Now;

                        }

                        if (regData.RegStatus == "O")
                        {
                            regData.reg_observe_end_time = DateTime.Now;
                        }

                        if (regData.RegStatus == "T")
                        {
                            regData.RegExamEndTime = DateTime.Now;
                        }

                    }
                    if (inType == "reg_end")
                    {
                        if(regData.RegEndTime  == null && regData.RegStatus == "*")
                        {
                            regData.RegEndTime = DateTime.Now;
                           
                        }

                    }

                    if (inType == "exam_start")
                    {
                        if(regData.RegExamStartTime == null && regData.RegStatus == "T")
                        {
                            regData.RegExamStartTime = DateTime.Now;
                        }
                        if (regData.reg_observe_end_time == null && regData.RegStatus == "O")
                        {
                            regData.reg_observe_end_time = DateTime.Now;
                        }
                    }
                    if(inType == "observe_start")
                    {
                        if(regData.reg_observe_start_time == null && regData.RegStatus == "O")
                        {
                        regData.reg_observe_start_time = DateTime.Now;

                        }
                        if (regData.RegExamEndTime == null && regData.RegStatus == "T")
                        {
                            regData.RegExamEndTime = DateTime.Now;

                        }
                    }

                    //2023.06.26 add by 1050325 每次存檔都要記錄看診醫師
                    if (!string.IsNullOrWhiteSpace(inEmpCode))
                    {
                        regData.RegDoctorId = inEmpCode;
                    }
                    

                    _context.Update(regData);
                    _context.SaveChanges();
                }

                return;
            }
        }


        #endregion
    }

}
