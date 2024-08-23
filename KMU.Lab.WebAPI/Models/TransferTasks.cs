

namespace KMU.Lab.WebAPI.Models
{
 
        public class Patientinfo
        {
        public string patientId { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public string LName { get; set; }
        public string? Address { get; set; }
        public string sex { get; set; }
        public DateOnly? birthDate { get; set; }
        public string? mobilePhone { get; set; }
    }
   
        public class hisorderpalan
        {
            public int orderplanid { get; set; }
            public string inhospid { get; set; }
            public string health_id { get; set; }
            public string hplan_type { get; set; }
            public string plan_code { get; set; }
            public string plan_des { get; set; }
            public DateTime? createDate { get; set; }
            public char? status { get; set; }

    }


    public class Root
   {
       public Patientinfo patientInfo { get; set; }    
       public List<hisorderpalan> others { get; set; }  
   }
    
}
