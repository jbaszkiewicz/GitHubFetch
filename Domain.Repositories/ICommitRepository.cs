using Domain.Models;

namespace Domain.Repositories
{
    public interface ICommitRepository
    {
        Task<List<CommitInfo>> ReturnNonExistingCommitsAsync(List<CommitInfo> commitsToCheck);
        Task SaveCommitsAsync(string owner, string repo, List<CommitInfo> commits);
    }
}
