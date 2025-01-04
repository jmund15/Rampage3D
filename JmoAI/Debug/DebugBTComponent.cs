using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[GlobalClass, Tool]
public partial class DebugBTComponent : Tree
{
	[Export]
	private BehaviorTree _bt;
	[Export]
	private Vector2 _size = new Vector2(350, 648);

    private Color _baseBgColor = new Color(Colors.Black, 0.05f);
	private float _bgAlpha = 0.25f;

	private Dictionary<BehaviorTask, TreeItem> _btMap = new Dictionary<BehaviorTask, TreeItem>(); 
	private Dictionary<BehaviorTask, float> _taskRunTime = new Dictionary<BehaviorTask, float>();

	private List<BehaviorTask> _runningTasks = new List<BehaviorTask>();

	private bool _resettingTree = false;
	//private Tree _tree;
	public override void _Ready()
	{
		SetColumnExpand(0, true);
		SetColumnExpandRatio(0, 1);
        SetColumnExpand(1, true);
        SetColumnExpandRatio(1, 0);
		SetColumnCustomMinimumWidth(1, 50);

		if (!_bt.IsValid())
		{
			GD.PrintErr("ERROR | BLACKBOARD IS NOT VALID!");
			//error?
			return;
        }
        _bt.TreeInitialized += OnTreeInitialized;
        _bt.TreeFinishedLoop += OnBehaviorTreeReset;
    }
    public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			Size = _size;
			return;
        }
		foreach (var runningTask in _runningTasks)
		{
			_taskRunTime[runningTask] += (float)delta;
			_btMap[runningTask].SetText(1, _taskRunTime[runningTask].ToString("n2"));
		}
		if (_resettingTree)
		{
			_resettingTree = false;
        }
    }
    private void OnTreeInitialized()
    {
        GD.Print("behavior tree name: ", _bt.TreeName);
        Name = _bt.TreeName;
        var rootTask = _bt.RootTask;
        TreeItem root = CreateItem();
        HideRoot = false;
        root.SetText(0, rootTask.TaskName);
        _btMap.Add(rootTask, root);
        _taskRunTime.Add(rootTask, 0.0f);
		_runningTasks.Add(rootTask);
        //root.SetCustomBgColor(1, new Color(Colors.Yellow, _bgAlpha));

        CreateBranchesRecursive(root, rootTask);
		ResetTree();
    }
    public void CreateBranchesRecursive(TreeItem item, BehaviorTask task)
	{
		foreach (var subTask in task.GetChildrenOfType<BehaviorTask>(false))
		{
			var branch = CreateItem(item);
			branch.SetText(0, subTask.TaskName);
			branch.SetTextOverrunBehavior(0, TextServer.OverrunBehavior.TrimWordEllipsis);
			branch.SetTooltipText(0, subTask.TaskName);
            GD.Print($"created item for task {subTask.TaskName}.");
            if (subTask.GetChildrenOfType<BehaviorTask>(false).Count > 0)
			{
                // Will continue to create branches as long as tasks have task children
                CreateBranchesRecursive(branch, subTask);
			}
			_btMap.Add(subTask, branch);
			_taskRunTime.Add(subTask, 0.0f);
			if (subTask.Status == TaskStatus.RUNNING)
			{
                _runningTasks.Add(task);
				branch.SetCustomBgColor(1, new Color(Colors.Yellow, _bgAlpha));
			}
            subTask.TaskStatusChanged += (newStatus) => OnTaskStatusChange(subTask, newStatus);
		}
	}

	private void OnTaskStatusChange(BehaviorTask task, TaskStatus newStatus)
	{

		// TODO: ADD A FLASH WHEN A TASK SUCCEEDS/FAILS OF THE TASK ITSELF BEFORE RETURNING TO GREY!!
		FlashItem(_btMap[task], newStatus);


        if (_resettingTree && newStatus != TaskStatus.RUNNING)
		{ // if tree is resetting due to a pass/fail, don't change colors here bc they will immediately reset
            _runningTasks.Remove(task); //remove if it was in running tasks (if it isn't just continues)
            _taskRunTime[task] = 0.0f;
            _btMap[task].SetCustomBgColor(1, _baseBgColor);
            _btMap[task].SetText(1, _taskRunTime[task].ToString("n2"));
            return;
			// TODO: probably want a popup to the right/left of this task saying the tree passed/failed on this task and is resetting
		}
		switch (newStatus)
		{
			case TaskStatus.RUNNING:
				if (!_runningTasks.Contains(task))
				{
					_runningTasks.Add(task);
				}
				_taskRunTime[task] = 0.0f; //reset time bc the task just started running
				_btMap[task].SetCustomBgColor(1, new Color(Colors.Yellow, _bgAlpha));
				//if (task is CompositeTask compositeTask)
				//{
				//	ResetChildren(compositeTask);
				//}
				break;
			case TaskStatus.SUCCESS:
				_runningTasks.Remove(task); //remove if it was in running tasks (if it isn't just continues)
                _btMap[task].SetCustomBgColor(1, new Color(Colors.Green, _bgAlpha));
                break;
			case TaskStatus.FAILURE:
                _runningTasks.Remove(task); //remove if it was in running tasks (if it isn't just continues)
                _btMap[task].SetCustomBgColor(1, new Color(Colors.Red, _bgAlpha));
                break;
		}
	}
    private void OnBehaviorTreeReset(TaskStatus treeFinishStatus)
    {
		//_taskRunTime[_bt.RootTask] = 0.0f;
		ResetTree();
    }
	private void ResetTree()
	{
        foreach (var task in _btMap.Keys)
        {
            if (task.Status != TaskStatus.RUNNING)
            {
                _taskRunTime[task] = 0.0f;
                //_btMap[task].CallDeferred(TreeItem.MethodName.SetCustomBgColor, 1, baseBgColor);
                //_btMap[task].CallDeferred(TreeItem.MethodName.SetText, 1, _taskRunTime[task].ToString("n2"));
                _btMap[task].SetCustomBgColor(1, _baseBgColor);
                _btMap[task].SetText(1, _taskRunTime[task].ToString("n2"));
            }
        }
        _taskRunTime[_bt.RootTask] = 0.0f;
        GetRoot().SetCustomBgColor(1, new Color(Colors.Yellow, _bgAlpha));
        _resettingTree = true;
    }
	private void FlashItem(TreeItem item, TaskStatus taskStatus)
	{
		var flashTween = CreateTween();
		var flashTime = 0.0f;
		Color flashColor = _baseBgColor;

        switch (taskStatus)
        {
            case TaskStatus.RUNNING:
				flashTime = 0.25f;
				flashColor = new Color(Colors.Yellow, _bgAlpha);
                break;
            case TaskStatus.SUCCESS:
                flashTime = 0.75f;
                flashColor = new Color(Colors.Green, _bgAlpha);
                break;
            case TaskStatus.FAILURE:
                flashTime = 0.75f;
                flashColor = new Color(Colors.Red, _bgAlpha);
                break;
        }
        item.SetCustomBgColor(0, flashColor);

        flashTween.TweenMethod(Callable.From(
			(float prog) => InterpolateBackToBaseColor(item, item.GetCustomBgColor(0), prog)),
			0.0f, 1.0f, 2.5f).SetDelay(flashTime);
    }
	private void InterpolateBackToBaseColor(TreeItem item, Color ogColor, float progress)
	{
		item.SetCustomBgColor(0, ogColor.Lerp(_baseBgColor, progress));
	}
	//private void ResetChildren(CompositeTask compTask)
	//{
 //       foreach (var childTask in compTask.ChildTasks)
 //       {
 //           if (childTask.Status != TaskStatus.RUNNING)
 //           {
 //               _taskRunTime[childTask] = 0.0f;
 //               _btMap[childTask].SetCustomBgColor(1, baseBgColor);
 //               _btMap[childTask].SetText(1, _taskRunTime[childTask].ToString("n2"));
 //           }
 //           if (childTask is CompositeTask compositeTask)
 //           {
 //               ResetChildren(compositeTask);
 //           }
 //       }
 //   }
}
