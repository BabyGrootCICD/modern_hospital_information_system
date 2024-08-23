namespace KMU.Lab.WebAPI.Model
{
    public class ApIExtendClass
    {
        public class ResultDTO 
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public bool isSuccess { get; set; }
            public string returnValue { get; set; }
            public string VerifyResult { get; set; }
        }

    }
}
