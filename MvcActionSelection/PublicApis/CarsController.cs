using Microsoft.AspNetCore.Mvc;

namespace PublicApis
{
    [Route("cars")]
    public class CarsController : ControllerBase
    {
        [HttpGet]
        public string[] Get()
        {
            return new[] 
            {
                "Car 1",
                "Car 2",
                "Car 3"
            };
        }        
    }
}