using Godot;

using Jmo.AI.Perception;

namespace Jmo.AI.Navigation;
/// <summary>
/// An immutable, high-performance struct that provides a snapshot of all necessary world
/// and agent state for a single decision-making frame. It is created by the AIAgent
/// and passed to the AISteeringProcessor to ensure all considerations operate on the
/// same consistent data set.
/// </summary>
public readonly struct DecisionContext
{
    public readonly AIPerceptionManager Memory;
    public readonly Vector3 AgentPosition;
    public readonly Vector3 AgentForwardVector;
    public readonly Vector3 AgentVelocity;
    public readonly Vector3 HighLevelTarget; // The goal provided by the Behavior Tree

    public DecisionContext(AIPerceptionManager memory, Vector3 agentPosition, Vector3 agentForward, Vector3 agentVelocity, Vector3 highLevelTarget)
    {
        Memory = memory;
        AgentPosition = agentPosition;
        AgentForwardVector = agentForward;
        AgentVelocity = agentVelocity;
        HighLevelTarget = highLevelTarget;
    }
}