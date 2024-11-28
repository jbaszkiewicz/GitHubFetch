using Domain.Models;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inftrastructure.Repositories
{
    public class CommitRepository : ICommitRepository
    {
        private readonly SqliteDbContext _context;

        public CommitRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<List<CommitInfo>> ReturnNonExistingCommitsAsync(List<CommitInfo> commitsToCheck)
        {
            var shasToCheck = commitsToCheck.Select(c => c.Sha).ToList();

            var existingShas = await _context.Commits
                .Where(c => shasToCheck.Contains(c.Sha))
                .Select(c => c.Sha)
                .ToListAsync();

            var nonExistentCommits = commitsToCheck
                .Where(c => !existingShas.Contains(c.Sha))
                .ToList();

            return nonExistentCommits;
        }

        public async Task SaveCommitsAsync(string owner, string repo, List<CommitInfo> commits)
        {
            foreach (var commit in commits)
            {
                if (!await _context.Commits.AnyAsync(c => c.Sha == commit.Sha))
                {
                    _context.Commits.Add(new CommitRecord()
                    {
                        Sha = commit.Sha,
                        Owner = owner,
                        Repo = repo,
                        Message = commit.Commit.Message,
                        Committer = $"{commit.Commit.Committer.Name}/{commit.Commit.Committer.Email}"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }

}
