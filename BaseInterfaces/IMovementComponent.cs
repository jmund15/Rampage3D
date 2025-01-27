using Godot;
using System;

public enum EightDirection
{
    Right,
    DownRight,
    Down,
    DownLeft,
    Left,
    UpLeft,
    Up,
    UpRight
}
public enum OrthogDirection
{
    DownRight,
    DownLeft,
    UpRight,
    UpLeft
}
public enum AnimDirection
{
    Down,
    Up
}
public interface IMovementComponent
{
    public AnimDirection GetAnimDirection();
    public OrthogDirection GetFaceDirection();
    public OrthogDirection GetDesiredFaceDirection();
    public Vector2 GetDesiredDirection(); // TODO (in all implementations): if queued more than once in the same frame, use stored value instead of calc'ing again
    public Vector2 GetDesiredDirectionNormalized();
    public bool WantsJump();
    public bool WantsAttack();
    //public bool WantsSpecialAttack();
    public float TimeSinceAttackRequest(); // for buffering Attack
    public bool WantsStrafe();
    public float GetRunSpeedMult();
    //public Vector2 GetVelocity();
    public static OrthogDirection GetOrthogDirection(AnimDirection animDir, bool flippedH)
    {
        if (animDir == AnimDirection.Down)
        {
            if (flippedH)
            { return OrthogDirection.DownLeft; }
            else
            { return OrthogDirection.DownRight; }
        }
        else
        {
            if (flippedH)
            { return OrthogDirection.UpLeft; }
            else
            { return OrthogDirection.UpRight; }
        }
    }
}
public static partial class MovementExtensions
{
    #region ORTHOG_EXTENSIONS
    public static AnimDirection GetAnimDirection(this AnimationPlayer animPlayer)
    {
        if (animPlayer.AssignedAnimation.ToString().Contains("Down"))
        {
            return AnimDirection.Down;
        }
        else
        {
            return AnimDirection.Up;
        }
    }
    public static AnimDirection GetAnimDir(this OrthogDirection orthogDir)
    {
        switch (orthogDir)
        {
            case OrthogDirection.UpLeft or OrthogDirection.UpRight:
                return AnimDirection.Up;
            case OrthogDirection.DownLeft or OrthogDirection.DownRight:
                return AnimDirection.Down;
            default:
                throw new Exception($"ORTHOG DIR: {orthogDir} GAVE NO ANIM DIRECTION??");
        }
    }
    public static bool GetFlipH(this OrthogDirection orthogDir)
    {
        switch (orthogDir)
        {
            case OrthogDirection.DownRight or OrthogDirection.UpRight:
                return false;
            case OrthogDirection.DownLeft or OrthogDirection.UpLeft:
                return true;
            default:
                throw new Exception($"ORTHOG DIR: {orthogDir} GAVE NO FLIP H??");
        }
    }
    public static OrthogDirection GetOppositeDir(this OrthogDirection direction)
    {
        switch (direction)
        {
            case OrthogDirection.UpLeft:
                return OrthogDirection.DownRight;
            case OrthogDirection.UpRight:
                return OrthogDirection.DownLeft;
            case OrthogDirection.DownRight:
                return OrthogDirection.UpLeft;
            case OrthogDirection.DownLeft:
                return OrthogDirection.UpRight;
            default:
                GD.Print("not any face direction?? facedir = " + direction.ToString());
                return OrthogDirection.DownLeft;
        }
    }
    public static Vector2 GetVector2(this OrthogDirection orthogDir)
    {
        switch (orthogDir)
        {
            case OrthogDirection.UpLeft:
                return new Vector2(-1f, 0f);
                //return new Vector2(-0.707f, -0.707f);
            case OrthogDirection.UpRight:
                return new Vector2(0f, -1f);
                //return new Vector2(0.707f, -0.707f);
            case OrthogDirection.DownLeft:
                return new Vector2(0f, 1f);
                //return new Vector2(-0.707f, 0.707f);
            case OrthogDirection.DownRight:
                return new Vector2(1f, 0f);
                //return new Vector2(0.707f, 0.707f);
            default:
                throw new Exception($"ORTHOG DIR: {orthogDir} GAVE NO VECTOR??");
        }
    }
    public static Vector3 GetVector3(this OrthogDirection orthogDir)
    {
        var vec2 = GetVector2(orthogDir);
        return new Vector3(vec2.X, 0f, vec2.Y);
    }
    public static string GetAnimationString(this AnimDirection dir)
    {
        switch (dir)
        {
            case AnimDirection.Down:
                return "Down";
            case AnimDirection.Up:
                return "Up";
            default:
                GD.PrintErr("not any anim direction?? facedir = " + dir.ToString());
                return "Null";
        }
    }
    private static OrthogDirection GetDirectionFromXY(float x, float y)
    {
        if (x < 0 && y <= 0)
        {
            // IMPORTANT: Straight Left = UpLeft
            return OrthogDirection.UpLeft;
        }
        else if (x >= 0 && y < 0)
        {
            // IMPORTANT: Straight Up = UpRight
            return OrthogDirection.UpRight;
        }
        else if (x <= 0 && y > 0)
        {
            // IMPORTANT: Straight Down = DownLeft
            return OrthogDirection.DownLeft;
        }
        else if (x > 0 && y >= 0)
        {
            // IMPORTANT: Straight Right = DownRight
            return OrthogDirection.DownRight;
        }
        else { throw new Exception($"VALUES: ({x}, {y}) GAVE NO ORTHOG DIRECTION??"); }
    }
    public static OrthogDirection GetOrthogDirection(this Vector2 dir)
    {
        return GetDirectionFromXY(dir.X, dir.Y);
    }
    public static OrthogDirection GetOrthogDirection(this Vector3 dir)
    {
        return GetDirectionFromXY(dir.X, dir.Z);
    }
    private static AnimDirection GetAnimDirection(float forwardDir)
    {
        if (forwardDir > 0)
        { return AnimDirection.Down; }
        else
        { return AnimDirection.Up; }
    }
    public static AnimDirection GetAnimDir(this Vector2 dir)
    {
        return GetAnimDirection(dir.Y);
    }
    public static AnimDirection GetAnimDir(this Vector3 dir)
    {
        return GetAnimDirection(dir.Z);
    }
    public static bool GetFlipH(float horizDir)
    {
        if (horizDir < 0)
        { return true; }
        else
        { return false; }
    }
    public static bool GetFlipH(this Vector2 dir)
    {
        return GetFlipH(dir.X);
    }
    public static bool GetFlipH(this Vector3 dir)
    {
        return GetFlipH(dir.X);
    }
    public static Vector3 ClampIsometric(this Vector3 direction)
    {
        return direction.GetOrthogDirection().GetVector3();
    }
    public static Vector2 ClampIsometric(this Vector2 direction)
    {
        return direction.GetOrthogDirection().GetVector2();
    }
    public static Vector3 ClampInputToIsometric(this Vector2 inputDir)
    {
        // INPUT DIRECTION MEANS UP Y IS NEGATIVE
        return ClampIsometric(new Vector3(inputDir.X, 0f, -inputDir.Y));
    }
    #endregion
    #region VELOCITY_EXTENSIONS
    public static Vector3 GetWeightedGravity(this CharacterBody3D body)
    {
        const float WEIGHT_PERCENTAGE = 0.06f;
        Vector3 weightedGrav;
        if (body.Velocity.Y < 0)
        {
            weightedGrav = body.GetGravity() - (body.Velocity * body.GetGravity() * WEIGHT_PERCENTAGE);
        }
        else
        {
            weightedGrav = body.GetGravity();
        }
        //GD.Print("weighted grav: ", weightedGrav);
        return weightedGrav;
    }
    #endregion

    public static Vector2 GetVector(this EightDirection dir)
    {
        switch (dir) //RETURN NORMALIZED VECTOR
        {
            case EightDirection.Down:
                return new Vector2(0, 1);
            case EightDirection.Up:
                return new Vector2(0, -1);
            case EightDirection.Left:
                return new Vector2(-1, 0);
            case EightDirection.Right:
                return new Vector2(1, 0);
            case EightDirection.DownLeft:
                return new Vector2(-0.707f, 0.707f);
            case EightDirection.DownRight:
                return new Vector2(0.707f, 0.707f);
            case EightDirection.UpLeft:
                return new Vector2(-0.707f, -0.707f);
            case EightDirection.UpRight:
                return new Vector2(0.707f, -0.707f);
            default:
                GD.Print("not any face direction?? facedir = " + dir.ToString());
                return Vector2.Zero;
        }
    }
}
