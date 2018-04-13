using Microsoft.AspNetCore.Mvc;

namespace InternalApis
{
    [Route("limits")]
    public class LimitsController : ControllerBase
    {
        [HttpGet]
        public int[] GetLimits() 
        {
            return new [] 
            {
                1, 2, 3, 4, 5
            };
        }
    }
}