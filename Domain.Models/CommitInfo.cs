namespace Domain.Models
{
    public class CommitInfo
    {
        public required string Sha { get; set; }
        public required CommitDetails Commit { get; set; }
    }
}
