# GitHubFetch

GitHubFetch is a lightweight application designed to fetch commit data from a specified GitHub repository and store it in a SQLite database.

---

## Prerequisites

- .NET Core SDK (version 8.0 or higher)
- A [GitHub Personal Access Token](https://github.com/settings/tokens) with `repo` scope permissions to access private repositories.

## Configuration

To authenticate with GitHub's API, you must provide your **Personal Access Token** in the `appsettings.json` file. When generating the token make sure it has a "repo:status" scope picked.  

Update the configuration file as follows:

```json
{
  "Github": {
    "Token": "your_personal_access_token_here"
  }
}
```

## How to Run

After cloning and compiling the repository, run the command like this:
```bash
GitHubFetch.exe <owner> <repo>
```

## Output

The fetched commits will be stored in a SQLite database named ```commits.db```.
