using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    public class SecretController : Controller
    {
        public string Index()
        {
            return "secret message";
        }
    }
}
