using SHIS.HisOrder.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHIS.HisOrder.MVC.Models;
using System.Data;
using SHIS.HisOrder.MVC.Areas.HisOrder.Models;
using System.Collections;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Controllers
{
    [Area("Maintenance")]
    //[Authorize(Roles = "Maintain_ShisDepartments")]
    public class MergePatientsController : Controller
    {
        private readonly SHISContext _context;

        public MergePatientsController(SHISContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? phone)
        {
            return View();
        }
        public IActionResult ChartMerged()
        {
            var merged_patients = _context.shis_chart_MergeHistory.ToList();
            return View(merged_patients);
        }
        public IActionResult getpatients(string? phone, string? name)
        {

            var lphone = "+252 063 " + phone;
            var patients = _context.ShisCharts.Select(p => new {
                p.ChrHealthId,
                p.ChrPatientFirstname,
                p.ChrPatientMidname,
                p.ChrPatientLastname,
                p.ChrSex,
                p.ChrMobilePhone,
                p.ModifyTime,
            }).Where(p => p.ChrMobilePhone == lphone && p.ChrPatientFirstname.ToUpper().Contains(name.ToUpper())).OrderBy(p => p.ModifyTime).Take(2);

            return Json(patients);
        }

        public IActionResult Merge(string[] patientids, shis_chart_MergeHistory mergedpatient)
        {
            var ID1 = patientids[0];
            var ID2 = patientids[1];
            ArrayList inhospIdList = new ArrayList();

            var regPatient = _context.Registrations.Where(p => p.RegHealthId == ID2);
            var shis = _context.SHIS_MergeHistory;
            foreach (var item in regPatient)
            {
                inhospIdList.Add(item.Inhospid);
                item.RegHealthId = ID1;

                _context.Registrations.Update(item);
            }

            var hisorderpatient = _context.Hisorderplans.Where(p => p.HealthId == ID2);
            foreach (var item in hisorderpatient)
            {
                item.HealthId = ID1;
                _context.Hisorderplans.Update(item);
            }

            var soapPatient = _context.Hisordersoas.Where(p => p.HealthId == ID2);
            foreach (var item in soapPatient)
            {
                item.HealthId = ID1;
                _context.Hisordersoas.Update(item);
            }

            var chartPatient = _context.ShisCharts.FirstOrDefault(p => p.ChrHealthId == ID2);

            if (chartPatient != null)
            {

                var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                mergedpatient.chr_halth_id = ID1;
                mergedpatient.mh_health_id = ID2;
                mergedpatient.merged_time = DateTime.Now;
                mergedpatient.merger_user = login.EMPCODE;

                mergedpatient.ChrPatientFirstname = chartPatient.ChrPatientFirstname;
                mergedpatient.ChrPatientMidname = chartPatient.ChrPatientMidname;
                mergedpatient.ChrPatientLastname = chartPatient.ChrPatientLastname;
                mergedpatient.ChrSex = chartPatient.ChrSex;
                mergedpatient.ChrBirthDate = chartPatient.ChrBirthDate;
                mergedpatient.ChrMobilePhone = chartPatient.ChrMobilePhone;
                mergedpatient.ChrAddress = chartPatient.ChrAddress;
                mergedpatient.ChrEmgContact = chartPatient.ChrEmgContact;
                mergedpatient.ChrContactRelation = chartPatient.ChrContactRelation;
                mergedpatient.ChrContactPhone = chartPatient.ChrContactPhone;
                mergedpatient.ChrCombineFlag = chartPatient.ChrCombineFlag;
                mergedpatient.ChrRemark = chartPatient.ChrRemark;
                mergedpatient.ModifyUser = chartPatient.ModifyUser;
                mergedpatient.ModifyTime = chartPatient.ModifyTime;
                mergedpatient.ChrAreaCode = chartPatient.ChrAreaCode;
                mergedpatient.ChrRefugeeFlag = chartPatient.ChrRefugeeFlag;
                mergedpatient.ChrNationalId = chartPatient.ChrNationalId;

                _context.shis_chart_MergeHistory.Add(mergedpatient);
                _context.ShisCharts.Remove(chartPatient);
            }
            _context.SaveChanges();
            return Json(inhospIdList);

        }


        public IActionResult SaveInhospid(string inhospidList, string[] chrId_mhId, SHIS_MergeHistory shisMergeHistory)
        {
            var ID1 = chrId_mhId[0];
            var ID2 = chrId_mhId[1];

            shisMergeHistory.InhospId = inhospidList;
            shisMergeHistory.chr_halth_id = ID1;
            shisMergeHistory.mh_health_id = ID2;
            shisMergeHistory.merged_time = DateTime.Now;
            _context.SHIS_MergeHistory.Add(shisMergeHistory);
            _context.SaveChanges();

            return Json(chrId_mhId);
        }

        public async Task<IActionResult> Undo(string id)
        {

            var mHistoryList = _context.SHIS_MergeHistory.Where(m => m.mh_health_id == id).ToList();
            return View(mHistoryList);
        }

        public IActionResult Save(string[] inhospitalIdList, string chr_id, string mh_id, ShisChart shischart)
        {


            var findmergedPatient = _context.shis_chart_MergeHistory.SingleOrDefault(p => p.mh_health_id == mh_id);
            var removePatient = _context.shis_chart_MergeHistory.SingleOrDefault(p => p.mh_health_id == mh_id);
            if (findmergedPatient != null && removePatient != null)
            {
                shischart.ChrHealthId = mh_id;
                shischart.ChrPatientFirstname = findmergedPatient.ChrPatientFirstname;
                shischart.ChrPatientMidname = findmergedPatient.ChrPatientMidname;
                shischart.ChrPatientLastname = findmergedPatient.ChrPatientLastname;
                shischart.ChrSex = findmergedPatient.ChrSex;
                shischart.ChrBirthDate = findmergedPatient.ChrBirthDate;
                shischart.ChrMobilePhone = findmergedPatient.ChrMobilePhone;
                shischart.ChrAddress = findmergedPatient.ChrAddress;
                shischart.ChrEmgContact = findmergedPatient.ChrEmgContact;
                shischart.ChrContactRelation = findmergedPatient.ChrContactRelation;
                shischart.ChrContactPhone = findmergedPatient.ChrContactPhone;
                shischart.ChrCombineFlag = findmergedPatient.ChrCombineFlag;
                shischart.ChrRemark = findmergedPatient.ChrRemark;
                shischart.ModifyUser = findmergedPatient.ModifyUser;
                shischart.ModifyTime = findmergedPatient.ModifyTime;
                shischart.ChrAreaCode = findmergedPatient.ChrAreaCode;
                shischart.ChrRefugeeFlag = findmergedPatient.ChrRefugeeFlag;
                shischart.ChrNationalId = findmergedPatient.ChrNationalId;
                foreach (var item in inhospitalIdList)
                {
                    var finregistration = _context.Registrations.SingleOrDefault(r => r.Inhospid == item);
                    finregistration.RegHealthId = mh_id;
                    _context.Registrations.Update(finregistration);

                    var findsoap = _context.Hisordersoas.Where(r => r.Inhospid == item);
                    foreach (var item2 in findsoap)
                    {
                        item2.HealthId = mh_id;
                        _context.Hisordersoas.Update(item2);
                    }

                    var hisplan = _context.Hisorderplans.Where(p => p.Inhospid == item);
                    foreach (var item2 in hisplan)
                    {
                        item2.HealthId = mh_id;
                        _context.Hisorderplans.Update(item2);

                    }
                }


                _context.ShisCharts.Add(shischart);
                _context.shis_chart_MergeHistory.Remove(removePatient);
                var removePatientSHISMerge = _context.SHIS_MergeHistory.Where(p => p.mh_health_id == mh_id).ToList();
                foreach (var item in removePatientSHISMerge)
                {
                    _context.SHIS_MergeHistory.Remove(item);
                }
            }
            _context.SaveChanges();
            return Json(inhospitalIdList + "" + chr_id + "" + mh_id);
        }


    }
}
