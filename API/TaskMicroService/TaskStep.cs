namespace TaskMicroService;

public class TaskStep
{
    public int Id { get; set; }
    public int TaskId { get; set; } // Refererer til den opgave, trinnet hører til
    public string ImageUrl { get; set; } // URL til billede, der viser trinnet
    public string Text { get; set; } // Beskrivelse af trinnet
    public int StepNumber { get; set; } // Nummeret på trinnet i sekvensen
}