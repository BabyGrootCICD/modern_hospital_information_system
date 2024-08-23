using AutoMapper.Configuration.Annotations;
using KMU.HisOrder.MVC.Areas.HisOrder.Models;
using KMU.HisOrder.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using static KMU.HisOrder.MVC.Models.EnumClass;

namespace KMU.HisOrder.MVC.Areas.Reservation.Conrollers
{
    [Area("Reservation")]
    [Authorize(Roles = "Reservation")]//登入後可依據設定的 專案名稱 project_id 作為判斷依據
    public class ReserveController : Controller
    {
        private readonly KMUContext _context;

        public ReserveController(KMUContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Close(string id)
        {

            var result = await _context.Registrations.SingleOrDefaultAsync(r => r.Inhospid == id);
            if (result.RegStatus == "N")
            {
                result.RegStatus = "C";
                result.ModifyTime = DateTime.Now;
            }

            _context.Update(result);
            _context.SaveChanges();

            return RedirectToAction("List");
        }

        public async Task<IActionResult> ErClose(string id)
        {

            var result = await _context.Registrations.SingleOrDefaultAsync(r => r.Inhospid == id);
            if (result.RegStatus == "N")
            {
                result.RegStatus = "C";
                result.ModifyTime = DateTime.Now;
            }

            _context.Update(result);
            _context.SaveChanges();

            return RedirectToAction("ErList");
        }

        public IActionResult Details(string? id, string? cancelList, string? hideList)
        {

            TempData["DepName"] = _context.KmuDepartments.SingleOrDefault(e => e.DptCode == id).DptName;
            TempData["DeptId"] = _context.KmuDepartments.SingleOrDefault(e => e.DptCode == id).DptCode;

            if (id == null || id == "")
            {
                return NotFound();
            }

            DateOnly d = DateOnly.FromDateTime(DateTime.Today);

            if (cancelList != null)
            {
                var kan = _context.Registrations.Where(R => R.RegDepartment == id && R.RegDate == d).ToList();
                return View(kan);
            }

            var result = _context.Registrations.Where(R => R.RegDepartment == id && R.RegDate == d && R.RegStatus != "C").ToList();


            return View(result);
        }

        public IActionResult List(string? firstdate, string? seconddate, string? status, List<string>? ENT)
        {


            //Duration Search Limit.

            var between = (Convert.ToDateTime(seconddate) - Convert.ToDateTime(firstdate)).Days;

            // Duration With Month.
            //var totalDays = (Convert.ToDateTime(between) - Convert.ToDateTime(firstdate));

            if (between < 32)
            {
                if (firstdate != null && seconddate != null && status != null)
                {
                    if (status == "All")
                    {
                        var kan = _context.Registrations.Where(R => R.ModifyTime >= Convert.ToDateTime(firstdate) && R.ModifyTime <= Convert.ToDateTime(seconddate) && ENT.Contains(R.RegDepartment)).ToList();
                        return View(kan);
                    }
                    else
                    {
                        var kan = _context.Registrations.Where(R => R.ModifyTime >= Convert.ToDateTime(firstdate) && R.RegStatus == status && R.ModifyTime <= Convert.ToDateTime(seconddate) && ENT.Contains(R.RegDepartment)).ToList();
                        return View(kan);
                    }
                }
            }
            else
            {
                // Eror Message Display When the duration is Over 1 months.

                TempData["Eror"] = "You can not filter data more than 1 month. You selected " + between + " Days";
                var list = _context.Registrations.Where(e => e.Inhospid == null);
                return View(list);
            }

            var result = _context.Registrations.Where(R => R.ModifyTime >= DateTime.Today && R.RegStatus == "N" && Convert.ToInt32(R.RegDepartment) != Convert.ToInt32("1600") && Convert.ToInt32(R.RegDepartment) != Convert.ToInt32("1604") && Convert.ToInt32(R.RegDepartment) != Convert.ToInt32("1601") && Convert.ToInt32(R.RegDepartment) != Convert.ToInt32("1602") && Convert.ToInt32(R.RegDepartment) != Convert.ToInt32("1603")).ToList();

            return View(result);

        }

        public IActionResult ErList(string? firstdate, string? seconddate, string? status, List<string>? ENT)
        {


            //Duration Search Limit.

            var between = (Convert.ToDateTime(seconddate) - Convert.ToDateTime(firstdate)).Days;

            // Duration With Month.
            //var totalDays = (Convert.ToDateTime(between) - Convert.ToDateTime(firstdate));

            if (between < 32)
            {
                if (firstdate != null && seconddate != null && status != null)
                {
                    if (status == "All")
                    {
                        var kan = _context.Registrations.Where(R => R.ModifyTime >= Convert.ToDateTime(firstdate) && R.ModifyTime <= Convert.ToDateTime(seconddate) && ENT.Contains(R.RegDepartment)).ToList();
                        return View(kan);
                    }
                    else
                    {
                        var kan = _context.Registrations.Where(R => R.ModifyTime >= Convert.ToDateTime(firstdate) && R.RegStatus == status && R.ModifyTime <= Convert.ToDateTime(seconddate) && ENT.Contains(R.RegDepartment)).ToList();
                        return View(kan);
                    }
                }
            }
            else
            {
                // Eror Message Display When the duration is Over 1 months.

                TempData["Eror"] = "You can not filter data more than 1 month. You selected " + between + " Days";
                var list = _context.Registrations.Where(e => e.Inhospid == null);
                return View("ErList", list);
            }

            var result = _context.Registrations.Where(R => R.ModifyTime >= DateTime.Today && R.RegStatus == "N" && Convert.ToInt32(R.RegDepartment) >= Convert.ToInt32("1600") && Convert.ToInt32(R.RegDepartment) <= Convert.ToInt32("1604")).ToList();

            return View(result);

        }

        public IActionResult Reserve(string reserveType)
        {
            List<EnumAnonymous> anonymousList = new List<EnumAnonymous>();

            foreach (EnumClass.EnumAnonymous anonymous in Enum.GetValues(typeof(EnumAnonymous)))
            {
                if (EnumClass.EnumAnonymous.Normal != anonymous)
                {
                    anonymousList.Add(anonymous);
                }
            }

            ViewData["reserveType"] = reserveType;
            ViewData["anonymousList"] = anonymousList;

            if (reserveType == "OPD")
            {
                ViewData["Title"] = "Appointment";
            }
            else if (reserveType == "EMG")
            {
                ViewData["Title"] = "Emergency";
            }

            return View();
        }

        public IActionResult NurseFunction(string reserveType)
        {
            return View();
        }

        public async Task<IActionResult> getData(string PatientID)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.Now);
            var dpt = _context.KmuDepartments.Where(e => e.DptParent == "6000" || e.DptParent == "3000").ToList();
            List<String> dptCodes = new List<string>();
            foreach (var a in dpt)
            {
                dptCodes.Add(a.DptCode);
            }

            var getPatient = await _context.Registrations.FirstOrDefaultAsync(e => e.RegHealthId == PatientID && e.RegDate == dateNow && dptCodes.Contains(e.RegDepartment));

            if (getPatient != null)
            {

                DateTime dat = DateTime.Today;

                var getVital = _context.PhysicalSigns.Where(e => e.Inhospid == getPatient.Inhospid).ToList();
                return Json(getVital);

            }
            return Json(null);
        }

        public async Task<IActionResult> NR(string PatientID)
        {
            var dpt = _context.KmuDepartments.Where(e => e.DptParent == "6000" || e.DptParent == "3000").ToList();
            List<String> dptCodes = new List<string>();
            foreach (var a in dpt)
            {
                dptCodes.Add(a.DptCode);
            }

            var getPatient = _context.KmuCharts.FirstOrDefault(e => e.ChrHealthId == PatientID);

            if (getPatient == null)
            {
                var Nodata = "This patient does'nt exits.";
                return Json(Nodata);
            }

            var dateNow = DateOnly.FromDateTime(DateTime.Now);

            var inhospital = await _context.Registrations.SingleOrDefaultAsync(e => e.RegHealthId == getPatient.ChrHealthId && e.RegDate == dateNow && dptCodes.Contains(e.RegDepartment) && e.RegStatus != "C");


            var patient = new KmuChart();
            patient.ChrAddress = getPatient.ChrAddress;
            patient.ChrMobilePhone = getPatient.ChrMobilePhone;
            patient.ChrPatientFirstname = getPatient.ChrPatientFirstname;
            patient.ChrPatientMidname = getPatient.ChrPatientMidname;
            patient.ChrPatientLastname = getPatient.ChrPatientLastname;
            patient.ChrHealthId = getPatient.ChrHealthId;
            patient.ChrEmgContact = getPatient.ChrBirthDate.ToString();
            patient.ChrSex = getPatient.ChrSex;

            if (patient.ChrSex == "M")
            {
                patient.ChrSex = "Male";
            }
            else if (patient.ChrSex == "F")
            {
                patient.ChrSex = "Female";
            }

            if (inhospital != null)
            {
                return Json(patient);
            }
            var validmessage = "This patient does'nt have appointment for today.";
            return Json(validmessage);
        }

        [HttpPost]
        public async Task<IActionResult> vitalSign(string healthId, string[] type, string[] value, PhysicalSign physicalSign)
        {
            var date = DateOnly.FromDateTime(DateTime.Now);
            var dpt = _context.KmuDepartments.Where(e => e.DptParent == "6000" || e.DptParent == "3000").ToList();
            List<String> dptCodes = new List<string>();
            foreach (var a in dpt)
            {
                dptCodes.Add(a.DptCode);
            }

            var inhospId = await _context.Registrations.SingleOrDefaultAsync(e => e.RegHealthId == healthId && e.RegDate == date && dptCodes.Contains(e.RegDepartment) && e.RegStatus != "C");
            if (inhospId.Inhospid == null)
            {
                Console.WriteLine("InhospitalID is null...");
            }

            var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
            var exitVital = _context.PhysicalSigns.Where(e => e.Inhospid == inhospId.Inhospid && e.ModifyTime == DateTime.Today).ToList();


            if (exitVital.Any())
            {
                var error = "This patient have a vital sign.";
                return Json(error);
            }
            else
            {
                foreach (var item in type)
                {
                    physicalSign.PhyId = "";
                    physicalSign.PhyType = item;
                    physicalSign.Inhospid = inhospId.Inhospid;
                    physicalSign.PhyValue = "";
                    physicalSign.ModifyTime = DateTime.Today;
                    physicalSign.ModifyUser = login.EMPCODE;
                    //physicalSign.PhyDept = "Ncd_Physical";
                    await _context.PhysicalSigns.AddRangeAsync(physicalSign);

                    await _context.SaveChangesAsync();
                }
                DateTime dat = DateTime.Today;
                var findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Height" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[0];
                    _context.PhysicalSigns.Update(item);
                }

                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Weight" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[1];
                    _context.PhysicalSigns.Update(item);
                }

                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Temp" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[2];
                    _context.PhysicalSigns.Update(item);
                }

                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == Convert.ToString(type[3]) && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[3];
                    _context.PhysicalSigns.Update(item);
                }

                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "SpO2" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[4];
                    _context.PhysicalSigns.Update(item);
                }

                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Systolic (Sys)" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[5];
                    _context.PhysicalSigns.Update(item);
                }

                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Diastolic (Dias)" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[6];
                    _context.PhysicalSigns.Update(item);
                }
                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "HR" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[7];
                    _context.PhysicalSigns.Update(item);
                }
                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "RBS" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[8];
                    _context.PhysicalSigns.Update(item);
                }
                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "FBS" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[9];
                    _context.PhysicalSigns.Update(item);
                }

                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Pulse" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[10];
                    _context.PhysicalSigns.Update(item);
                }

                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "MUAC" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[11];
                    _context.PhysicalSigns.Update(item);
                }

                findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "HBA1C" && p.ModifyTime == dat);
                foreach (var item in findinhospid)
                {
                    item.PhyValue = value[12];
                    _context.PhysicalSigns.Update(item);
                }

                await _context.SaveChangesAsync();
                return Json(type);
            }

            return null;
        }

        [HttpPost]
        public async Task<IActionResult> update(string healthId, string[] type, string[] value, PhysicalSign physicalSign)
        {
            var date = DateOnly.FromDateTime(DateTime.Now);
            var dpt = _context.KmuDepartments.Where(e => e.DptParent == "6000" || e.DptParent == "3000").ToList();
            List<String> dptCodes = new List<string>();
            foreach (var a in dpt)
            {
                dptCodes.Add(a.DptCode);
            }

            var inhospId = await _context.Registrations.SingleOrDefaultAsync(e => e.RegHealthId == healthId && e.RegDate == date && dptCodes.Contains(e.RegDepartment) && e.RegStatus != "C");
            if (inhospId.Inhospid == null)
            {
                Console.WriteLine("InhospitalID is null...");
            }

            var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
            var exitVital = _context.PhysicalSigns.Where(e => e.Inhospid == inhospId.Inhospid && e.ModifyTime == DateTime.Today).ToList();

            foreach (var data in exitVital)
            {
                _context.Remove(data);
                _context.SaveChanges();
            }


            foreach (var item in type)
            {
                physicalSign.PhyId = "";
                physicalSign.PhyType = item;
                physicalSign.Inhospid = inhospId.Inhospid;
                physicalSign.PhyValue = "";
                physicalSign.ModifyTime = DateTime.Today;
                physicalSign.ModifyUser = login.EMPCODE;
                //physicalSign.PhyDept = "Ncd_Physical";
                await _context.PhysicalSigns.AddRangeAsync(physicalSign);

                await _context.SaveChangesAsync();
            }
            DateTime dat = DateTime.Today;
            var findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Height" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[0];
                _context.PhysicalSigns.Update(item);
            }

            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Weight" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[1];
                _context.PhysicalSigns.Update(item);
            }

            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Temp" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[2];
                _context.PhysicalSigns.Update(item);
            }

            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == Convert.ToString(type[3]) && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[3];
                _context.PhysicalSigns.Update(item);
            }

            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "SpO2" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[4];
                _context.PhysicalSigns.Update(item);
            }

            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Systolic (Sys)" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[5];
                _context.PhysicalSigns.Update(item);
            }

            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Diastolic (Dias)" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[6];
                _context.PhysicalSigns.Update(item);
            }
            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "HR" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[7];
                _context.PhysicalSigns.Update(item);
            }
            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "RBS" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[8];
                _context.PhysicalSigns.Update(item);
            }
            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "FBS" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[9];
                _context.PhysicalSigns.Update(item);
            }

            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "Pulse" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[10];
                _context.PhysicalSigns.Update(item);
            }

            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "MUAC" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[11];
                _context.PhysicalSigns.Update(item);
            }

            findinhospid = _context.PhysicalSigns.Where(p => p.Inhospid == inhospId.Inhospid && p.PhyType == "HBA1C" && p.ModifyTime == dat);
            foreach (var item in findinhospid)
            {
                item.PhyValue = value[12];
                _context.PhysicalSigns.Update(item);
            }

            await _context.SaveChangesAsync();
            return Json(type);

        }

        public async Task<IActionResult> HomeVitalSign(home_physicalsign data, string healthId, string type)
        {
            var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");


            var date = DateOnly.FromDateTime(DateTime.Now);
            var dpt = _context.KmuDepartments.Where(e => e.DptParent == "6000" || e.DptParent == "3000").ToList();
            List<String> dptCodes = new List<string>();
            foreach (var a in dpt)
            {
                dptCodes.Add(a.DptCode);
            }

            var inhospId = await _context.Registrations.SingleOrDefaultAsync(e => e.RegHealthId == healthId && e.RegDate == date && dptCodes.Contains(e.RegDepartment));
            if (inhospId.Inhospid == null)
            {
                Console.WriteLine("InhospitalID is null...");
                return View();
            }

            data.inhospid = inhospId.Inhospid;
            if (type == "Hyper")
            {
                data.category = "Hypertension";
            }
            else if (type == "Diab")
            {
                data.category = "diabetes";
            }
            data.modify_time = DateTime.Today;
            data.modify_user = login.EMPCODE;
            await _context.home_physicalsign.AddAsync(data);
            _context.SaveChanges();
            return Json(data);
        }

    }
}