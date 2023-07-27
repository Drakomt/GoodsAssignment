using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace GoodsAssignment.Models
{
    public class User
    {
        [Key]
        [Required]
        public int ID { get; set; }
        [Required, MaxLength(1024)]
        public string FullName { get; set; }

        [Required, MaxLength(254)]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool Active { get; set; } = true;
    }

    public class BPType
    {
        [Key]
        [Required, MaxLength(1)]
        public string TypeCode { get; set; }
        [Required, MaxLength(20)]
        public string TypeName { get; set; }
    }

    public class BusinessPartner
    {
        [Key]
        [Required, MaxLength(128)]
        public string BPCode { get; set; }
        [Required, MaxLength(254)]
        public string BPName { get; set; }
        [Required, MaxLength(1)]
        public string BPType { get; set; }
        [Required]
        public bool Active { get; set; } = true;
    }

    public class Item
    {
        [Key]
        [Required, MaxLength(128)]
        public string ItemCode { get; set; }
        [Required, MaxLength(254)]
        public string ItemName { get; set; }
        [Required]
        public bool Active { get; set; } = true;
    }

    public class SaleOrder
    {
        [Key]
        [Required]
        public int ID { get; set; }
        [Required, MaxLength(128)]
        public string BPCode { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        [Required]
        public int CreatedBy { get; set; }
        public int? LastUpdatedBy { get; set; }
        public virtual ICollection<SaleOrderLine> SaleOrderLines { get; set; }
    }

    public class SaleOrderLine
    {
        [Key]
        [Required]
        public int LineID { get; set; }
        [Required]
        public int DocID { get; set; }
        [Required, MaxLength(128)]
        public string ItemCode { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        [Required]
        public int CreatedBy { get; set; }
        public int LastUpdatedBy { get; set; }
    }

    public class SaleOrderLineComment
    {
        [Key]
        [Required]
        public int CommentLineID { get; set; }
        [Required]
        public int DocID { get; set; }
        [Required]
        public int LineID { get; set; }
        [Required]
        public string Comment { get; set; }
    }

    public class PurchaseOrder
    {
        [Key]
        [Required]
        public int ID { get; set; }
        [Required, MaxLength(128)]
        public string BPCode { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        [Required]
        public int CreatedBy { get; set; }
        public int? LastUpdatedBy { get; set; }
        public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; }
    }

    public class PurchaseOrderLine
    {
        [Key]
        [Required]
        public int LineID { get; set; }
        [Required]
        public int DocID { get; set; }
        [Required, MaxLength(128)]
        public string ItemCode { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        [Required]
        public int CreatedBy { get; set; }
        public int LastUpdatedBy { get; set; }
    }


    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

}
