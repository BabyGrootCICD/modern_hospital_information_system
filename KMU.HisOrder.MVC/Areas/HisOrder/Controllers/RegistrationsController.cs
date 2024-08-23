using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KMU.HisOrder.MVC.Models;

namespace KMU.HisOrder.MVC.Areas.HisOrder.Controllers
{
    [Area("HisOrder")]
    public class RegistrationsController : Controller
    {
        private readonly KMUContext _context;

        public RegistrationsController(KMUContext context)
        {
            _context = context;
        }

        // GET: HisOrder/Registrations
        public async Task<IActionResult> Index(string? id)
        {
            HttpContext.Session.SetString("PatientId", (string)id);
             if (id == null)
            {
                id = (string)HttpContext.Session.GetString("PatientId");

            }
            TempData["PatientId"] = (string)HttpContext.Session.GetString("PatientId");
            var registration = _context.Registrations.Where(r => r.RegHealthId == id && r.RegTriage != null).ToList();
              return View(registration);
        }

        // GET: HisOrder/Registrations/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var physicalSign = _context.PhysicalSigns.Where(p => p.Inhospid == id).ToList();
            if (physicalSign == null)
            {
                return NotFound();
            }

            return View(physicalSign);
        }

        // GET: HisOrder/Registrations/Create
        public IActionResult Create()
        {
            return View();
        }

       
    }
}
