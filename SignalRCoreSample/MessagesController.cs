using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace SignalRCoreSample
{
    public class MessageRequestModel 
    {
        [Required]
        public string Message { get; set; }
    }

    [Route("messages")]
    public class MessagesController : Controller
    {
        private readonly HubLifetimeManager<ChatHub> _chatHub;

        public MessagesController(HubLifetimeManager<ChatHub> chatHub)
        {
            _chatHub = chatHub;
        }

        [HttpPost]
        public IActionResult Post([FromBody]MessageRequestModel requestModel)
        {
            string timestamp = DateTime.Now.ToShortTimeString();

            _chatHub.SendAllAsync("Message_Received", new[] 
            { 
                new
                {
                    Message = requestModel.Message,
                    Timestamp = timestamp
                }
            });

            return Ok();
        }
    }
}
