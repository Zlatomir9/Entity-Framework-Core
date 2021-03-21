using System.Xml.Serialization;

namespace ProductShop.DTO.Output
{
    [XmlType("SoldProducts")]
    public class SoldProductsCount
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("products")]
        public SoldProductsOutputModel[] SoldProducts { get; set; }
    }
}
