using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace LojaRemastered.Models
{
    public class Product
    {

        // ATRIBUIDOS AUTOMATICAMENTE
        [Key]
        public int Id { get; set; }



        [Required]
        [Display(Name = "ID do vendedor")]
        public string SellerId { get; set; }

        [Required]
        [Display(Name = "Nome do Vendedor")]
        public string SellerName { get; set; }

        [Required]
        [Display(Name = "Data do Anuncio")]
        public DateTime DataAnuncio { get; set; } = DateTime.Now;



        // ATRIBUIDOS EXPLICITAMENTE PELO USUARIO

        [Required(ErrorMessage = "Digite o nome do produto")]
        [StringLength(150)]
        [Display(Name = "Nome do produto")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Digite o Preço do produto")]
        [Display(Name = "Preço do produto")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }


        [Required(ErrorMessage = "Unidades do produto")]
        [Display(Name="Unidades")]
        public int Stocks { get; set; } = 1;






    }
}
