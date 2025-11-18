// Dosya: Universalscada.WebAPI/Controllers/MachinesController.cs - DÜZELTİLMİŞ VERSİYON

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Universalscada.Core.Repositories;
using Universalscada.Core.Services; // PlcPollingService için gerekli
using Universalscada.Models;
using Universalscada.Services; // IPlcManager için gerekli

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MachinesController : ControllerBase
{
    private readonly IMachineRepository _machineRepository;
    // DÜZELTME: Polling servisinden aktif PLC Manager listesine erişmek için eklendi.
    private readonly PlcPollingService _pollingService;

    // IPlcManagerFactory'yi constructor'dan kaldırıyoruz, çünkü artık aktif manager'ları 
    // doğrudan PollingService tutuyor. Yeni bir manager oluşturmaya ihtiyaç kalmadı.
    // private readonly IPlcManagerFactory _plcManagerFactory; 

    public MachinesController(IMachineRepository machineRepository, PlcPollingService pollingService)
    {
        _machineRepository = machineRepository;
        _pollingService = pollingService;
        // _plcManagerFactory = plcManagerFactory; // KALDIRILDI
    }

    /// <summary>
    /// Evrensel Machine modellerini döndürür.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<Machine>> GetAllMachines()
    {
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
    /// Polling servisinin yönettiği aktif bağlantıyı kullanır.
    /// </summary>
    [HttpPost("{id}/acknowledgeAlarm")]
    public async Task<IActionResult> AcknowledgeAlarm(int id)
    {
        var machine = _machineRepository.GetMachineById(id);
        if (machine == null) return NotFound("Makine bulunamadı.");

        // 1. Polling servisinden aktif PLC Manager'ı al.
        var activeManagers = _pollingService.GetPlcManagers();
        IPlcManager plcManager;

        if (!activeManagers.TryGetValue(id, out plcManager) || plcManager == null)
        {
            // Bağlantı listesinde yoksa (aktif olarak sorgulanmıyorsa)
            return StatusCode(503, new { Message = $"{machine.MachineName} şu anda aktif olarak bağlı değil veya Polling servisi başlamamış." });
        }

        // 2. Jenerik PLC komutunu çağır.
        var result = await plcManager.AcknowledgeAlarm();

        if (result.IsSuccess)
        {
            return Ok(new { Message = $"{machine.MachineName} alarmı aktif bağlantı üzerinden onaylandı." });
        }

        // 3. PLC komutu hata verdi (Örn: PLC'ye erişim kesildi, komut yazılamadı)
        return BadRequest(new { Message = $"Alarm onaylama işlemi başarısız: {result.Message}" });
    }

    // PUT ve POST metotları da jenerik Machine modelini kabul etmelidir.
}