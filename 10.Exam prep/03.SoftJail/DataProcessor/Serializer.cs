namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners.ToList()
                .Where(x => ids.Contains(x.Id))
                .Select(p => new
                {
                    p.Id,
                    Name = p.FullName,
                    p.Cell.CellNumber,
                    Officers = p.PrisonerOfficers.ToList().Select(po => new
                    {
                        OfficerName = po.Officer.FullName,
                        Department = po.Officer.Department.Name,
                    }).OrderBy(x => x.OfficerName),
                    TotalOfficerSalary = p.PrisonerOfficers.Sum(s => s.Officer.Salary)
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id);

            return JsonConvert.SerializeObject(prisoners, Formatting.Indented);
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var splittedPrisonersNames = prisonersNames.Split(',');

            var prisoners = context.Prisoners
                .ToList()
                .Where(x => splittedPrisonersNames.Contains(x.FullName))
                .Select(p => new PrisonerXmlExportModel
                {
                    Id = p.Id,
                    Name = p.FullName,
                    IncarcerationDate = p.IncarcerationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EncryptedMessages = p.Mails.Select(m => new MessageXmlExportModel
                    {
                        Description = new string(m.Description.ToCharArray().Reverse().ToArray())
                    }).ToArray()
                })
                .OrderBy(x => x.Name).ThenBy(x => x.Id)
                .ToArray();

            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(PrisonerXmlExportModel[]), new XmlRootAttribute("Prisoners"));

            var writer = new StringWriter();
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            xmlSerializer.Serialize(writer, prisoners, ns);
            return writer.ToString();
        }
    }
}