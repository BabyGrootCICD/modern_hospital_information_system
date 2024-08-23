using KMU.HisOrder.MVC.Areas.HisOrder.Models;
using KMU.HisOrder.MVC.Areas.HisOrder.ViewModels;
using KMU.HisOrder.MVC.Areas.MedicalRecord.Models;
using KMU.HisOrder.MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KMU.HisOrder.MVC.Areas.HisOrder.Controllers
{
    [Area("HisOrder")]
    public class TestResultController : Controller
    {
      
        public readonly KMUContext _context;
        public TestResultController(KMUContext context)
        {
            _context = context;
           
        }

        public IActionResult Detail(string patientId, string sex)
        {

            var departments = from rg in _context.Registrations
                              join plan in _context.Hisorderplans
                              on rg.Inhospid equals plan.Inhospid
                              join dpt in _context.KmuDepartments
                              on rg.RegDepartment equals dpt.DptCode
                              where plan.HealthId == patientId & (plan.HplanType == "Lab" || plan.HplanType == "Path" || plan.HplanType == "Exam")
                              select new departmentDetail
                              {
                                  departmentName = dpt.DptName,
                                  reg_date = rg.RegDate,
                                  inhospid = plan.Inhospid,
                                  sex = sex,
                                  patientid = patientId
                              };

            return View(departments);
        }

        
        public async Task<IActionResult> ListResult(string? inhospid,string patientid, string sex)
        {
            try
            {

                var existresult = _context.testresults.Where(r => r.refbillorder == inhospid);
                if (!existresult.Any())
                {          
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                       
                        var url = "http://192.168.30.233:120/";
                        var response = await httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                        
                        url = $"http://192.168.30.233:120/api/pateint/Details?pateintcode={patientid}&approved=1";
                        response = await httpClient.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            var patientDetails = await response.Content.ReadAsStringAsync();
                            var patientResponse = JsonConvert.DeserializeObject<PatientResponse>(patientDetails);

                            if (patientResponse.testDetails != null && patientResponse.testDetails.Count > 0)
                            {
                                var existResultDetial = patientResponse.testDetails.Where(t => t.refbillorder == inhospid);
                                if (existResultDetial.Any())
                                {
                                    var root = new PatientResponse();
                                    root.testresultSave = new List<TestResult>();
                                    foreach (var testresult in patientResponse.testDetails)
                                    {
                                        if (testresult.refbillorder == inhospid)
                                        {
                                            root.testresultSave.Add(testresult);
                                        }
                                    }
                                    _context.testresults.AddRange(root.testresultSave);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        // Handle the exception
                        Console.WriteLine($"Error: {ex.Message}");
                        TempData["error"] = ex.Message;
                        // You can add additional logic here, such as logging the error or notifying the user
                        // In this case, we'll just pass the API call

                    }
                }
                }

                var listresult = _context.testresults.Where(t => t.refbillorder == inhospid).GroupBy(x => x.code)
                    .Select(g => new TestResultViewModel
                    {
                        sex = sex,
                        Code = g.Key,
                        TestName = g.Select(r => r.TestName).FirstOrDefault(),
                        TestGroup = g.Select(r => r.TestGroup).FirstOrDefault(),
                        subresults = g.Select(r => new subresult
                        {
                            contents = r.Contents,
                            result = r.Result,
                            sufix = r.Sufix,
                            NvalueMale = r.NvalueMale,
                            NvalueFemale = r.NvalueFemale
                        }).ToList()
                    })
                     .ToList();


                    return View(listresult);
                
            

            }
            catch (Exception ex)
            {

                return View(ex.Message);
            }
      
            
         }




       
    }
}
