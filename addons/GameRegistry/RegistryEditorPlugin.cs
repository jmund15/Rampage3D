// GameRegistryEditorPlugin.cs (place in an `addons` folder)
#if TOOLS
using Godot;

[Tool]
public partial class RegistryEditorPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        // Add a custom type to the Inspector sidebar
        var script = GD.Load<Script>("res://Global/GameRegistry.cs");
        AddCustomType("GameRegistry", "Resource", script, null);
    }

    public override void _ExitTree()
    {
        RemoveCustomType("GameRegistry");
    }

    public override bool _Handles(GodotObject @object)
    {
        return @object is Jmo.Core.GameRegistry;
    }

    public override void _MakeVisible(bool visible)
    {
        if (!visible) return;
        var registry = (Jmo.Core.GameRegistry)GetEditedObject();
        var rebuildButton = new Button { Text = "Rebuild Registry from Collections" };
        rebuildButton.Pressed += () =>
        {
            registry.RebuildRegistry();
            // This is a bit of a hack to force the inspector to refresh and show the new data
            // In a real plugin, you might need more robust update logic.
            GetEditedObject().NotifyPropertyListChanged();
        };

        AddCustomControl(rebuildButton);
    }
}
#endif