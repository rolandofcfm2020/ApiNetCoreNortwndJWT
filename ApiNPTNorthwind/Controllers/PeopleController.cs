using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiNPTNorthwind.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiNPTNorthwind.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PeopleController : ControllerBase
    {

        [HttpGet]
        public List<People> getAllPeople()
        {
            List<People> myPeopleList = new List<People>();

            myPeopleList.Add(new People() { Age = 21, LastName = "Lozano", Name = "Alejandro", NSS = "34234525443" });
            myPeopleList.Add(new People() { Age = 21, LastName = "Garza", Name = "Matha", NSS = "345637737333" });
            myPeopleList.Add(new People() { Age = 21, LastName = "Barcenas", Name = "Joel", NSS = "111234898843" });

            return myPeopleList;
        }

        [HttpPost]
        public string postPeople()
        {
            var message = "Esta es un post a la API";
            return message;
        }
    }
}
