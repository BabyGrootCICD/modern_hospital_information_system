using SHIS.HisOrder.MVC.Areas.HisOrder.Models;
using SHIS.HisOrder.MVC.Areas.HisOrder.ViewModels;
using SHIS.HisOrder.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.DiaSymReader;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System.Data;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace SHIS.HisOrder.MVC.Areas.HisOrder.Controllers
{
    [CheckClinicSessionAttribute]
    [CheckSessionTimeOutAttribute]
    [Area("HisOrder")]
    [Authorize(Roles = "HisOrder")]//登入後可依據設定的 專案名稱 project_id 作為判斷依據
    public class InputHelperController : Controller
    {
        private readonly SHISContext _context;

        public InputHelperController(SHISContext context)
        {
            _context = context;
        }

        #region ICD Menu
        public PartialViewResult getICDMenu()
        {
            List<ShisIcd> data = _context.ShisIcds.Where(c => c.IcdType == "CM" && c.ShowMode == "Left" && c.Status == "1").OrderBy(c => c.IcdCode).ToList();
            List<VShisIcd> vdata = new List<VShisIcd>();
            foreach (ShisIcd p in data)
            {
                vdata.Add(new VShisIcd(p));
            }

            return PartialView("Areas/HisOrder/Views/InputHelper/ICD/_ICDMenu.cshtml", vdata);
        }

        public PartialViewResult getChildIcdNodes(string inKey, string ShowMode = "Menu")
        {
            if (string.IsNullOrWhiteSpace(inKey)) { inKey = ""; };

            IEnumerable<ShisIcd> AllItems;
            List<ShisIcd> data = new List<ShisIcd>();

            if (ShowMode == "Menu")
            {
                ViewData["NodeMode"] = "MenuNode";

                data = _context.ShisIcds.Where(c => c.ParentCode == inKey && c.Status == "1").OrderBy(c => c.IcdCode).ToList();
            }
            else if (ShowMode == "Search")
            {
                ViewData["NodeMode"] = "SearchNode";

                string[] inKeyArr = inKey.Replace(".", "").ToUpper().Trim().Split(' ');

                // Optimization search,only enumerate once. Not reused .where()
                if (inKey.Contains('.'))
                {
                    #region 組合搜尋(共列舉兩次)，僅分組搜尋並非分組模糊搜尋(不使用)
                    //// Filter Items IcdCode with dot.
                    //IEnumerable<ShisIcd> MatchItems = from p in _context.ShisIcds
                    //                 where p.Status == "1" && p.ShowMode == "Right" && p.IcdCode.Contains(".")
                    //                 orderby p.IcdCode
                    //                 select p;

                    //// Intersect Compare Not Contains
                    //data.AddRange(MatchItems.Where(c => c.IcdEnglishName.ToUpper().Split(" ").Intersect(inKeyArr).Count() == inKeyArr.Count() ||
                    //                                inKeyArr.Contains(c.IcdCode)));
                    #endregion

                    // Filter Items IcdCode with dot.
                    var IcdData = _context.ShisIcds.Where(c => c.Status == "1" && c.ShowMode == "Right" && c.IcdCode.Contains("."));

                    // IcdCode and IcdName contains all keyword
                    foreach (string keyword in inKeyArr)
                    {
                        IcdData = IcdData.Where(c => c.IcdCodeUndot.ToUpper().Contains(keyword) || c.IcdEnglishName.ToUpper().Contains(keyword));
                    }

                    data.AddRange(IcdData);
                }
                else
                {
                    // Filter Items IcdCode not dot.
                    var IcdData = _context.ShisIcds.Where(c => c.Status == "1" && c.ShowMode == "Right" && !c.IcdCode.Contains("."));

                    // IcdCode and IcdName contains all keyword
                    foreach (string keyword in inKeyArr)
                    {
                        IcdData = IcdData.Where(c => c.IcdCodeUndot.ToUpper().Contains(keyword) || c.IcdEnglishName.ToUpper().Contains(keyword));
                    }

                    data.AddRange(IcdData);
                }

                data = data.OrderBy(c => c.IcdCode).ToList();
            }

            List<VShisIcd> Nodes = new List<VShisIcd>();

            foreach (ShisIcd p in data)
            {
                VShisIcd vp = new VShisIcd(p);
                if (ShowMode == "Search" && inKey.Contains('.'))
                {
                    vp.IsParent = false;
                }
                else
                {
                    // Is parent nodes check.
                    vp.IsParent = _context.ShisIcds.Where(c => c.ParentCode == p.IcdCode).FirstOrDefault() != null ? true : false;
                }
                Nodes.Add(vp);
            }

            return PartialView("Areas/HisOrder/Views/InputHelper/ICD/_v_NodeList.cshtml", Nodes);
        }

        #endregion

        #region Med Menu
        public PartialViewResult getMedMenu()
        {
            // List<ShisMedicine> data = _context.ShisMedicines.Where(c => c.Status == '1' && c.StartDate <= DateTime.Now).OrderBy(c => c.GenericName).ThenBy(c => c.BrandName).ThenBy(c => c.MedId).ToList();
            List<ShisMedicine> data = _context.ShisMedicines.Where(c => c.Status == '1' && c.StartDate <= DateTime.Now).OrderBy(c => c.GenericName).ThenBy(c => c.BrandName).ThenBy(c => c.MedId).ToList();

            // && c.EndDate > DateTime.Now
            List<VShisMedicine> vdata = new List<VShisMedicine>();
            foreach (ShisMedicine p in data)
            {
                vdata.Add(new VShisMedicine(p));
            }

            return PartialView("Areas/HisOrder/Views/InputHelper/Med/_MedMenu.cshtml", vdata);
        }
        #endregion

        #region NonMed Menu
        public PartialViewResult getNonMedMenu(string inNonMedType)
        {
            // List<ShisNonMedicine> data = _context.ShisNonMedicines.Where(c => c.Status == '1' && c.StartDate <= DateTime.Now).OrderBy(c => c.ShowSeq).ThenBy(c => c.ItemName).ThenBy(c => c.ItemId).ToList();
            // && c.EndDate > DateTime.Now

            IEnumerable<ExtendShisNonMedicine> data = (from a in _context.ShisNonMedicines
                                                      join b in _context.ShisCoderefs on a.GroupCode equals b.RefCode
                                                      where a.Status == '1' && b.RefCodetype == "group_code"
                                                      orderby a.ShowSeq, a.ItemId
                                                      select new ExtendShisNonMedicine()
                                                      {
                                                          ItemId = a.ItemId,
                                                          ItemName = a.ItemName,
                                                          ItemType = a.ItemType,
                                                          ItemSpec = a.ItemSpec,
                                                          StartDate = a.StartDate,
                                                          EndDate = a.EndDate,
                                                          Status = a.Status,
                                                          CreateUser = a.CreateUser,
                                                          CreateDate = a.CreateDate,
                                                          ModifyUser = a.ModifyUser,
                                                          ModifyDate = a.ModifyDate,
                                                          Remark = a.Remark,
                                                          ShowSeq = a.ShowSeq,
                                                          GroupCode = a.GroupCode,
                                                          RefName = b.RefName,
                                                          RefShowseq = b.RefShowseq,
                                                      });

            switch (inNonMedType)
            {
                case "Lab":
                    data = data.Where(c => c.ItemType == "5").ToList();
                    break;
                case "Exam":
                    data = data.Where(c => c.ItemType == "6").ToList();
                    break;
                case "Path":
                    data = data.Where(c => c.ItemType == "7").ToList();
                    break;
                case "Supply":
                    data = data.Where(c => c.ItemType != "5" && c.ItemType != "6" && c.ItemType != "7" && c.ItemType != "9" && c.ItemType != "10" && c.ItemType != "11" && c.ItemType != "12" && c.ItemType != "13" && c.ItemType != "14" && c.ItemType != "15" && c.ItemType != "16" && c.ItemType != "17" && c.ItemType != "18" && c.ItemType != "19" && c.ItemType != "20" && c.ItemType != "21" && c.ItemType != "22" && c.ItemType != "23" && c.ItemType != "24").ToList(); // Supply and Other
                    break;
                case "Blood":
                    data = data.Where(c => c.ItemType == "9").ToList();
                    break;
                default:
                    break;
            }

            List<VShisNonMedicine> vdata = new List<VShisNonMedicine>();
            foreach (ExtendShisNonMedicine p in data)
            {
                p.ItemType = p.ItemType == "5" ? "Lab" : p.ItemType == "6" ? "Exam" : p.ItemType == "7" ? "Path" : p.ItemType == "8" ? "Supply" : p.ItemType == "9" ? "Blood" : "";
                vdata.Add(new VShisNonMedicine(p));
            }

            return PartialView("Areas/HisOrder/Views/InputHelper/NonMed/_NonMedMenu.cshtml", vdata);
        }

        public JsonResult SearchNonMed(string inKey, string inType)
        {
            inKey = inKey.ToUpper().Trim();
            string[] inKeyArr = inKey.Split(" ");

            string TypeCode = inType == "Lab" ? "5" : inType == "Exam" ? "6" : inType == "Path" ? "7" : inType == "Supply" ? "8" : inType == "Blood" ? "9" : "";

            IEnumerable<ShisNonMedicine> NonMedData = _context.ShisNonMedicines.Where(c => c.Status == '1' && c.ItemType == TypeCode).OrderBy(c => c.ShowSeq).ThenBy(c => c.ItemName).ThenBy(c => c.ItemId);

            foreach (string keyword in inKeyArr)
            {
                NonMedData = NonMedData.Where(c => c.ItemId.ToUpper().Contains(keyword) || c.ItemName.ToUpper().Contains(keyword));
            }

            string[] AutoCompleteSource = NonMedData.Select(c => c.ItemId + " " + c.ItemName + " " + c.ItemSpec).ToArray();

            return Json(AutoCompleteSource);
        }
        #endregion
    }
}
