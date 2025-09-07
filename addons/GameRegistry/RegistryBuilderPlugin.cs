#if TOOLS
using Godot;
using GodotPlugins;

[Tool]
public partial class RegistryBuilderPlugin : EditorBuildPlugin
{
    public override string _GetPluginName() => "RegistryBuilder";

    public override void _Build()
    {
        GD.Print("RegistryBuilderPlugin: Running pre-build steps...");

        // Define the path to your registry resource
        const string registryPath = "res://Core/game_registry.tres";

        if (!ResourceLoader.Exists(registryPath))
        {
            GD.PrintErr($"RegistryBuilderPlugin: GameRegistry not found at '{registryPath}'. Aborting build hook.");
            return;
        }

        var gameRegistry = ResourceLoader.Load<Jmo.Core.GameRegistry>(registryPath);
        if (gameRegistry == null)
        {
            GD.PrintErr($"RegistryBuilderPlugin: Failed to load GameRegistry from '{registryPath}'.");
            return;
        }

        // Run the same logic as our button
        gameRegistry.RebuildRegistry();

        // IMPORTANT: Save the modified resource back to disk so the exporter packs the new version
        var error = ResourceSaver.Save(gameRegistry, registryPath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"RegistryBuilderPlugin: Failed to save updated GameRegistry! Build may contain stale data. Error: {error}");
        }
        else
        {
            GD.Print("RegistryBuilderPlugin: Successfully rebuilt and saved GameRegistry.");
        }
    }
}
#endif