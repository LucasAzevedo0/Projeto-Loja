using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaRemastered.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }


        [Display(Name = "ID do Carinho")]
        [Required]
        public int CartId { get; set; }
        public virtual Cart Cart { get; set; }


        [Display(Name ="ID do Produto ")]
        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }


        [Display(Name ="Preço do Produto")]
        [Required]
        public decimal Price { get; set; }

        [Display(Name = "Nome do Produto")]
        [Required]
        public string ProductName { get; set; }



        [Display(Name="Quantidade do Produto")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
        public int Quantity { get; set; } = 1;



        [NotMapped]
        public decimal TotalPrice => Price * Quantity;
    }
}
