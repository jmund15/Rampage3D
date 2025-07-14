using Godot;
using Godot.Collections;
using BaseInterfaces;

[GlobalClass, Tool]
public partial class OccupantComponent3D : Node3D, IDriver
{
    #region COMPONENT_VARIABLES
    private Node _originalDriverParent;
    private Node3D _driver;
    private AINav3DComponent _driverAI;
    [Export]
    public bool CanDrive { get; private set; } = true;
    [Export]
	public DriverBehavior DriverBehavior { get; private set; }
    public IVehicleComponent3D? VehicleComponent { get; set; }
    public VehicleOccupantsComponent? VehicleOccupantsComponent { get; set; }
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        _driver = GetOwner<Node3D>();
        _originalDriverParent = _driver.GetParent();
        _driverAI = _driver.GetFirstChildOfType<AINav3DComponent>();
    }
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
    public DriverBehavior GetDriverBehavior()
    {
        return DriverBehavior;
    }
    public IVehicleComponent3D GetVehicleComponent()
    {
        return VehicleComponent;
    }
    public VehicleOccupantsComponent GetVehicleOccupantsComponent()
    {
        return VehicleOccupantsComponent;
    }

    public Vector3 GetDesiredDriveLoc()
    {
        return _driverAI.WeightedNextPathDirection;
    }
    public bool WantsDrive()
    {
        throw new System.NotImplementedException();
    }
    #endregion
    #region COMPONENT_HELPER
    public void Embarking(IVehicleComponent3D vehicle, VehicleOccupantsComponent occComp)
    {
        VehicleComponent = vehicle;
        VehicleOccupantsComponent = occComp;
        _driver.Hide();
        _driver.Reparent(occComp);
    }
    public void Disembarking()
    {
        VehicleComponent = null;
        VehicleOccupantsComponent = null;
        _driver.Show();
        _driver.Reparent(_originalDriverParent);
    }

    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
