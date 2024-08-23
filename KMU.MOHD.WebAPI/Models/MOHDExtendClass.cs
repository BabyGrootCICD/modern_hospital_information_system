namespace KMU.MOHD.WebAPI.Models
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
}
