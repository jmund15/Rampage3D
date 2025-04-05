using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

[GlobalClass, Tool]
public partial class BuildingComponent : RigidBody3D
{
	#region COMPONENT_VARIABLES
	[Export]
	public bool Enterable { get; private set; }
    private MeshInstance3D _doorEntranceIndicator;
	[Export]
	public Vector2 DoorEntranceOffset { get; private set; }
	[Export]
	public int MinimumFloorsToCollapse { get; private set; } = 0;
	[Export(PropertyHint.Range, "0,100,0.1")]
	public float PercentageDamageToCollapse { get; private set; } = 100f;
	[Export] //TODO?: TICK FASTER AS LESS HEALTH
	private float _collapseTickInterval = 1.0f;
	[Export(PropertyHint.Range, "0,100,0.1")]
	private float _collapseTickDamagePercentage = 5.0f;
	
	//[Export]
	private float _maxBuildingHealth = 0f;
	private float _currentBuildingHealth = 0f;
	private int _floorsDestroyed = 0;

	public Vector2 XRange { get; private set; } = new Vector2();
	public Vector2 YRange { get; private set; } = new Vector2();
	public Vector2 ZRange { get; private set; } = new Vector2();
	public Vector3 Dimensions { get; private set; } = new Vector3(); // X width, z length, and y height

	private RoofComponent _roofComp;
	private HurtboxComponent3D _hurtboxComp;
	private CollisionShape3D _hurtboxCollShape;
	private CollisionShape3D _collShape;

	private Timer _collapseTicker;
	
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
        //_doorEntranceIndicator = GetNode<MeshInstance3D>("DoorEntranceIndicator");
        //if (!Engine.IsEditorHint() || !Enterable)
        //{
        //    _doorEntranceIndicator.Hide();
        //}
        //else
        //{
        //    _doorEntranceIndicator.Show();
        //}
        //RequestReady();
		if (!IsInGroup("FullNav"))
		{
			AddToGroup("FullNav");
		}

        _roofComp = GetNode<RoofComponent>("RoofComponent");

        SetBuildingLocDirs();
		SetBuildingTypeDirs();
		SetFloorTextureFiles();
		SetRoofTextureFiles();

		var floors = this.GetChildrenOfType<BuildingFloorComponent>().ToList();
		_numFloors = floors.Count;
		//NotifyPropertyListChanged();
		//this.ChildEnteredTree += OnChildEnteredBuilding;
		//this.ChildExitingTree += OnChildExitingBuilding;
		
		if (!Engine.IsEditorHint())
		{
			//convert to percentages
			PercentageDamageToCollapse = PercentageDamageToCollapse / 100f;
			_collapseTickDamagePercentage = _collapseTickDamagePercentage / 100f;
		}

		_collapseTicker = GetNode<Timer>("CollapseTicker");
		_collapseTicker.Timeout += OnCollapseTick;

		_hurtboxComp = this.GetFirstChildOfType<HurtboxComponent3D>();
		_hurtboxComp.HitboxEntered += OnHitboxEntered;
		//GD.Print("num floors: ", _numFloors);

		// Step 1: Sort the heights in ascending order
		_sortedFloors = floors.OrderBy(f => f.YCenter).ToList();
		//TODO: Eventually calc these here and propogate down to the floors (since all floors of buildings will have same length/width)
		XFacePoses = _sortedFloors[0].XFacePoses;
		ZFacePoses = _sortedFloors[0].ZFacePoses;

		// Step 2: Set collision shapes of building based on floor meshes
		_collShape = this.GetFirstChildOfType<CollisionShape3D>();
		_hurtboxCollShape = _hurtboxComp.GetFirstChildOfType<CollisionShape3D>();
		_collShape.MakeConvexFromSiblings();
		_hurtboxCollShape.Shape = _collShape.Shape;
		//thisCollShape.Rotate(Vector3.Up, this.GlobalRotation.Y - this.Rotation.Y);
		var convexShape = _collShape.Shape as ConvexPolygonShape3D;
		float xMin = float.MaxValue; float xMax = float.MinValue;
		float yMin = float.MaxValue; float yMax = float.MinValue;
		float zMin = float.MaxValue; float zMax = float.MinValue;
		
		// Step 3: Using collision shape, calculate building dimensions
		foreach (var p in convexShape.Points)
		{
			// PUT IN GLOBAL COORDS
			var globalP = _collShape.ToGlobal(p);
			//var globalP = p.Rotated(Vector3.Up, this.GlobalRotation.Y - this.Rotation.Y);
			//var globalP = p;

			//var translatedP = p + (this.GlobalPosition - ;
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
			(xMax - xMin) * this.Scale.X,
			(yMax - yMin) * this.Scale.Y,
			(zMax - zMin) * this.Scale.Z
			);
		//if (this.Name == "Testing" || this.Name == "Middle")
		//{
		//    GD.Print(
		//        $"Building Start Pos: {new Vector3(xMin, yMin, zMin)}" +
		//        $"\nBuilding Size: {new Vector3(Dimensions.X, Dimensions.Y, Dimensions.Z)}" +
		//        $"");
		//}
		//GD.Print($"Building {this.Name}'s dimensions: {Dimensions}.");

		// Step 4: initialize the floors health and calc full building health
		CallDeferred(MethodName.InitializeFloorHealthConnections);
		

		// Step 5: setup destruction smoke
		_destructionCenterSmoke = this.GetNode<AnimatedSprite3D>("buildingDestructionSmokeCenter");
		_destructionDirectionalSmoke = this.GetNode<AnimatedSprite3D>("buildingDestructionSmokeDirectional");
		CallDeferred(MethodName.InitializeBaseSmoke);


		// Step 6: Set floor textures
		if (!Engine.IsEditorHint())
		{
			SetTextures();
		}
		else
		{
            NotifyPropertyListChanged();
        }


        if (Engine.IsEditorHint() && DoorEntranceOffset.IsZeroApprox())
        {
            DoorEntranceOffset = new Vector2(Dimensions.X / 2, Dimensions.Z / 2);
        }
    }

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Engine.IsEditorHint())
		{
			MinimumFloorsToCollapse = Mathf.Clamp(MinimumFloorsToCollapse, 0, _numFloors);
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
        if (Engine.IsEditorHint() && Enterable)
        {
            DrawDoorEntrancePoint();
        }
        //NotifyPropertyListChanged();
    }
    private void DrawDoorEntrancePoint()
    {
        var _time = Time.GetTicksMsec() / 1000.0f;
        //var sphere = _doorEntranceIndicator.Mesh as SphereMesh;
        //sphere.Radius = 0.25f * Mathf.Sin(_time * 2f);
        //sphere.Height = sphere.Radius * 2;
        //_doorEntranceIndicator.GlobalPosition = new Vector3(
        //    _doorEntranceIndicator.GlobalPosition.X, 0, _doorEntranceIndicator.GlobalPosition.Z);

        //DoorEntranceOffset = new Vector2(
        //    _doorEntranceIndicator.Position.X, _doorEntranceIndicator.Position.Z);
        var pointRadius = Mathf.Abs(0.15f * Mathf.Sin(_time * 4f)) + 0.15f;
        var pointLoc = GlobalPosition +
            new Vector3(DoorEntranceOffset.X, pointRadius, DoorEntranceOffset.Y);
        DebugDraw3D.DrawSphere(pointLoc, pointRadius, Colors.DeepSkyBlue);
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
			//GD.Print("DAMAGING FLOOR: ", floor.Name);
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
			//floor.HealthComp.HealthInitialized += (sender, args) =>
			//{
			//    _maxBuildingHealth += floor.HealthComp.MaxHealth;
			//    _currentBuildingHealth += floor.HealthComp.Health;
			//    if (Name == "Middle")
			//    {
			//        GD.Print("added building health to: ", _maxBuildingHealth);
			//    }
			//};
			_maxBuildingHealth += floor.HealthComp.MaxHealth;
			_currentBuildingHealth += floor.HealthComp.Health;
			if (Name == "Middle")
			{
				GD.Print("added building health to: ", _maxBuildingHealth);
			}

			floor.HealthComp.Destroyed += (update) => OnFloorDestroyed(floor, i, update);
			floor.HealthComp.HealthChanged += OnFloorDamaged;

			foreach (var wallCrack in floor.WallCracks)
			{
				wallCrack.RotateY(this.GlobalRotation.Y);
			}
		}
	}
	private void InitializeBaseSmoke()
	{
		var smokeOffset = 0.5f;
		_destructionCenterSmoke.GlobalPosition = new Vector3(
			XRange.Y + smokeOffset,// + this.GlobalPosition.X,
			YRange.X,// + this.GlobalPosition.Y,
			ZRange.Y + smokeOffset// + this.GlobalPosition.Z
			);
		_destructionCenterSmoke.RotationDegrees = /*RotationDegrees +*/ new Vector3(0,45,0);

		//var baseFloor = _floors[1]; // get bottom floor

		var smokeX = _destructionDirectionalSmoke.Duplicate() as AnimatedSprite3D;
		AddChild(smokeX);
		smokeX.FlipH = true;
		smokeX.GlobalRotationDegrees = _destructionDirectionalSmoke.GlobalRotationDegrees - new Vector3(0, 45, 0);
		smokeX.GlobalPosition = new Vector3(
			XRange.X + smokeOffset,// + this.GlobalPosition.X,
			YRange.X,// + this.GlobalPosition.Y,
			ZRange.Y + smokeOffset// + this.GlobalPosition.Z
		);
		_smokeSprites.Add(smokeX);

		var smokeZ = _destructionDirectionalSmoke.Duplicate() as AnimatedSprite3D;
		AddChild(smokeZ);
		smokeZ.GlobalRotationDegrees = _destructionDirectionalSmoke.GlobalRotationDegrees - new Vector3(0, 45, 0);
		smokeZ.GlobalPosition = new Vector3(
			XRange.Y + smokeOffset,
			YRange.X,
			ZRange.X + smokeOffset
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
		if (Name == "Middle")
		{
			GD.Print($"smoke center sprite pos: {_destructionCenterSmoke.GlobalPosition}" +
			$"\nsmokeX glob pos: {smokeX.GlobalPosition}" +
			$"\nsmokeZ glob pos: {smokeZ.GlobalPosition}");
		}
		
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
			GD.Print("smoke sprite pos: ", smokeSprite.GlobalPosition);
		}


		int numShakePoses = Global.Rnd.Next(6, 12);
		float timeToCollapse = Global.GetRndInRange(2.0f, 3.0f);

		var shakePoses = new List<Vector3>();
		for (int i = 0; i < numShakePoses; i++)
		{
			var shakePos = new Vector3
				(Global.GetRndInRange(-0.25f, 0.25f), 0f, Global.GetRndInRange(-0.25f, 0.25f));
			shakePoses.Add(this.Position + shakePos);
		}
		var destroyTween = GetTree().CreateTween();
		destroyTween.TweenProperty(this, "position:y",
				this.Position.Y - Dimensions.Y - 0.5f, timeToCollapse).SetEase(Tween.EaseType.InOut);
		var shakeTween = GetTree().CreateTween();
		while (timeToCollapse > 0f)
		{
			foreach (var pos in shakePoses)
			{
				shakeTween.TweenProperty(this, "position:x",
					pos.X, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
				shakeTween.Parallel().TweenProperty(this, "position:z",
					pos.Z, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
				timeToCollapse -= 0.05f;
			}
		}
		shakeTween.TweenProperty(this, "position:x",
			 this.Position.X, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
		shakeTween.Parallel().TweenProperty(this, "position:z",
			this.Position.Z, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
		
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
			this.QueueFree();
		};
	}

	private void SetTextures()
	{
		for (int i = 1; i <= _numFloors; i++)
		{
			var floor = _sortedFloors[i - 1];
			var floorTexture = _floorTextureMap[i];
			var text = ResourceLoader.Load<CompressedTexture2D>(floorTexture);

			//GD.Print($"floor {i} text: {_floorTextureMap[i]}" +
			//	$"\nFor BuildingTypeDir: {BuildingTypeDir}");

			floor.Texture = text;

			var normalMap = GetNormalMapOfTexture(floorTexture);
			//GD.Print($"normal map: \n{normalMap}");
            if (normalMap != string.Empty)
            {
				var normText = ResourceLoader.Load<CompressedTexture2D>(normalMap);
				floor.NormalMap = normText;
            }
			else
			{
				floor.NormalMap = null;
			}
        }
		if (_haveRoofProperty)
		{
			var roofText = ResourceLoader.Load<CompressedTexture2D>(_roofTexture);

			_roofComp.Texture = roofText;

            var normalMap = GetNormalMapOfTexture(_roofTexture);
            if (normalMap != string.Empty)
            {
                var normText = ResourceLoader.Load<CompressedTexture2D>(normalMap);
                _roofComp.NormalMap = normText;
            }
            else
            {
                _roofComp.NormalMap = null;
            }
        }
	}
    #endregion

    #region EDITOR_HELPERS
    [ExportGroup("Building Creator")]
	private string _buildingTypeRootDir = "res://Areas";
	[Export(PropertyHint.Dir)]
	public string BuildingTypeRootDir
	{
		get => _buildingTypeRootDir;
		private set
		{
			_buildingTypeRootDir = value;
			NotifyPropertyListChanged();
		}
	}

    private void SetBuildingLocDirs()
    {
        //GD.Print($"building type root dir: {_buildingTypeRootDir}");
        // Check if the directory exists
        if (!DirAccess.DirExistsAbsolute(_buildingTypeRootDir))
        {
            GD.PrintErr($"BUILDING LOCATION ERROR || Building Root @ '{_buildingTypeRootDir}' has no directories inside!");
            return;// properties;
        }
        // Get all directories inside the specified directory
        string[] buildingTypeDirs = DirAccess.GetDirectoriesAt(BuildingTypeRootDir);
        _buildingLocPropHint = string.Empty;
        // sep by commma
        for (int i = 0; i < buildingTypeDirs.Length; i++)
        {
            var dir = buildingTypeDirs[i];
            var absDir = BuildingTypeRootDir
                + "//" + dir + "//" + _buildingLocDirIdentifier;
            if (DirAccess.DirExistsAbsolute(absDir))
            {
                if (_buildingLocationDirMap.ContainsKey(i))
                {
                    _buildingLocationDirMap[i] = absDir;
                }
                else
                {
                    _buildingLocationDirMap.Add(i, absDir);
                    //GD.Print($"added '{i}' to _buildingLocationDirMap.");
                }

                _buildingLocPropHint += dir + ",";
            }
        }
        _buildingLocPropHint = _buildingLocPropHint.Remove(_buildingLocPropHint.Length - 1); // remove last comma
    }
    private void SetBuildingTypeDirs()
    {
        if (!DirAccess.DirExistsAbsolute(BuildingLocationDir) || BuildingLocationDir == string.Empty)
        {
            GD.PrintErr($"BUILDING TYPE ERROR || Building Location @ '{BuildingLocationDir}' does not exist!");
            return;// properties;
        }
        //GD.Print("building location dir: ", BuildingLocationDir);
        string[] textureDirs = DirAccess.GetDirectoriesAt(BuildingLocationDir);
        _buildingTypePropHint = string.Empty;
        if (textureDirs.IsEmpty())
        {
            GD.PrintErr($"BUILDING TYPE ERROR || Building Location @ '{BuildingLocationDir}' has no building type directories inside!");
            return; // properties;
        }
        // sep by commma
        for (int i = 0; i < textureDirs.Length; i++)
        {
            var dir = textureDirs[i];
            var absDir = BuildingLocationDir + "//" + dir;
            if (_buildingTypeDirMap.ContainsKey(i))
            {
                _buildingTypeDirMap[i] = absDir;
            }
            else { _buildingTypeDirMap.Add(i, absDir); }
            _buildingTypePropHint += dir + ",";
        }
        _buildingTypePropHint = _buildingTypePropHint.Remove(_buildingTypePropHint.Length - 1); // remove last comma
    }
    private void SetFloorTextureFiles()
    {
        if (!DirAccess.DirExistsAbsolute(BuildingTypeDir) || BuildingTypeDir == string.Empty)
        {
            GD.PrintErr($"BUILDING TYPE ERROR || Building Type @ '{BuildingTypeDir}' does not exist!");
            return;// properties;
        }

        //GD.Print("building type dir: ", BuildingTypeDir);
        string[] files = DirAccess.GetFilesAt(BuildingTypeDir);
        var textureFiles = new List<string>();
        foreach (var file in files)
        {
            var ext = file.GetExtension();
            if (ext == _textureFileExt && !IsFileNormalMap(file)
                && !file.ToLower().Contains("roof"))
            {
                textureFiles.Add(file);
            }
        }
        //GD.Print("amt of texture files: ", textureFiles.Length);
        if (textureFiles.Count == 0)
        {
            GD.PrintErr($"BUILDING TYPE ERROR || Building Type @ '{BuildingTypeDir}' has no texture files inside!" +
                $"\nabs num of files: {files.Length}");
            return;// properties;
        }

        _floorPropHint = string.Empty;
        // sep by commma
        for (int i = 0; i < textureFiles.Count; i++)
        {
            var file = textureFiles[i];

            var absPath = BuildingTypeDir + "//" + file;
            if (_availableFloorTexturePathMap.ContainsKey(i))
            {
                _availableFloorTexturePathMap[i] = absPath;
            }
            else { _availableFloorTexturePathMap.Add(i, absPath); }
            _floorPropHint += file + ",";
        }
        _floorPropHint = _floorPropHint.Remove(_floorPropHint.Length - 1); // remove last comma
    }
    private void SetRoofTextureFiles()
    {
        if (_roofComp == null || _roofComp.RoofMesh == null || _roofComp.RoofMesh is BuildingFloorComponent)
        {
            _haveRoofProperty = false;
            return;
        }
        if (!DirAccess.DirExistsAbsolute(BuildingTypeDir) || BuildingTypeDir == string.Empty)
        {
            GD.PrintErr($"SetRoofTextureFiles BUILDING TYPE ERROR || Building Type @ '{BuildingTypeDir}' does not exist!");
            return;// properties;
        }

        //GD.Print("building type dir: ", BuildingTypeDir);
        string[] files = DirAccess.GetFilesAt(BuildingTypeDir);
        var roofFiles = new List<string>();
        foreach (var file in files)
        {
            var ext = file.GetExtension();

            if (ext == _textureFileExt && !IsFileNormalMap(file)
               && file.ToLower().Contains("roof"))
            {
                roofFiles.Add(file);
            }
        }
        //GD.Print("amt of texture files: ", textureFiles.Length);
        if (roofFiles.Count == 0) // no roofs
        {
            //GD.PrintErr($"SetRoofTextureFiles BUILDING TYPE ERROR || Building Type @ '{BuildingTypeDir}' has no texture files inside!");
            _haveRoofProperty = false;
            return;// properties;
        }

        _roofPropHint = string.Empty;
        // sep by commma
        for (int i = 0; i < roofFiles.Count; i++)
        {
            var file = roofFiles[i];

            var absPath = BuildingTypeDir + "//" + file;
            if (_availableRoofTexturePathMap.ContainsKey(i))
            {
                _availableRoofTexturePathMap[i] = absPath;
            }
            else { _availableRoofTexturePathMap.Add(i, absPath); }
            _roofPropHint += file + ",";
        }
        _roofPropHint = _roofPropHint.Remove(_roofPropHint.Length - 1); // remove last comma
        _haveRoofProperty = true;
    }

    public override bool _PropertyCanRevert(StringName property)
    {
        if (property.Equals(_customBuildingLocPropName))
        {
            return true;
        }
        else if (property.Equals(_customBuildingTypePropName))
        {
            return true;
        }
        else if (property.ToString().StartsWith(_floorBuildingPropStart))
        {
            return true;
        }
        else if (property.Equals(_roofBuildingProp))
        {
            return true;
        }
        return base._PropertyCanRevert(property);
    }

    public override Variant _PropertyGetRevert(StringName property)
    {
        if (property.Equals(_customBuildingLocPropName))
        {
            return Variant.From(0);
        }
        else if (property.Equals(_customBuildingTypePropName))
        {
            return Variant.From(0);
        }
        else if (property.ToString().StartsWith(_floorBuildingPropStart))
        {
            int floorNum = int.Parse(property.ToString().Substring(_floorBuildingPropStart.Length));
            return Variant.From(floorNum - 1);
        }
        else if (property.Equals(_roofBuildingProp))
        {
            return Variant.From(0);
        }
        return base._PropertyGetRevert(property);
    }
    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
    {
        var properties = new Godot.Collections.Array<Godot.Collections.Dictionary>();

        if (!_buildingLocationDirMap.ContainsKey(0))
        {
            //GD.PrintErr($"BUILDING LOCATION ERROR || Building Root @ '{_buildingTypeRootDir}' has no directories inside!");
            return properties;
        }
        properties.Add(new Godot.Collections.Dictionary()
            {
                { "name", $"{_customBuildingLocPropName}" },
                { "type", (int)Variant.Type.Int },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", $"{_buildingLocPropHint}" },
            });
        if (BuildingLocationDir == string.Empty) // not set yet
        {
            GD.Print("INFO || BuildingLocationDir has not been set yet, defaulting...");
            _buildingLocDirIdx = 0;
            BuildingLocationDir = _buildingLocationDirMap[0];
        }
        //_Get(_customBuildingLocPropName);
        //_Set(_customBuildingLocPropName, _PropertyGetRevert(_customBuildingLocPropName));
        //GD.Print("added property: ", properties[properties.Count - 1]);


        if (!_buildingTypeDirMap.ContainsKey(0))
        {
            //GD.PrintErr($"BUILDING LOCATION ERROR || Building Root @ '{_buildingTypeRootDir}' has no directories inside!");
            return properties;
        }
        properties.Add(new Godot.Collections.Dictionary()
            {
                { "name", $"{_customBuildingTypePropName}" },
                { "type", (int)Variant.Type.Int },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", $"{_buildingTypePropHint}" },
            });
        if (BuildingTypeDir == string.Empty) // not set yet
        {
            GD.Print("INFO || BuildingTypeDir has not been set yet, defaulting...");
            _buildingTypeDirIdx = 0;
            BuildingTypeDir = _buildingTypeDirMap[0];
        }
        //_Get(_customBuildingTypePropName);
        //_Set(_customBuildingTypePropName, _PropertyGetRevert(_customBuildingTypePropName));
        //GD.Print("added property: ", properties[properties.Count - 1]);


        // CHOOSE FLOOR TEXTURES
        if (!_availableFloorTexturePathMap.ContainsKey(0))
        {
            //GD.PrintErr($"BUILDING LOCATION ERROR || Building Root @ '{_buildingTypeRootDir}' has no directories inside!");
            return properties;
        }
        //GD.Print("propHint: ", propHint, "\nnum floors: ", _numFloors);
        for (int i = 1; i <= _numFloors; i++)
        {
            properties.Add(new Godot.Collections.Dictionary()
                {
                    { "name", $"{_floorBuildingPropStart} {i}"},
                    { "type", (int)Variant.Type.Int },
                    { "hint", (int)PropertyHint.Enum },
                    { "hint_string", $"{_floorPropHint}" },
                });
            if (!_floorTextureIdxMap.ContainsKey(i) || !_floorTextureMap.ContainsKey(i)) // not set yet
            {
                GD.Print($"INFO || {_floorBuildingPropStart}{i} Texture has not been set yet, defaulting...");
                if (!_floorTextureIdxMap.ContainsKey(i))
                {
                    if (_availableFloorTexturePathMap.ContainsKey(i - 1))
                    {
                        _floorTextureIdxMap.Add(i, i - 1);
                    }
                    else
                    {
                        _floorTextureIdxMap.Add(i, 0);
                    }
                }
                if (!_floorTextureMap.ContainsKey(i))
                {
                    if (_availableFloorTexturePathMap.ContainsKey(i - 1))
                    {
                        _floorTextureMap.Add(i, _availableFloorTexturePathMap[i - 1]);
                    }
                    else
                    {
                        _floorTextureMap.Add(i, _availableFloorTexturePathMap[0]);
                    }
                }
            }
            //_Get($"{_floorBuildingPropStart} {i}");
            //GD.Print("added property: ", properties[properties.Count - 1]);
            //_Set($"{_floorBuildingPropStart} {i}", _PropertyGetRevert($"{_floorBuildingPropStart} {i}"));
        }

        if (_haveRoofProperty)
        {
            // CHOOSE ROOF TEXTURES
            if (!_availableRoofTexturePathMap.ContainsKey(0))
            {
                //GD.PrintErr($"BUILDING LOCATION ERROR || Building Root @ '{_buildingTypeRootDir}' has no directories inside!");
                return properties;
            }

            properties.Add(new Godot.Collections.Dictionary()
            {
                { "name", $"{_roofBuildingProp}"},
                { "type", (int)Variant.Type.Int },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", $"{_roofPropHint}" },
            });
            if (_roofTexture == string.Empty) // not set yet
            {
                GD.Print("INFO || Roof Texture has not been set yet, defaulting...");
                _roofTextureIdx = 0;
                _roofTexture = _availableRoofTexturePathMap[0];
            }
            //_Get($"{_floorBuildingPropStart} {i}");
            //GD.Print("added property: ", properties[properties.Count - 1]);
            //_Set($"{_floorBuildingPropStart} {i}", _PropertyGetRevert($"{_floorBuildingPropStart} {i}"));
        }

        if (_sortedFloors.Count > 0)
        {
            SetTextures();
        }

        return properties;
    }

    public override Variant _Get(StringName property)
    {
        string propertyName = property.ToString();
        if (propertyName.Equals(_customBuildingLocPropName))
        {
            //if (_buildingLocDirIdx == -1) // HASN'T BEEN SET!
            //{
            //    //_Set(property, _PropertyGetRevert(property));
            //    _buildingTypeDirIdx = _PropertyGetRevert(_customBuildingLocPropName).AsInt32();
            //    NotifyPropertyListChanged();
            //}
            return _buildingLocDirIdx;
        }
        else if (propertyName.Equals(_customBuildingTypePropName))
        {
            //if (_buildingTypeDirIdx == -1) // HASN'T BEEN SET!
            //{
            //    //_Set(property, _PropertyGetRevert(property));
            //    _buildingTypeDirIdx = _PropertyGetRevert(propertyName).AsInt32();
            //    NotifyPropertyListChanged();
            //}
            return _buildingTypeDirIdx;
        }
        else if (propertyName.StartsWith(_floorBuildingPropStart))
        {
            // don't do 'Length - 1' bc of space
            int floorNum = int.Parse(propertyName.Substring(_floorBuildingPropStart.Length));

            //if (!_floorTextureIdxMap.ContainsKey(floorNum))
            //{
            //    _Set(property, _PropertyGetRevert(property));
            //    //var defProp = _PropertyGetRevert(propertyName).AsInt32();
            //    //var texture = _availableFloorTexturePathMap[defProp];
            //    //_floorTextureIdxMap.Add(floorNum, defProp);
            //    //_floorTextureMap.Add(floorNum, texture);
            //    //NotifyPropertyListChanged();
            //}
            return _floorTextureIdxMap[floorNum];
        }
        else if (propertyName.Equals(_roofBuildingProp))
        {
            return _roofTextureIdx;
        }
        return default;
        //return new Variant();//base._Get(property);
    }

    public override bool _Set(StringName property, Variant value)
    {
        string propertyName = property.ToString();
        if (propertyName.Equals(_customBuildingLocPropName))
        {
            if (!_buildingTypeDirMap.ContainsKey(value.AsInt32()))
            {
                SetBuildingLocDirs();
            }
            _buildingLocDirIdx = value.AsInt32();
            BuildingLocationDir = _buildingLocationDirMap[value.AsInt32()];

            // default building type and textures
            _buildingTypeDirMap.Clear();
            SetBuildingTypeDirs();

            _buildingTypeDirIdx = 0;
            BuildingTypeDir = _buildingTypeDirMap[_buildingTypeDirIdx];

            _floorTextureIdxMap.Clear();
            _floorTextureMap.Clear();
            _roofTexture = string.Empty;
            _roofTextureIdx = 0;

            SetFloorTextureFiles();
            SetRoofTextureFiles();

            NotifyPropertyListChanged();
            //SetFloorTextures();
            return true;
        }
        else if (propertyName.Equals(_customBuildingTypePropName))
        {
            if (!_buildingTypeDirMap.ContainsKey(value.AsInt32()))
            {
                SetBuildingTypeDirs();
            }
            _buildingTypeDirIdx = value.AsInt32();
            BuildingTypeDir = _buildingTypeDirMap[value.AsInt32()];
            _floorTextureIdxMap.Clear();
            _floorTextureMap.Clear();
            _roofTexture = string.Empty;
            _roofTextureIdx = 0;

            SetFloorTextureFiles();
            SetRoofTextureFiles();

            NotifyPropertyListChanged();
            //SetFloorTextures();
            return true;
        }
        else if (propertyName.StartsWith(_floorBuildingPropStart))
        {
            if (!_availableFloorTexturePathMap.ContainsKey(value.AsInt32()))
            {
                SetFloorTextureFiles();
            }
            if (!_availableFloorTexturePathMap.ContainsKey(value.AsInt32()))
            {
                GD.PrintErr($"FLOOR SET ERROR || don't have floor text {value.AsInt32()}");
                return false;
            }
            //GD.Print("SETTING FLOOR PROP: ", propertyName);
            var texture = _availableFloorTexturePathMap[value.AsInt32()];
            // don't do 'Length - 1' bc of space
            int floorNum = int.Parse(propertyName.Substring(_floorBuildingPropStart.Length));

            //GD.Print($"Changed floor {floorNum}'s texture to: {texture}");

            if (_floorTextureMap.ContainsKey(floorNum))
            {
                _floorTextureIdxMap[floorNum] = value.AsInt32();
                _floorTextureMap[floorNum] = texture;
            }
            else
            {
                _floorTextureIdxMap.Add(floorNum, value.AsInt32());
                _floorTextureMap.Add(floorNum, texture);
            }
            NotifyPropertyListChanged();
            //SetFloorTextures();
            return true;
        }
        else if (propertyName.Equals(_roofBuildingProp))
        {
            if (!_haveRoofProperty)
            {
                return false;
            }
            if (!_availableRoofTexturePathMap.ContainsKey(value.AsInt32()))
            {
                SetRoofTextureFiles();
            }
            if (!_availableRoofTexturePathMap.ContainsKey(value.AsInt32()))
            {
                GD.PrintErr($"ROOF SET ERROR on '{Name}' || Don't have roof text {value.AsInt32()}");
                return false;
            }
            _roofTextureIdx = value.AsInt32();
            _roofTexture = _availableRoofTexturePathMap[value.AsInt32()];

            NotifyPropertyListChanged();
            return true;
        }
        return false;
        //return base._Set(property, value);
    }
    private void OnChildEnteredBuilding(Node node)
    {
        if (!Engine.IsEditorHint()) { return; }
        if (node is BuildingFloorComponent)
        {
            _numFloors = this.GetChildrenOfType<BuildingFloorComponent>().Count;
            NotifyPropertyListChanged();
        }
    }
    private void OnChildExitingBuilding(Node node)
    {
        if (!Engine.IsEditorHint()) { return; }
        if (node is BuildingFloorComponent)
        {
            _numFloors = this.GetChildrenOfType<BuildingFloorComponent>().Count;
            NotifyPropertyListChanged();
        }
    }
    private bool IsFileNormalMap(string filePath)
    {
        string fileName = filePath.GetBaseName();
        if (fileName.Length < 2)
        {
            return false; // can't be normal map
        }
        var last2 = fileName.Substring(fileName.Length - 2);
        if (last2 == _normalMapIdentifier)
        {
            return true;
        }
        return false;
    }
    private string GetNormalMapOfTexture(string texture)
    {
        var dir = texture.GetBaseDir();//Path.GetDirectoryName(texture);
        var fileName = Path.GetFileName(texture); //texture.GetBaseName();
        string noExtName = Path.GetFileNameWithoutExtension(fileName);
        //GD.Print("NoExt file name: ", noExtName);
        var normalName = noExtName + _normalMapIdentifier + "." + _textureFileExt;
        var normalPath = dir.PathJoin(normalName);
        //GD.Print("normalPath Check: \n", normalPath);
        if (Godot.FileAccess.FileExists(normalPath))
        {
            //GD.Print("found normal map!");
            return normalPath;
        }
        return string.Empty;
    }


    private string _customBuildingLocPropName = "Building Location";
	private string _buildingLocDirIdentifier = "Buildings//Textures";
	[ExportGroup("CREATOR PROPS *DON'T EDIT*")]
	[Export]
	public string BuildingLocationDir { get; private set; } = "";
	[Export]
	private int _buildingLocDirIdx = 0;
	[Export]
	private Godot.Collections.Dictionary<int, string> _buildingLocationDirMap = new();
	[Export]
	private string _buildingLocPropHint = "";

	private string _customBuildingTypePropName = "Building Type";
	[Export]
	public string BuildingTypeDir { get; private set; } = "";
	[Export]
	private int _buildingTypeDirIdx = 0;
	[Export]
	private Godot.Collections.Dictionary<int, string> _buildingTypeDirMap = new();
	[Export]
	private string _buildingTypePropHint = "";

	[Export]
	private int _numFloors = 0;
	// int => floor num; string => texture path
	[Export]
	private Godot.Collections.Dictionary<int, string> _floorTextureMap = new();
	private string _floorBuildingPropStart = "Floor";
	[Export]
	// int => texture num; string => texture path
	private Godot.Collections.Dictionary<int, string> _availableFloorTexturePathMap = new();
	[Export]
	// int => floor num; int => texture num
	private Godot.Collections.Dictionary<int, int> _floorTextureIdxMap = new();
	private string _textureFileExt = "png";
	[Export]
	private string _floorPropHint = "";
	private string _normalMapIdentifier = "_n";

	[Export]
	private bool _haveRoofProperty;
    [Export]
    private string _roofTexture = "";
    private string _roofBuildingProp = "Roof";
    [Export]
    // int => texture num; string => texture path
    private Godot.Collections.Dictionary<int, string> _availableRoofTexturePathMap = new();
    [Export]
    // int => roof num; int => texture num
    private int _roofTextureIdx = 0;
    [Export]
    private string _roofPropHint = "";

    
    #endregion
}
