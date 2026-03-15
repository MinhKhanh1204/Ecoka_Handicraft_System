using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

[Table("CART")]
public class Cart
{
    [Key]
    [Column("CartID")]
    public int CartId { get; set; }

    [Column("CustomerID")]
    public string CustomerId { get; set; }  // string để khớp với AccountID dạng "CUS001"

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    public ICollection<CartItem> CartItems { get; set; }
}

[Table("CART_ITEMS")]
public class CartItem
{
    [Key]
    [Column("CartItemID")]
    public int CartItemID { get; set; }

    [Column("CartID")]
    public int CartId { get; set; }

    [Column("ProductID")]
    public string ProductID { get; set; }   // string để khớp với ProductId của ProductAPI (GUID/string)

    [Column("Quantity")]
    public int Quantity { get; set; }

    [JsonIgnore]  // Tránh circular reference khi OData serialize
    public Cart Cart { get; set; }
}