using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BuildingDmgManager : Node
{
    #region COMPONENT_VARIABLES
    [Export]
    private HurtboxComponent3D _hurtboxComp;
    [Export]
    private Node3D _structure;

    private int _numFloors;
    private Dictionary<int, BuildingFloorComponent> _floors = new Dictionary<int, BuildingFloorComponent>();
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
    {
        base._Ready();
        _hurtboxComp.HitboxEntered += OnHitboxEntered;

        var floors = _structure.GetChildrenOfType<BuildingFloorComponent>().ToList();

        // Step 1: Sort the heights in ascending order
        var sortedHeights = floors.OrderBy(f => f.YCenter).ToList();

        _numFloors = _floors.Count;
        for (int i = 1; i <= _numFloors; i++) // i = floor number
        {
            var floor = sortedHeights[i - 1];
            _floors.Add(i, floor);
            floor.HealthComp.Destroyed += (update) => OnFloorDestroyed(floor, i, update);
        }
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
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
        List<BuildingFloorComponent> floorsEffected = new List<BuildingFloorComponent>
        { closestFloor };

        var highFloor = closestFloorNum;
        var lowFloor = closestFloorNum;
        while (numFloorsEffected > 1)
        {
            highFloor++;
            lowFloor--;
            if (_floors.ContainsKey(highFloor))
            {
                floorsEffected.Add(_floors[highFloor]);
            }
            if (_floors.ContainsKey(lowFloor))
            {
                floorsEffected.Add(_floors[lowFloor]);
            }
            numFloorsEffected--;
        }
        foreach (var floor in floorsEffected)
        {
            floor.HealthComp.DamageWithAttack(hitbox.CurrentAttack);
        }
    }
    private void OnFloorDestroyed(BuildingFloorComponent floorComp, int floorNum, HealthUpdate destroyUpdate)
    {
        //Handle building destruction if needed

        //HANDLE SPILLOVER once no destruction is confirmed
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
    #endregion
    #region COMPONENT_HELPER
    #endregion
}
