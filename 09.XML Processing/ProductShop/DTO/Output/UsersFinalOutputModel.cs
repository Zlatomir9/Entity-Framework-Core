using System.Xml.Serialization;

namespace ProductShop.DTO.Output
{
    [XmlType("users")]
    public class UsersFinalOutputModel
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("users")]
        public UserOutputModel[] Users { get; set; }
    }
}
