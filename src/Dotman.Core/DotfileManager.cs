namespace Dotman.Core;

public class DotfileManager
{
    private readonly IConfigManager _configManager;
    private readonly IVCSBackend _vcsBackend;
    private readonly ITrackingModule _trackingModule;
    private readonly ISyncModule _syncModule;

    public DotfileManager(
        IConfigManager configManager,
        IVCSBackend vcsBackend,
        ITrackingModule trackingModule,
        ISyncModule syncModule)
    {
        _configManager = configManager;
        _vcsBackend = vcsBackend;
        _trackingModule = trackingModule;
        _syncModule = syncModule;
    }

    // Methods to orchestrate the modules will be added here later.
}
