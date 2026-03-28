using SHIS.HisOrder.MVC.Areas.HisOrder.Models;
using SHIS.HisOrder.MVC.Areas.HisOrder.ViewModels;
using SHIS.HisOrder.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Controllers
{
    [Area("Maintenance")]
    [Authorize(Roles = "Maintain_CallingSetting")]//登入後可依據設定的 專案名稱 Calling_Settings 作為判斷依據
    public class CallingSettingsController : Controller
    {
        private readonly SHISContext _context;

        public CallingSettingsController(SHISContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index() 
        {
            return View(await _context.ShisCoderefs.Where(x => x.RefCodetype == "call_area" | x.RefCodetype == "clinic_room").OrderBy(x => x.RefCode).ToListAsync());
        }

        public IActionResult Close(string id)
        {
            var result = _context.ShisCoderefs.Find(id);
            if (result.RefCasetype == "Y")
            {
                result.RefCasetype = "N";
            }
            else
            {
                result.RefCasetype = "Y";
            }
            var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
            result.ModifyId = login.EMPCODE;
            _context.Update(result);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Search(string Code, string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                var temp = _context.ShisCoderefs.OrderBy(x => x.RefCodetype).ThenBy(x => x.RefCode).ToListAsync();

                return View(await temp);
            }
            else
            {
                Name = Name.Trim().ToLower();

                var temp = _context.ShisCoderefs.Where(x =>
                (
                    (Code == null) ? x.RefName == x.RefName :
                        ((Code == "-") ? x.RefName == x.RefName : x.RefName == Code)
                )
                && (
                        (Name == null) ? x.RefCode == x.RefCode :
                        (x.RefCode.ToLower().Contains(Code) || x.RefName.ToLower().Contains(Name))
                    )
                ).OrderBy(x => x.RefCode).ThenBy(x => x.RefName).ToListAsync();

                return View(await temp);
            }
        }

        public IActionResult CreateRoom()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom([Bind("RefCode,RefCodetype,RefName,RefDes,RefId,RefCasetype,RefShowseq,RefDes2,ModifyId,ModifyTime,RefDefaultFlag")] ShisCoderef shisCoderef)
        {
            if (ModelState.IsValid)
            {
                
                var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                shisCoderef.ModifyId = login.EMPCODE;
                shisCoderef.ModifyTime = DateTime.Now;
                shisCoderef.RefId = "";

                _context.Add(shisCoderef);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(shisCoderef);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( ShisCoderef shisCoderef)
        {
            if (ModelState.IsValid)
            {
                

                var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                shisCoderef.ModifyId = login.EMPCODE;
                shisCoderef.ModifyTime= DateTime.Now;
                shisCoderef.RefCasetype = "Y";

                shisCoderef.RefId = "";
                _context.Add(shisCoderef);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(shisCoderef);
        }
       

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ShisCoderefs == null)
            {
                return NotFound();
            }

            var shisCoderef = await _context.ShisCoderefs.FindAsync(id);
            if (shisCoderef == null)
            {
                return NotFound();
            }
            return View(shisCoderef);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("RefCode,RefCodetype,RefName,RefDes,RefId,RefCasetype,RefShowseq,RefDes2,ModifyId,ModifyTime,RefDefaultFlag")] ShisCoderef shisCoderef)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var login = HttpContext.Session.GetObject<LoginDTO>("LoginDTO");
                    shisCoderef.ModifyId= login.EMPCODE;
                    shisCoderef.ModifyTime = DateTime.Now;
                    shisCoderef.RefId = id;

                    _context.Update(shisCoderef);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShisCodeRefExists(shisCoderef.RefId))
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
            return View(shisCoderef);
        }


        // GET: Maintenance/ShisCodref/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ShisCoderefs == null)
            {
                return NotFound();
            }

            var shisCoderef = await _context.ShisCoderefs
                .FirstOrDefaultAsync(m => m.RefId == id);
            if (shisCoderef == null)
            {
                return NotFound();
            }

            return View(shisCoderef);
        }

        // POST: Maintenance/ShisCoderefs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ShisCoderefs == null)
            {
                return Problem("Entity set 'SHISContext.ShisCodeRefs'  is null.");
            }
            var ShisCoderefs = await _context.ShisCoderefs.FindAsync(id);
            if (ShisCoderefs != null)
            {
                _context.ShisCoderefs.Remove(ShisCoderefs);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool ShisCodeRefExists(string id)
        {
            return _context.ShisCoderefs.Any(e => e.RefId == id);
        }

    }
}
