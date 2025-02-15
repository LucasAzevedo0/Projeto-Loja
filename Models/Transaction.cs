using System.ComponentModel.DataAnnotations;

namespace LojaRemastered.Models
{
    // ENUM PARA O TIPO DA TRANSACAO, VENDA, COMPRA, DEPOSITO
    public enum TransactionType
    {
        Deposit,
        Purchase,
        Sale
    }
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Usuario da Transação")]
        public string UserId { get; set; }


        [Required]
        [Display(Name = "Usuario Relacionado a Transação")]
        public string RelatedUserId { get; set; }


        [Required]
        [Display(Name = "Valor da Transação")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Tipo da Transação")]
        public TransactionType TransactionType { get; set; }


        [Required]
        [Display(Name = "Data da Transação")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Saldo Após a Transação")]
        public decimal BalanceAfterTransaction { get; set; }


        

    }
}
