using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using QueueAPI.Model;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QueueAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TeamController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly string serviceBusConnectionString;

        public TeamController(IConfiguration config)
        {
            _config = config;
            serviceBusConnectionString = _config.GetValue<string>("ServiceBus:ConnectionString");
        }

        [HttpPost]
        [Route("queue")]
        public async Task<IActionResult> SaveAtQueue(Team team)
        {
            await SendMessageToQueue(team);
            return Ok(team);
        }

        [HttpPost]
        [Route("topic")]
        public async Task<IActionResult> SaveAtTopic(Team team)
        {
            await SendMessageToTopic(team);
            return Ok(team);
        }

        private async Task SendMessageToQueue(Team team)
        {
            var queueName = "team";

            var client = new QueueClient(serviceBusConnectionString, queueName, ReceiveMode.PeekLock);
            string messageBody = JsonSerializer.Serialize(team);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            await client.SendAsync(message);
            await client.CloseAsync();
        }

        private async Task SendMessageToTopic(Team team)
        {
            var topicName = "team-topic";

            var client = new TopicClient(serviceBusConnectionString, topicName);
            string messageBody = JsonSerializer.Serialize(team);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            await client.SendAsync(message);
            await client.CloseAsync();
        }

    }
}
