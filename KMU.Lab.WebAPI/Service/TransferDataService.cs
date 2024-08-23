
using AutoMapper;
using KMU.Lab.WebAPI.Data;
using KMU.Lab.WebAPI.Models;
using Newtonsoft.Json;

namespace KMU.Lab.WebAPI.Service
{
    public class TransferDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public TransferDataService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public ResultDTO GetPatientById(string? patientId)
        {
            var result = new ResultDTO() { isSuccess = false};
            try
            {
                if(patientId == null)
                {
                    result.Message = "Patient Id is empty";
                    return result;
                }
                else
                {
                    var patientExist = _context.KmuChart.SingleOrDefault(p => p.HealthId == patientId);
                    if(patientExist == null)
                    {
                        result.Message = "Patient Id is not correct";
                        return result;
                    }
                    else
                    {
                        var patients = _context.KmuChart.Where(p => p.HealthId == patientId).ToList();
                        result.Kmuchart = new List<KmuChart> { };
                        if(patients != null && patients.Count > 0)
                        {
                        foreach(var pt in patients)
                           {
                               result.Kmuchart.Add(pt);
                           }

                        }
                        
                    }
                }
                result.isSuccess = true;
                return result;
            }
            catch (Exception error)
            {
                result.Message = error.ToString();
                return result;
            }
        }


        public ResultDTO GetTestOrderById(string? patientId, string? testorderId)
        {
            var result = new ResultDTO() { isSuccess = false };
            try
            {
                if (testorderId == null)
                {
                    result.Message = "Test order Id is empty";
                    return result;
                }
                if(patientId == null)
                {
                    result.Message = "Patient Id is empty";
                    return result;
                }
                else
                {
                    var testOrderExist = _context.Hisorderplans.Where( t => t.PatientId == patientId && t.TestOrderId == testorderId);
                    if (!testOrderExist.Any())
                    {
                        result.Message = "Please enter a valid patientId and test orderId";
                        return result;
                    }
                    else
                    {
                        var testOrders = _context.Hisorderplans.Where(t => (t.TestOrderId == testorderId && t.PatientId == patientId) && (t.TestType == "Lab" && t.Status == '2')).ToList();
                        result.hisorderpalan = new List<Hisorderplan> { };
                        if (testOrders != null && testOrders.Count > 0)
                        {
                            foreach (var testorder in testOrders)
                            {

                                result.hisorderpalan.Add(testorder);
                            }
                        }

                    }
                }
                result.isSuccess = true;
                return result;
            }
            catch (Exception error)
            {
                result.Message = error.ToString();
                return result;
            }
        }




        
        
    }
}
