using Godot;


namespace Jmo.AI.Blackboard;
/// <summary>
/// Provides a centralized and performant registry of StringName keys for use with the Blackboard system.
/// Using a static registry combines the performance benefits of StringName with the discoverability and
/// typo-prevention of enums.
/// </summary>
public static class BBDataSig
{
    #region CORE_PROPERTIES
    public static readonly StringName Agent = new StringName("Agent");
    public static readonly StringName Sprite = new StringName("Sprite");
    public static readonly StringName Anim = new StringName("Anim");
    public static readonly StringName CurrentTarget = new StringName("CurrentTarget");
    public static readonly StringName MoveComp = new StringName("MoveComp");
    public static readonly StringName VelComp = new StringName("VelComp");
    public static readonly StringName HealthComp = new StringName("HealthComp");
    public static readonly StringName HurtboxComp = new StringName("HurtboxComp");
    public static readonly StringName HitboxComp = new StringName("HitboxComp");
    public static readonly StringName AINavComp = new StringName("AINavComp");
    public static readonly StringName DetectComp = new StringName("DetectComp");
    public static readonly StringName SquadComp = new StringName("SquadComp");
    public static readonly StringName Affinities = new StringName("Affinities");
    public static readonly StringName CombatComp = new StringName("CombatComp");
    public static readonly StringName MovementSM = new StringName("MovementSM");
    public static readonly StringName AISM = new StringName("AISM");
    public static readonly StringName QueuedNextAttack = new StringName("QueuedNextAttack");
    public static readonly StringName SelfInteruptible = new StringName("SelfInteruptible");
    #endregion

    #region ROBBER_PROPERTIES
    public static readonly StringName RobberBag = new StringName("RobberBag");
    public static readonly StringName RobberEffects = new StringName("RobberEffects");
    #endregion

    #region RAMPAGE_PROPERTIES
    public static readonly StringName ClimberComp = new StringName("ClimberComp");
    public static readonly StringName CurrentAttackType = new StringName("CurrentAttackType");
    public static readonly StringName GroundNormalAttack = new StringName("GroundNormalAttack");
    public static readonly StringName GroundSpecialAttack = new StringName("GroundSpecialAttack");
    public static readonly StringName WallNormalAttack = new StringName("WallNormalAttack");
    public static readonly StringName WallSpecialAttack = new StringName("WallSpecialAttack");
    public static readonly StringName EaterComp = new StringName("EaterComp");
    public static readonly StringName EatableComp = new StringName("EatableComp");
    public static readonly StringName OccupantComp = new StringName("OccupantComp");
    public static readonly StringName JumpsLeft = new StringName("JumpsLeft");
    #endregion

    #region AI_PROPERTIES
    public static readonly StringName OwnedVehicle = new StringName("OwnedVehicle");
    public static readonly StringName TargetOrOccupiedVehicleSeat = new StringName("TargetOrOccupiedVehicleSeat");
    public static readonly StringName TargetOrOccupiedVehicle = new StringName("TargetOrOccupiedVehicle");
    #endregion

    #region SQUAD_PROPERTIES
    public static readonly StringName ActiveSquadTag = new StringName("ActiveSquadTag");
    public static readonly StringName HasSquadTag = new StringName("HasSquadTag");
    public static readonly StringName SquadAverageHealth = new StringName("SquadAverageHealth");
    #endregion
}