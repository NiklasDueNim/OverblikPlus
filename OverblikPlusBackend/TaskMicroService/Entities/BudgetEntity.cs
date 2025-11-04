namespace TaskMicroService.Entities;

public class BudgetEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime Date { get; set; }
    public string Activity { get; set; }
    public string? Voucher { get; set; }
    public decimal MoneyIn { get; set; }
    public decimal MoneyOut { get; set; }
    public string? Note { get; set; }
}
