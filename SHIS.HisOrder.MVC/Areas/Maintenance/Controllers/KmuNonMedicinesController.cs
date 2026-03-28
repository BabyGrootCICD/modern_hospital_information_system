using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SHIS.HisOrder.MVC.Models;
using SHIS.HisOrder.MVC.Areas.Maintenance.ViewModels;
using SHIS.HisOrder.MVC.Areas.HisOrder.ViewModels;
using SHIS.HisOrder.MVC.Areas.HisOrder.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Controllers
{
    [Area("Maintenance")]
    [Authorize(Roles = "Maintain_ShisNonMedicines")]//登入後可依據設定的 專案名稱 project_id 作為判斷依據
    public class ShisNonMedicinesController : Controller
    {
        private readonly SHISContext _context;

        public ShisNonMedicinesController(SHISContext context)
        {
            _context = context;
        }

        // GET: Maintenance/ShisNonMedicines
        public async Task<IActionResult> Index()
        {
            //return View(await _context.ShisNonMedicines.ToListAsync());
            return View(await _context.ShisNonMedicines.OrderBy(x => x.ItemName).ToListAsync());
        }

        // GET: Maintenance/ShisNonMedicines/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ShisNonMedicines == null)
            {
                return NotFound();
            }

            var shisNonMedicine = await _context.ShisNonMedicines
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (shisNonMedicine == null)
            {
                return NotFound();
            }

            return View(shisNonMedicine);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string ItemName)
        {
            ItemName = ItemName.Trim();

            return View(await _context.ShisNonMedicines.Where(x =>
                    (ItemName == null) ? x.ItemId == x.ItemId :
                    (x.ItemName.ToLower().Contains(ItemName.ToLower()))
                ).OrderBy(x => x.ItemName).ToListAsync());
        }


        // GET: Maintenance/ShisNonMedicines/Create
        public IActionResult Create()
        {
            ViewData["GroupCodeData"] = _context.ShisCoderefs.Where(c => c.RefCodetype == "group_code").OrderBy(c => c.RefShowseq);

            return View();
        }

        // POST: Maintenance/ShisNonMedicines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,ItemName,ItemType,ItemSpec,StartDate,EndDate,Remark,ShowSeq,GroupCode,Status")] ShisNonMedicine shisNonMedicine)
        {
            if (ModelState.IsValid)
            {
                //ShisNonMedicineExists
                if (!ShisNonMedicineExists(shisNonMedicine.ItemId.Trim()))
                {
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                    shisNonMedicine.ItemId = shisNonMedicine.ItemId.Trim();
                    shisNonMedicine.ModifyUser = login.EMPCODE;
                    shisNonMedicine.CreateUser = login.EMPCODE;

                    _context.Add(shisNonMedicine);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["Msg"] = "ItemId Repeat";
                    return View(shisNonMedicine);
                }

            }
            return View(shisNonMedicine);
        }

        // GET: Maintenance/ShisNonMedicines/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ShisNonMedicines == null)
            {
                return NotFound();
            }

            var shisNonMedicine = await _context.ShisNonMedicines.FindAsync(id);
            if (shisNonMedicine == null)
            {
                return NotFound();
            }

            ViewData["GroupCodeData"] = _context.ShisCoderefs.Where(c => c.RefCodetype == "group_code").OrderBy(c => c.RefShowseq);

            return View(shisNonMedicine);
        }

        // POST: Maintenance/ShisNonMedicines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ItemId,ItemName,ItemType,ItemSpec,StartDate,EndDate,Status,CreateUser,CreateDate,Remark,ShowSeq,GroupCode")] ShisNonMedicine shisNonMedicine)
        {
            if (id != shisNonMedicine.ItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                    shisNonMedicine.ModifyUser = login.EMPCODE;
                    shisNonMedicine.ModifyDate = DateTime.Now;

                    _context.Update(shisNonMedicine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShisNonMedicineExists(shisNonMedicine.ItemId))
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
            return View(shisNonMedicine);
        }

        // GET: Maintenance/ShisNonMedicines/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ShisNonMedicines == null)
            {
                return NotFound();
            }

            var shisNonMedicine = await _context.ShisNonMedicines
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (shisNonMedicine == null)
            {
                return NotFound();
            }

            return View(shisNonMedicine);
        }

        // POST: Maintenance/ShisNonMedicines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ShisNonMedicines == null)
            {
                return Problem("Entity set 'SHISContext.ShisNonMedicines' is null.");
            }
            var shisNonMedicine = await _context.ShisNonMedicines.FindAsync(id);
            if (shisNonMedicine != null)
            {
                _context.ShisNonMedicines.Remove(shisNonMedicine);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShisNonMedicineExists(string id)
        {
            return _context.ShisNonMedicines.Any(e => e.ItemId == id);
        }

        public string UpdateStatus(string inItemId)
        {
            string strR = "";
            string strStatus = "";

            if (inItemId == null || _context.ShisNonMedicines == null)
            {
                strR = "修改出現錯誤!!";

            }
            else
            {
                if (!ShisNonMedicineExists(inItemId))
                {
                    strR = "修改出現錯誤!!";
                }
                else
                {
                    List<ShisNonMedicine> NonMedList = new List<ShisNonMedicine>();
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");

                    NonMedList = _context.ShisNonMedicines.Where(e => e.ItemId == inItemId).ToList();

                    if (NonMedList.Any())
                    {
                        try
                        {
                            ShisNonMedicine NonMed = NonMedList.First();
                            NonMed = NonMedList.First();

                            strStatus = NonMed.Status.ToString();

                            if (strStatus == "1")
                            {
                                NonMed.Status = '2';
                            }
                            else
                            {
                                NonMed.Status = '1';
                            }

                            NonMed.ModifyDate = DateTime.Now;
                            NonMed.ModifyUser = login.EMPCODE;
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
