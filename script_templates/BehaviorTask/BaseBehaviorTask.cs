using _BINDINGS_NAMESPACE_;
using System.Collections.Generic;
using System.Linq;

// meta-name: Base Behavior Task
// meta-description: Basic Behavior Task Template
// meta-default: true
// meta-space-indent: 4
[Tool]
public partial class _CLASS_ : _BASE_
{
    //TEMPLATE FOR BEHAVIOR TASKS
    #region TASK_VARIABLES
    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
    }
    #endregion
    #region TASK_HELPER
    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        //

        return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
    }
    #endregion
}
