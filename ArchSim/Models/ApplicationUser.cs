namespace ArchSim.Models;

public class ApplicationUser
{
    public int Id { get; set; }
    public string GoogleId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    public ICollection<UserScoreRecord> ScoreRecords { get; set; } = new List<UserScoreRecord>();
}
