using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruitment.Data;
using Recruitment.DTOs;

namespace Recruitment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionnaireController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuestionnaireController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/companies/{companyId}/pipelines/{pipelineId}
        [HttpGet("{companyId:int}/questionnaire/{questionnaireId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Seleccion_PipelineDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetQuestionnaireById(int companyId, int questionnaireId)
        {
            //Validar que los IDs en la URL tengan sentido
            if (companyId <= 0 || questionnaireId <= 0)
            {
                return BadRequest(new { message = "Tanto el ID de la empresa como el del pipeline deben ser mayores a cero." });
            }

            // 2. Buscar en la base de datos usando tus clases reales
            // Nota: Asegúrate de que en tu AppDbContext la propiedad se llame 'SeleccionPipeline'
            //var pipelineStep = await _context.SelectionPipeline.FirstOrDefaultAsync(p => p.pipeline_id == pipelineId && p.company_id == companyId);
            var questionnaire = await _context.Questionnaire.FirstOrDefaultAsync(p => p.questionnaire_id == questionnaireId && p.company_id == companyId);

            // 3. Si no existe o no coincide la relación Empresa -> Pipeline, devolvemos 404
            if (questionnaire == null)
            {
                return NotFound(new
                {
                    message = $"No se encontró el paso de pipeline con ID {questionnaireId} asociado a la empresa {companyId}."
                });
            }

            // 4. Mapear a tu DTO (SeleccionPipelineDto) usando tus propiedades reales
            var dto = new QuestionnaireDto
            {

                questionnaire_id = questionnaire.questionnaire_id,
                company_id = questionnaire.company_id,
                title = questionnaire.title,
                description = questionnaire.description,        
                created_at = questionnaire.created_at

                //created_at no se mapea porque tu DTO no lo incluye. ¡Perfecto!
            };

            // 5. Devolver la respuesta exitosa
            return Ok(dto);
        }
        // GET: api/v1/companies/{companyId}/pipelines
        [HttpGet("{companyId:int}/questionnaire")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Seleccion_PipelineDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetQuestionnaireStepById(int companyId)
        {
            //Validar que los IDs en la URL tengan sentido
            if (companyId <= 0)
            {
                return BadRequest(new { message = "Tanto el ID de la empresa como el del pipeline deben ser mayores a cero." });
            }

            //Buscar en la base de datos usando tus clases reales
            // Nota: Asegúrate de que en tu AppDbContext la propiedad se llame 'SeleccionPipeline'
            //var pipelineStep = await _context.SelectionPipeline.FirstOrDefaultAsync(p => p.pipeline_id == pipelineId && p.company_id == companyId);
            var questionnaireStep = await _context.Questionnaire.AnyAsync(p => p.company_id == companyId);



            //Obtenemos TODOS los pipelines de esa empresa de forma asíncrona
            // Usamos .Where() en lugar de .FirstOrDefaultAsync()
            var questionnaireSteps = await _context.Questionnaire
                .Where(p => p.company_id == companyId)
                .ToListAsync();



            //Mapeamos la lista de modelos a una lista de DTOs usando LINQ (.Select)
            var dtos = questionnaireSteps.Select(p => new QuestionnaireDto
            {
                questionnaire_id = p.questionnaire_id,
                company_id = p.company_id,
                title = p.title,
                description = p.description,
                created_at = p.created_at
            }).ToList();

            // 5. Devolver la lista (si está vacía, devolverá [ ] lo cual es correcto en REST para listados)
            return Ok(dtos);
        }
    }
}
