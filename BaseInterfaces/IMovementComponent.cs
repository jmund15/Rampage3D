using Godot;
//using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
public enum Dir16
{
    R, // = 0 Degrees 
    DRR, // = 22.5 degrees OR pi/8
    DR,
    DDR,
    D,
    DDL,
    DL,
    DLL,
    L,
    ULL,
    UL,
    UUL,
    U,
    UUR,
    UR,
    URR
}
public enum Dir8
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
public enum Dir4
{
    Right,
    Down,
    Up,
    Left
}
public enum AnimDirection
{
    Down,
    Up
}
public interface IMovementComponent
{
    public AnimDirection GetAnimDirection();
    public Dir4 GetFaceDirection();
    public Dir4 GetDesiredFaceDirection();
    public Vector2 GetDesiredDirection(); // TODO (in all implementations): if queued more than once in the same frame, use stored value instead of calc'ing again
    public Vector2 GetDesiredDirectionNormalized();
    public bool WantsJump();
    public bool WantsAttack();
    //public bool WantsSpecialAttack();
    public float TimeSinceAttackRequest(); // for buffering Attack
    public bool WantsStrafe();
    public static Dir4 GetOrthogDirection(AnimDirection animDir, bool flippedH)
    {
        if (animDir == AnimDirection.Down)
        {
            if (flippedH)
            { return Dir4.Down; }
            else
            { return Dir4.Right; }
        }
        else
        {
            if (flippedH)
            { return Dir4.Left; }
            else
            { return Dir4.Up; }
        }
    }
}

