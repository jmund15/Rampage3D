using Godot;

namespace Jmo.Core
{
    /// <summary>
    /// A global singleton (Autoload) that provides convenient, static access to the
    /// project's central GameRegistry resource, which acts as the database for the framework.
    /// </summary>
    public partial class Registry : Node
    {
        /// <summary>A static instance of the singleton for easy access.</summary>
        public static Registry Instance { get; private set; }

        /// <summary>The loaded GameRegistry resource, providing the database API.</summary>
        public static GameRegistry DB { get; private set; }

        [Export(PropertyHint.File, "*.tres")]
        private string _registryResourcePath;

        public override void _EnterTree()
        {
            if (Instance != null) { QueueFree(); return; }
            Instance = this;

            if (string.IsNullOrEmpty(_registryResourcePath)) { GD.PrintErr("Registry requires a path to a GameRegistry resource."); return; }
            DB = GD.Load<GameRegistry>(_registryResourcePath);
            if (DB == null) { GD.PrintErr($"Failed to load GameRegistry from path: {_registryResourcePath}"); }
        }
    }
}