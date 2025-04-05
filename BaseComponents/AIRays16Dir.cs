using Godot;
using System.Collections.Generic;
using System.Linq;

//TODO: Update to be more modular
[GlobalClass, Tool]
public partial class AIRays16Dir : Node3D
{
    #region COMPONENT_VARIABLES
    [Export]
    public float RayLength { get; private set; } = 5f;
	public Dictionary<Dir16, RayCast3D> Raycasts { get; private set; } = new Dictionary<Dir16, RayCast3D>();
	public RayCast3D RayU { get; private set; }
    public RayCast3D RayUUR { get; private set; }
    public RayCast3D RayUR { get; private set; }
    public RayCast3D RayURR { get; private set; }
    public RayCast3D RayR { get; private set; }
    public RayCast3D RayDRR { get; private set; }
    public RayCast3D RayDR { get; private set; }
    public RayCast3D RayDDR { get; private set; }
    public RayCast3D RayD { get; private set; }
    public RayCast3D RayDDL { get; private set; }
    public RayCast3D RayDL { get; private set; }
    public RayCast3D RayDLL { get; private set; }
    public RayCast3D RayL { get; private set; }
    public RayCast3D RayULL { get; private set; }
    public RayCast3D RayUL { get; private set; }
    public RayCast3D RayUUL { get; private set; }

    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        
        RayU = GetNode<RayCast3D>("RayUp");
        RayUUR = GetNode<RayCast3D>("RayUUR");
        RayUR = GetNode<RayCast3D>("RayUpRight");
        RayURR = GetNode<RayCast3D>("RayURR");
        RayR = GetNode<RayCast3D>("RayRight");
        RayDRR = GetNode<RayCast3D>("RayDRR");
        RayDR = GetNode<RayCast3D>("RayDownRight");
        RayDDR = GetNode<RayCast3D>("RayDDR");
        RayD = GetNode<RayCast3D>("RayDown");
        RayDDL = GetNode<RayCast3D>("RayDDL");
        RayDL = GetNode<RayCast3D>("RayDownLeft");
        RayDLL = GetNode<RayCast3D>("RayDLL");
        RayL = GetNode<RayCast3D>("RayLeft");
        RayULL = GetNode<RayCast3D>("RayULL");
        RayUL = GetNode<RayCast3D>("RayUpLeft");
        RayUUL = GetNode<RayCast3D>("RayUUL");

        Raycasts.Add(Dir16.U, RayU);
        Raycasts.Add(Dir16.UUR, RayUUR);
        Raycasts.Add(Dir16.UR, RayUR);
        Raycasts.Add(Dir16.URR, RayURR);
        Raycasts.Add(Dir16.R, RayR);
        Raycasts.Add(Dir16.DRR, RayDRR);
        Raycasts.Add(Dir16.DR, RayDR);
        Raycasts.Add(Dir16.DDR, RayDDR);
        Raycasts.Add(Dir16.D, RayD);
        Raycasts.Add(Dir16.DDL, RayDDL);
        Raycasts.Add(Dir16.DL, RayDL);
        Raycasts.Add(Dir16.DLL, RayDLL);
        Raycasts.Add(Dir16.L, RayL);
        Raycasts.Add(Dir16.ULL, RayULL);
        Raycasts.Add(Dir16.UL, RayUL);
        Raycasts.Add(Dir16.UUL, RayUUL);

        if (Engine.IsEditorHint())
        {
            ////FOR INITIALIZATION
            //foreach (var rayPair in Raycasts)
            //{
            //    var dir = rayPair.Key;
            //    var ray = rayPair.Value;

            //    ray.TargetPosition = dir.GetVector3();
            //}
            return;
        }

        GlobalScale(Vector3.One);
        GlobalRotation = Vector3.Zero;
        foreach (var raycast in Raycasts.Values)
        {
            raycast.TargetPosition *= RayLength;
            //GD.Print("ray length: ", raycast.TargetPosition.Length());
        }

        
    }
    public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
	#endregion
	#region COMPONENT_HELPER
    public List<RayCast3D> GetAllRaycasts()
    {
        return Raycasts.Values.ToList();
    }
    public RayCast3D GetRaycastOfDirection(Vector2 dir)
    {
        var normDir = dir.Normalized();
        foreach (var ray in Raycasts.Values)
        {
            var rayDir = new Vector2(ray.TargetPosition.X, ray.TargetPosition.Z).Normalized();
            if (rayDir.IsEqualApprox(normDir))
            {
                return ray;
            }
        }
        return null;
    }
    public RayCast3D GetRaycastOfDirection(Vector3 dir)
    {
        var normDir = dir.Normalized();
        foreach (var ray in Raycasts.Values)
        {
            var rayDir = ray.TargetPosition.Normalized();
            if (rayDir.IsEqualApprox(normDir))
            {
                return ray;
            }
        }
        return null;
    }
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
