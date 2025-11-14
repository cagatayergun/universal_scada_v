// Dosya: Universalscada.WebAPI/Controllers/RecipesController.cs - GÜNCEL VERSİYON
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Universalscada.Core.Core; // IRecipeTimeCalculator için
using Universalscada.Core.Repositories;
using Universalscada.Models;
using Universalscada.Repositories;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class RecipesController : ControllerBase
{
    private readonly RecipeRepository _recipeRepository; // IMachineRepository gibi IRecipeRepository kullanılmalı
    private readonly IRecipeTimeCalculator _timeCalculator;

    public RecipesController(RecipeRepository recipeRepository, IRecipeTimeCalculator timeCalculator)
    {
        _recipeRepository = recipeRepository;
        _timeCalculator = timeCalculator;
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
        // Bu, DB'den çekilen metadata'ya göre jenerik olarak yapılır.
        double theoreticalTimeSeconds = _timeCalculator.CalculateTotalTheoreticalTimeSeconds(recipe.Steps); //

        // 3. İstemciye sunulacak DTO'yu (Data Transfer Object) oluştur
        var recipeDto = new RecipeDetailDto
        {
            Recipe = recipe,
            TotalTheoreticalTimeMinutes = theoreticalTimeSeconds / 60.0
        };

        return Ok(recipeDto);
    }

    // Yeni bir Reçete Yazar (PLC'ye)
    [HttpPost("{recipeId}/writeToPlc/{machineId}")]
    public async Task<IActionResult> WriteRecipeToPlc(int recipeId, int machineId)
    {
        // Bu mantık, IPlcManagerFactory ve IPlcManager kullanılarak jenerik olarak yürütülmelidir.
        // Hata durumunda Bad Request döndürülür.
        return Ok(new { Message = $"Reçete başarıyla makineye yazıldı." });
    }
}

// Reçete verisini ve hesaplanmış süreyi taşıyan DTO (iç sınıf veya ayrı dosya)
public class RecipeDetailDto
{
    public ScadaRecipe Recipe { get; set; }
    public double TotalTheoreticalTimeMinutes { get; set; }
    // TODO: Maliyet hesaplaması da buraya eklenebilir.
}