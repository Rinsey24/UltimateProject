namespace UltimateProject.Models;

public class MainEntity : Entity
{
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    // 🔥 VIRTUAL для Lazy Loading
    public virtual ICollection<RelatedEntity> RelatedEntities { get; set; } = new List<RelatedEntity>();
}