using Application.Services;
using Domain.Repositories;
using Infrastructure.Services;
using Inftrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace GitHubFetchConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            var token = config["Github:Token"];

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Please fill in the token in app settings. See ya right after!");
                return;
            }

            //DI
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddHttpClient<IGitHubService, GitHubService>(client =>
            {
                client.BaseAddress = new Uri("https://api.github.com");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitHubFetchConsoleApp", "1.0"));
            });
            serviceCollection.AddDbContext<SqliteDbContext>(options => options.UseSqlite("Data Source=commits.db"));
            serviceCollection.AddScoped<ICommitRepository, CommitRepository>();

            if (args.Length != 2)
            {
                Console.WriteLine("Incorrect parameter's number. Please fill in the owner (first param) and the repo (second param) as an app params and try again.");
                return;
            }

            string owner = args[0];
            string repo = args[1];

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();
            var commitRepository = serviceProvider.GetRequiredService<ICommitRepository>();

            try
            {
                var repoInfo = await gitHubService.GetRepositoryInfoAsync(owner, repo);

                //Checks
                if (!repoInfo.Authorized)
                {
                    Console.WriteLine($"\nYou're not authorized to make a calls. Fill in the correct token in app settings and try again.");
                    return;
                }
                else if (!repoInfo.RepoExists)
                {
                    Console.WriteLine($"\nGiven repository {owner}/{repo} does not exist. Correct params and try again.");
                    return;
                }

                //Simple control of the commit count to prevent long running tasks and huge amounts of data (here more than 10 000). It can be extended
                if (repoInfo.CommitCount > 10000)
                {
                    Console.WriteLine($"\nYou're about to fetch about {repoInfo.CommitCount} commits. Do you want to continue (type 'y')?");

                    ConsoleKeyInfo cki = Console.ReadKey();
                    if (cki.Key.ToString() != "Y")
                        return;
                }


                //App logic
                Console.WriteLine($"\nFetching commits for {owner}/{repo}...");

                await foreach (var (isSuccessful, commits) in gitHubService.FetchCommitsAsync(owner, repo))
                {
                    if (!isSuccessful)
                    {
                        Console.WriteLine($"\n Limit of the calls to GitHub API has been reached and not all the commits have been fetched. Try again after one hour.");
                        return;
                    }

                    foreach (var commit in commits)
                    {
                        Console.WriteLine($"[{owner}/{repo}]/[{commit.Sha}]: {commit.Commit?.Message} [{commit.Commit?.Committer.Name}/{commit.Commit?.Committer.Email}]");
                    }

                    var commitsToInsert = await commitRepository.ReturnNonExistingCommitsAsync(commits);
                    
                    if (commitsToInsert.Count > 0)
                        await commitRepository.SaveCommitsAsync(owner, repo, commitsToInsert);
                }

                Console.WriteLine($"\nThat's all, folks!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured: {ex.Message}");
            }
        }
    }
}
