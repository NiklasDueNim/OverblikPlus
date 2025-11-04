namespace TaskMicroService.Dtos.Budget;

public class ReadBudgetDto
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
