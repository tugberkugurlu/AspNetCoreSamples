using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace TurquoiseSoftware.DotNetTools.Swagger.TestApp
{
    public class ValuesController : Controller
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]ValueRequestModel value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]ValueRequestModel value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
