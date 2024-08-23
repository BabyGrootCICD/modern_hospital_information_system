using KMU.Lab.WebAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KMU.Lab.WebAPI.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using KMU.Lab.WebAPI.Data;

namespace KMU.Lab.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
   
    public class TransferDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TransferDataService _transferdataservice;
        public TransferDataController(ApplicationDbContext context, TransferDataService transferDataService)
        {
            _context = context;

            _transferdataservice = transferDataService;
        }





        [HttpGet("GetPatientById")]
        public IActionResult GetPatientById(string? patientId)
        {
            var result = _transferdataservice.GetPatientById(patientId);

            try
            {
                if(result != null && result.isSuccess == true)
                {
                    return Ok(result.Kmuchart);
                }
                else
                {
                    return Ok(result.Message);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
               
            }            
        }

        [HttpGet("GetTestOrderById")]
        public IActionResult GetTestOrderById(string? patientId , string? testorderId)
        {
            var result = _transferdataservice.GetTestOrderById(patientId,testorderId);

            try
            {
                if (result != null && result.isSuccess == true)
                {
                    return Ok(result.hisorderpalan);
                }
                else
                {
                    return Ok(result.Message);
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex);

            }
        }

    }
    }

