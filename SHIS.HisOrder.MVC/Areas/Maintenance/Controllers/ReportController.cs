using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SHIS.HisOrder.MVC.Models;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Text;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Controllers
{
    [Area("Maintenance")]
    public class ReportController : Controller
    {
        private readonly SHISContext _context;

        public ReportController(SHISContext context)
        {
            _context = context;
        }

        // GET: Statistic/Report
        public async Task<IActionResult> Index(string? month, string? year)
        {
          
            TempData["month"] = "";
            if(month==""|| month == null)
            {
                TempData["month"] = DateTime.Now.Month;
                TempData["year"] = DateTime.Now.Year;

              
                return View(month);
            }
            else
            {

           TempData["month"] = month;
           TempData["year"] = year;
           var department = _context.ShisDepartments.ToList();
           return View();
            }
        }

        //public IActionResult CardData(int Year, int Month)
        //{

        //    return Json();
        //}

        [HttpPost]
        public FileResult ExporttoExcel(string HtmlTable)
        {
            return File(Encoding.ASCII.GetBytes(HtmlTable), "application/vnd.ms-excel", "Report.xls");
         
        }


        // GET: Statistic/Report/Details/5
        public async Task<IActionResult> Details()
        {
          return View();

        }

        // GET: Statistic/Report/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Statistic/Report/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DptCode,DptName,DptCategory,DptDepth,DptStatus,DptRemark,DptDefaultAttr,ModifyUser,ModifyTime,DptParent")] ShisDepartment shisDepartment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shisDepartment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shisDepartment);
        }

        // GET: Statistic/Report/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ShisDepartments == null)
            {
                return NotFound();
            }

            var shisDepartment = await _context.ShisDepartments.FindAsync(id);
            if (shisDepartment == null)
            {
                return NotFound();
            }
            return View(shisDepartment);
        }

        // POST: Statistic/Report/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("DptCode,DptName,DptCategory,DptDepth,DptStatus,DptRemark,DptDefaultAttr,ModifyUser,ModifyTime,DptParent")] ShisDepartment shisDepartment)
        {
            if (id != shisDepartment.DptCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shisDepartment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShisDepartmentExists(shisDepartment.DptCode))
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
            return View(shisDepartment);
        }

        // GET: Statistic/Report/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ShisDepartments == null)
            {
                return NotFound();
            }

            var shisDepartment = await _context.ShisDepartments
                .FirstOrDefaultAsync(m => m.DptCode == id);
            if (shisDepartment == null)
            {
                return NotFound();
            }

            return View(shisDepartment);
        }

        // POST: Statistic/Report/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ShisDepartments == null)
            {
                return Problem("Entity set 'SHISContext.ShisDepartments'  is null.");
            }
            var shisDepartment = await _context.ShisDepartments.FindAsync(id);
            if (shisDepartment != null)
            {
                _context.ShisDepartments.Remove(shisDepartment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShisDepartmentExists(string id)
        {
          return _context.ShisDepartments.Any(e => e.DptCode == id);
        }
    }
}
