using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruitment.Data;
using Recruitment.DTOs;
using Recruitment.Models;

namespace Recruitment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inyección de dependencias
        public DocumentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/candidate/{id}/documents
        [HttpGet("~/api/candidate/{id:int}/documents")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DocumentsDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentsByCandidate(int id)
        {
            // Validación básica
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "El ID del candidato debe ser mayor a cero."
                });
            }

            // Consulta asíncrona
            var documents = await _context.Documents
                .Where(d => d.candidate_id == id)
                .ToListAsync();

            // Validar existencia
            if (documents == null || !documents.Any())
            {
                return NotFound(new
                {
                    message = $"No se encontraron documentos para el candidato {id}."
                });
            }

            // Mapear DTO
            var documentsDto = documents.Select(d => new DocumentsDto
            {
                id = d.id,
                candidate_id = d.candidate_id,
                name = d.name,
                file_path = d.file_path,
                type = d.type,
                created_at = d.created_at,
                updated_at = d.updated_at
            }).ToList();

            // Respuesta OK
            return Ok(documentsDto);
        }

        // POST: api/candidate/{id}/documents
        [HttpPost("~/api/candidate/{id:int}/documents")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DocumentsDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateDocument(
            int id,
            [FromBody] DocumentsDto createDto)
        {
            // Validación ID
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "El ID del candidato debe ser mayor a cero."
                });
            }

            // Validación DTO
            if (createDto == null)
            {
                return BadRequest(new
                {
                    message = "Los datos del documento no pueden ser nulos."
                });
            }

            // Validar candidate_id
            if (id != createDto.candidate_id)
            {
                return BadRequest(new
                {
                    message = "El ID de la ruta no coincide con el candidate_id del body."
                });
            }

            // Validar candidato existente
            var candidateExists = await _context.Candidate
                .AnyAsync(c => c.candidate_id == id);

            if (!candidateExists)
            {
                return NotFound(new
                {
                    message = $"No existe el candidato con ID {id}."
                });
            }

            // Validar fechas SQL DATETIME
            var sqlMinDate = new DateTime(1753, 1, 1);

            if (createDto.created_at < sqlMinDate)
            {
                return BadRequest(new
                {
                    message = "created_at tiene una fecha inválida."
                });
            }

            if (createDto.updated_at < sqlMinDate)
            {
                return BadRequest(new
                {
                    message = "updated_at tiene una fecha inválida."
                });
            }

            // Mapear DTO -> Entidad
            var newDocument = new Documents
            {
                candidate_id = createDto.candidate_id,
                name = createDto.name,
                file_path = createDto.file_path,
                type = createDto.type,
                created_at = createDto.created_at,
                updated_at = createDto.updated_at
            };

            // Guardar
            _context.Documents.Add(newDocument);

            await _context.SaveChangesAsync();

            // DTO respuesta
            var responseDto = new DocumentsDto
            {
                id = newDocument.id,
                candidate_id = newDocument.candidate_id,
                name = newDocument.name,
                file_path = newDocument.file_path,
                type = newDocument.type,
                created_at = newDocument.created_at,
                updated_at = newDocument.updated_at
            };

            // Respuesta REST
            return CreatedAtAction(
                nameof(GetDocumentsByCandidate),
                new { id = responseDto.candidate_id },
                responseDto);
        }
        // POST: api/candidate/{id}/documents
        [HttpPost("~/api/candidate/{id:int}/documents/file")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DocumentsDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateDocument(int id, [FromForm] UploadDocumentDto request)
        {
            // 1. Validaciones de nulos y IDs
            if (id <= 0) return BadRequest(new { message = "El ID debe ser mayor a cero." });
            if (request == null || request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "Debe adjuntar un archivo válido junto con los datos." });
            }
            if (id != request.candidate_id)
            {
                return BadRequest(new { message = "El ID de la ruta no coincide con el candidate_id." });
            }

            // 2. Validar candidato existente
            var candidateExists = await _context.Candidate.AnyAsync(c => c.candidate_id == id);
            if (!candidateExists) return NotFound(new { message = $"No existe el candidato {id}." });

            // 3. Procesar archivo físico usando 'request.File'
            var trustedFileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.File.FileName)}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var fullPath = Path.Combine(uploadsFolder, trustedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            // 4. Mapear a la entidad de la Base de Datos
            var newDocument = new Documents
            {
                candidate_id = request.candidate_id,
                name = string.IsNullOrEmpty(request.name) ? request.File.FileName : request.name,
                file_path = $"/uploads/{trustedFileName}",
                type = string.IsNullOrEmpty(request.type) ? Path.GetExtension(request.File.FileName) : request.type,
                created_at = request.created_at == DateTime.MinValue ? DateTime.UtcNow : request.created_at,
                updated_at = request.updated_at == DateTime.MinValue ? DateTime.UtcNow : request.updated_at
            };

            _context.Documents.Add(newDocument);
            await _context.SaveChangesAsync();

            // 5. Mapear al DTO de salida (el original que tenías)
            var responseDto = new DocumentsDto
            {
                id = newDocument.id,
                candidate_id = newDocument.candidate_id,
                name = newDocument.name,
                file_path = newDocument.file_path,
                type = newDocument.type,
                created_at = newDocument.created_at,
                updated_at = newDocument.updated_at
            };

            return CreatedAtAction("GetDocumentsByCandidate", new { id = responseDto.candidate_id }, responseDto);
        }

    }
}