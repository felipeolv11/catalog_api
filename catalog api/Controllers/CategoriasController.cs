using AutoMapper;
using catalog_api.Context;
using catalog_api.DTOs;
using catalog_api.DTOs.Mappings;
using catalog_api.Models;
using catalog_api.Pagination;
using catalog_api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace catalog_api.Controllers;

[Route("[controller]")]
[ApiController]
public class CategoriasController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CategoriasController(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    [HttpGet("Paginação")]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get([FromQuery] CategoriasParameters categoriasParameters)
    {
        var categorias = await _uow.CategoriaRepository.GetCategoriasAsync(categoriasParameters);

        var metadata = new
        {
            categorias.Count,
            categorias.PageSize,
            categorias.PageCount,
            categorias.TotalItemCount,
            categorias.HasNextPage,
            categorias.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var categoriasDto = _mapper.Map<IEnumerable<CategoriaDTO>>(categorias);

        return Ok(categoriasDto);
    }

    [HttpGet("Filtro/Nome/Paginação")]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltradas([FromQuery] CategoriasFiltroNome categoriasFilterParameters)
    {
        var categorias = await _uow.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriasFilterParameters);

        var metadata = new
        {
            categorias.Count,
            categorias.PageSize,
            categorias.PageCount,
            categorias.TotalItemCount,
            categorias.HasNextPage,
            categorias.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var categoriasDto = _mapper.Map<IEnumerable<CategoriaDTO>>(categorias);

        return Ok(categoriasDto);
    }

    [HttpGet]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()
    {
        var categorias = await _uow.CategoriaRepository.GetAllAsync();

        if (categorias is null)
            return NotFound();

        var categoriasDto = _mapper.Map<IEnumerable<CategoriaDTO>>(categorias);

        return Ok(categoriasDto);
    }

    [HttpGet("{id:int}", Name = "ObterCategoria")]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<CategoriaDTO>> Get(int id)
    {
        var categoria = await _uow.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

        if (categoria is null)
            return NotFound();

        var categoriaDto = _mapper.Map<CategoriaDTO>(categoria);

        return Ok(categoriaDto);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
            return BadRequest();

        var categoria = _mapper.Map<Categoria>(categoriaDto);

        var novaCategoria = _uow.CategoriaRepository.Create(categoria);
        await _uow.CommitAsync();

        var novaCategoriaDto = _mapper.Map<CategoriaDTO>(novaCategoria);

        return new CreatedAtRouteResult("ObterCategoria",
            new { id = novaCategoriaDto.CategoriaId }, novaCategoriaDto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
            return BadRequest();

        var categoria = _mapper.Map<Categoria>(categoriaDto);

        var categoriaAtualizada = _uow.CategoriaRepository.Update(categoria);
        await _uow.CommitAsync();

        var categoriaAtualizadaDto = _mapper.Map<CategoriaDTO>(categoriaAtualizada);

        return Ok(categoriaAtualizadaDto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<CategoriaDTO>> Delete(int id)
    {
        var categoria = await _uow.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

        if (categoria is null)
            return NotFound();

        var categoriaExcluida = _uow.CategoriaRepository.Delete(categoria);
        await _uow.CommitAsync();

        var categoriaExcluidaDto = _mapper.Map<CategoriaDTO>(categoriaExcluida);

        return Ok(categoriaExcluidaDto);
    }
}
