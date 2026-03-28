namespace SHIS.Lab.WebAPI.Models
{
    public class Hisorderplan
    {
        public long Id { get; set; }
        public string PatientId { get; set; }
        public string TestOrderId { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string TestType { get; set; }
        public char Status { get; set; }
        public string? Remark { get; set; }      
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
