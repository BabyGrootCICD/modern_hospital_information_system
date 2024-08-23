using KMU.MOHD.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KMU.MOHD.WebAPI.Service
{
    public class LoggingService
    {
        private readonly MOHDContext _mohdContext;

        public LoggingService(MOHDContext mohdContext)
        {
            _mohdContext = mohdContext;
        }

        public void LogInformation(bool inSuccess , string inMessage , string ipAddress, string endpoint, object requestData)
        {
            _mohdContext.MohdLogentries.Add(new MohdLogentry
            {
                Loglevel = "Information",
                ResultSuccess = inSuccess,
                ResultMessage = inMessage,
                Ipaddress = ipAddress,
                Endpoint = endpoint,
                Requestdata = requestData != null ? JsonConvert.SerializeObject(requestData) : "",
                Exception = "",
                Createtime = DateTime.Now
            });


            _mohdContext.SaveChanges();
        }

        public void LogError(bool inSuccess, string inMessage, string ipAddress, string endpoint, object requestData, Exception ex )
        {
            _mohdContext.MohdLogentries.Add(new MohdLogentry
            {
                Loglevel = "Error",
                ResultSuccess = inSuccess,
                ResultMessage = inMessage,
                Ipaddress = ipAddress,
                Endpoint = endpoint,
                Requestdata = requestData != null ? JsonConvert.SerializeObject(requestData) : "",
                Exception = ex!=null ? ex.ToString(): "",
                Createtime = DateTime.Now
            });

            _mohdContext.SaveChanges();
        }

    }
}
