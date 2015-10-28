using System.Linq;
using Microsoft.AspNet.Mvc;

namespace Loial
{
    public class HomeController : Controller
    {
        private readonly LoialDb _db;

        public HomeController(LoialDb db)
        {
            _db = db;
        }

        public dynamic Index()
        {
            return "Loial " + _db.Projects.Count();
        }

        public dynamic Error()
        {
            return "error";
        }
    };
}
