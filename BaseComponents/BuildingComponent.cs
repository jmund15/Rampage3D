using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class BuildingComponent : Node
{
	#region COMPONENT_VARIABLES
	[Export]
	public int MinimumFloorsToCollapse { get; private set; } = 1;
	[Export(PropertyHint.Range, "0,100,0.1")]
	public float PercentageDamageToCollapse { get; private set; } = 100f;
	[Export] //TODO?: TICK FASTER AS LESS HEALTH
	private float _collapseTickInterval = 1.0f;
	[Export(PropertyHint.Range, "0,100,0.1")]
	private float _collapseTickDamagePercentage = 1.0f;
	
	//[Export]
	private float _maxBuildingHealth = 0f;
	private float _currentBuildingHealth = 0f;
	private int _floorsDestroyed = 0;

	public Vector2 XRange { get; private set; } = new Vector2();
	public Vector2 YRange { get; private set; } = new Vector2();
	public Vector2 ZRange { get; private set; } = new Vector2();
	public Vector3 Dimensions { get; private set; } = new Vector3(); // X width, z length, and y height

	[Export]
	private HurtboxComponent3D _hurtboxComp;
	private CollisionShape3D _hurtboxCollShape;
	[Export]
	private Node3D _structure;
	private CollisionShape3D _structureCollShape;

	private Timer _collapseTicker;

	private int _numFloors;
	private List<BuildingFloorComponent> _sortedFloors = new List<BuildingFloorComponent>();
	private Dictionary<int, BuildingFloorComponent> _floors = new Dictionary<int, BuildingFloorComponent>();
	public List<Vector2> XFacePoses { get; private set; } = new List<Vector2>();
	public List<Vector2> ZFacePoses { get; private set; } = new List<Vector2>();

	private AnimatedSprite3D _destructionCenterSmoke;
	private AnimatedSprite3D _destructionDirectionalSmoke;
	private List<AnimatedSprite3D> _smokeSprites = new List<AnimatedSprite3D>();
	public bool IsBuildingCollapsing { get; private set; } = false;
	public bool IsBuildingDestroyed { get; private set; } = false;


	public event EventHandler BuildingCollapse;
	public event EventHandler BuildingDestroyed;
	#endregion
	#region COMPONENT_UPDATES
	public override void _Ready()
	{
		base._Ready();
		if (!Engine.IsEditorHint())
		{
			//convert to percentages
			PercentageDamageToCollapse = PercentageDamageToCollapse / 100f;
			_collapseTickDamagePercentage = _collapseTickDamagePercentage / 100f;
		}

		_collapseTicker = GetNode<Timer>("CollapseTicker");
		_collapseTicker.Timeout += OnCollapseTick;

		_hurtboxComp.HitboxEntered += OnHitboxEntered;

		var floors = _structure.GetChildrenOfType<BuildingFloorComponent>().ToList();
		_numFloors = floors.Count;
		//GD.Print("num floors: ", _numFloors);

		// Step 1: Sort the heights in ascending order
		_sortedFloors = floors.OrderBy(f => f.YCenter).ToList();
		//TODO: Eventually calc these here and propogate down to the floors (since all floors of buildings will have same length/width)
		XFacePoses = _sortedFloors[0].XFacePoses;
		ZFacePoses = _sortedFloors[0].ZFacePoses;

		// Step 2: Set collision shapes of building based on floor meshes
		_structureCollShape = _structure.GetFirstChildOfType<CollisionShape3D>();
		_hurtboxCollShape = _hurtboxComp.GetFirstChildOfType<CollisionShape3D>();
		_structureCollShape.MakeConvexFromSiblings();
		_hurtboxCollShape.Shape = _structureCollShape.Shape;
		//_structureCollShape.Rotate(Vector3.Up, _structure.GlobalRotation.Y - _structure.Rotation.Y);
		var convexShape = _structureCollShape.Shape as ConvexPolygonShape3D;
		float xMin = float.MaxValue; float xMax = float.MinValue;
		float yMin = float.MaxValue; float yMax = float.MinValue;
		float zMin = float.MaxValue; float zMax = float.MinValue;
		
		// Step 3: Using collision shape, calculate building dimensions
		foreach (var p in convexShape.Points)
		{
			// PUT IN GLOBAL COORDS
			var globalP = _structureCollShape.ToGlobal(p);
			//var globalP = p.Rotated(Vector3.Up, _structure.GlobalRotation.Y - _structure.Rotation.Y);
			//var globalP = p;

			//var translatedP = p + (_structure.GlobalPosition - ;
			//var rotatedTransfrom = scaledTransform.Rotated(Vector3.Up, GlobalRotation.Y - Rotation.Y);
			//var translatedTransform = rotatedTransfrom.Translated(GlobalPosition - Position);
			//var globalAabb = translatedTransform * localAabb;


			if (globalP.X < xMin) { xMin = globalP.X; }
			if (globalP.X > xMax) { xMax = globalP.X; }
			if (globalP.Y < yMin) { yMin = globalP.Y; }
			if (globalP.Y > yMax) { yMax = globalP.Y; }
			if (globalP.Z < zMin) { zMin = globalP.Z; }
			if (globalP.Z > zMax) { zMax = globalP.Z; }
		}
		XRange = new Vector2(xMin, xMax);
		YRange = new Vector2(yMin, yMax);
		ZRange = new Vector2(zMin, zMax);
		Dimensions = new Vector3(
			(xMax - xMin) * _structure.Scale.X,
			(yMax - yMin) * _structure.Scale.Y,
			(zMax - zMin) * _structure.Scale.Z
			);
		if (_structure.Name == "Testing" || _structure.Name == "Middle")
		{
			GD.Print(
				$"Building Start Pos: {new Vector3(xMin, yMin, zMin)}" +
				$"\nBuilding Size: {new Vector3(Dimensions.X, Dimensions.Y, Dimensions.Z)}" +
				$"");
		}
		//GD.Print($"Building {_structure.Name}'s dimensions: {Dimensions}.");

		// Step 4: initialize the floors health and calc full building health
		CallDeferred(MethodName.InitializeFloorHealthConnections);
		

		// Step 5: setup destruction smoke
		_destructionCenterSmoke = _structure.GetNode<AnimatedSprite3D>("buildingDestructionSmokeCenter");
		_destructionDirectionalSmoke = _structure.GetNode<AnimatedSprite3D>("buildingDestructionSmokeDirectional");
		CallDeferred(MethodName.InitializeBaseSmoke);
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
		//GD.Print("determined closest floor number: ", closestFloorNum, ", named: ", closestFloor.Name);
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
		if (IsBuildingDestroyed)
		{
			_collapseTicker.Stop();
			return;
		}
		//GD.Print("COLLAPSE TICK! DAMAGE: ", _maxBuildingHealth * _collapseTickDamagePercentage);
		foreach (var floor in _floors.Values)
		{
			if (floor.HealthComp.IsDead) { continue; }

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
			floor.HealthComp.HealthInitialized += (sender, args) =>
			{
				_maxBuildingHealth += floor.HealthComp.MaxHealth;
				_currentBuildingHealth += floor.HealthComp.Health;
			};

			floor.HealthComp.Destroyed += (update) => OnFloorDestroyed(floor, i, update);
			floor.HealthComp.HealthChanged += OnFloorDamaged;

			foreach (var wallCrack in floor.WallCracks)
			{
				wallCrack.RotateY(_structure.GlobalRotation.Y);
			}
		}
	}
	private void InitializeBaseSmoke()
	{
		var smokeOffset = 0.5f;
		_destructionCenterSmoke.Position = new Vector3(
			XRange.Y + smokeOffset + _structure.GlobalPosition.X,
			YRange.X + _structure.GlobalPosition.Y,
			ZRange.Y + smokeOffset + _structure.GlobalPosition.Z
			);
		_destructionCenterSmoke.RotationDegrees = new Vector3(0,45,0);

		//var baseFloor = _floors[1]; // get bottom floor

		var smokeX = _destructionDirectionalSmoke.Duplicate() as AnimatedSprite3D;
		_structure.AddChild(smokeX);
		smokeX.FlipH = true;
		smokeX.RotationDegrees = new Vector3(0, 45, 0);
		smokeX.GlobalPosition = new Vector3(
			XRange.X + smokeOffset + _structure.GlobalPosition.X,
			YRange.X + _structure.GlobalPosition.Y,
			ZRange.Y + smokeOffset + _structure.GlobalPosition.Z
		);
		_smokeSprites.Add(smokeX);

		var smokeZ = _destructionDirectionalSmoke.Duplicate() as AnimatedSprite3D;
		_structure.AddChild(smokeZ);
		smokeZ.RotationDegrees = new Vector3(0, 45, 0);
		smokeZ.GlobalPosition = new Vector3(
			XRange.Y + smokeOffset + _structure.GlobalPosition.X,
			YRange.X + _structure.GlobalPosition.Y,
			ZRange.X + smokeOffset + _structure.GlobalPosition.Z
		);
		_smokeSprites.Add(smokeZ);

		//_destructionCenterSmoke.Play("idle");
		_destructionCenterSmoke.Hide();
		_destructionCenterSmoke.TopLevel = true;
		foreach (var smokeSprite in _smokeSprites)
		{
			smokeSprite.TopLevel = true;
			//GD.Print("smoke dir sprite pos: ", smokeSprite.GlobalPosition);
			//smokeSprite.Play("idle");
			smokeSprite.Hide();
		}
		//GD.Print("smoke center sprite pos: ", _destructionCenterSmoke.GlobalPosition);
	}
	private void CheckCollapseStatus()
	{
		if (IsBuildingCollapsing) { return; }
		var collapseHealth = _maxBuildingHealth * (1 - PercentageDamageToCollapse);

		GD.Print($"building health at {_currentBuildingHealth} out of {_maxBuildingHealth}");
		GD.Print($"health to collapse: {collapseHealth}");

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
		//TODO: USE VELOCITY INSTEAD OF POSITION SO CHARACTERS STICK TO ROOF???
		// OR launch players off of roof when destroying (last resort)
		_hurtboxComp.DeactivateHurtbox();

		_destructionCenterSmoke.Show();
		_destructionCenterSmoke.Play("emit");
		_destructionCenterSmoke.AnimationFinished += ChangeSmokeAnimsToIdle;
		foreach (var smokeSprite in _smokeSprites)
		{
			smokeSprite.Show();
			smokeSprite.Play("emit");
		}


		int numShakePoses = Global.Rnd.Next(6, 12);
		float timeToCollapse = Global.GetRndInRange(2.0f, 3.0f);

		var shakePoses = new List<Vector3>();
		for (int i = 0; i < numShakePoses; i++)
		{
			var shakePos = new Vector3
				(Global.GetRndInRange(-0.25f, 0.25f), 0f, Global.GetRndInRange(-0.25f, 0.25f));
			shakePoses.Add(_structure.Position + shakePos);
		}
		var destroyTween = GetTree().CreateTween();
		destroyTween.TweenProperty(_structure, "position:y",
				_structure.Position.Y - Dimensions.Y - 0.5f, timeToCollapse).SetEase(Tween.EaseType.InOut);
		var shakeTween = GetTree().CreateTween();
		while (timeToCollapse > 0f)
		{
			foreach (var pos in shakePoses)
			{
				shakeTween.TweenProperty(_structure, "position:x",
					pos.X, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
				shakeTween.Parallel().TweenProperty(_structure, "position:z",
					pos.Z, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
				timeToCollapse -= 0.05f;
			}
		}
		shakeTween.TweenProperty(_structure, "position:x",
			 _structure.Position.X, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
		shakeTween.Parallel().TweenProperty(_structure, "position:z",
			_structure.Position.Z, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
		
		destroyTween.TweenCallback(Callable.From(ChangeSmokeAnimsToDispurseAndFree));
		
	}
	private void ChangeSmokeAnimsToIdle()
	{
		_destructionCenterSmoke.Play("idle");
		foreach (var smokeSprite in _smokeSprites)
		{
			smokeSprite.Play("idle");
		}
	}
	private void ChangeSmokeAnimsToDispurseAndFree()
	{
		_destructionCenterSmoke.AnimationFinished -= ChangeSmokeAnimsToIdle;
		_destructionCenterSmoke.Play("dispurse");
		foreach (var smokeSprite in _smokeSprites)
		{
			smokeSprite.Play("dispurse");
		}
		_destructionCenterSmoke.AnimationFinished += () =>
		{
			//_destructionCenterSmoke.QueueFree();
			//foreach (var smokeSprite in _smokeSprites)
			//{
			//    smokeSprite.QueueFree();
			//}
			_structure.QueueFree();
		};
	}
	#endregion
}
