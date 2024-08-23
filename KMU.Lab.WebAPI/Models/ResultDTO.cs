namespace KMU.Lab.WebAPI.Models
{
    public class ResultDTO
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public bool isSuccess { get; set; }
        public string returnValue { get; set; }
        public string verifyResult { get; set; }
        public List<KmuChart> Kmuchart { get; set; }
        public List<Hisorderplan> hisorderpalan { get; set; }
    }
}
