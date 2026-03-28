using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SHIS.HisOrder.MVC.Models;
using System.ComponentModel.DataAnnotations;
using SHIS.HisOrder.MVC.Areas.Maintenance.ViewModels;
using SHIS.HisOrder.MVC.Areas.HisOrder.ViewModels;
using Newtonsoft.Json.Linq;
using System.Collections;
using SHIS.HisOrder.MVC.Areas.HisOrder.Models;
using SHIS.HisOrder.MVC.Areas.Maintenance.Models;
using Newtonsoft.Json;
using SHIS.HisOrder.MVC.Extesion;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Controllers
{
    [Area("Maintenance")]
    [Authorize(Roles = "Maintain_ShisIcd")]//登入後可依據設定的 專案名稱 project_id 作為判斷依據
    public class ShisIcdController : Controller
    {
        private readonly SHISContext _context;

        public ShisIcdController(SHISContext context)
        {
            _context = context;
        }






        // GET: Maintenance/ShisIcd
        public async Task<IActionResult> Index()
        {
            //List<ShisIcd> ShisIcdsTemp = new List<ShisIcd>();
            //var list = new List<string>() { "A", "B", "C" };

            //foreach (var iii in list)
            //{
            //    if (!string.IsNullOrEmpty(iii))
            //    {
            //        ShisIcdsTemp.AddRange(
            //            _context.ShisIcds.Where(x => x.IcdCode.ToLower().StartsWith(iii.ToLower()))
            //            );
            //    }
            //}
            //var listD = ShisIcdsTemp.Select(x => x.IcdCode.ToLower());
            //return View(await _context.ShisIcds.Where(x => listD.Contains(x.IcdCode.ToLower())).OrderBy(x => x.IcdEnglishName).ToListAsync());


            return View(await _context.ShisIcds.Where(x => x.IcdCode.ToLower().StartsWith("A".ToLower())).OrderBy(x => x.IcdCode).ThenBy(x => x.IcdEnglishName).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Search(string IcdCode, string EnglishName)
        {

            #region 資料處理
            var splitArry = IcdCode.Split(',').ToList();
            EnglishName = EnglishName.Trim();

            List<ShisIcd> ShisIcdsTemp = new List<ShisIcd>();
            foreach (var iii in splitArry)
            {
                if (!string.IsNullOrEmpty(iii))
                {
                    ShisIcdsTemp.AddRange(
                        _context.ShisIcds.Where(x => x.IcdCode.ToLower().StartsWith(iii.ToLower()))
                        );
                }
            }
            var listD = ShisIcdsTemp.Select(x => x.IcdCode.ToLower());
            #endregion

            return View(await _context.ShisIcds.Where(x =>
                (IcdCode == null) ? x.IcdCode.ToLower().StartsWith("A") :
                (listD.Contains(x.IcdCode.ToLower()))
                &&
                (
                    (EnglishName == null) ? x.IcdEnglishName == x.IcdEnglishName :
                    x.IcdEnglishName.ToLower().Contains(EnglishName.ToLower())
                )
            ).OrderBy(x => x.IcdCode).ThenBy(x => x.IcdEnglishName).ToListAsync());

            //return View(await _context.ShisIcds.Where(x =>
            //        (IcdCode == null) ? x.IcdCode.ToLower().StartsWith("A") :
            //        (x.IcdCode.ToLower().StartsWith(IcdCode.ToLower()))
            //        &&
            //        (
            //            (EnglishName == null) ? x.IcdEnglishName == x.IcdEnglishName :
            //            x.IcdEnglishName.ToLower().Contains(EnglishName.ToLower())
            //        )
            //    ).OrderBy(x => x.IcdEnglishName).ToListAsync());

        }


        // GET: Maintenance/ShisIcd/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ShisMedicines == null)
            {
                return NotFound();
            }

            var shisMedicine = await _context.ShisMedicines
                .FirstOrDefaultAsync(m => m.MedId == id);
            if (shisMedicine == null)
            {
                return NotFound();
            }

            return View(shisMedicine);
        }

        // GET: Maintenance/ShisIcd/Create
        public IActionResult Create()
        {
            ViewData["Dhis2Data"] = _context.Dhis2Diseases.OrderBy(c => c.ShowSeq);

            return View();
        }

        // POST: Maintenance/ShisIcd/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IcdCode,IcdEnglishName,Status,IcdType,ShowMode,Versioncode,Dhis2Code,ParentCode,Status")] ShisIcd shisIcd)
        {
            if (ModelState.IsValid)
            {
                if (!ShisIcdExists(shisIcd.IcdCode))
                {
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                    shisIcd.ModifyUser = login.EMPCODE;
                    shisIcd.ModifyDate = DateTime.Now;
                    shisIcd.IcdCodeUndot = shisIcd.IcdCode.Replace(".", "");

                    _context.Add(shisIcd);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["Msg"] = "IcdCode Repeat";

                    return View(shisIcd);
                }

            }
            return View(shisIcd);
        }

        // GET: Maintenance/ShisIcd/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ShisIcds == null)
            {
                return NotFound();
            }

            var ShisIcds = await _context.ShisIcds.FindAsync(id);
            if (ShisIcds == null)
            {
                return NotFound();
            }

            ViewData["Dhis2Data"] = _context.Dhis2Diseases.OrderBy(c => c.ShowSeq);

            return View(ShisIcds);
        }

        // POST: Maintenance/ShisIcd/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IcdCode,IcdEnglishName,Status,IcdType,ShowMode,Versioncode,Dhis2Code,ParentCode")] ShisIcd ShisIcd)
        {
            if (id != ShisIcd.IcdCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                    ShisIcd.ModifyUser = login.EMPCODE;
                    ShisIcd.ModifyDate = DateTime.Now;
                    ShisIcd.IcdCodeUndot = ShisIcd.IcdCode.Replace(".", "");

                    _context.Update(ShisIcd);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShisIcdExists(ShisIcd.IcdCode))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ShisIcd);
        }

        // GET: Maintenance/ShisIcd/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ShisIcds == null)
            {
                return NotFound();
            }

            var ShisIcd = await _context.ShisIcds
                .FirstOrDefaultAsync(m => m.IcdCode == id);
            if (ShisIcd == null)
            {
                return NotFound();
            }

            return View(ShisIcd);
        }

        // POST: Maintenance/ShisIcd/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ShisIcds == null)
            {
                return Problem("Entity set 'SHISContext.ShisIcd'  is null.");
            }
            var ShisIcd = await _context.ShisIcds.FindAsync(id);
            if (ShisIcd != null)
            {
                _context.ShisIcds.Remove(ShisIcd);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShisIcdExists(string id)
        {
            return _context.ShisIcds.Any(e => e.IcdCode == id);
        }



        public string UpdateStatus(string inIcdCode)
        {
            string strR = "";
            string strStatus = "";

            if (inIcdCode == null || _context.ShisIcds == null)
            {
                strR = "修改出現錯誤!!";

            }
            else
            {
                if (!ShisIcdExists(inIcdCode))
                {
                    strR = "修改出現錯誤!!";
                }
                else
                {
                    List<ShisIcd> icdList = new List<ShisIcd>();
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");

                    icdList = _context.ShisIcds.Where(e => e.IcdCode == inIcdCode).ToList();

                    if (icdList.Any())
                    {
                        try
                        {
                            ShisIcd icd = icdList.First();
                            icd = icdList.First();

                            strStatus = icd.Status;

                            if (strStatus == "1")
                            {
                                icd.Status = "2";
                            }
                            else
                            {
                                icd.Status = "1";
                            }

                            icd.ModifyDate = DateTime.Now;
                            icd.ModifyUser = login.EMPCODE;
                            _context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            strR = ex.ToString();
                        }
                    }
                    else
                    {
                        strR = "修改出現錯誤!!";
                    }

                }
            }
            return strR;
        }

    }
}
