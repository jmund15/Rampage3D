// --- SquadManager.cs ---
using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class SquadManager : Node
{
    // These tags can be defined by a designer for this specific squad type.
    [Export] private StringName _highThreatTag = "Squad_Panic";
    [Export] private StringName _regroupTag = "Squad_Regroup";
    [Export] private StringName _attackTag = "Squad_Attack";
    [Export(PropertyHint.Range, "0.0, 1.0, 0.05")] private float _panicHealthThreshold = 0.25f;

    private Blackboard _squadBlackboard; 
    private List<IBlackboard> _memberBlackboards = new();

    public override void _Ready()
    {
        _squadBlackboard = GetNode<Blackboard>("SquadBlackboard");
        // ... timer setup, etc. ...
    }

    private void OnMemberAdded(Node node)
    {
        var bb = node.GetFirstChildOfInterface<IBlackboard>();
        if (bb != null)
        {
            _memberBlackboards.Add(bb);
            bb.SetParent(_squadBlackboard); // Use the generic SetParent method.
        }
    }
    
    // Periodically update the squad's shared state.
    private void UpdateSquadBlackboard()
    {
        if (_memberBlackboards.Count == 0) return;

        float averageHealth = CalculateAverageHealth();
        _squadBlackboard.SetPrimVar(BBDataSig.SquadAverageHealth, averageHealth);

        // This is now the "brain" logic for this squad type.
        // It sets a boolean tag on the shared blackboard.
        // First, clear old tags to prevent conflicts.
        _squadBlackboard.SetPrimVar(BBDataSig.HasSquadTag, false); // A generic reset

        if (averageHealth < _panicHealthThreshold)
        {
            _squadBlackboard.SetVar(BBDataSig.ActiveSquadTag, _highThreatTag);
        }
        else
        {
            _squadBlackboard.SetVar(BBDataSig.ActiveSquadTag, _attackTag);
        }
    }

    private float CalculateAverageHealth() { /* ... implementation ... */ return 0.5f; }
}

// In BBDataSig enum, we replace the SquadOrder enum with a generic tag system.
//public enum BBDataSig 
//{
//    // ...
//    ActiveSquadTag, // Holds the current StringName tag for the squad
//    HasSquadTag,    // Simple bool flag
//    // ...
//}