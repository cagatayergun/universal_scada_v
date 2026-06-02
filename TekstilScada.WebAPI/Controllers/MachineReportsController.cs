using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telemetry.Core.Models;
using Telemetry.WebAPI.Hubs;

namespace Telemetry.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MachineReportsController : ControllerBase
    {
        private readonly IHubContext<ScadaHub> _hubContext;

        public MachineReportsController(IHubContext<ScadaHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Geliştirici Özel Teşhis Endpoint'i (Swagger kapalıyken gerçeği gösterir)
        /// İstek Adresi: GET https://api.yilmaktelemetry.com/api/machinereports/active-connections
        /// </summary>
        [HttpGet("active-connections")]
        public IActionResult GetActiveConnections()
        {
            var connections = ScadaHub.GetActiveConnectionsDebug();
            return Ok(connections);
        }

        /// <summary>
        /// Müşterinin REST API üzerinden çağıracağı resmi Laundry şablon çıktı noktası.
        /// </summary>
        [HttpGet("laundry-data")]
        public async Task<ActionResult<List<LaundryMachineReportDto>>> GetLaundryData(
            [FromQuery] int factoryId,
            [FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime)
        {
            try
            {
                var start = startTime ?? DateTime.Today;
                var end = endTime ?? DateTime.Today.AddDays(1).AddTicks(-1);

                // --- GÜVENLİK DÜZELTMESİ: 15 GÜNLÜK MAKSİMUM TARİH LİMİTİ ---
                // Kullanıcının sunucuyu ve tüneli kilitlememesi için sınır koyuyoruz
                if ((end.Date - start.Date).TotalDays > 15)
                {
                    return BadRequest(new { message = "Due to security and system performance reasons, you can only generate reports covering a maximum of 15 days at a time. Please specify a shorter date range." });
                }
                // -----------------------------------------------------------

                var filters = new ReportFilters
                {
                    StartTime = start,
                    EndTime = end
                };

                var result = await ScadaHub.GetLaundryReportsFromController(_hubContext, factoryId, filters);

                if (result == null)
                {
                    return NotFound(new { message = $"No active SCADA Gateway connection was found for the target factory (ID: {factoryId})." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"REST API Data Communication Error: {ex.Message}" });
            }
        }
    }
}