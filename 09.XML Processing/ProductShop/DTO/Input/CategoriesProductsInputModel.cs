using System.Xml.Serialization;

namespace ProductShop.DTO.Input
{
    [XmlType("CategoryProduct")]
    public class CategoriesProductsInputModel
    {
        [XmlElement("CategoryId")]
        public int CategoryId { get; set; }

        [XmlElement("ProductId")]
        public int ProductId { get; set; }
    }
}
