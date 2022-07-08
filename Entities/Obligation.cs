namespace API.Entities
{
    public class Obligation
    {
       public int Id { get; set; } 
       public long Amount { get; set; }
       public string? Type { get; set; }
    }
}