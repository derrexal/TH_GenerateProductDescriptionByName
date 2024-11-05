using System.ComponentModel.DataAnnotations.Schema;

namespace HN_GenerateProductDescriptionByName.Server
{
    [Table("AppProductDetails")]
    public class ProductDetails
    {
        public int Id { get; set; }
        public long detailId { get; set; }
        public string detailName { get; set; }  

        public long PortalCategoryId { get; set; }

        public string PortalCategoryName { get; set; }

        public string CategoryDataType { get; set; }
    }
}
