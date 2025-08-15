using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CsvImportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsvImportController : ControllerBase
    {
        private readonly AppDbContext _context; // контекст

        public CsvImportController (AppDbContext context) // конструктор
        {
            _context = context;
        }
        
    }
}
