using AutoMapper;
using KMU.HisOrder.MVC.Areas.HisOrder.Models;
using KMU.HisOrder.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace KMU.HisOrder.MVC.Areas.HisOrder.Controllers
{
    [CheckClinicSessionAttribute]
    [CheckSessionTimeOutAttribute]
    [Area("HisOrder")]
    public class NonMedController : Controller
    {
        private readonly KMUContext _context;
        private readonly IMapper _mapper;

        public NonMedController(KMUContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }



        public IActionResult ReloadPartialView()
        {
            GlobalVariableDTO grv = null;

            using (HisOrderController col = new HisOrderController(_context))
            {
                grv = col.getGlobalVariablDTO(HttpContext);
                if (grv.Patient != null)
                {
                    var orderList = _context.Hisorderplans
                        .Where(c => c.Inhospid == grv.Patient.Inhospid
                        && c.HealthId == grv.Patient.RegPatientId
                        && (c.HplanType != "Med" && c.HplanType != "ICD")
                        && c.DcDate == null).OrderBy(d => d.SeqNo);

                    ViewData["PatientDTO"] = grv.Patient;
                    //取得非藥相關
                    if (_context.KmuCoderefs.Where(c => c.RefCodetype == "NonMedLocation").Count() > 0)
                    {
                        ViewBag.NonMedLocation = _context.KmuCoderefs.Where(c => c.RefCodetype == "NonMedLocation").OrderBy(d=>d.RefShowseq).ToList();
                    }
                    else
                    {
                        ViewBag.NonMedLocation = new List<KmuCoderef>();
                    }

                    if (orderList.Any())
                    {
                        return PartialView("~/Areas/HisOrder/Views/HisOrder/PartialViews/_NonMedPartialView.cshtml", orderList.ToList());
                    }
                    else
                    {
                        return PartialView("~/Areas/HisOrder/Views/HisOrder/PartialViews/_NonMedPartialView.cshtml", null);
                    }
                }
                else
                {
                    return PartialView("~/Areas/HisOrder/Views/HisOrder/PartialViews/_NonMedPartialView.cshtml", null);
                }
            }
        }

        public string GetHisOrderPlan(string inhospid)
        {
            ResultDTO result = new ResultDTO() { isSuccess = false };
            var data = _context.Hisorderplans.Where(c => c.Inhospid == inhospid && ( c.HplanType != "Med" && c.HplanType != "ICD")  && c.DcDate == null).OrderBy(d=>d.SeqNo).ToList();

            if (data.Any())
            {
                result.isSuccess = true;
                result.returnValue = JsonConvert.SerializeObject(data);
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public string ModifyNonMedOrder(string inOrder,  string inStatus)
        {
            ResultDTO result = new ResultDTO() { isSuccess = false };
            GlobalVariableDTO grv = null;

            using (HisOrderController col = new HisOrderController(_context))
            {
                grv = col.getGlobalVariablDTO(HttpContext);
                if (!string.IsNullOrWhiteSpace(inOrder))
                {
                    var data = (List<ExtendHisorderplan>)JsonConvert.DeserializeObject(inOrder, typeof(List<ExtendHisorderplan>));
                    if (data != null)
                    {
                        foreach (var obj in data)
                        {
                            var _hisorderplan = _mapper.Map<Hisorderplan>(obj);
                            switch (obj.ModifyType) 
                            {
                                case "":
                                    if(inStatus == "confirm")
                                    {
                                        UpdateNonMedByObject(_hisorderplan, grv, inStatus);
                                    }
                                    break;
                                case "I":
                                    InsertNonMedByObject(_hisorderplan, grv , inStatus);
                                    break;
                                case "D":
                                    DcNonMedByObject(_hisorderplan, grv, inStatus);
                                    break;
                                case "U":
                                    //2022.12.29 update by 1050325
                                    DcNonMedByObject(_hisorderplan, grv, inStatus , false);
                                    InsertNonMedByObject(_hisorderplan, grv, inStatus , true);
                                    //_context.SaveChanges();
                                    break;  
                            }
                        }
                    }
                }

                result.isSuccess = true;
                return JsonConvert.SerializeObject(result);
            }
        }

        private string DcNonMedByObject(Hisorderplan inOrder, GlobalVariableDTO inGrv, string inStatus, bool doSaveChange = true)
        {
            try
            {
                var result = new ResultDTO() { isSuccess = false };

                var _order = _context.Hisorderplans.Where(c => c.Inhospid == inGrv.Patient.Inhospid && c.Orderplanid == inOrder.Orderplanid).FirstOrDefault();

                if (_order != null)
                {

                    _order.DcDate = DateTime.Now;
                    _order.DcStatus = '2';
                    _order.DcUser = inGrv.Login.EMPCODE;
                    _order.Status = '0';

                    _context.Update(_order);

                    if (doSaveChange)
                    {
                        _context.SaveChanges();
                    }

                    result.isSuccess = true;
                }

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception EX)
            {
                return JsonConvert.SerializeObject(new ResultDTO() { isSuccess = false });
            }
        }

        private string UpdateNonMedByObject(Hisorderplan inOrder, GlobalVariableDTO inGrv, string inStatus)
        {
            try
            {
                var result = new ResultDTO() { isSuccess = false };


                if (inOrder.Orderplanid == null)
                {
                    var _iResult =(ResultDTO) JsonConvert.DeserializeObject(InsertNonMedByObject(inOrder, inGrv, inStatus), typeof(ResultDTO));
                    if(_iResult.isSuccess == true && _iResult.returnValue !=null)
                    {
                        var id = long.Parse(_iResult.returnValue);

                        var _order = _context.Hisorderplans.Where(c => c.Inhospid == inGrv.Patient.Inhospid && c.Orderplanid == id).FirstOrDefault();

                        if (_order != null)
                        {
                            _order.Status = inStatus == "confirm" ? '2' : _order.Status;

                            //_order.QtyDose = inOrder.QtyDose;
                            //_order.Remark = inOrder.Remark;

                            _context.Update(_order);
                            _context.SaveChanges();

                            result.isSuccess = true;
                        }

                    }
                }
                else
                { 
                    var _order = _context.Hisorderplans.Where(c => c.Inhospid == inGrv.Patient.Inhospid && c.Orderplanid == inOrder.Orderplanid).FirstOrDefault();

                    if (_order != null)
                    {
                        _order.Status = inStatus == "confirm" ? '2' : _order.Status;
                        _order.QtyDose = inOrder.QtyDose;
                        _order.Remark = inOrder.Remark;

                        _context.Update(_order);
                        _context.SaveChanges();

                        result.isSuccess = true;
                    }

                }

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception EX)
            {
                return JsonConvert.SerializeObject(new ResultDTO() { isSuccess = false });
            }


        }

        private string InsertNonMedByObject(Hisorderplan inOrder, GlobalVariableDTO inGrv, string inStatus, bool doSaveChange = true)
        {
            try
            {
                var result = new ResultDTO() { isSuccess = false };

                inOrder.Orderplanid = -1;
                inOrder.Inhospid = inGrv.Patient.Inhospid;
                inOrder.HealthId = inGrv.Patient.RegPatientId;
                //inOrder.HplanType = "Exam";
                //inOrder.SeqNo = 99;
                inOrder.CreateDate = DateTime.Now;
                inOrder.CreateUser = inGrv.Login.EMPCODE;
                inOrder.PlanDays = 1;
                inOrder.QtyDose = inOrder.QtyDose;
                inOrder.Status = string.IsNullOrWhiteSpace(inStatus) ? '0':'2';
                inOrder.PrintDate = null;
                inOrder.PrintUser = "";

                _context.Add(inOrder);

                if (doSaveChange)
                {
                    _context.SaveChanges();
                    _context.Entry(inOrder).Reload();
                }

                //result.returnValue = inOrder.Orderplanid != null ? inOrder.Orderplanid.ToString() : null;
                result.returnValue = inOrder.Orderplanid.ToString();
                result.isSuccess = true;

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception EX)
            {
                return JsonConvert.SerializeObject(new ResultDTO() { isSuccess = false });
            }
        }
    }
}
