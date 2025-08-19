namespace Dotman.Core;

public class SyncModule : ISyncModule
{
    private readonly IVCSBackend _vcsBackend;

    public SyncModule(IVCSBackend vcsBackend)
    {
        _vcsBackend = vcsBackend;
    }

    public async Task SyncAsync()
    {
        // In a more advanced implementation, we might check for local uncommitted changes first.
        // For now, we follow the simple pull-then-push model.
        await _vcsBackend.PullAsync();
        await _vcsBackend.PushAsync();
    }

    public async Task<string> GetRemoteStatusAsync()
    {
        // This currently returns the local working tree status.
        // A full implementation would require fetching from the remote and comparing.
        return await _vcsBackend.StatusAsync();
    }
}
