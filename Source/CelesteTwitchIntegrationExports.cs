using MonoMod.ModInterop;

namespace Celeste.Mod.CelesteTwitchIntegration
{
    /// <summary>
    /// Provides export functions for other mods to import.
    /// If you do not need to export any functions, delete this class and the corresponding call
    /// to ModInterop() in <see cref="CelesteTwitchIntegrationModule.Load"/>
    /// </summary>
    [ModExportName("CelesteTwitchIntegration")]
    public static class CelesteTwitchIntegrationExports
    {
        // TODO: add your mod's exports, if required
    }
}