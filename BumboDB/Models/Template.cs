namespace BumboDB.Models;

public class Template
{
    public int TemplateId { get; set; }
    public string Name { get; set; } = null!;
    public int? ChapterId { get; set; } // Ensure this property exists
    public string? Description { get; set; }
    public int PredictedCustomers { get; set; }
    public int PredictedCargo { get; set; }

    public virtual Chapter? Chapter { get; set; } = null!;
}

