using Godot;
using Jmo.Core;
using Jmo.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Jmo.AI.Perception
{
    /// <summary>
    /// The central hub of the perception system, acting as the AI's short-term memory. It collects
    /// stateless `Percept` events from all registered sensors and manages a stateful collection of
    /// `PerceptionInfo` records. It provides the AI Brain with a clean, processed, and efficiently
    /// queryable view of the game world.
    /// </summary>
    [GlobalClass]
    public partial class AIPerceptionManager : Node, IGodotNodeInterface
    {
        /// <summary>A list of all Nodes in the scene that implement the IAISensor interface. Assign in the Godot Editor.</summary>
        [Export] private Godot.Collections.Array<Node> _sensors = new();

        private readonly Dictionary<Node3D, PerceptionInfo> _memoryByTarget = new();
        private readonly Dictionary<Category, HashSet<PerceptionInfo>> _memoryByCategory = new();

        public override void _Ready()
        {
            foreach (var node in _sensors) if (node is IAISensor sensor) sensor.PerceptUpdated += OnPerceptUpdated;
        }

        private void OnPerceptUpdated(object sender, PerceptEventArgs args)
        {
            var percept = args.Percept;
            if (percept.Target == null || percept.Identity == null) return;

            if (_memoryByTarget.TryGetValue(percept.Target, out PerceptionInfo info))
            {
                RemoveFromCategoryCache(info);
                info.Update(percept);
                AddToCategoryCache(info);
            }
            else
            {
                var newInfo = new PerceptionInfo(percept);
                _memoryByTarget.Add(percept.Target, newInfo);
                AddToCategoryCache(newInfo);
            }
        }

        private void AddToCategoryCache(PerceptionInfo info)
        {
            if (info.Identity?.Categories == null) return;
            foreach (var category in info.Identity.Categories)
            {
                if (category == null) continue;
                if (!_memoryByCategory.TryGetValue(category, out var set))
                {
                    set = new HashSet<PerceptionInfo>();
                    _memoryByCategory.Add(category, set);
                }
                set.Add(info);
            }
        }

        private void RemoveFromCategoryCache(PerceptionInfo info)
        {
            if (info.Identity?.Categories == null) return;
            foreach (var category in info.Identity.Categories)
            {
                if (category == null) continue;
                if (_memoryByCategory.TryGetValue(category, out var set)) set.Remove(info);
            }
        }

        /// <summary>This method should be connected to a Timer's timeout signal for periodic, performant cleanup of expired memories.</summary>
        private void OnCleanupTimerTimeout()
        {
            var forgottenKeys = _memoryByTarget.Where(kvp => !kvp.Value.IsActive).Select(kvp => kvp.Key).ToList();
            foreach (var key in forgottenKeys)
            {
                if (!_memoryByTarget.TryGetValue(key, out var info)) continue;
                RemoveFromCategoryCache(info);
                _memoryByTarget.Remove(key);
            }
        }

        #region Public API for AI Brain & Other Systems

        /// <summary>Tries to retrieve the memory record for a specific target.</summary>
        public bool TryGetMemoryOf(Node3D target, out PerceptionInfo info) => _memoryByTarget.TryGetValue(target, out info) && info.IsActive;

        /// <summary>Returns an enumerable collection of all currently active memory records.</summary>
        public IEnumerable<PerceptionInfo> GetAllActiveMemories() => _memoryByTarget.Values.Where(info => info.IsActive);

        /// <summary>Finds the most prominent (highest confidence) active memory belonging to a specific category. This is a highly performant query.</summary>
        public PerceptionInfo GetBestMemoryForCategory(Category category)
        {
            if (category == null || !_memoryByCategory.TryGetValue(category, out var memorySet)) return null;

            PerceptionInfo bestMatch = null;
            float maxConfidence = -1.0f;

            foreach (var info in memorySet)
                if (info.IsActive && info.CurrentConfidence > maxConfidence)
                { maxConfidence = info.CurrentConfidence; bestMatch = info; }
            return bestMatch;
        }

        public Node GetInterfaceNode() => this;
        #endregion
    }
}