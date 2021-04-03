namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid Data";

        private const string ImportedDepartment = "Imported {0} with {1} cells";

        private const string ImportedPrisoner = "Imported {0} {1} years old";

        private const string ImportedOfficer = "Imported {0} ({1} prisoners)";

        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var departmentDtos = JsonConvert.DeserializeObject<DepartmentImportModel[]>(jsonString);

            foreach (var jsonDeparment in departmentDtos)
            {
                if (!IsValid(jsonDeparment) || !jsonDeparment.Cells.Any() || !jsonDeparment.Cells.All(IsValid))
                {
                    stringBuilder.AppendLine(ErrorMessage);
                    continue;
                }

                var department = new Department
                {
                    Name = jsonDeparment.Name,
                    Cells = jsonDeparment.Cells.Select(c => new Cell 
                    {
                        CellNumber = c.CellNumber,
                        HasWindow = c.HasWindow
                    }).ToArray()
                };

                //bool isInvalidCell = false;

                //foreach (var jsonCell in jsonDeparment.Cells)
                //{
                //    if (!IsValid(jsonCell))
                //    {
                //        stringBuilder.AppendLine(ErrorMessage);
                //        isInvalidCell = true;
                //        break;
                //    }

                //    var cell = new Cell
                //    {
                //        CellNumber = jsonCell.CellNumber,
                //        HasWindow = jsonCell.HasWindow
                //    };

                //    department.Cells.Add(cell);
                //}

                //if (isInvalidCell)
                //{
                //    continue;
                //}

                context.Departments.Add(department);
                context.SaveChanges();
                stringBuilder.AppendLine(String.Format(ImportedDepartment, department.Name, department.Cells.Count));
            }

            return stringBuilder.ToString();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var prisonerDtos = JsonConvert.DeserializeObject<PrisonerImportModel[]>(jsonString);

            foreach (var jsonPrisoner in prisonerDtos)
            {
                if (!IsValid(jsonPrisoner))
                {
                    stringBuilder.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime releaseDateValue;
                bool isReleaseDateValid = DateTime.TryParseExact(jsonPrisoner.ReleaseDate, "dd/MM/yyyy",
                     CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDateValue);

                var prisoner = new Prisoner
                {
                    Nickname = jsonPrisoner.Nickname,
                    FullName = jsonPrisoner.FullName,
                    Age = jsonPrisoner.Age,
                    IncarcerationDate = DateTime.ParseExact(jsonPrisoner.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    ReleaseDate = releaseDateValue,
                    Bail = jsonPrisoner.Bail,
                    CellId = jsonPrisoner.CellId
                };

                bool isInvalidMail = false;

                foreach (var jsonMail in jsonPrisoner.Mails)
                {
                    if (!IsValid(jsonMail))
                    {
                        stringBuilder.AppendLine(ErrorMessage);
                        isInvalidMail = true;
                        break;
                    }

                    var mail = new Mail
                    {
                        Description = jsonMail.Description,
                        Address = jsonMail.Address,
                        Sender = jsonMail.Sender
                    };

                    prisoner.Mails.Add(mail);
                }

                if (isInvalidMail)
                {
                    continue;
                }

                context.Prisoners.Add(prisoner);
                context.SaveChanges();
                stringBuilder.AppendLine(String.Format(ImportedPrisoner, prisoner.FullName, prisoner.Age.ToString()));
            }

            return stringBuilder.ToString();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            XmlSerializer serializer = new XmlSerializer(typeof(OfficerImportModel[]), new XmlRootAttribute("Officers"));
            var officersDtos = (OfficerImportModel[])serializer.Deserialize(new StringReader(xmlString));

            foreach (var officerDto in officersDtos)
            {
                if (!IsValid(officerDto))
                {
                    stringBuilder.AppendLine(ErrorMessage);
                    continue;
                }

                Object positionResult;
                bool isValidPosition = Enum.TryParse(typeof(Position), officerDto.Position.ToString(), out positionResult);

                if (!isValidPosition)
                {
                    stringBuilder.AppendLine(ErrorMessage);
                    continue;
                }

                Object weaponResult;
                bool isValidWeapon = Enum.TryParse(typeof(Weapon), officerDto.Weapon.ToString(), out weaponResult);

                if (!isValidWeapon)
                {
                    stringBuilder.AppendLine(ErrorMessage);
                    continue;
                }

                var officer = new Officer
                {
                    FullName = officerDto.Name,
                    Salary = officerDto.Money,
                    DepartmentId = officerDto.DepartmentId,
                    Position = (Position)positionResult,
                    Weapon = (Weapon)weaponResult
                };

                foreach (var prisonerDto in officerDto.Prisoners)
                {
                    officer.OfficerPrisoners.Add(new OfficerPrisoner() 
                    {
                        Officer = officer,
                        PrisonerId = prisonerDto.Id
                    });
                }

                context.Officers.Add(officer);
                context.SaveChanges();

                stringBuilder.AppendLine(String.Format(ImportedOfficer, officer.FullName, officer.OfficerPrisoners.Count()));
            }

            return stringBuilder.ToString();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}