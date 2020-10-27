using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ApiNPTNorthwind.DataAccess;
using ApiNPTNorthwind.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiNPTNorthwind.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {

        [HttpGet]
        public List<EmployeeDTO> GetAllEmployees()
        {

            DataAccess.NORTHWNDContext dataContext = new DataAccess.NORTHWNDContext();

            var employees = dataContext.Employees.ToList();  // toList() makes a query materialized select *from Employees

            List<EmployeeDTO> listEmployees = new List<EmployeeDTO>();

            Stopwatch swForeach = new Stopwatch();
            swForeach.Start();

            foreach (var emp in employees)
            {
                var e = new EmployeeDTO();
                e.Name = emp.FirstName;
                e.LastName = emp.LastName;
                e.BirthDate = emp.BirthDate;
                e.Phone = emp.HomePhone;

                listEmployees.Add(e);

            }
            swForeach.Stop();

            var tiemeForeach = swForeach.ElapsedMilliseconds;

            Stopwatch swSelect = new Stopwatch();
            swSelect.Start();

            var employeesResult = employees.Select(empElement => new EmployeeDTO { Name = empElement.FirstName, LastName = empElement.LastName, BirthDate = empElement.BirthDate, Phone = empElement.HomePhone,}).ToList();  // select FirstName as Name, LastName, BirthDate, HomePhone as Phone from Employees

            swSelect.Stop();

            var timeSelect = swSelect.ElapsedMilliseconds;

            return employeesResult;
        }


        [HttpGet]
        [Authorize]
        //[AllowAnonymous]
        public EmployeeDTO GetEmployeeByID(int id)
        {
            DataAccess.NORTHWNDContext dataContext = new DataAccess.NORTHWNDContext();
            var employee = dataContext.Employees.Where(w => w.EmployeeId == id).FirstOrDefault();

            if (employee == null)
                return null;

            var employeeDTO = new EmployeeDTO()
            {
                LastName = employee.LastName,
                Name = employee.FirstName,
                BirthDate = employee.BirthDate,
                Phone = employee.HomePhone

            };

            return employeeDTO;

        }


        [HttpPost]
        public bool AddNewEmployee([FromBody] NewEmployee employee)
        {

            try
            {
                DataAccess.NORTHWNDContext dataContext = new DataAccess.NORTHWNDContext();
                var newEmployeeInDB = new Employees()
                {

                    FirstName = employee.FirstName, 
                    LastName = employee.LastName,
                    Title = employee.Title,

                };

                dataContext.Employees.Add(newEmployeeInDB); // insert into Employees values ('Rolando', 'Dominguez', 'Mr.')
                dataContext.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;


        }

        [HttpDelete]
        public string DeleteEmployeeByID(int id)
        {

            try
            {
                DataAccess.NORTHWNDContext dataContext = new DataAccess.NORTHWNDContext();
                var employee = dataContext.Employees.Where(w => w.EmployeeId == id).FirstOrDefault();

                if (employee == null)
                    return "Empleado no encontrado, favor de verificar el ID que mandas";

                dataContext.Employees.Remove(employee); // delete from Employees where EmployeeID = id
                dataContext.SaveChanges();
            }
            catch (Exception)
            {
                return "El empleado no ha podido ser eliminado, ha ocurrido un error por parte del servidor";
            }

            return "El empleado con ID " + id + " ha sido eliminado de la base de datos";


        }
    }
}
