// Dosya: Universalscada.WebAPI/Controllers/MachinesController.cs - GÜNCEL VERSİYON
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Universalscada.Core.Repositories;
using Universalscada.Core.Services;
using Universalscada.Models;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MachinesController : ControllerBase
{
    // IMachineRepository, Machine modelinin esnek Sector ve MachineConfigurationJson alanlarını içerir.
    private readonly IMachineRepository _machineRepository;
    private readonly IPlcManagerFactory _plcManagerFactory; // PLC komutları için gerekli

    public MachinesController(IMachineRepository machineRepository, IPlcManagerFactory plcManagerFactory)
    {
        _machineRepository = machineRepository;
        _plcManagerFactory = plcManagerFactory;
    }

    /// <summary>
    /// Evrensel Machine modellerini döndürür.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<Machine>> GetAllMachines()
    {
        // Repository'den jenerik Machine listesini çek.
        var machines = _machineRepository.GetAllMachines();
        return Ok(machines);
    }

    /// <summary>
    /// Tek bir makineyi döndürür (Makine konfigürasyonunu JSON dahil).
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<Machine> GetMachine(int id)
    {
        var machine = _machineRepository.GetMachineById(id);
        if (machine == null)
        {
            return NotFound();
        }
        return Ok(machine);
    }

    /// <summary>
    /// Makineye PLC'den AcknowledgeAlarm komutu gönderir.
    /// IPlcManager arayüzü sayesinde tüm PLC tipleriyle uyumludur.
    /// </summary>
    [HttpPost("{id}/acknowledgeAlarm")]
    public async Task<IActionResult> AcknowledgeAlarm(int id)
    {
        var machine = _machineRepository.GetMachineById(id);
        if (machine == null) return NotFound();

        // Fabrika kullanarak makine tipine uygun PLC Manager'ı al.
        var plcManager = _plcManagerFactory.CreatePlcManager(machine); //

        // Jenerik PLC komutunu çağır.
        var result = await plcManager.AcknowledgeAlarm(); //

        if (result.IsSuccess)
        {
            return Ok(new { Message = $"{machine.MachineName} alarmı onaylandı." });
        }
        return BadRequest(result.Message);
    }

    // PUT ve POST metotları da jenerik Machine modelini kabul etmelidir.
}