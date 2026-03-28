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
using SHIS.HisOrder.MVC.Areas.HisOrder.Models;
using SHIS.HisOrder.MVC.Areas.HisOrder.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Controllers
{
    [Area("Maintenance")]
    [Authorize(Roles = "Maintain_ShisMedfrequency")]//登入後可依據設定的 專案名稱 project_id 作為判斷依據
    public class ShisMedfrequencyController : Controller
    {
        private readonly SHISContext _context;

        public ShisMedfrequencyController(SHISContext context)
        {
            _context = context;
        }

        // GET: Maintenance/ShisMedfrequency
        public async Task<IActionResult> Index()
        {
            //SelectList selectList = new SelectList(this.GetCustomers(), "MedId", "GenericName");
            //ViewBag.SelectList = selectList;

            return View(await _context.ShisMedfrequencies.OrderBy(x => x.FrqSeqNo).ThenBy(c => c.FrqCode).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Search()
        {

            //var temp = _context.ShisMedicines.Where(x =>
            //(
            //    (MedTypeQuery == null) ? x.MedType == x.MedType :
            //        ((MedTypeQuery == "-") ? x.MedType == x.MedType : x.MedType == MedTypeQuery)
            //)
            //&& (
            //        (MedName == null) ? x.GenericName == x.GenericName :
            //        (x.GenericName.ToLower().Contains(MedName.ToLower()) || x.BrandName.ToLower().Contains(MedName.ToLower()))
            //    )
            //).OrderBy(x => x.GenericName).ToListAsync();

            //return View(await temp);

            return View(await _context.ShisMedfrequencies.OrderBy(x => x.FrqSeqNo).ThenBy(x => x.FrqCode).ToListAsync());
        }




        // GET: Maintenance/ShisMedfrequency/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ShisMedfrequencies == null)
            {
                return NotFound();
            }

            var shisMedfrequencies = await _context.ShisMedfrequencies
                .FirstOrDefaultAsync(m => m.FrqCode == id);
            if (shisMedfrequencies == null)
            {
                return NotFound();
            }

            return View(shisMedfrequencies);
        }

        // GET: Maintenance/ShisMedfrequency/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: Maintenance/ShisMedicines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FrqCode,FreqDesc,FrqForDays,FrqForTimes,FrqOneDayTimes,EnableStatus,FrqSeqNo")] ShisMedfrequency shisMedfrequency)
        {
            if (ModelState.IsValid)
            {
                if (!ShisMedfrequenciesExists(shisMedfrequency.FrqCode))
                {
                    var intMed_id = _context.ShisMedicines.Select(x => int.Parse(x.MedId)).ToList().Max() + 1;

                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                    shisMedfrequency.CreateUser = login.EMPCODE;
                    shisMedfrequency.ModifyUser = login.EMPCODE;
                    //shisMedfrequency.MedId = intMed_id.ToString();

                    _context.Add(shisMedfrequency);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["Msg"] = "ItemId Repeat";
                    return View(shisMedfrequency);
                }

            }
            return View(shisMedfrequency);
        }

        // GET: Maintenance/ShisMedfrequency/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ShisMedfrequencies == null)
            {
                return NotFound();
            }

            var ShisMedfrequencies = await _context.ShisMedfrequencies.FindAsync(id);
            if (ShisMedfrequencies == null)
            {
                return NotFound();
            }
            return View(ShisMedfrequencies);
        }

        //public async Task<IActionResult> Edit(string id, [Bind("MedId,ProductName,Nomenclature,Sepc,Unit,StartDate,EndDate,Status,CreateUser,CreateDate,ModifyUser,ModifyDate")] ShisMedicine shisMedicine)
        // POST: Maintenance/ShisMedfrequency/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("FrqCode,FreqDesc,FrqForDays,FrqForTimes,FrqOneDayTimes,EnableStatus,FrqSeqNo,CreateUser")] ShisMedfrequency shisMedfrequency)
        {
            if (id != shisMedfrequency.FrqCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");

                    shisMedfrequency.ModifyUser = login.EMPCODE;

                    _context.Update(shisMedfrequency);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShisMedfrequenciesExists(shisMedfrequency.FrqCode))
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
            return View(shisMedfrequency);
        }

        // GET: Maintenance/ShisMedfrequency/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ShisMedfrequencies == null)
            {
                return NotFound();
            }

            var shisMedfrequencies = await _context.ShisMedfrequencies
                .FirstOrDefaultAsync(m => m.FrqCode == id);
            if (shisMedfrequencies == null)
            {
                return NotFound();
            }

            return View(shisMedfrequencies);
        }

        // POST: Maintenance/ShisMedfrequency/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ShisMedicines == null)
            {
                return Problem("Entity set 'SHISContext.ShisMedfrequency'  is null.");
            }
            var shisMedfrequencies = await _context.ShisMedfrequencies.FindAsync(id);
            if (shisMedfrequencies != null)
            {
                _context.ShisMedfrequencies.Remove(shisMedfrequencies);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShisMedfrequenciesExists(string id)
        {
            return _context.ShisMedfrequencies.Any(e => e.FrqCode == id);
        }

        public string UpdateStatus(string inFrqCode)
        {
            string strR = "";
            string strStatus = "";

            if (inFrqCode == null || _context.ShisMedfrequencies == null)
            {
                strR = "修改出現錯誤!!";

            }
            else
            {
                if (!ShisMedfrequenciesExists(inFrqCode))
                {
                    strR = "修改出現錯誤!!";
                }
                else
                {
                    List<ShisMedfrequency> MedFList = new List<ShisMedfrequency>();
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");

                    MedFList = _context.ShisMedfrequencies.Where(e => e.FrqCode == inFrqCode).ToList();

                    if (MedFList.Any())
                    {
                        try
                        {
                            ShisMedfrequency medf = MedFList.First();
                            medf = MedFList.First();

                            strStatus = medf.EnableStatus.ToString();

                            if (strStatus == "1")
                            {
                                medf.EnableStatus = '2';
                            }
                            else
                            {
                                medf.EnableStatus = '1';
                            }

                            medf.ModifyUser = login.EMPCODE;
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
