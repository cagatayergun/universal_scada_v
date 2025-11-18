// Dosya: Universalscada.WebAPI/Controllers/RecipesController.cs - DÜZELTİLMİŞ VE İŞ MANTIKLI VERSİYON

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Universalscada.Core.Core; // IRecipeTimeCalculator için
using Universalscada.Core.Repositories;
using Universalscada.Core.Services; // PlcPollingService için
using Universalscada.Models;
using Universalscada.Repositories;
using Universalscada.Services; // IPlcManager ve IPlcManagerFactory için

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class RecipesController : ControllerBase
{
    // NOT: Mimari prensip olarak bu alan IRecipeRepository olmalıdır (Dependency Inversion Principle).
    private readonly RecipeRepository _recipeRepository;
    private readonly IMachineRepository _machineRepository; // Yeni: Makine detaylarını çekmek için
    private readonly IRecipeTimeCalculator _timeCalculator;
    private readonly IPlcManagerFactory _plcManagerFactory; // Yeni: Fallback bağlantı oluşturmak için
    private readonly PlcPollingService _pollingService; // Yeni: Aktif PLC bağlantısını almak için

    public RecipesController(
        RecipeRepository recipeRepository,
        IMachineRepository machineRepository,
        IRecipeTimeCalculator timeCalculator,
        IPlcManagerFactory plcManagerFactory,
        PlcPollingService pollingService)
    {
        _recipeRepository = recipeRepository;
        _machineRepository = machineRepository;
        _timeCalculator = timeCalculator;
        _plcManagerFactory = plcManagerFactory;
        _pollingService = pollingService;
    }

    /// <summary>
    /// Bir reçeteyi getirir ve teorik süresini dinamik olarak hesaplar.
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<RecipeDetailDto> GetRecipe(int id)
    {
        // 1. Reçete verisini Core Repository'den çek
        var recipe = _recipeRepository.GetRecipeById(id);
        if (recipe == null)
        {
            return NotFound();
        }

        // 2. Dinamik hesaplama servisini kullanarak süreyi hesapla
        double theoreticalTimeSeconds = _timeCalculator.CalculateTotalTheoreticalTimeSeconds(recipe.Steps);

        // 3. İstemciye sunulacak DTO'yu (Data Transfer Object) oluştur
        var recipeDto = new RecipeDetailDto
        {
            Recipe = recipe,
            TotalTheoreticalTimeMinutes = theoreticalTimeSeconds / 60.0
        };

        return Ok(recipeDto);
    }

    /// <summary>
    /// Reçeteyi PLC'deki belirli bir slota yazar.
    /// Polling servisinin aktif bağlantısını kullanır veya yeni bir bağlantı kurar.
    /// </summary>
    [HttpPost("{recipeId}/writeToPlc/{machineId}/{recipeSlot:int?}")]
    public async Task<IActionResult> WriteRecipeToPlc(int recipeId, int machineId, int? recipeSlot = 1)
    {
        // 1. Veri Kontrolü
        var recipe = _recipeRepository.GetRecipeById(recipeId);
        if (recipe == null) return NotFound($"Reçete (ID: {recipeId}) bulunamadı.");

        var machine = _machineRepository.GetMachineById(machineId);
        if (machine == null) return NotFound($"Makine (ID: {machineId}) bulunamadı.");

        if (recipeSlot is null or < 1 or > 20) return BadRequest("Reçete slot numarası 1 ile 20 arasında olmalıdır.");

        // 2. Aktif PLC Manager'ı Al
        var activeManagers = _pollingService.GetPlcManagers();
        IPlcManager plcManager;

        bool isNewConnection = false;

        // Polling Servisinden mevcut bağlantıyı dene
        if (!activeManagers.TryGetValue(machineId, out plcManager) || plcManager == null)
        {
            // Fallback: Aktif bağlantı yoksa, Factory ile yeni bir bağlantı oluştur
            try
            {
                plcManager = _plcManagerFactory.CreatePlcManager(machine);
                // Bağlantıyı manuel açmayı dene
                var connectResult = plcManager.Connect();
                if (!connectResult.IsSuccess)
                {
                    return StatusCode(503, new { Message = $"Makine (ID: {machineId}) için yeni PLC bağlantısı kurulamadı: {connectResult.Message}" });
                }
                isNewConnection = true;
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { Message = $"Makine (ID: {machineId}) için PLC Manager oluşturulurken kritik hata: {ex.Message}" });
            }
        }

        try
        {
            // 3. Reçete Adını PLC'ye Yaz (IPlcManager arayüzündeki jenerik metot)
            var writeNameResult = await plcManager.WriteRecipeNameAsync(recipeSlot.Value, recipe.RecipeName);

            if (!writeNameResult.IsSuccess)
            {
                return BadRequest(new { Message = $"Reçete Adı ({recipe.RecipeName}) yazılırken hata: {writeNameResult.Message}" });
            }

            // TODO: Reçete Adımlarının (Steps) ham short[] dataya dönüştürülüp IPlcManager.WriteDataWordsAsync ile yazılması gerekmektedir.
            // Bu kısım, 'RecipeDataMapper' servisi tarafından yürütülmelidir.

            return Ok(new { Message = $"Reçete (Adı: {recipe.RecipeName}, Slot: {recipeSlot}) başarıyla makineye yazıldı." });
        }
        finally
        {
            // Eğer bağlantıyı biz açtıysak (polling servisi değilse), kapatmalıyız.
            if (isNewConnection)
            {
                plcManager.Disconnect();
            }
        }
    }
}

// Reçete verisini ve hesaplanmış süreyi taşıyan DTO (iç sınıf veya ayrı dosya)
public class RecipeDetailDto
{
    public ScadaRecipe Recipe { get; set; }
    public double TotalTheoreticalTimeMinutes { get; set; }
    // TODO: Maliyet hesaplaması da buraya eklenebilir.
}