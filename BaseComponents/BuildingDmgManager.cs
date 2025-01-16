using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class BuildingDmgManager : Node
{
    #region COMPONENT_VARIABLES
    [Export]
    public int MinimumFloorsToCollapse { get; private set; } = 0;
    [Export(PropertyHint.Range, "0,100,0.1")]
    public float PercentageDamageToCollapse { get; private set; } = 100f;
    [Export] //TODO: TICK FASTER AS LESS HEALTH
    private float _collapseTickInterval = 1.0f;
    [Export(PropertyHint.Range, "0,100,0.1")]
    private float _collapseTickDamagePercentage = 1.0f;
    //[Export]
    private float _maxBuildingHealth = 0f;
    private float _currentBuildingHealth = 0f;
    private int _floorsDestroyed = 0;

    [Export]
    private HurtboxComponent3D _hurtboxComp;
    [Export]
    private Node3D _structure;
    
    private Timer _collapseTicker;

    private int _numFloors;
    private List<BuildingFloorComponent> _sortedFloors = new List<BuildingFloorComponent>();
    private Dictionary<int, BuildingFloorComponent> _floors = new Dictionary<int, BuildingFloorComponent>();

    public bool IsBuildingCollapsing { get; private set; } = false;
    public bool IsBuildingDestroyed { get; private set; } = false;
    public event EventHandler BuildingCollapse;
    public event EventHandler BuildingDestroyed;
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
    {
        base._Ready();
        //convert to percentages
        PercentageDamageToCollapse = PercentageDamageToCollapse / 100f; 
        _collapseTickDamagePercentage = _collapseTickDamagePercentage / 100f;

        _collapseTicker = GetNode<Timer>("CollapseTicker");
        _collapseTicker.Timeout += OnCollapseTick;

        _hurtboxComp.HitboxEntered += OnHitboxEntered;

        var floors = _structure.GetChildrenOfType<BuildingFloorComponent>().ToList();
        _numFloors = floors.Count;
        GD.Print("num floors: ", _numFloors);

        // Step 1: Sort the heights in ascending order
        _sortedFloors = floors.OrderBy(f => f.YCenter).ToList();


        CallDeferred(MethodName.InitializeFloorHealthConnections);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Engine.IsEditorHint())
        {
            MinimumFloorsToCollapse = Mathf.Clamp(MinimumFloorsToCollapse, 0, _numFloors);
        }
    }
    #endregion
    #region SIGNAL_LISTENERS
    private void OnHitboxEntered(HitboxComponent3D hitbox)
    {
        var attackDamage = hitbox.CurrentAttack.Damage;
        var attackPOC = hitbox.CollisionShape.GlobalPosition;
        var attackHeight = attackPOC.Y;
        var numFloorsEffected = hitbox.CurrentAttack.BuildingEffect.FloorsEffected;

        int closestFloorNum = 1;
        BuildingFloorComponent closestFloor = _floors[1];
        float closestYDist = float.MaxValue;
        foreach (var floorPair in _floors)
        {
            var floor = floorPair.Value;
            var attackHeightDist = Mathf.Abs(attackHeight - floor.YCenter);
            if (attackHeightDist < closestYDist)
            {
                closestYDist = attackHeightDist;
                closestFloorNum = floorPair.Key;
                closestFloor = floor;
            }
        }
        GD.Print("determined closest floor number: ", closestFloorNum, ", named: ", closestFloor.Name);
        List<BuildingFloorComponent> floorsAffected = new List<BuildingFloorComponent>
        { closestFloor };

        var highFloor = closestFloorNum;
        var lowFloor = closestFloorNum;
        while (numFloorsEffected > 1)
        {
            highFloor++;
            lowFloor--;
            if (_floors.ContainsKey(highFloor))
            {
                floorsAffected.Add(_floors[highFloor]);
            }
            if (_floors.ContainsKey(lowFloor))
            {
                floorsAffected.Add(_floors[lowFloor]);
            }
            numFloorsEffected--;
        }
        foreach (var floor in floorsAffected)
        {
            GD.Print("DAMAGING FLOOR: ", floor.Name);
            floor.HealthComp.DamageWithAttack(hitbox.CurrentAttack);
        }
    }
    private void OnFloorDamaged(HealthUpdate update)
    {
        _currentBuildingHealth += update.HealthChange;
        CheckCollapseStatus();
        GD.Print($"building health at {_currentBuildingHealth} out of {_maxBuildingHealth}");
    }

    private void OnFloorDestroyed(BuildingFloorComponent floorComp, int floorNum, HealthUpdate destroyUpdate)
    {
        _floorsDestroyed++;
        CheckCollapseStatus();
        CheckDestroyStatus();


        //HANDLE SPILLOVER
        var spillover = _hurtboxComp.LatestAttack.BuildingEffect.SpilloverRate;
        var origDamage = -destroyUpdate.HealthChange;
        var spilloverDamage = spillover * origDamage;
        if (spillover == 0f) { return; }
        if (_floors.ContainsKey(floorNum + 1))
        {
            _floors[floorNum + 1].HealthComp.Damage(spilloverDamage);
        }
        if (_floors.ContainsKey(floorNum - 1))
        {
            _floors[floorNum - 1].HealthComp.Damage(spilloverDamage);
        }
    }
    private void OnCollapseTick()
    {
        foreach (var floor in _floors.Values)
        {
            if (floor.HealthComp.IsDead) { return; }

            floor.HealthComp.Damage(_maxBuildingHealth * _collapseTickDamagePercentage);
        }
    }
    #endregion
    #region COMPONENT_HELPER
    private void InitializeFloorHealthConnections()
    {
        _maxBuildingHealth = 0f; _currentBuildingHealth = 0f;
        for (int i = 1; i <= _numFloors; i++) // i = floor number
        {
            var floor = _sortedFloors[i - 1];
            _floors.Add(i, floor);
            _maxBuildingHealth += floor.HealthComp.MaxHealth;
            _currentBuildingHealth += floor.HealthComp.Health;
            floor.HealthComp.Destroyed += (update) => OnFloorDestroyed(floor, i, update);
            floor.HealthComp.HealthChanged += OnFloorDamaged;
        }
    }
    private void CheckCollapseStatus()
    {
        if (IsBuildingCollapsing) { return; }
        var collapseHealth = _maxBuildingHealth * (1 - PercentageDamageToCollapse);

        GD.Print($"curr building health: {_currentBuildingHealth}, " +
            $"\nhealth to collapse: {collapseHealth}");
        if (_currentBuildingHealth <= collapseHealth)
        {
            if (_floorsDestroyed < MinimumFloorsToCollapse)
            {
                return;
            }
            _collapseTicker.Start();
            IsBuildingCollapsing = true;
            BuildingCollapse?.Invoke(this, EventArgs.Empty);
            GD.Print("Building Starting Collapse!!");
        }
    }
    private void CheckDestroyStatus()
    {
        if (IsBuildingDestroyed) { return; }
        if (_currentBuildingHealth <= 0f)
        {
            IsBuildingDestroyed = true;
            BuildingDestroyed?.Invoke(this, EventArgs.Empty);
            GD.Print("Building Starting Destruction!!");
            BuildingDestructionEffect();
        }
    }
    private void BuildingDestructionEffect()
    {
        _hurtboxComp.DeactivateHurtbox();

        int numShakePoses = Global.Rnd.Next(4, 8);
        float timeToCollapse = Global.GetRndInRange(2.0f, 5.0f);

        var shakePoses = new List<Vector3>();
        for (int i = 0; i < numShakePoses; i++)
        {
            var shakePos = new Vector3(Global.GetRndInRange(-0.5f, 0.5f), 0f, Global.GetRndInRange(-0.5f, 0.5f));
            shakePoses.Add(_structure.GlobalPosition + shakePos);
        }
        var destroyTween = GetTree().CreateTween();
        destroyTween.TweenProperty(_structure, "position:y",
                _structure.Position.Y - 5.0f, timeToCollapse).SetEase(Tween.EaseType.InOut);
        var shakeTween = GetTree().CreateTween();
        while (timeToCollapse > 0f)
        {
            foreach (var pos in shakePoses)
            {
                shakeTween.TweenProperty(_structure, "position:x",
                    pos.X, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
                shakeTween.Parallel().TweenProperty(_structure, "position:z",
                    pos.Z, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
                timeToCollapse -= 0.2f;
            }
        }
        destroyTween.TweenCallback(Callable.From(QueueFree));
    }
    #endregion
}
