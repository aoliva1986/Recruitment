using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruitment.Data;
using Recruitment.DTOs;
using Recruitment.Models;

namespace Recruitment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : Controller
    {

        private readonly AppDbContext _context;

        //Inyección de dependencias de la base de datos 
        public CandidateController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/v1/candidate/{candidate_id}
        [HttpGet("{candidate_id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CandidateDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCandidateById(int candidate_id)
        {
            //Validación del parámetro
            if (candidate_id <= 0)
            {
                return BadRequest(new { message = "El ID debe ser mayor a cero." });
            }

            //Consulta asíncrona a tu tabla 'Candidate' (singular, tal como está en tu AppDbContext)
            var candidate = await _context.Candidate.FindAsync(candidate_id);

            //Validación de existencia
            if (candidate == null)
            {
                return NotFound(new { message = $"No se encontró ningun candidato con el ID {candidate_id}." });
            }

            //Mapeo del Modelo al DTO de salida
            var candidateDto = new CandidateDto
            {
                candidate_id = candidate.candidate_id,
                company_id = candidate.company_id,
                position_id = candidate.position_id,
                current_stage_id = candidate.current_stage_id,
                full_name = candidate.full_name,
                email = candidate.email,
                phone = candidate.phone,
                cv_url = candidate.cv_url,
                origin = candidate.origin,
                status = candidate.status,
                created_at = candidate.created_at
            };

            //Retorno exitoso
            return Ok(candidateDto);
        }
        private bool CandidateExists(int id)
        {
            return _context.Candidate.Any(e => e.candidate_id == id);
        }
        // PUT: api/candidate/{candidate_id}
        [HttpPut("{candidate_id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCandidate(int candidate_id, [FromBody] CandidateDto updateDto)
        {
            // 1. Validación básica del parámetro de la ruta
            if (candidate_id <= 0)
            {
                return BadRequest(new { message = "El ID de la ruta debe ser mayor a cero." });
            }

            // 2. Validación de seguridad: El ID de la URL debe coincidir con el ID dentro del cuerpo (DTO)
            if (candidate_id != updateDto.candidate_id)
            {
                return BadRequest(new { message = "El ID de la URL no coincide con el ID del cuerpo de la solicitud." });
            }

            // 3. Buscar la entidad existente en la base de datos
            var candidate = await _context.Candidate.FindAsync(candidate_id);

            if (candidate == null)
            {
                return NotFound(new { message = $"No se encontró ninguna posición con el ID {candidate_id} para actualizar." });
            }

            // 4. Pasar los datos del DTO al Modelo (Manteniendo las llaves foráneas intactas)

            candidate.full_name = updateDto.full_name;
            candidate.email = updateDto.email;
            candidate.phone = updateDto.phone;
            candidate.cv_url = updateDto.cv_url;
            candidate.origin = updateDto.origin;
            candidate.status = updateDto.status;
            
            
            // EF Core ya sabe que es la misma entidad porque la rastrea por su ID.

            //Guardar los cambios de manera asíncrona en la base de datos
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Manejo por si dos usuarios intentan actualizar exactamente al mismo tiempo
                if (!CandidateExists(candidate_id))
                {
                    return NotFound(new { message = "El candidato ya no existe en el sistema." });
                }
                else
                {
                    throw; // Si es otro error de concurrencia, lo relanzamos
                }
            }

            // 6. El estándar REST para un PUT exitoso es retornar 204 NoContent (Sin contenido)
            return NoContent();
        }
        // GET: api/v1/candidates
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CandidateDto>))]
        public async Task<IActionResult> GetAllCandidates()
        {
            //Consultar de forma asíncrona todas las empresas en la base de datos
            var candidates = await _context.Candidate.ToListAsync();

            //Mapear la lista de modelos a una lista de DTOs limpios usando LINQ
            var candidateDto = candidates.Select(c => new CandidateDto
            {
                candidate_id = c.candidate_id,
                full_name = c.full_name,
                email = c.email,
                phone = c.phone,
                cv_url = c.cv_url,
                origin = c.origin,
                status = c.status,
                created_at = c.created_at

            }).ToList();

            //Retornar la lista con un estado HTTP 200 OK
            // Nota: Si no hay candidatos, devolverá una lista vacía [ ], lo cual es correcto en REST
            return Ok(candidateDto);
        }
        // POST: api/candidate
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CandidateDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCandidate([FromBody] CandidateDto createDto)
        {
            // 1. Validación básica del modelo recibido
            if (createDto == null)
            {
                return BadRequest(new { message = "Los datos del candidato no pueden ser nulos." });
            }

            // 2. Mapear del DTO hacia la Entidad de Base de Datos
            var newCandidate = new Candidate
            {
                // Nota: NO asignamos position_id porque la base de datos lo genera automáticamente (Identity)
                company_id = createDto.company_id,
                position_id = createDto.position_id,
                current_stage_id = createDto.current_stage_id,
                full_name = createDto.full_name,
                email = createDto.email,
                phone = createDto.phone,
                cv_url = createDto.cv_url,
                origin = createDto.origin,
                status = createDto.status,
                created_at = DateTime.UtcNow // Asignamos la fecha de creación al momento de guardar

            };

            //Añadir la entidad al contexto y guardar en la base de datos
            _context.Candidate.Add(newCandidate);
            await _context.SaveChangesAsync(); // Aquí la base de datos genera el nuevo ID y se lo asigna a newPosition

            //Mapear de vuelta la entidad guardada (ya con su ID real) al DTO de respuesta
            var responseDto = new CandidateDto
            {

                candidate_id = newCandidate.candidate_id, // Capturamos el ID auto-generado
                company_id = newCandidate.company_id,
                position_id = newCandidate.position_id,
                current_stage_id = newCandidate.current_stage_id,
                full_name = newCandidate.full_name,
                email = newCandidate.email,
                phone = newCandidate.phone,
                cv_url = newCandidate.cv_url,
                origin = newCandidate.origin,
                status = newCandidate.status,
                created_at = newCandidate.created_at
            };         

            //El estándar REST: Retorna 201 Created y la cabecera 'Location' apuntando al GET de este nuevo ID
            return CreatedAtAction(nameof(GetCandidateById), new { position_id = responseDto.candidate_id }, responseDto);
        }
        // GET: api/v1/candidate/{candidate_id}/position/{position_id}/company/{company_id}
        [HttpGet("{candidate_id:int}/position/{position_id:int}/company/{company_id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CandidateDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCandidateByIdPositionAndCompany(int candidate_id, int position_id, int company_id)
        {
            // 1. Validación de los parámetros de entrada
            if (candidate_id <= 0 || position_id <= 0 || company_id <= 0)
            {
                return BadRequest(new { message = "Todos los IDs (candidato, posición y empresa) deben ser mayores a cero." });
            }

            // 2. Consulta asíncrona filtrando por los tres campos clave
            var candidate = await _context.Candidate
                .FirstOrDefaultAsync(c => c.candidate_id == candidate_id &&
                                          c.position_id == position_id &&
                                          c.company_id == company_id);

            // 3. Validación de existencia
            if (candidate == null)
            {
                return NotFound(new { message = $"No se encontró ningún candidato con ID {candidate_id} para la posición {position_id} en la empresa {company_id}." });
            }

            // 4. Mapeo del Modelo al DTO de salida
            var candidateDto = new CandidateDto
            {
                candidate_id = candidate.candidate_id,
                company_id = candidate.company_id,
                position_id = candidate.position_id,
                current_stage_id = candidate.current_stage_id,
                full_name = candidate.full_name,
                email = candidate.email,
                phone = candidate.phone,
                cv_url = candidate.cv_url,
                origin = candidate.origin,
                status = candidate.status,
                created_at = candidate.created_at
            };

            // 5. Retorno del objeto único
            return Ok(candidateDto);
        }
    }
}
