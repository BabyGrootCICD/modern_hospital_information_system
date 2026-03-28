
using AutoMapper;
using SHIS.MOHD.WebAPI.Models;
using SHIS.MOHD.WebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;

namespace SHIS.MOHD.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadTaskController : ControllerBase
    {
        private readonly IMapper _mapper;
        //private readonly ILogger<UploadTaskController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _confing;
        private readonly MOHDContext _context;
        private readonly MedicalRecordService _mediicalRecordService;
        private readonly LoggingService _loggingService;


        public UploadTaskController(MOHDContext context,
            IConfiguration config,
            IMapper mapper,
            MedicalRecordService medicalRecordService,
            LoggingService loggingService,
            IHttpContextAccessor httpContextAccessor)
        {
            //_logger = logger;
            _context = context;
            _confing = config;
            _mapper = mapper;
            _mediicalRecordService = medicalRecordService;
            _httpContextAccessor = httpContextAccessor;
            _loggingService = loggingService;
        }

        private (string ipAddress, string endpoint) GetRequestInfo()
        {
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
            var endpoint = _httpContextAccessor.HttpContext.Request.Path;
            return (ipAddress, endpoint);
        }

        [HttpGet]
        public IEnumerable<MohdHosp> Get()
        {
            var (ipAddress, endpoint) = GetRequestInfo();
            // Your logic
            _loggingService.LogInformation(true, "This is an informational message.", ipAddress, endpoint, "");
            return _context.MohdHosps.ToList();
        }


        [HttpGet("GET_HOSP_LIST")]
        public IEnumerable<MohdHosp> GetMohdHosp()
        {
            return _context.MohdHosps.ToList();
        }

        [HttpPatch("CUSTOM1")] // custom router  ex: /api/my/custom1 
        public IActionResult Custom1()
        {
            // custom get request 
            return Ok("Custom GET method 1");
        }

        [HttpPost("patientLog")]
        public IActionResult MOHD_PL([FromBody] MohdPatientLog mohdpatientlog)
        {
            
            try
            {
                var result = _mediicalRecordService.mohdPatientLogs(mohdpatientlog);
                return CreatedAtAction(nameof(MOHD_PL), result);
            }
            catch (Exception)
            {

                throw;
            }
        
        }
        /// <summary>
        /// upload_daily_medical_record
        /// </summary>
        /// <param name="_root"></param>
        /// <returns></returns>
        [HttpPost("MOHD_TK_01")]

        public IActionResult MOHD_TK_01([FromBody] Root _root)
        {

            try
            {
                var result = _mediicalRecordService.modifyMedicalRecord(_root);                
                if (result != null && result.isSuccess == true)
                {

                    var (ipAddress, endpoint) = GetRequestInfo();
                    _loggingService.LogInformation(
                        result.isSuccess,
                        result.Message,
                        ipAddress,
                        endpoint,
                        new
                        {
                            HospCode = (_root != null && _root.HospInfo != null) ? _root.HospInfo.HospCode : "",
                            HealthId = (_root != null && _root.PatientInfo != null) ? _root.PatientInfo.HealthId : "",
                            Inhospid = (_root != null && _root.RegInfo != null) ? _root.RegInfo.Inhospid : ""
                        });

                    return CreatedAtAction(nameof(MOHD_TK_01), result);
                }
                else
                {
                    var (ipAddress, endpoint) = GetRequestInfo();
                    _loggingService.LogInformation(
                        result.isSuccess,
                        result.Message,
                        ipAddress,
                        endpoint,
                        new
                        {
                            HospCode = (_root != null && _root.HospInfo != null) ? _root.HospInfo.HospCode : "",
                            HealthId = (_root != null && _root.PatientInfo != null) ? _root.PatientInfo.HealthId : "",
                            Inhospid = (_root != null && _root.RegInfo != null) ? _root.RegInfo.Inhospid : ""
                        });

                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                ResultDTO result = new ResultDTO()
                { isSuccess = false, Message = ex.ToString(), returnValue = _root.RegInfo.Inhospid };

                var (ipAddress, endpoint) = GetRequestInfo();
                _loggingService.LogError(result.isSuccess, result.Message, ipAddress, endpoint, _root, ex);

                return BadRequest(result);
            }
        }

        /// <summary>
        /// upload_patient_basic_info
        /// </summary>
        /// <param name="_pInfo"></param>
        /// <returns></returns>
        [HttpPut("MOHD_TK_02")]
        public IActionResult MOHD_TK_02([FromBody] PatientInfo _pInfo)
        {
           
            
            try
            {
                var result = _mediicalRecordService.modifyPatientBasicInfo(_pInfo);
                if (result != null && result.isSuccess == true)
                {
                    if (result.returnValue == null)
                    {
                        var (ipAddress, endpoint) = GetRequestInfo();
                        _loggingService.LogInformation(
                            result.isSuccess,
                            result.Message,
                            ipAddress,
                            endpoint,
                            new
                            {
                                HospCode = (_pInfo != null && _pInfo.FromHosp != null) ? _pInfo.FromHosp : "",
                                HealthId = (_pInfo != null && _pInfo.HealthId != null) ? _pInfo.HealthId : ""
                            });
                        return Ok(result);
                    }
                    else
                    {
                        var (ipAddress, endpoint) = GetRequestInfo();
                        _loggingService.LogInformation(
                            result.isSuccess,
                            result.Message,
                            ipAddress,
                            endpoint,
                            new
                            {
                                HospCode = (_pInfo != null && _pInfo.FromHosp != null) ? _pInfo.FromHosp : "",
                                HealthId = (_pInfo != null && _pInfo.HealthId != null) ? _pInfo.HealthId : ""
                            });
                        return CreatedAtAction(nameof(MOHD_TK_02), result);
                    }
                }
                else
                {
                    var (ipAddress, endpoint) = GetRequestInfo();
                    _loggingService.LogInformation(
                        result.isSuccess,
                        result.Message,
                        ipAddress,
                        endpoint,
                        new
                        {
                            HospCode = (_pInfo != null && _pInfo.FromHosp != null) ? _pInfo.FromHosp : "",
                            HealthId = (_pInfo != null && _pInfo.HealthId != null) ? _pInfo.HealthId : ""
                        });
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                ResultDTO result = new ResultDTO()
                { isSuccess = false, Message = ex.ToString(), returnValue = _pInfo.HealthId };

                var (ipAddress, endpoint) = GetRequestInfo();
                _loggingService.LogError(result.isSuccess, result.Message, ipAddress, endpoint, _pInfo, ex);

                return BadRequest(result);
            }

        }

        [HttpPut("MOHD_TK_03")]
        public IActionResult MOHD_TK_03([FromBody] MergePatient _mergeP)
        {

            try
            {
                var result = _mediicalRecordService.ModifyMergePatient(_mergeP);
                if (result != null && result.isSuccess == true)
                {
                    if (result.returnValue == null)
                    {
                        var (ipAddress, endpoint) = GetRequestInfo();
                        _loggingService.LogInformation(
                            result.isSuccess,
                            result.Message,
                            ipAddress,
                            endpoint,
                            new
                            {
                                HospCode = (_mergeP != null && _mergeP.FromHosp != null) ? _mergeP.FromHosp : "",
                                HealthId = (_mergeP != null && _mergeP.ChrHalthId != null) ? _mergeP.ChrHalthId : ""
                            });
                        return Ok(result);
                    }
                    else
                    {
                        var (ipAddress, endpoint) = GetRequestInfo();
                        _loggingService.LogInformation(
                            result.isSuccess,
                            result.Message,
                            ipAddress,
                            endpoint,
                            new
                            {
                                HospCode = (_mergeP != null && _mergeP.FromHosp != null) ? _mergeP.FromHosp : "",
                                HealthId = (_mergeP != null && _mergeP.ChrHalthId != null) ? _mergeP.ChrHalthId : ""
                            });
                        return CreatedAtAction(nameof(MOHD_TK_03), result);
                    }
                }
                else
                {
                    var (ipAddress, endpoint) = GetRequestInfo();
                    _loggingService.LogInformation(
                        result.isSuccess,
                        result.Message,
                        ipAddress,
                        endpoint,
                        new
                        {
                            HospCode = (_mergeP != null && _mergeP.FromHosp != null) ? _mergeP.FromHosp : "",
                            HealthId = (_mergeP != null && _mergeP.ChrHalthId != null) ? _mergeP.ChrHalthId : ""
                        });
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                ResultDTO result = new ResultDTO()
                { isSuccess = false, Message = ex.ToString(), returnValue = _mergeP.ChrHalthId };

                var (ipAddress, endpoint) = GetRequestInfo();
                _loggingService.LogError(result.isSuccess, result.Message, ipAddress, endpoint, _mergeP, ex);

                return BadRequest(result);
            }
        }

    };
}
