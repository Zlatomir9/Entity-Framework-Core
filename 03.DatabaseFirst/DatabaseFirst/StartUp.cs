using System;
using System.Linq;
using SoftUni.Models;
using SoftUni.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var softUniContext = new SoftUniContext();
            var result = RemoveTown(softUniContext);
            Console.WriteLine(result);
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees
                    .Select(c => new { c.EmployeeId, c.FirstName, c.LastName, c.MiddleName, c.JobTitle, c.Salary })
                    .OrderBy(x => x.EmployeeId)
                    .ToList();

            StringBuilder result = new StringBuilder();

            foreach (var employee in employees)
            {
                result.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:F2}");
            }

            return result.ToString().TrimEnd();
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                    .Select(x => new { x.FirstName, x.Salary })
                    .Where(x => x.Salary > 50000)
                    .OrderBy(x => x.FirstName)
                    .ToList();

            StringBuilder result = new StringBuilder();

            foreach (var employee in employees)
            {
                result.AppendLine($"{employee.FirstName} - {employee.Salary:F2}");
            }

            return result.ToString().TrimEnd();
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(x => new { x.FirstName, x.LastName, x.Department.Name, x.Salary })
                .Where(x => x.Name == "Research and Development")
                .OrderBy(x => x.Salary)
                .ThenByDescending(x => x.FirstName)
                .ToList();

            StringBuilder result = new StringBuilder();

            foreach (var employee in employees)
            {
                result.AppendLine($"{employee.FirstName} {employee.LastName} from {employee.Name} - ${employee.Salary:F2}");
            }

            return result.ToString().TrimEnd();
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            Address newAdress = new Address { AddressText = "Vitoshka 15", TownId = 4 };
            context.Addresses.Add(newAdress);
            context.SaveChanges();

            var employee = context.Employees
                 .FirstOrDefault(x => x.LastName == "Nakov");

            employee.AddressId = newAdress.AddressId;
            context.SaveChanges();

            StringBuilder result = new StringBuilder();

            var employeesAdresses = context.Employees
                .Select(e => new { e.Address.AddressText, e.Address.AddressId })
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .ToList();

            foreach (var adress in employeesAdresses)
            {
                result.AppendLine($"{adress.AddressText}");
            }

            return result.ToString().TrimEnd();
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context.Employees
                .Include(x => x.EmployeesProjects)
                .ThenInclude(x => x.Project)
                .Where(x => x.EmployeesProjects.Any(p => p.Project.StartDate.Year >= 2001 && p.Project.StartDate.Year <= 2003))
                .Select(x => new
                {
                    EmployeeFirstName = x.FirstName,
                    EmployeeLastName = x.LastName,
                    ManagerFirstName = x.Manager.FirstName,
                    ManagerLastName = x.Manager.LastName,
                    Projects = x.EmployeesProjects.Select(p => new
                    {
                        ProjectName = p.Project.Name,
                        StartDate = p.Project.StartDate,
                        EndDate = p.Project.EndDate
                    })
                })
                .Take(10)
                .ToList();

            StringBuilder result = new StringBuilder();

            foreach (var employee in employees)
            {
                result.AppendLine($"{employee.EmployeeFirstName} {employee.EmployeeLastName} - Manager: " +
                    $"{employee.ManagerFirstName} {employee.ManagerLastName}");

                foreach (var employeeProject in employee.Projects)
                {
                    if (employeeProject.EndDate.HasValue)
                    {
                        result.AppendLine($"--{employeeProject.ProjectName} " +
                            $"- {employeeProject.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)} " +
                            $"- {employeeProject.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)}");
                    }
                    else
                    {
                        result.AppendLine($"--{employeeProject.ProjectName} " +
                            $"- {employeeProject.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)} " +
                            $"- not finished");
                    }
                }
            }

            return result.ToString().TrimEnd();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var adresses = context.Addresses
                .OrderByDescending(x => x.Employees.Count)
                .ThenBy(x => x.Town.Name)
                .ThenBy(x => x.AddressText)
                .Take(10)
                .Select(x => new
                {
                    x.AddressText,
                    x.Town.Name,
                    Count = x.Employees.Count
                })
                .ToList();

            var result = new StringBuilder();

            foreach (var adress in adresses)
            {
                result.AppendLine($"{adress.AddressText}, {adress.Name} - {adress.Count} employees");
            }

            return result.ToString().TrimEnd();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var employee = context.Employees
                .Where(x => x.EmployeeId == 147)
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.JobTitle,
                    x.EmployeeId,
                    Projects = x.EmployeesProjects.Select(x => x.Project.Name).OrderBy(p => p).ToList()
                })
                .FirstOrDefault();

            var result = new StringBuilder();
            result.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");

            foreach (var project in employee.Projects)
            {
                result.AppendLine(project);
            }

            return result.ToString().TrimEnd();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                .Where(d => d.Employees.Count > 5)
                .OrderBy(x => x.Employees.Count)
                .ThenBy(x => x.Name)
                .Select(x => new
                {
                    x.Name,
                    x.Manager.FirstName,
                    x.Manager.LastName,
                    Employees = x.Employees
                        .Select(x => new
                        {
                            x.FirstName,
                            x.LastName,
                            x.JobTitle
                        })
                })
                .ToList();

            var result = new StringBuilder();

            foreach (var department in departments)
            {
                result.AppendLine($"{department.Name} - {department.FirstName} {department.LastName}");

                foreach (var employee in department.Employees.OrderBy(x => x.FirstName).ThenBy(x => x.LastName))
                {
                    result.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");
                }
            }

            return result.ToString().TrimEnd();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .OrderByDescending(x => x.StartDate)
                .Select(x => new
                {
                    x.Name,
                    x.Description,
                    x.StartDate
                })
                .Take(10)
                .OrderBy(x => x.Name)
                .ToList();

            var result = new StringBuilder();

            foreach (var project in projects)
            {
                result.AppendLine($"{project.Name}");
                result.AppendLine($"{project.Description}");
                result.AppendLine($"{project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)}");
            }

            return result.ToString().TrimEnd();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            context.Employees
                   .Where(e => new[] { "Engineering", "Tool Design", "Marketing", "Information Services" }
                   .Contains(e.Department.Name))
                   .ToList()
                   .ForEach(e => e.Salary *= 1.12m);
            context.SaveChanges();

            var employees = context.Employees
                .Where(e => new[] { "Engineering", "Tool Design", "Marketing", "Information Services" }
                    .Contains(e.Department.Name))
                .Select(x => new 
                {
                    x.FirstName,
                    x.LastName,
                    x.Salary
                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            var result = new StringBuilder();

            foreach (var employee in employees)
            {
                result.AppendLine($"{employee.FirstName} {employee.LastName} (${employee.Salary:F2})");
            }

            return result.ToString().TrimEnd();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.FirstName.StartsWith("Sa"))
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.JobTitle,
                    x.Salary
                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            var result = new StringBuilder();

            foreach (var employee in employees)
            {
                result.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle} - (${employee.Salary:F2})");
            }

            return result.ToString().TrimEnd();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var project = context.Projects.Find(2);

            context.EmployeesProjects
                .Where(x => x.ProjectId == project.ProjectId)
                .ToList()
                .ForEach(ep => context.EmployeesProjects.Remove(ep));

            context.Remove(project);

            context.SaveChanges();

            var projects = context.Projects
                .Take(10)
                .Select(x => x.Name)
                .ToList();

            var result = new StringBuilder();

            foreach (var proj in projects)
            {
                result.AppendLine(proj);
            }

            return result.ToString().TrimEnd();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var townToDelete = context.Towns
                .Include(x => x.Addresses)
                .FirstOrDefault(x => x.Name == "Seattle");

            var adresses = townToDelete.Addresses.Select(x => x.AddressId).ToList();

            var employees = context.Employees
                .Where(x => x.AddressId.HasValue && adresses.Contains(x.AddressId.Value))
                .ToList();

            foreach (var employee in employees)
            {
                employee.AddressId = null;
            }

            foreach (var adressId in adresses)
            {
                var adress = context.Addresses
                    .FirstOrDefault(x => x.AddressId == adressId);

                context.Addresses.Remove(adress);
            }

            context.Towns.Remove(townToDelete);

            context.SaveChanges();

            return $"{adresses.Count} addresses in Seattle were deleted";
        }
    }
}
