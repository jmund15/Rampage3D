using Godot;
using System;

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
    public static AnimDirection GetAnimPlayerDirection(AnimationPlayer animPlayer)
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
    public static AnimDirection GetAnimDirFromOrthog(OrthogDirection orthogDir)
    {
        switch (orthogDir)
        {
            case OrthogDirection.DownRight or OrthogDirection.DownLeft:
                return AnimDirection.Down;
            case OrthogDirection.UpLeft or OrthogDirection.UpRight:
                return AnimDirection.Up;
            default: return AnimDirection.Up;
        }
    }
    public static bool GetFlipHFromOrthog(OrthogDirection orthogDir)
    {
        switch (orthogDir)
        {
            case OrthogDirection.DownRight or OrthogDirection.UpRight:
                return false;
            case OrthogDirection.UpLeft or OrthogDirection.DownLeft:
                return true;
            default: return false;
        }
    }
    public static OrthogDirection GetSpriteOrthogDirection(Sprite3D sprite, AnimationPlayer animPlayer)
    {
        if (animPlayer.CurrentAnimation.ToString().Contains("Down"))
        {
            if (sprite.FlipH)
            {
                return OrthogDirection.DownLeft;
            }
            else
            {
                return OrthogDirection.DownRight;
            }
        }
        else
        {
            if (sprite.FlipH)
            {
                return OrthogDirection.UpLeft;
            }
            else
            {
                return OrthogDirection.UpRight;
            }
        }
    }

    public static OrthogDirection GetOrthogDirection(AnimDirection animDir, bool flippedH)
    {
        if (animDir == AnimDirection.Down)
        {
            if (flippedH)
            {
                return OrthogDirection.DownLeft;
            }
            else
            {
                return OrthogDirection.DownRight;
            }
        }
        else
        {
            if (flippedH)
            {
                return OrthogDirection.UpLeft;
            }
            else
            {
                return OrthogDirection.UpRight;
            }
        }
    }
    public static OrthogDirection GetOppositeDirection(OrthogDirection direction)
    {
        switch (direction)
        {
           
            default:
                GD.Print("not any face direction?? facedir = " + direction.ToString());
                return OrthogDirection.DownLeft;
        }
    }
    public static AnimDirection GetAnimDirectionFromVector(Vector2 direction)
    {
        if (direction.Y > 0)
        {
            return AnimDirection.Down;
        }
        else {
            return AnimDirection.Up;
        }
    }
    public static bool GetDesiredFlipH(Vector2 direction)
    {
        if (direction.X > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    //public static EightDirection GetDirectionFromVector(Vector2 direction)
    //{
    //    if (direction.X < 0)
    //    {
    //        switch (direction.X / direction.Y + float.Epsilon)
    //        {
    //            case float n when (Math.Abs(n) > 2):
    //                return EightDirection.LEFT;
    //            case float n when (Math.Abs(n) > 0.5):
    //                if (direction.Y >= 0)
    //                {
    //                    return EightDirection.DOWNLEFT;
    //                }
    //                else
    //                {
    //                    return EightDirection.UPLEFT;
    //                }
    //            default:
    //                if (direction.Y >= 0)
    //                {
    //                    return EightDirection.DOWN;
    //                }
    //                else
    //                {
    //                    return EightDirection.UP;
    //                }
    //        }
    //    }
    //    else
    //    {
    //        switch (direction.X / direction.Y + float.Epsilon)
    //        {
    //            case float n when (Math.Abs(n) > 2):
    //                return EightDirection.RIGHT;
    //            case float n when (Math.Abs(n) > 0.5):
    //                if (direction.Y >= 0)
    //                {
    //                    return EightDirection.DOWNRIGHT;
    //                }
    //                else
    //                {
    //                    return EightDirection.UPRIGHT;
    //                }
    //            default:
    //                if (direction.Y >= 0)
    //                {
    //                    return EightDirection.DOWN;
    //                }
    //                else
    //                {
    //                    return EightDirection.UP;
    //                }
    //        }
    //    }
    //    //if (direction.X < 0)
    //    //{
    //    //    return MovementDirection.LEFT;
    //    //}
    //    //else if (direction.X > 0)
    //    //{
    //    //    return MovementDirection.RIGHT;
    //    //}
    //    //else if (direction.Y < 0)
    //    //{
    //    //    return MovementDirection.UP;
    //    //}
    //    //else
    //    //{
    //    //    return MovementDirection.DOWN;
    //    //}
    //}
    public static string GetFaceDirectionString(AnimDirection direction)
    {
        switch (direction)
        {
            case AnimDirection.Down:
                return "Down";
            case AnimDirection.Up:
                return "Up";
            default:
                GD.PrintErr("not any face direction?? facedir = " + direction.ToString());
                return "Null";
        }
    }
    public static Vector2 GetVectorFromDirection(OrthogDirection dir)
    {
        switch (dir)
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
                GD.Print("not any face direction?? facedir = " + dir.ToString());
                return Vector2.Zero;
        }
    }
    public static OrthogDirection GetDirectionFromVector(Vector3 dir)
    {
        switch (dir)
        {
            //case OrthogDirection.UpLeft:
            //case OrthogDirection.UpRight:
            //case OrthogDirection.DownLeft:
            //case OrthogDirection.DownRight:
            default:
                GD.Print("not any face direction?? vector = " + dir.ToString());
                return OrthogDirection.UpLeft ;
        }
    }
    public static OrthogDirection GetDirectionFromVector(Vector2 dir)
    {
        switch (dir)
        {
            //case OrthogDirection.UpLeft:
            //case OrthogDirection.UpRight:
            //case OrthogDirection.DownLeft:
            //case OrthogDirection.DownRight:
            default:
                GD.Print("not any face direction?? vector = " + dir.ToString());
                return OrthogDirection.UpLeft;
        }
    }

}
