using Microsoft.AspNetCore.Mvc;

namespace WebAPI2Entidad.Controllers
{
    public class EntidadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

//http://localhost:5068/WeatherForecast
