using Microsoft.AspNetCore.Mvc;
using TekstilScada.Models;
using TekstilScada.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;

namespace TekstilScada.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeConfigurationsController : ControllerBase
    {
        private readonly RecipeConfigurationRepository _repository;

        public RecipeConfigurationsController()
        {
            _repository = new RecipeConfigurationRepository();
        }

        [HttpGet("subtypes")]
        public ActionResult<List<string>> GetMachineSubTypes()
        {
            return Ok(_repository.GetMachineSubTypes());
        }

        [HttpGet("steptypes")]
        public ActionResult<List<StepTypeDtoDesign>> GetStepTypes()
        {
            var dt = _repository.GetStepTypes();
            var list = new List<StepTypeDtoDesign>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new StepTypeDtoDesign
                {
                    Id = Convert.ToInt32(row["Id"]),
                    StepName = row["StepName"].ToString()
                });
            }
            return Ok(list);
        }

        // --- DÜZELTME BURADA YAPILDI ---
        [HttpGet("layout")]
        public ActionResult<List<ControlMetadata>> GetLayout([FromQuery] string subType, [FromQuery] int stepTypeId)
        {
            // 1. Veritabanından JSON string'i ham olarak çekiyoruz
            var jsonString = _repository.GetLayoutJson(subType, stepTypeId);

            if (string.IsNullOrEmpty(jsonString))
            {
                return Ok(new List<ControlMetadata>());
            }

            try
            {
                // --- KRİTİK DÜZELTME ---
                // WinForms veriyi "ControlType" (Büyük Harf) olarak kaydetmiş olabilir.
                // WebAPI varsayılan olarak "controlType" (Küçük Harf) arar.
                // Bu ayar ile büyük/küçük harf farkını ortadan kaldırıyoruz.
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var layout = JsonSerializer.Deserialize<List<ControlMetadata>>(jsonString, options);
                return Ok(layout);
            }
            catch
            {
                return Ok(new List<ControlMetadata>());
            }
        }

        [HttpPost("layout")]
        public ActionResult SaveLayout([FromQuery] string subType, [FromQuery] int stepTypeId, [FromBody] List<ControlMetadata> layout)
        {
            // Kaydederken de düzenli (Indented) kaydedelim ki veritabanında okuması kolay olsun
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(layout, options);

            string layoutName = $"{subType} - StepID:{stepTypeId}";

            _repository.SaveLayout(layoutName, subType, stepTypeId, jsonString);
            return Ok();
        }
    }
}