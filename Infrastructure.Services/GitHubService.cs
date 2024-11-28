using Application.DTOs;
using Application.Services;
using Domain.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infrastructure.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;

        public GitHubService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GetRepositoryInfoResult> GetRepositoryInfoAsync(string owner, string repo)
        {
            var result = new GetRepositoryInfoResult { RepoExists = false, CommitCount = -1, Authorized = false };

            try
            {
                string url = $"repos/{owner}/{repo}/commits?per_page=1&page=1";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    result.RepoExists = result.Authorized = true;

                    if (response.Headers.TryGetValues("Link", out var linkHeaderValues))
                    {
                        string pattern = @"<https[^>]*[?&]page=(\d+)>; rel=""last""";
                        Match match = Regex.Match(linkHeaderValues.FirstOrDefault() ?? "", pattern);

                        result.CommitCount = match.Success ? int.Parse(match.Groups[1].Value) : 1;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    result.Authorized = false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    result.Authorized = true;
                }
                else
                {
                    throw new Exception($"Http {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when getting repository info: {ex.Message}");
            }

            return result;
        }


        public async IAsyncEnumerable<(bool IsSuccessful, List<CommitInfo> Commits)> FetchCommitsAsync(string owner, string repo)
        {
            int perPage = 100;
            int page = 1;
            bool hasMore = true;

            while (hasMore)
            {
                List<CommitInfo> commits = new();
                bool isSuccessful = true;

                try
                {
                    string url = $"repos/{owner}/{repo}/commits?per_page={perPage}&page={page}";
                    var response = await _httpClient.GetAsync(url);

                    // GitHub API has a limit of 5000 calls per hour.
                    if (response.Headers.Contains("X-RateLimit-Remaining"))
                    {
                        int remainingRequests = int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault() ?? "0");
                        if (remainingRequests <= 0)
                        {
                            isSuccessful = false;
                            hasMore = false;
                        }
                    }

                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    commits = JsonSerializer.Deserialize<List<CommitInfo>>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<CommitInfo>();

                    if (commits.Count == 0)
                    {
                        hasMore = false;
                    }
                    else
                    {
                        page++;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error when fetching commits: {ex.Message}");
                }

                yield return (isSuccessful, commits);

                if (!isSuccessful)
                {
                    yield break;
                }
            }
        }


    }
}
