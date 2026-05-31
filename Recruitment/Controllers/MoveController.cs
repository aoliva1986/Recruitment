using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Recruitment.Data;
using Recruitment.DTOs;
using Recruitment.Models;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Recruitment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoveController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Inyección de dependencias
        public MoveController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Move/{id}
        // Este endpoint es necesario para que CreatedAtAction pueda generar la URL del recurso creado
        [HttpGet("{id:int}", Name = "GetMoveById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MoveDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMoveById(int id)
        {
            var move = await _context.Move.FindAsync(id);
            if (move == null)
            {
                return NotFound(new { message = $"No se encontró el movimiento con ID {id}." });
            }

            var dto = new MoveDto
            {
                move_id = move.move_id,
                candidate_id = move.candidate_id,
                stage_id = move.stage_id,
                comments = move.comments,
                created_at = move.created_at
            };

            return Ok(dto);
        }

        // POST: api/candidate/{id}/move
        [HttpPost("/api/candidate/{id:int}/move")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MoveDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateMove(int id, [FromBody] MoveDto createDto)
        {
            // Validar ID ruta
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "El ID del candidato debe ser mayor a cero."
                });
            }

            // Validar DTO
            if (createDto == null)
            {
                return BadRequest(new
                {
                    message = "Los datos del movimiento no pueden ser nulos."
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

            // Validar existencia candidato
            var candidateExists = await _context.Candidate.AnyAsync(c => c.candidate_id == id);

            if (!candidateExists)
            {
                return NotFound(new
                {
                    message = $"No existe el candidato con ID {id}."
                });
            }

            // VALIDACIÓN CORREGIDA: Se compara s.stage_id contra createDto.stage_id
            var stageExists = await _context.Pipeline_Stage.AnyAsync(s => s.stage_id == createDto.stage_id);

            if (!stageExists)
            {
                return NotFound(new
                {
                    message = $"No existe el stage con ID {createDto.stage_id}."
                });
            }

            // Validar fecha SQL
            var sqlMinDate = new DateTime(1753, 1, 1);

            if (createDto.created_at < sqlMinDate)
            {
                return BadRequest(new
                {
                    message = "created_at tiene una fecha inválida."
                });
            }

            // Mapear DTO -> Entidad
            var newMove = new Move
            {
                candidate_id = createDto.candidate_id,
                stage_id = createDto.stage_id,
                comments = createDto.comments,
                created_at = createDto.created_at
            };

            // Guardar
            _context.Move.Add(newMove);
            await _context.SaveChangesAsync();

            // DTO respuesta
            var responseDto = new MoveDto
            {
                move_id = newMove.move_id,
                candidate_id = newMove.candidate_id,
                stage_id = newMove.stage_id,
                comments = newMove.comments,
                created_at = newMove.created_at
            };

            // Respuesta REST Corregida: Apunta al nuevo endpoint usando el move_id creado
            return CreatedAtRoute(
                "GetMoveById",
                new { id = responseDto.move_id },
                responseDto);
        }
        // ========================================================
        // 3. MÉTODO PRIVADO MODIFICADO PARA USAR LA CONFIGURACIÓN
        // ========================================================
        private async Task<string?> SendEmailAsync(string to, string subject, string body)
        {
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587;

            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(senderEmail!, "Talent");
                    message.To.Add(new MailAddress(to));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = false;

                    using (var client = new SmtpClient(smtpHost, smtpPort))
                    {
                        client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                        client.EnableSsl = true;

                        await client.SendMailAsync(message);
                    }
                }
                return null; // Si todo sale bien, no hay error
            }
            catch (Exception ex)
            {
                // Retornamos el mensaje detallado de la excepción interna si existe
                return ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            }
        }
        // PUT: api/Move/candidate/{candidateId}/stage/{stageId}
        [HttpPut("candidate/{candidateId:int}/stage/{stageId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CandidateDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCandidateStage(int candidateId, int stageId, [FromBody] string? comments = null)
        {
            // 1. Validaciones básicas de los parámetros de la URL
            if (candidateId <= 0)
            {
                return BadRequest(new { message = "El ID del candidato debe ser mayor a cero." });
            }

            if (stageId <= 0)
            {
                return BadRequest(new { message = "El ID de la etapa (stage) debe ser mayor a cero." });
            }

            // 2. Verificar existencia del Stage destino
            var stageExists = await _context.Pipeline_Stage.AnyAsync(s => s.stage_id == stageId);
            if (!stageExists)
            {
                return NotFound(new { message = $"No existe la etapa (stage) con ID {stageId}." });
            }

            // 3. Buscar el registro original del Candidato
            var candidate = await _context.Candidate.FirstOrDefaultAsync(c => c.candidate_id == candidateId);
            if (candidate == null)
            {
                return NotFound(new { message = $"No existe el candidato con ID {candidateId}." });
            }

            int previousStageId = candidate.current_stage_id;

            // Actualizar estado principal de la entidad
            candidate.current_stage_id = stageId;

            // Registrar fila en el histórico de movimientos
            var historyMove = new Move
            {
                candidate_id = candidate.candidate_id,
                stage_id = stageId,
                // Si el body viene vacío, genera un comentario automático
                comments = !string.IsNullOrWhiteSpace(comments) ? comments : $"Cambio de estado automático desde etapa {previousStageId} a la etapa {stageId}.",
                created_at = DateTime.UtcNow
            };
            _context.Move.Add(historyMove);

            // Guardar cambios de manera atómica en la Base de Datos
            await _context.SaveChangesAsync();

            //Evaluar reglas de negocio para envío de notificaciones reales por correo
            string targetEmail = "egonzalezr22@miumg.edu.gt";
            string? emailError = null;

            switch (stageId)
            {
                case 3:
                case 6:
                    emailError = await SendEmailAsync(targetEmail, "Actualización de Proceso - Talent", "Gracias por participar en el proceso de selección.");
                    break;

                case 4:
                    emailError = await SendEmailAsync(targetEmail, "Programación de Entrevista - Talent", "Se ha programado una entrevista para el 15/07/2026.");
                    break;

                case 5:
                    emailError = await SendEmailAsync(targetEmail, "Oferta Laboral - Talent", "Ha sido seleccionado para la posicion con una propuesta salarial de Q10,000.");
                    break;

                case 8:
                    emailError = await SendEmailAsync(targetEmail, "Bienvenida - Talent", "Bienvenido a la compañia.");
                    break;

                default:
                    break;
            }

            if (emailError != null)
            {
                return BadRequest(new
                {
                    message = "Los datos se guardaron en la base de datos, pero el correo no se pudo enviar.",
                    detalle_error = emailError
                });
            }

            // 7. Retornar el estado actualizado del candidato
            var responseDto = new CandidateDto
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

            return Ok(responseDto);
        }


    }

}