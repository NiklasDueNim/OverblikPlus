namespace OverblikPlus.Models.Dtos.Budget;

public class BudgetDto
{
    public Guid Id { get; set; }
    public Guid User { get; set; }
    public DateTime Date { get; set; }
    public string Activity { get; set; }
    public string Voucher { get; set; }
    public decimal MoneyIn { get; set; }
    public decimal MoneyOut { get; set; }

    public string Note { get; set; }
}