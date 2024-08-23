using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU.HisOrder.Console.Gateway.Models
{
    public class ResultDTO
    {
        /// <summary>
        /// 狀態(ex:00,66等...)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 訊息(ex:Exception...)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 此作業是否完成(true/false)
        /// </summary>
        public bool isSuccess { get; set; }

        /// <summary>
        /// 回傳值(ex:HealthPlanID...)
        /// </summary>
        public string returnValue { get; set; }

        /// <summary>
        /// 該診次中目前最新的檢核結果
        /// </summary>
        public string VerifyResult { get; set; }



    }


    enum ConnectionStatus { 
    
        Online,
        Export
    }

    public class ImportResult 
    {
        public int FailedImports_RootCount { get { return FailedImports_Root.Count; } }
        public int SuccessfulImports_RootCount { get { return SuccessfulImports_Root.Count; } }
        public int FailedImports_PinfoCount { get { return FailedImports_Pinfo.Count; } }
        public int SuccessfulImports_PinfoCount { get { return SuccessfulImports_Pinfo.Count; } }
        public int FailedImports_MPCount { get { return FailedImports_MP.Count; } }
        public int SuccessfulImports_MPCount { get { return SuccessfulImports_MP.Count; } }

        // 新增方法來計算成功和失敗的總數量
        public int TotalFailedCount
        {
            get
            {
                return FailedImports_RootCount + FailedImports_PinfoCount + FailedImports_MPCount;
            }
        }

        public int TotalSuccessfulCount
        {
            get
            {
                return SuccessfulImports_RootCount + SuccessfulImports_PinfoCount + SuccessfulImports_MPCount;
            }
        }

        // 新增方法來計算成功和失敗的總筆數
        public int TotalCount
        {
            get
            {
                return TotalFailedCount + TotalSuccessfulCount;
            }
        }

        public int totalRoot = 0;
        public List<Root> FailedImports_Root = new List<Root>();
        public List<Root> SuccessfulImports_Root  = new List<Root>();
        public List<PatientInfo> FailedImports_Pinfo = new List<PatientInfo>();
        public List<PatientInfo> SuccessfulImports_Pinfo = new List<PatientInfo>();
        public List<MergePatient> FailedImports_MP = new List<MergePatient>();
        public List<MergePatient> SuccessfulImports_MP = new List<MergePatient>();
    }


}
