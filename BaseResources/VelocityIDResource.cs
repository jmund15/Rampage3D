using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ***** VELOCITY FORMULA *****
// MaxSpeed = Acceleration / Friction
// ****************************
[GlobalClass, Tool]
public partial class VelocityIDResource : Resource
{
    private float _maxSpeed;
    [Export]
    public float MaxSpeed
    {
        get => _maxSpeed;
        set
        {
            if (_maxSpeed == value) { return; }
            _maxSpeed = value;
            if (Acceleration != MaxSpeed * Friction)
            {
                Acceleration = MaxSpeed * Friction;
            }
        }
    }

    private float _acceleration;
    [Export]
    public float Acceleration
    {
        get => _acceleration;
        set
        {
            if (value == _acceleration) { return; }
            if (InstantMovement && value != -1) { return; }
            _acceleration = value;
            if (InstantMovement) { return; }
            
            if (MaxSpeed == 0) { MaxSpeed = Acceleration; }
            if (Friction != Acceleration / MaxSpeed)
            {
                Friction = Acceleration / MaxSpeed;
            }
        }
    }
    private float _friction;
    [Export]
    public float Friction
    { 
        get => _friction;
        set
        {
            if (value == _friction) { return; }
            if (InstantMovement && value != -1) { return; }
            _friction = value;
            if (InstantMovement) { return; }

            if (Acceleration != MaxSpeed * Friction)
            {
                Acceleration = MaxSpeed * Friction;
            }
        }
    }
    private float _brakingFriction = 1f;
    [Export]
    public float BrakingFrictionMod
    { 
        get => _brakingFriction;
        set
        {
            if (value == _brakingFriction) { return; }
            if (InstantMovement && value != -1) { return; }
            _brakingFriction = value;
        }
    }

    private bool _instantMovement;
    [Export]
    public bool InstantMovement
    {
        get => _instantMovement;
        set
        {
            if (value == _instantMovement) { return; }
            _instantMovement = value;
            if (_instantMovement)
            {
                _lastAcceleration = Acceleration;
                _lastFriction = Friction;
                _lastBreakingFriction = BrakingFrictionMod;
                Acceleration = -1;
                Friction = -1;
                BrakingFrictionMod = -1;
            }
            else
            {
                Acceleration = _lastAcceleration;
                Friction = _lastFriction;
                BrakingFrictionMod = _lastBreakingFriction;
            }
        }
    }
    private float _lastAcceleration;
    private float _lastFriction;
    private float _lastBreakingFriction;

    public VelocityIDResource()
    {
        MaxSpeed = 0f;
        Acceleration = 0f;
        Friction = 0f;
        BrakingFrictionMod = 1f;

        InstantMovement = false;
        _lastAcceleration = 0f;
        _lastFriction = 0f;
        _lastBreakingFriction = 0f;
    }

    public VelocityID GetVelocityID()
    {
        if (InstantMovement)
        {
            return new VelocityID(MaxSpeed);
        }
        else
        {
            return new VelocityID(
                MaxSpeed,
                Acceleration,
                Friction,
                BrakingFrictionMod);
        }
    }
}

