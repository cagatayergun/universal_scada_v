using Microsoft.AspNetCore.Mvc;
using TekstilScada.Models;
using TekstilScada.Repositories;
using System.Collections.Generic;

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlarmsController : ControllerBase
    {
        private readonly AlarmRepository _alarmRepository;

        public AlarmsController()
        {
            // Gerçek projede DI kullanın
            _alarmRepository = new AlarmRepository();
        }

        [HttpGet]
        public ActionResult<List<AlarmDefinition>> GetAll()
        {
            return Ok(_alarmRepository.GetAllAlarmDefinitions());
        }

        [HttpPost]
        public ActionResult Add([FromBody] AlarmDefinition alarm)
        {
            _alarmRepository.AddAlarmDefinition(alarm); // Repository'de bu metodun olduğundan emin olun
            return Ok();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] AlarmDefinition alarm)
        {
            alarm.Id = id;
            _alarmRepository.UpdateAlarmDefinition(alarm); // Repository'de bu metodun olduğundan emin olun
            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            _alarmRepository.DeleteAlarmDefinition(id); // Repository'de bu metodun olduğundan emin olun
            return Ok();
        }
    }
}