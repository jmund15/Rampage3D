// --- UtilitySelector.cs (FULLY REFACTORED) ---
using Godot;
using System.Linq;
using JmoAI.UtilityAI; // You will remove this namespace if you delete UtilityContext

public enum TieBreaker
{
    HighestPriority, // Child with higher priority wins
    FirstInList,     // The one that appears first in the scene tree wins
    Random           // Choose randomly among the highest-scoring children
}

[GlobalClass, Tool]
public partial class UtilitySelector : CompositeTask
{
    #region TASK_VARIABLES
    [Export]
    private float _reassessmentInterval = 0.5f; // How often to re-evaluate the best action. 0 = every frame.
    [Export]
    private TieBreaker _tieBreaker = TieBreaker.HighestPriority;

    private IUtilityTask _currentUtilityTask;
    private float _reassessmentTimer;
    #endregion

    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        TaskName += "_UtilitySelector";

        // Validate that children are the correct type during initialization.
        foreach (var childTask in ChildTasks)
        {
            if (childTask is not IUtilityTask)
            {
                GD.PrintErr($"UtilitySelector ERROR: Child task '{childTask.Name}' does not implement IUtilityTask and will be ignored.");
            }
        }
    }

    public override void Enter()
    {
        base.Enter(); // IMPORTANT: This runs the conditions on this node.
        _reassessmentTimer = 0f; // Assess immediately on first entry.
        SelectBestAction();
    }

    // TODO: CHECK THIS AND FAILURE CALLS WithIN CLASS
    public override void Exit()
	{
		base.Exit();
	}

    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);

        // Don't do anything if the running task is not interruptible.
        if (_currentUtilityTask == null || !_currentUtilityTask.Interruptible)
        {
            return;
        }

        _reassessmentTimer -= (float)delta;
        if (_reassessmentTimer <= 0f)
        {
            _reassessmentTimer = _reassessmentInterval;
            SelectBestAction();
        }
    }
    #endregion

    #region TASK_HELPER
    protected virtual void SelectBestAction()
    {
        var validTasks = ChildTasks.OfType<IUtilityTask>().ToList();
        if (validTasks.Count == 0)
        {
            GD.PrintErr($"{Name}: No valid children implementing IUtilityTask found.");
            Status = TaskStatus.FAILURE;
            return;
        }

        // --- Step 1: Find the highest utility score among all children. ---
        float maxScore = -1.0f;
        foreach (var task in validTasks)
        {
            // Pass the Blackboard directly to the consideration!
            float score = task.Consideration.Evaluate(BB);
            if (score > maxScore)
            {
                maxScore = score;
            }
        }
        
        // If no action had a score above 0, it means nothing is desirable.
        if (maxScore <= 0)
        {
            // Optional: You might want to run a "default" action or just fail.
            // For now, we fail if no action is viable.
            Status = TaskStatus.FAILURE;
            RunningChild.IfValid()?.Exit();
            RunningChild = null;
            _currentUtilityTask = null;
            return;
        }

        // --- Step 2: Get all tasks that share the highest score. ---
        var topTasks = validTasks.Where(t => t.Consideration.Evaluate(BB) >= maxScore).ToList();
        
        // --- Step 3: Use the tie-breaker to select the single best action from the top contenders. ---
        IUtilityTask bestAction;
        if (topTasks.Count == 1)
        {
            bestAction = topTasks[0];
        }
        else
        {
            switch (_tieBreaker)
            {
                case TieBreaker.HighestPriority:
                    bestAction = topTasks.OrderByDescending(t => t.Priority).First();
                    break;
                case TieBreaker.Random:
                    bestAction = topTasks[Global.Rnd.Next(0, topTasks.Count)];
                    break;
                case TieBreaker.FirstInList:
                default:
                    bestAction = topTasks[0];
                    break;
            }
        }

        // --- Step 4: Switch to the best action only if it's not already the running one. ---
        if (_currentUtilityTask != bestAction)
        {
            RunningChild.IfValid()?.Exit(); // Exit the old task.
            
            _currentUtilityTask = bestAction;
            RunningChild = bestAction as BehaviorTask;
            
            RunningChild.Enter(); // Enter the new task.
        }
    }

    // TODO: CHOOSE DESIRED BEHAVIOR
    protected override void OnRunningChildStatusChange(TaskStatus newStatus)
    {
        base.OnRunningChildStatusChange(newStatus);
        // Let the state of the selector mirror the state of its running child.
        // If the child succeeds or fails, the selector does too.
        Status = newStatus;

        // Optional: if a child fails, you might want to immediately re-select a new best action
        // instead of letting the selector itself fail.
        if (newStatus == TaskStatus.FAILURE)
        {
             SelectBestAction();
        }
    }
    #endregion
}