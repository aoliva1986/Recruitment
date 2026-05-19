using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruitment.Data;
using Recruitment.DTOs;


namespace Recruitment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {

        private readonly AppDbContext _context;

        // Inyección de dependencias de la base de datos
        public CompanyController(AppDbContext context)
        {
            _context = context;
        }        
        // GET: api/v1/companies/{companyId}
        [HttpGet("{companyId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCompanyById(int companyId)
        {
            //Validación del parámetro
            if (companyId <= 0)
            {
                return BadRequest(new { message = "El ID debe ser mayor a cero." });
            }

            //Consulta asíncrona a tu tabla 'Company' (singular, tal como está en tu AppDbContext)
            var company = await _context.Company.FindAsync(companyId);

            //Validación de existencia
            if (company == null)
            {
                return NotFound(new { message = $"No se encontró ninguna empresa con el ID {companyId}." });
            }

            //Mapeo del Modelo al DTO de salida
            var companyDto = new CompanyDto
            {
                company_id = company.company_id,
                name = company.name, // Mapea a la columna real de tu base de datos
                tax_id = company.tax_id,
                website = company.website
            };

            //Retorno exitoso
            return Ok(companyDto);
        }
        // GET: api/v1/companies/{companyId}/pipelines/{pipelineId}
        [HttpGet("{companyId:int}/pipelines/{pipelineId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Seleccion_PipelineDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPipelineStepById(int companyId, int pipelineId)
        {
            //Validar que los IDs en la URL tengan sentido
            if (companyId <= 0 || pipelineId <= 0)
            {
                return BadRequest(new { message = "Tanto el ID de la empresa como el del pipeline deben ser mayores a cero." });
            }

            // 2. Buscar en la base de datos usando tus clases reales
            // Nota: Asegúrate de que en tu AppDbContext la propiedad se llame 'SeleccionPipeline'
            //var pipelineStep = await _context.SelectionPipeline.FirstOrDefaultAsync(p => p.pipeline_id == pipelineId && p.company_id == companyId);
            var pipelineStep = await _context.Selection_Pipeline.FirstOrDefaultAsync(p => p.pipeline_id == pipelineId && p.company_id == companyId);

            // 3. Si no existe o no coincide la relación Empresa -> Pipeline, devolvemos 404
            if (pipelineStep == null)
            {
                return NotFound(new
                {
                    message = $"No se encontró el paso de pipeline con ID {pipelineId} asociado a la empresa {companyId}."
                });
            }

            // 4. Mapear a tu DTO (SeleccionPipelineDto) usando tus propiedades reales
            var dto = new Seleccion_PipelineDto
            {
                pipeline_id = pipelineStep.pipeline_id,
                company_id = pipelineStep.company_id,
                name = pipelineStep.name
                //created_at no se mapea porque tu DTO no lo incluye. ¡Perfecto!
            };

            // 5. Devolver la respuesta exitosa
            return Ok(dto);
        }

        // GET: api/v1/companies/{companyId}/pipelines
        [HttpGet("{companyId:int}/pipelines")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Seleccion_PipelineDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPipelineStepById(int companyId)
        {
            //Validar que los IDs en la URL tengan sentido
            if (companyId <= 0)
            {
                return BadRequest(new { message = "Tanto el ID de la empresa como el del pipeline deben ser mayores a cero." });
            }

            //Buscar en la base de datos usando tus clases reales
            // Nota: Asegúrate de que en tu AppDbContext la propiedad se llame 'SeleccionPipeline'
            //var pipelineStep = await _context.SelectionPipeline.FirstOrDefaultAsync(p => p.pipeline_id == pipelineId && p.company_id == companyId);
            var pipelineStep = await _context.Selection_Pipeline.AnyAsync(p =>  p.company_id == companyId);



            //Obtenemos TODOS los pipelines de esa empresa de forma asíncrona
            // Usamos .Where() en lugar de .FirstOrDefaultAsync()
            var pipelineSteps = await _context.Selection_Pipeline
                .Where(p => p.company_id == companyId)
                .ToListAsync();



            //Mapeamos la lista de modelos a una lista de DTOs usando LINQ (.Select)
            var dtos = pipelineSteps.Select(p => new Seleccion_PipelineDto
            {
                pipeline_id = p.pipeline_id, // 🔹 Es buena práctica incluirlo para que el frontend sepa cuál es cuál
                company_id = p.company_id,
                name = p.name
            }).ToList();

            // 5. Devolver la lista (si está vacía, devolverá [ ] lo cual es correcto en REST para listados)
            return Ok(dtos);
        }
        // GET: api/v1/companies
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompanyDto>))]
        public async Task<IActionResult> GetAllCompanies()
        {
            //Consultar de forma asíncrona todas las empresas en la base de datos
            var companies = await _context.Company.ToListAsync();

            //Mapear la lista de modelos a una lista de DTOs limpios usando LINQ
            var companyDtos = companies.Select(c => new CompanyDto
            {
                company_id = c.company_id,
                name = c.name,
                tax_id = c.tax_id,
                website = c.website 
                

            }).ToList();

            //Retornar la lista con un estado HTTP 200 OK
            // Nota: Si no hay empresas, devolverá una lista vacía [ ], lo cual es correcto en REST
            return Ok(companyDtos);
        }
    }

}

