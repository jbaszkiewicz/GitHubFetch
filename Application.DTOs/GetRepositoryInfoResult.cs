namespace Application.DTOs
{
    public class GetRepositoryInfoResult
    {
        public bool Authorized { get; set; }
        public bool RepoExists { get; set; }
        public int CommitCount { get; set; }
    }
}
