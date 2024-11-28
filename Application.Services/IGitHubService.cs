using Application.DTOs;
using Domain.Models;

namespace Application.Services
{
    public interface IGitHubService
    {
        Task<GetRepositoryInfoResult> GetRepositoryInfoAsync(string owner, string repo);
        IAsyncEnumerable<(bool IsSuccessful, List<CommitInfo> Commits)> FetchCommitsAsync(string owner, string repo);
    }
}
