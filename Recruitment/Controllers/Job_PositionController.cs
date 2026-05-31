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
    public class Job_PositionController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inyección de dependencias de la base de datoss
        public Job_PositionController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/v1/job_position/{position_id}
        [HttpGet("{position_id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Job_PositionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPositionById(int position_id)
        {
            //Validación del parámetro
            if (position_id <= 0)
            {
                return BadRequest(new { message = "El ID debe ser mayor a cero." });
            }

            //Consulta asíncrona a tu tabla 'Company' (singular, tal como está en tu AppDbContext)
            var position = await _context.Job_Position.FindAsync(position_id);

            //Validación de existencia
            if (position == null)
            {
                return NotFound(new { message = $"No se encontró ninguna posicion con el ID {position_id}." });
            }

            //Mapeo del Modelo al DTO de salida
            var positionDto = new Job_PositionDto
            {
                position_id = position.position_id,
                company_id = position.company_id,
                internal_id = position.internal_id,
                title = position.title,
                department = position.department,
                location = position.location,
                is_remote = position.is_remote,
                min_salary = position.min_salary,
                max_salary = position.max_salary,
                status = position.status,
                pipeline_id = position.pipeline_id,
                questionnaire_id = position.questionnaire_id,
                scorecard_template_id = position.scorecard_template_id, 
                created_at = position.created_at

            };

            //Retorno exitoso
            return Ok(positionDto);
        }

        // PUT: api/job_position/{position_id}
        [HttpPut("{position_id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePosition(int position_id, [FromBody] Job_PositionDto updateDto)
        {
            // 1. Validación básica del parámetro de la ruta
            if (position_id <= 0)
            {
                return BadRequest(new { message = "El ID de la ruta debe ser mayor a cero." });
            }

            // 2. Validación de seguridad: El ID de la URL debe coincidir con el ID dentro del cuerpo (DTO)
            if (position_id != updateDto.position_id)
            {
                return BadRequest(new { message = "El ID de la URL no coincide con el ID del cuerpo de la solicitud." });
            }

            // 3. Buscar la entidad existente en la base de datos
            var position = await _context.Job_Position.FindAsync(position_id);

            if (position == null)
            {
                return NotFound(new { message = $"No se encontró ninguna posición con el ID {position_id} para actualizar." });
            }

            // 4. Pasar los datos del DTO al Modelo (Manteniendo las llaves foráneas intactas)
            position.company_id = updateDto.company_id;
            position.internal_id = updateDto.internal_id;
            position.title = updateDto.title;
            position.department = updateDto.department;
            position.location = updateDto.location;
            position.is_remote = updateDto.is_remote;
            position.min_salary = updateDto.min_salary;
            position.max_salary = updateDto.max_salary;
            position.status = updateDto.status;
            position.pipeline_id = updateDto.pipeline_id;
            position.questionnaire_id = updateDto.questionnaire_id;
            position.scorecard_template_id = updateDto.scorecard_template_id;

            // Nota: Usualmente 'created_at' o 'job_position_id' NO se modifican en un PUT. 
            // EF Core ya sabe que es la misma entidad porque la rastrea por su ID.

            // 5. Guardar los cambios de manera asíncrona en la base de datos
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Manejo por si dos usuarios intentan actualizar exactamente al mismo tiempo
                if (!PositionExists(position_id))
                {
                    return NotFound(new { message = "La posición ya no existe en el sistema." });
                }
                else
                {
                    throw; // Si es otro error de concurrencia, lo relanzamos
                }
            }

            // 6. El estándar REST para un PUT exitoso es retornar 204 NoContent (Sin contenido)
            return NoContent();
        }

        // Método de soporte para validar concurrencia
        private bool PositionExists(int id)
        {
            return _context.Job_Position.Any(e => e.position_id == id);
        }

        // PUT: api/job_position/{position_id}/state
        [HttpPut("{position_id:int}/state")] // Mapea exactamente la ruta de tu imagen
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePositionState(int position_id, [FromBody] string newState)
        {
            // 1. Validación básica del parámetro de la ruta
            if (position_id <= 0)
            {
                return BadRequest(new { message = "El ID de la ruta debe ser mayor a cero." });
            }

            // Validación opcional: Asegurar que no manden un estado vacío
            if (string.IsNullOrWhiteSpace(newState))
            {
                return BadRequest(new { message = "El nuevo estado no puede estar vacío." });
            }

            // 2. Buscar únicamente la entidad existente en la base de datos
            var position = await _context.Job_Position.FindAsync(position_id);

            if (position == null)
            {
                return NotFound(new { message = $"No se encontró ninguna posición con el ID {position_id}." });
            }

            // 3. Modificar ÚNICAMENTE la propiedad de estado
            position.status = newState;

            // 4. Guardar los cambios de manera asíncrona
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PositionExists(position_id))
                {
                    return NotFound(new { message = "La posición ya no existe en el sistema." });
                }
                else
                {
                    throw;
                }
            }

            // 5. Retornar 204 No Content para confirmar el cambio exitoso
            return NoContent();
        }
        // GET: api/job_position
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Job_PositionDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPositions()
        {
            // 1. Obtener todos los registros de la base de datos de manera asíncrona
            var positions = await _context.Job_Position.ToListAsync();

            // 2. Proyectar/Mapear la lista de modelos hacia la lista de DTOs
            var positionsDto = positions.Select(position => new Job_PositionDto
            {
                position_id = position.position_id,
                company_id = position.company_id,
                internal_id = position.internal_id,
                title = position.title,
                department = position.department,
                location = position.location,
                is_remote = position.is_remote,
                min_salary = position.min_salary,
                max_salary = position.max_salary,
                status = position.status,
                pipeline_id = position.pipeline_id,
                questionnaire_id = position.questionnaire_id,
                scorecard_template_id = position.scorecard_template_id,
                created_at = position.created_at
            }).ToList();

            // 3. Retornar un estatus 200 OK con la lista formateada
            return Ok(positionsDto);
        }
        // POST: api/job_position
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Job_PositionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePosition([FromBody] Job_PositionDto createDto)
        {
            // 1. Validación básica del modelo recibido
            if (createDto == null)
            {
                return BadRequest(new { message = "Los datos de la posición no pueden ser nulos." });
            }

            // 2. Mapear del DTO hacia la Entidad de Base de Datos
            var newPosition = new Job_Position
            {
                // Nota: NO asignamos position_id porque la base de datos lo genera automáticamente (Identity)
                company_id = createDto.company_id,
                internal_id = createDto.internal_id,
                title = createDto.title,
                department = createDto.department,
                location = createDto.location,
                is_remote = createDto.is_remote,
                min_salary = createDto.min_salary,
                max_salary = createDto.max_salary,
                status = createDto.status ?? "Draft", // Estado por defecto si viene nulo
                pipeline_id = createDto.pipeline_id,
                questionnaire_id = createDto.questionnaire_id,
                scorecard_template_id = createDto.scorecard_template_id,
                created_at = DateTime.UtcNow // Asignamos la fecha de creación actual del servidor
            };

            // 3. Añadir la entidad al contexto y guardar en la base de datos
            _context.Job_Position.Add(newPosition);
            await _context.SaveChangesAsync(); // Aquí la base de datos genera el nuevo ID y se lo asigna a newPosition

            // 4. Mapear de vuelta la entidad guardada (ya con su ID real) al DTO de respuesta
            var responseDto = new Job_PositionDto
            {
                position_id = newPosition.position_id, // Capturamos el ID auto-generado
                company_id = newPosition.company_id,
                internal_id = newPosition.internal_id,
                title = newPosition.title,
                department = newPosition.department,
                location = newPosition.location,
                is_remote = newPosition.is_remote,
                min_salary = newPosition.min_salary,
                max_salary = newPosition.max_salary,
                status = newPosition.status,
                pipeline_id = newPosition.pipeline_id,
                questionnaire_id = newPosition.questionnaire_id,
                scorecard_template_id = newPosition.scorecard_template_id,
                created_at = newPosition.created_at
            };

            // 5. El estándar REST: Retorna 201 Created y la cabecera 'Location' apuntando al GET de este nuevo ID
            return CreatedAtAction(nameof(GetPositionById), new { position_id = responseDto.position_id }, responseDto);
        }
    }
}
