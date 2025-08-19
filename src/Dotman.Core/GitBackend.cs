using System.Diagnostics;
using System.Text;

namespace Dotman.Core;

public class GitBackend : IVCSBackend
{
    private readonly string _repositoryPath;

    public GitBackend(string repositoryPath)
    {
        _repositoryPath = repositoryPath;
    }

    private async Task<(int ExitCode, string Output, string Error)> RunGitCommandAsync(params string[] args)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git", // Assumes git is in the user's PATH
            ArgumentList = { "-C", _repositoryPath },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in args)
        {
            processStartInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = processStartInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return (process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
    }

    public async Task InitAsync()
    {
        var (exitCode, _, error) = await RunGitCommandAsync("init");
        if (exitCode != 0)
        {
            throw new Exception($"Git init failed: {error}");
        }
    }

    public async Task CommitAsync(string message)
    {
        var (addExitCode, _, addError) = await RunGitCommandAsync("add", ".");
        if (addExitCode != 0)
        {
            throw new Exception($"Git add failed: {addError}");
        }

        var (commitExitCode, _, commitError) = await RunGitCommandAsync("commit", "-m", message);
        if (commitExitCode != 0)
        {
            // Exit code 1 from commit can mean "nothing to commit", which is not an error for us.
            if (!commitError.Contains("nothing to commit"))
            {
                throw new Exception($"Git commit failed: {commitError}");
            }
        }
    }

    public async Task PullAsync()
    {
        var (exitCode, _, error) = await RunGitCommandAsync("pull", "--rebase");
        if (exitCode != 0)
        {
            throw new Exception($"Git pull failed: {error}");
        }
    }

    public async Task PushAsync()
    {
        var (exitCode, _, error) = await RunGitCommandAsync("push");
        if (exitCode != 0)
        {
            throw new Exception($"Git push failed: {error}");
        }
    }

    public async Task<string> StatusAsync()
    {
        var (exitCode, output, error) = await RunGitCommandAsync("status", "--porcelain");
        if (exitCode != 0)
        {
            throw new Exception($"Git status failed: {error}");
        }
        return output;
    }

    public async Task CloneAsync(string repoUrl, string localPath)
    {
        // We can't use RunGitCommandAsync here because it requires the repository to exist.
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        processStartInfo.ArgumentList.Add("clone");
        processStartInfo.ArgumentList.Add(repoUrl);
        processStartInfo.ArgumentList.Add(localPath);


        using var process = new Process { StartInfo = processStartInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Git clone failed: {errorBuilder.ToString()}");
        }
    }
}
