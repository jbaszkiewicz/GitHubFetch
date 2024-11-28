namespace Domain.Models
{
    public class CommitDetails
    {
        public required string Message { get; set; }
        public required Committer Committer { get; set; }
    }
}
