using Microsoft.AspNetCore.Mvc;
using TekstilScada.Models;
using TekstilScada.Repositories;
using System.Collections.Generic;

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CostsController : ControllerBase
    {
        private readonly CostRepository _costRepository;

        public CostsController()
        {
            // Gerçek projede Dependency Injection kullanılması önerilir
            _costRepository = new CostRepository();
        }

        [HttpGet]
        public ActionResult<List<CostParameter>> GetAll()
        {
            return Ok(_costRepository.GetAllParameters());
        }

        [HttpPost]
        public ActionResult UpdateAll([FromBody] List<CostParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return BadRequest("Liste boş olamaz.");

            _costRepository.UpdateParameters(parameters);
            return Ok();
        }
    }
}