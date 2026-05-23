using System.Runtime.Loader;
using Fantasy.Helper;

namespace Fantasy;

public static class AssemblyHelper
{
    private const string HotfixDll = "Hotfix";
    private static AssemblyLoadContext? assemblyLoadContext;

    public static void Initialize()
    {
        typeof(AssemblyHelper).Assembly.EnsureLoaded();
        LoadHotfixAssembly();
    }

    public static System.Reflection.Assembly LoadHotfixAssembly()
    {
        assemblyLoadContext?.Unload();
        GC.Collect();

        assemblyLoadContext = new AssemblyLoadContext(HotfixDll, true);
        var dllBytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, $"{HotfixDll}.dll"));
        var pdbPath = Path.Combine(AppContext.BaseDirectory, $"{HotfixDll}.pdb");
        var assembly = File.Exists(pdbPath)
            ? assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(File.ReadAllBytes(pdbPath)))
            : assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes));
        assembly.EnsureLoaded();
        return assembly;
    }
}
