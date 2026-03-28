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
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Controllers
{
    [Area("Maintenance")]
    [Authorize(Roles = "Maintain_ShisMedicines")]//登入後可依據設定的 專案名稱 project_id 作為判斷依據
    public class ShisMedicinesController : Controller
    {
        private readonly SHISContext _context;

        public ShisMedicinesController(SHISContext context)
        {
            _context = context;
        }

        // GET: Maintenance/ShisMedicines
        public async Task<IActionResult> Index()
        {
            //SelectList selectList = new SelectList(this.GetCustomers(), "MedId", "GenericName");
            //ViewBag.SelectList = selectList;

            return View(await _context.ShisMedicines.OrderBy(x => x.GenericName).ThenBy(x => x.BrandName).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Search(string MedTypeQuery, string MedName)
        {
            if (string.IsNullOrWhiteSpace(MedName))
            {
                var temp = _context.ShisMedicines.OrderBy(x => x.GenericName).ThenBy(x => x.BrandName).ToListAsync();

                return View(await temp);
            }
            else
            {
                MedName = MedName.Trim().ToLower();

                var temp = _context.ShisMedicines.Where(x =>
                (
                    (MedTypeQuery == null) ? x.MedType == x.MedType :
                        ((MedTypeQuery == "-") ? x.MedType == x.MedType : x.MedType == MedTypeQuery)
                )
                && (
                        (MedName == null) ? x.GenericName == x.GenericName :
                        (x.GenericName.ToLower().Contains(MedName) || x.BrandName.ToLower().Contains(MedName))
                    )
                ).OrderBy(x => x.GenericName).ThenBy(x => x.BrandName).ToListAsync();

                return View(await temp);
            }
        }


        //private IEnumerable<ShisMedicine> GetCustomers()
        //{

        //    var query = _context.ShisMedicines.OrderBy(x => x.GenericName);
        //    return query.ToList();

        //}

        // GET: Maintenance/ShisMedicines
        //public async Task<IActionResult> Index(string MedTypeQuery)
        //{
        //    return View(await _context.ShisMedicines.Where(x => x.MedType == MedTypeQuery).OrderBy(x => x.GenericName).ToListAsync());
        //    //return View(await _context.ShisMedicines.ToListAsync());
        //@Html.DropDownList("ShisMedicines", (SelectList) ViewBag.SelectList, "請選擇客戶", new { id = "Customers" })
        //}

        // GET: Maintenance/ShisMedicines/Details/5
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

        // GET: Maintenance/ShisMedicines/Create
        public IActionResult Create()
        {
            return View();
        }

        //public async Task<IActionResult> Create([Bind("MedId,ProductName,Nomenclature,Sepc,Unit,StartDate,EndDate,Status,CreateUser,CreateDate,ModifyUser,ModifyDate")] ShisMedicine shisMedicine)
        // POST: Maintenance/ShisMedicines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MedId,MedType,GenericName,BrandName,UnitSpec,PackSpec,DefaultFreq,RefDuration,Remarks,StartDate,EndDate,CreateUser,ModifyUser,Status")] ShisMedicine shisMedicine)
        {
            if (ModelState.IsValid)
            {
                var intMed_id = _context.ShisMedicines.Select(x => int.Parse(x.MedId)).ToList().Max() + 1;

                var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                shisMedicine.ModifyUser = login.EMPCODE;
                shisMedicine.CreateUser = login.EMPCODE;
                shisMedicine.MedId = intMed_id.ToString();

                _context.Add(shisMedicine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(shisMedicine);
        }

        // GET: Maintenance/ShisMedicines/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ShisMedicines == null)
            {
                return NotFound();
            }

            var shisMedicine = await _context.ShisMedicines.FindAsync(id);
            if (shisMedicine == null)
            {
                return NotFound();
            }
            return View(shisMedicine);
        }

        //public async Task<IActionResult> Edit(string id, [Bind("MedId,ProductName,Nomenclature,Sepc,Unit,StartDate,EndDate,Status,CreateUser,CreateDate,ModifyUser,ModifyDate")] ShisMedicine shisMedicine)
        // POST: Maintenance/ShisMedicines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MedId,MedType,GenericName,BrandName,UnitSpec,PackSpec,DefaultFreq,RefDuration,Remarks,StartDate,EndDate,CreateUser,ModifyUser,Status")] ShisMedicine shisMedicine)
        {
            if (id != shisMedicine.MedId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");

                    shisMedicine.ModifyUser = login.EMPCODE;

                    _context.Update(shisMedicine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShisMedicineExists(shisMedicine.MedId))
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
            return View(shisMedicine);
        }

        // GET: Maintenance/ShisMedicines/Delete/5
        public async Task<IActionResult> Delete(string id)
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

        // POST: Maintenance/ShisMedicines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ShisMedicines == null)
            {
                return Problem("Entity set 'SHISContext.ShisMedicines'  is null.");
            }
            var shisMedicine = await _context.ShisMedicines.FindAsync(id);
            if (shisMedicine != null)
            {
                _context.ShisMedicines.Remove(shisMedicine);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShisMedicineExists(string id)
        {
            return _context.ShisMedicines.Any(e => e.MedId == id);
        }

        public string UpdateStatus(string inMedId)
        {
            string strR = "";
            string strStatus = "";

            if (inMedId == null || _context.ShisMedicines == null)
            {
                strR = "修改出現錯誤!!";

            }
            else
            {
                if (!ShisMedicineExists(inMedId))
                {
                    strR = "修改出現錯誤!!";
                }
                else
                {
                    List<ShisMedicine> MedList = new List<ShisMedicine>();
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");

                    MedList = _context.ShisMedicines.Where(e => e.MedId == inMedId).ToList();

                    if (MedList.Any())
                    {
                        try
                        {
                            ShisMedicine med = MedList.First();
                            med = MedList.First();

                            strStatus = med.Status.ToString();

                            if (strStatus == "1")
                            {
                                med.Status = '2';
                            }
                            else
                            {
                                med.Status = '1';
                            }

                            med.ModifyDate = DateTime.Now;
                            med.ModifyUser = login.EMPCODE;
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
