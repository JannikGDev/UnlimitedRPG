using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Core.Model;
using UnlimitedRPG.Database;

namespace UnlimitedRPG.Api.Controllers;

[ApiController]
[Route("api/characters")]
public class CharactersController(IDbContextFactory<RPGContext> dbFactory) : ControllerBase
{
    /// <summary>Returns all characters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var characters = await db.PlayerCharacters
            .Select(c => new CharacterDto(c.Id, c.Name, c.Description))
            .ToListAsync();
        return Ok(characters);
    }

    /// <summary>Returns a single character by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var character = await db.PlayerCharacters.FindAsync(id);
        if (character is null) return NotFound();
        return Ok(new CharacterDto(character.Id, character.Name, character.Description));
    }

    /// <summary>Creates a new character.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCharacterRequest request)
    {
        var character = new PlayerCharacter
        {
            Name        = request.Name,
            Description = request.Description
        };

        await using var db = await dbFactory.CreateDbContextAsync();
        db.PlayerCharacters.Add(character);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new CharacterDto(character.Id, character.Name, character.Description));
    }
}

public record CharacterDto(Guid Id, string Name, string Description);
public record CreateCharacterRequest(string Name, string Description);
