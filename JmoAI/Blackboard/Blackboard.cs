using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
//using System.Collections.Generic;
public enum InteruptibleChange
{
    True, 
    False, 
    NoChange
}
public enum BBDataSig
{
    Agent,
    Sprite,
    Anim,
    CurrentTarget,
    MoveComp,
    VelComp,
    HealthComp,
    HurtboxComp,
    HitboxComp,
    AINavComp,
    DetectComp,
    SquadComp,
    Affinities,
    CombatComp,
    MovementSM,
    AISM,
    QueuedNextAttack,
    //TargetPosition,
    SelfInteruptible,
    #region ROBBER_SIGS
    RobberBag, RobberEffects,
    #endregion
    #region RAMPAGE_SIGS
    ClimberComp,
    CurrentAttackType,
    GroundNormalAttack,
    GroundSpecialAttack,
    WallNormalAttack,
    WallSpecialAttack,
    EaterComp,
    EatableComp,

    #endregion
    #region BREAKABLE_SIGS
    #endregion
}
[GlobalClass, Tool]
public partial class Blackboard : Node, IBlackboard//<BBDataSig>
{
    #region TASK_VARIABLES
    protected IBlackboard ParentBB { get; set; }
    protected IBlackboard ChildBB { get; set; }
    //[Export]
    protected Dictionary<BBDataSig, Variant> BBData { get; set; } = new Dictionary<BBDataSig, Variant>();
    #endregion
    #region TASK_UPDATES
    public override void _Ready()
    {
        base._Ready();
    }
    #endregion
    #region TASK_HELPER
    public T GetVar<T>(BBDataSig bbVar) where T : class // IDEA: make return nullable?
    {
        if (BBData.TryGetValue(bbVar, out var val))
        {
            // have to convert from variant to godot object, basically the opposite of "Variant.From"
            var gVal = val.AsGodotObject(); 
            if (gVal is T tVal)
            {
                return tVal;
            }
            GD.PrintErr($"BB \"{Name}\" ERROR || Requested data of \"{bbVar}\" exists, but" +
                $" the requested type \"{typeof(T).Name}\" does not match with existing type \"{gVal.GetType().Name}\" in BB!");
            return null;
            //return Error.InvalidData;
            //throw new Exception(); //TODO: more verbose
        }
        else if (ParentBB?.GetVar<T>(bbVar) is not null)
        {
            return ParentBB?.GetVar<T>(bbVar);
        }
        GD.PrintErr($"BB \"{Name}\" ERROR || Requested data \"{bbVar}\" does not exist!");
        return null;
        //throw new Exception(); //TODO: more verbose
        //if (ParentBB.IfValid()?.BBData.TryGetValue(bbvar, out var val))
        //{

        //}
        //if (val is not T tVal)
        //{
        //    throw new Exception(); //TODO: more verbose
        //}
        //return tVal;
    }
    public Error SetVar<T>(BBDataSig bbVar, T val) where T : class
    {
        if (BBData.TryGetValue(bbVar, out var oldVal))
        {
            //// have to convert from variant to godot object, basically the opposite of "Variant.From"
            //var gVal = oldVal.AsGodotObject();
            //if (gVal is not T)
            //{
            //    var lastFrame = new StackFrame(1);
            //    string className = lastFrame.GetMethod().DeclaringType.Name;
            //    string methodName = lastFrame.GetMethod().Name;
            //    //throw new Exception(); //TODO: more verbose
            //    GD.PrintErr($"Var attempted set from {className}'s {methodName}");
            //    GD.PrintErr($"BB \"{Name}\" ERROR || Inconsistent data type for BBDataSig \"{bbVar}\"!" +
            //        $"\nOriginal data was of type \"{gVal.GetType().Name}, but attempted set data was of type \"{typeof(T).Name}\".");
            //    return Error.InvalidData;
            //}
        }
        BBData[bbVar] = Variant.From(val).AsGodotObject();

        //Set child BB var here?
        ChildBB?.SetVar(bbVar, val);
        return Error.Ok;
    }
    
    public T? GetPrimVar<[MustBeVariant] T>(BBDataSig bbPrimVar) where T : struct
    {
        if (BBData.TryGetValue(bbPrimVar, out var val))
        {
            // have to convert from variant to godot object, basically the opposite of "Variant.From"
            var gVal = val.As<T>();
            if (gVal is T tVal)
            {
                return tVal;
            }
            GD.PrintErr($"BB \"{Name}\" ERROR || Requested data of \"{bbPrimVar}\" exists, but" +
                $" the requested type \"{typeof(T).Name}\" does not match with existing type \"{gVal.GetType().Name}\" in BB!");
            return null;
            //return Error.InvalidData;
            //throw new Exception(); //TODO: more verbose
        }
        else if (ParentBB?.GetPrimVar<T>(bbPrimVar) is not null)
        {
            return ParentBB?.GetPrimVar<T>(bbPrimVar);
        }
        GD.PrintErr($"BB \"{Name}\" ERROR || Requested data \"{bbPrimVar}\" does not exist!");
        return null;
    }

    public Error SetPrimVar<[MustBeVariant] T>(BBDataSig bbPrimVar, T val) where T : struct
    {
        if (BBData.TryGetValue(bbPrimVar, out var oldVal))
        {
            var gVal = oldVal.As<T>();
            if (gVal is T)
            {
                BBData[bbPrimVar] = Variant.From(val);
                //Set child BB var here?
                ChildBB?.SetPrimVar(bbPrimVar, val);
                return Error.Ok;
            }
            else
            {
                var lastFrame = new StackFrame(1);
                string className = lastFrame.GetMethod().DeclaringType.Name;
                string methodName = lastFrame.GetMethod().Name;
                //throw new Exception(); //TODO: more verbose
                GD.PrintErr($"Var attempted set from {className}'s {methodName}");
                GD.PrintErr($"BB \"{Name}\" ERROR || Inconsistent data type for BBDataSig \"{bbPrimVar}\"!" +
                    $"\nOriginal data was of type \"{oldVal.GetType().Name}, but attempted set data was of type \"{typeof(T).Name}\".");
                return Error.InvalidData;
            }
        }
        else
        {
            BBData[bbPrimVar] = Variant.From(val);
            //Set child BB var here?
            ChildBB?.SetPrimVar(bbPrimVar, val);
            return Error.Ok;
        }
    }
    #endregion
}
