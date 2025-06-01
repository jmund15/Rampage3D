using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum DetectorType
{
    Area,
    Ray,
}
[GlobalClass]
public abstract partial class AIEntityConsideration3D : Resource, IAIConsideration<Vector3>
{
    [Export(PropertyHint.Range, "-2.5,2.5,0.1,or_greater,or_less")]
    protected float Consideration; // negative values are danger, positive are interest

    [Export]
    public DetectorType PreferredDetector { get; protected set; } = DetectorType.Ray;

    protected IBlackboard BB;
    protected Node3D Agent;
    protected AINav3DComponent AINav;
    public AIEntityConsideration3D()
    {
    }
    public abstract void InitializeResources(IBlackboard bb);
    public abstract Dictionary<Vector3, float> GetConsiderationVector(IAIDetector3D detector);
}
