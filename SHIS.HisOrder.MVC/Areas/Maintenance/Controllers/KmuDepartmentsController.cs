using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SHIS.HisOrder.MVC.Models;
using SHIS.HisOrder.MVC.Areas.HisOrder.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using SHIS.HisOrder.MVC.Extesion;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Controllers
{
    [Area("Maintenance")]
    [Authorize(Roles = "Maintain_ShisDepartments")]
    public class ShisDepartmentsController : Controller
    {
        private readonly SHISContext _context;

        public ShisDepartmentsController(SHISContext context)
        {
            _context = context;
        }

        // GET: Maintenance/ShisDepartments
        public async Task<IActionResult> Index()
        {
            var department = _context.ShisDepartments.Where(d => d.DptParent != "").OrderBy(d=> d.DptCode).ToList();
            return View(department);
           // if (department == null)
           // {
           
           // }
          
           // var result = _context.ShisDepartments.Where(d => d.DptCategory.ToUpper().Contains(department.ToUpper()) & d.DptParent.ToUpper().Contains(DName.ToUpper()) & d.DptName.ToUpper().Contains(name.ToUpper()));
           //return View(result);
        }

        public IActionResult Close(string id)
        {
            var room = _context.ClinicSchedules.Where(r=> r.ScheDptCode == id && r.ScheOpenFlag == "Y").ToList();
            if (room.Any())
            {
                TempData["roomAlert"] = "go to clinicschedule and make off all the clinics has this room" +" "+ id;
                
            }
            else
            {
                var
                    result = _context.ShisDepartments.Find(id);
                if (result.DptStatus == "Y")
                {
                    result.DptStatus = "N";
                }
                else
                {
                    result.DptStatus = "Y";
                }
                var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                result.ModifyUser = login.EMPCODE;
                _context.Update(result);
                _context.SaveChanges();
            }
     
             

            return RedirectToAction("Index");
        }

        public IActionResult Editt(string id, string name)
        {
            var found = _context.ShisDepartments.Find(id);
            found.DptName = name;
            _context.Update(found);
            _context.SaveChanges();

            return RedirectToAction("Index", new {id = found.DptCode});
        }


        public IActionResult Search(string department, string DName, string name)
        {

            var result = _context.ShisDepartments.Where(d => d.DptCategory.ToUpper().Contains(department.ToUpper()) & d.DptParent.ToUpper().Contains(DName.ToUpper()) & d.DptName.ToUpper().Contains(name.ToUpper()));
            return View(result);
        }

        // GET: Maintenance/ShisDepartments/Details/5
        public async Task<IActionResult> Details(string id)
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

        // GET: Maintenance/ShisDepartments/Create
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Department(string CDpName)
        {

            var deparment = _context.ShisDepartments.Where(d => d.DptParent == "" & d.DptCategory == CDpName).ToList();
            return Json(deparment);
        }

        // POST: Maintenance/ShisDepartments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShisDepartment shisDepartment)
        {
          

            var maxdptCode1 = _context.ShisDepartments.Where(d => d.DptParent == shisDepartment.DptParent).Max(d => d.DptCode);
            int all = Convert.ToInt32(maxdptCode1);
            int last = all += 1;
            string lv = Convert.ToString(all);
            if (shisDepartment.DptCategory == "OPD")
            {
                shisDepartment.DptCode = "0" + lv;

            }
            if (shisDepartment.DptCategory == "EMG")
            {
                shisDepartment.DptCode = lv;

            }

            var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
            shisDepartment.ModifyUser = login.EMPCODE;
            shisDepartment.DptDepth = 2;
            shisDepartment.ModifyTime = DateTime.Now;
            shisDepartment.DptStatus = "Y";

            if (ModelState.IsValid)
            {
                _context.Add(shisDepartment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shisDepartment);
        }

        // GET: Maintenance/ShisDepartments/Edit/5
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

        // POST: Maintenance/ShisDepartments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id,ShisDepartment shisDepartment)
        {
            //var maxdptCode1 = _context.ShisDepartments.Where(d => d.DptParent == shisDepartment.DptParent).Max(d => d.DptCode);
            //int all = Convert.ToInt32(maxdptCode1);
            //int last = all += 1;
            //string lv = Convert.ToString(all);
            //if (shisDepartment.DptCategory == "OPD")
            //{
            //    shisDepartment.DptCode = "0" + lv;

            //}
            //if (shisDepartment.DptCategory == "EMG")
            //{
            //    shisDepartment.DptCode = lv;

            //}
            var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
            shisDepartment.ModifyUser = login.EMPCODE;
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
                return RedirectToAction("Index", "ShisDepartments");
            }
            return RedirectToAction("Index", "ShisDepartments");
        }

        // GET: Maintenance/ShisDepartments/Delete/5
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

        // POST: Maintenance/ShisDepartments/Delete/5
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
