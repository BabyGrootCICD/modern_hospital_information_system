
using AutoMapper;
using KMU.MOHD.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KMU.MOHD.WebAPI.Service
{
    public class MedicalRecordService
    {
        private readonly MOHDContext _mohdContext;
        private readonly IMapper _mapper;


        public MedicalRecordService(MOHDContext mohdContext, IMapper mapper)
        {
            _mohdContext = mohdContext;
            _mapper = mapper;
        }
        public MohdPatientLog mohdPatientLogs(MohdPatientLog patientlog)
        {
           if(patientlog != null)
            {
            _mohdContext.MohdPatientLog.Add(patientlog);
            _mohdContext.SaveChanges();
            }
            return patientlog;
        }

        public ResultDTO ModifyMergePatient(MergePatient mergePatient)
        {
            var result = new ResultDTO() { isSuccess = false };
            try
            {
                if (mergePatient == null)
                {
                    result.Message = "patientInfo is null";
                    return result;
                }

                var chartPatient = _mohdContext.MohdCharts.Where(p=> p.HealthId == mergePatient.MhHealthId);
              

                foreach(var item in chartPatient)
                {
                    
                    item.CombineFlag = '1';
                    _mohdContext.MohdCharts.Update(item);
                   
                }

                var reg = _mohdContext.MohdRegistrations.Where(p => p.HealthId == mergePatient.MhHealthId);
                foreach(var item in reg)
                {
                    item.HealthId = mergePatient.ChrHalthId;
                    _mohdContext.MohdRegistrations.Update(item);
                }
                var hoplan = _mohdContext.MohdHisorderplans.Where(p => p.HealthId == mergePatient.MhHealthId);
                foreach(var item in hoplan)
                {
                    item.HealthId = mergePatient.ChrHalthId;
                }

                var hosoa = _mohdContext.MohdHisordersoas.Where(p => p.HealthId == mergePatient.MhHealthId);
                foreach(var item in hosoa)
                {
                    item.HealthId = mergePatient.ChrHalthId;
                    _mohdContext.MohdHisordersoas.Update(item);
                }

                
                _mohdContext.SaveChanges();
                result.isSuccess = true;
                result.Message = "Data upload is complete, and it has been processed without any errors.";
                result.returnValue = mergePatient.ChrHalthId;
       
                return result;

            }
            catch (Exception ex)
            {
                result.Message += ex.ToString();
                return result;
            }
        }
        public ResultDTO modifyPatientBasicInfo(PatientInfo inpinfo)
        {
            var result = new ResultDTO() { isSuccess = false };
            try
            {
                if (inpinfo == null)
                {
                    result.Message = "patientInfo is null";
                    return result;
                }

                //搜尋是否是舊案(已上傳，再次上傳) 
                var orgChartExists = _mohdContext.MohdCharts
                    .Any(c => c.FromHosp == inpinfo.FromHosp && c.HealthId == inpinfo.HealthId);

                if (orgChartExists)
                {
                    var newChart = _mapper.Map<MohdChart>(inpinfo);
                    _mohdContext.MohdCharts.Update(newChart);
                    
                }
                else
                {
                    var newChart = _mapper.Map<MohdChart>(inpinfo);
                    _mohdContext.MohdCharts.Add(newChart);
                    result.returnValue = JsonConvert.SerializeObject(newChart);
                }


                _mohdContext.SaveChanges();
                result.isSuccess = true;
                result.Message = "Data upload is complete, and it has been processed without any errors.";
                
                
                return result;

            }
            catch (Exception ex)
            {
                result.Message += ex.ToString();
                return result;
            }

        }



        public ResultDTO modifyMedicalRecord(Root root)
        {
            var result = new ResultDTO() { isSuccess = false };

            try
            {
                if (root == null)
                {
                    result.Message = "root is null";
                    return result;
                }

                if (root.HospInfo == null)
                {
                    result.Message = "HospInfo is null";
                    return result;
                }

                if (root.PatientInfo == null)
                {
                    result.Message = "PatientInfo is null";
                    return result;
                }

                //搜尋是否是舊案(已上傳，再次上傳)
                var orgChartExists = _mohdContext.MohdCharts.Any(c => c.FromHosp == root.HospInfo.HospCode && c.HealthId == root.PatientInfo.HealthId);

                var orgRegisterExists = _mohdContext.MohdRegistrations.Any(c => c.HospCode == root.HospInfo.HospCode && c.Inhospid == root.RegInfo.Inhospid && c.RegDate == root.RegInfo.RegDate);




                if (orgChartExists)
                {
                    var newChart = _mapper.Map<MohdChart>(root.PatientInfo);
                    _mohdContext.MohdCharts.Update(newChart);
                }
                else
                {
                    var newChart = _mapper.Map<MohdChart>(root.PatientInfo);
                    _mohdContext.MohdCharts.Add(newChart);

                }

                if (orgRegisterExists)
                {
                    //step1 : 清除所有資訊
                    //掛號資訊
                    var oldReg = _mohdContext.MohdRegistrations
                        .Where(c => c.HospCode == root.HospInfo.HospCode
                        && c.Inhospid == root.RegInfo.Inhospid
                        && c.RegDate == root.RegInfo.RegDate)
                        .ToList();

                    if (oldReg != null && oldReg.Count > 0)
                    {
                        _mohdContext.MohdRegistrations.RemoveRange(oldReg);
                    }


                    var oldHisOrderPlan = _mohdContext.MohdHisorderplans
                        .Where(c => c.Inhospid == root.RegInfo.Inhospid
                        && c.HospCode == root.HospInfo.HospCode).ToList();


                    if (oldHisOrderPlan != null && oldHisOrderPlan.Count > 0)
                    {
                        _mohdContext.MohdHisorderplans.RemoveRange(oldHisOrderPlan);
                    }

                    var oldHisOrderSOA = _mohdContext.MohdHisordersoas
                        .Where(c => c.Inhospid == root.RegInfo.Inhospid
                        && c.HospCode == root.HospInfo.HospCode).ToList();


                    if (oldHisOrderSOA != null && oldHisOrderSOA.Count > 0)
                    {
                        _mohdContext.MohdHisordersoas.RemoveRange(oldHisOrderSOA);
                    }


                    //step2: 新增所有資訊
                    var newReg = _mapper.Map<MohdRegistration>(root.RegInfo);
                    _mohdContext.MohdRegistrations.Add(newReg);

                    var newDx = _mapper.Map<List<MohdHisorderplan>>(root.Dx);
                    var newMed = _mapper.Map<List<MohdHisorderplan>>(root.Med);
                    var newOther = _mapper.Map<List<MohdHisorderplan>>(root.Others);

                    if (newDx != null && newDx.Count > 0)
                    {
                        _mohdContext.MohdHisorderplans.AddRange(newDx);
                    }

                    if (newMed != null && newMed.Count > 0)
                    {
                        _mohdContext.MohdHisorderplans.AddRange(newMed);
                    }

                    if (newOther != null && newOther.Count > 0)
                    {
                        _mohdContext.MohdHisorderplans.AddRange(newOther);
                    }

                    var newSOA = _mapper.Map<List<MohdHisordersoa>>(root.SOAP);
                    if (newSOA != null && newSOA.Count > 0)
                    {
                        _mohdContext.MohdHisordersoas.AddRange(newSOA);
                    }

                }
                else
                {
                    //step2: 新增所有資訊
                    var newReg = _mapper.Map<MohdRegistration>(root.RegInfo);
                    _mohdContext.MohdRegistrations.Add(newReg);

                    var newDx = _mapper.Map<List<MohdHisorderplan>>(root.Dx);
                    var newMed = _mapper.Map<List<MohdHisorderplan>>(root.Med);
                    var newOther = _mapper.Map<List<MohdHisorderplan>>(root.Others);

                    if (newDx != null && newDx.Count > 0)
                    {
                        _mohdContext.MohdHisorderplans.AddRange(newDx);
                    }

                    if (newMed != null && newMed.Count > 0)
                    {
                        _mohdContext.MohdHisorderplans.AddRange(newMed);
                    }

                    if (newOther != null && newOther.Count > 0)
                    {
                        _mohdContext.MohdHisorderplans.AddRange(newOther);
                    }

                    var newSOA = _mapper.Map<List<MohdHisordersoa>>(root.SOAP);
                    if (newSOA != null && newSOA.Count > 0)
                    {
                        _mohdContext.MohdHisordersoas.AddRange(newSOA);
                    }
                }


                _mohdContext.SaveChanges();
                result.isSuccess = true;
                result.Message = "Data upload is complete, and it has been processed without any errors.";
                result.returnValue = root.RegInfo.Inhospid;
                return result;

            }
            catch (Exception e)
            {
                result.Message += e.ToString();
                return result;
            }
        }



    }
}
