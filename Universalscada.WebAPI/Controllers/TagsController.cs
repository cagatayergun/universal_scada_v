using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Universalscada.Core.Models;
using Universalscada.Core.Repositories;

[Route("api/[controller]")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly IMetaDataRepository _metaDataRepository;

    public TagsController(IMetaDataRepository metaDataRepository)
    {
        _metaDataRepository = metaDataRepository;
    }

    // GET: api/Tags/machine/1
    [HttpGet("machine/{machineId}")]
    public async Task<ActionResult<IEnumerable<PlcTagDefinition>>> GetPlcTags(int machineId)
    {
        var tags = await _metaDataRepository.GetAllPlcTagsAsync(machineId);
        return Ok(tags);
    }

    // GET: api/Tags/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PlcTagDefinition>> GetPlcTag(int id)
    {
        var tag = await _metaDataRepository.GetPlcTagByIdAsync(id);

        if (tag == null)
        {
            return NotFound();
        }

        return tag;
    }

    // POST: api/Tags
    [HttpPost]
    public async Task<ActionResult<PlcTagDefinition>> PostPlcTag(PlcTagDefinition tag)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var newTag = await _metaDataRepository.AddPlcTagAsync(tag);
        return CreatedAtAction(nameof(GetPlcTag), new { id = newTag.Id }, newTag);
    }

    // PUT: api/Tags/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlcTag(int id, PlcTagDefinition tag)
    {
        if (id != tag.Id)
        {
            return BadRequest();
        }

        try
        {
            await _metaDataRepository.UpdatePlcTagAsync(tag);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (await _metaDataRepository.GetPlcTagByIdAsync(id) == null)
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Tags/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlcTag(int id)
    {
        await _metaDataRepository.DeletePlcTagAsync(id);
        return NoContent();
    }
}