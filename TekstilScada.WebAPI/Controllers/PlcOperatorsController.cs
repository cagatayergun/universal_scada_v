using Microsoft.AspNetCore.Mvc;
using TekstilScada.Models;
using TekstilScada.Repositories;
using System.Collections.Generic;

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlcOperatorsController : ControllerBase
    {
        private readonly PlcOperatorRepository _repository;

        public PlcOperatorsController()
        {
            _repository = new PlcOperatorRepository();
        }

        [HttpGet]
        public ActionResult<List<PlcOperator>> GetAll()
        {
            return Ok(_repository.GetAll());
        }

        [HttpPost]
        public ActionResult Save([FromBody] PlcOperator op)
        {
            _repository.SaveOrUpdate(op);
            return Ok();
        }

        [HttpPost("default")]
        public ActionResult AddDefault()
        {
            _repository.AddDefaultOperator();
            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            _repository.Delete(id);
            return Ok();
        }
    }
}