using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ***** TERMINAL VELOCITY FORMULA *****
// MaxSpeed = Acceleration / Friction
// *************************************

public enum VelocityFormulas
{
    TerminalVelocityForm,
    InstantMovement,
    IndependentVariables
}

[GlobalClass, Tool]
public partial class VelocityIDResource : Resource
{
    private VelocityFormulas _velocityFormula = VelocityFormulas.IndependentVariables;
    [Export]
    public VelocityFormulas VelocityFormula 
    { 
        get => _velocityFormula; 
        private set
        {
            if (value == _velocityFormula) { return; }
            _velocityFormula = value;
            switch (value)
            {
                case VelocityFormulas.TerminalVelocityForm:
                    //if (_velocityFormula == VelocityFormulas.InstantMovement)
                    //Acceleration = _lastAcceleration;
                    //Friction = _lastFriction;
                    //BrakingFrictionMod = _lastBreakingFriction;
                    if (MaxSpeed == 0)
                    {
                        _acceleration = 0f;
                        _friction = 0f;
                    }
                    else if (Acceleration < 0 || Friction < 0)
                    {
                        _acceleration = MaxSpeed;
                        _friction = 1f;
                    }
                    else
                    {
                        _friction = Acceleration / MaxSpeed;
                    }
                    break;
                case VelocityFormulas.InstantMovement:
                    _lastAcceleration = Acceleration == -1 ? 0 : Acceleration;
                    _lastFriction = Friction == -1 ? 0 : Friction;
                    _lastBreakingFriction = BrakingFrictionMod == -1 ? 0 : BrakingFrictionMod;
                    _acceleration = -1;
                    _friction = -1;
                    _brakingFriction = -1;
                    break;
                case VelocityFormulas.IndependentVariables:
                    _brakingFriction = 1f;
                    //Acceleration = _lastAcceleration;
                    //Friction = _lastFriction;
                    //BrakingFrictionMod = _lastBreakingFriction;
                    //Acceleration = Mathf.Max(Acceleration, 0);
                    //Friction = Mathf.Max(Friction, 0);
                    break;
            }
            //_velocityFormula = value;
        }
    } 
        
    [Export]
    public VelocityType VelocityType { get; private set; }
    private float _maxSpeed;
    [Export]
    public float MaxSpeed
    {
        get => _maxSpeed;
        set
        {
            if (_maxSpeed == value) { return; }
            switch (VelocityFormula)
            {
                case VelocityFormulas.TerminalVelocityForm:
                    _maxSpeed = value;
                    if (Acceleration != MaxSpeed * Friction)
                    {
                        Acceleration = MaxSpeed * Friction;
                    }
                    break;
                case VelocityFormulas.InstantMovement:
                    _maxSpeed = value;
                    break;
                case VelocityFormulas.IndependentVariables:
                    _maxSpeed = value;
                    break;
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
            if (_acceleration == value) { return; }
            switch (VelocityFormula)
            {
                case VelocityFormulas.TerminalVelocityForm:
                    _acceleration = value;
                    if (MaxSpeed == 0) { MaxSpeed = Acceleration; }
                    if (Friction != Acceleration / MaxSpeed)
                    {
                        Friction = Acceleration / MaxSpeed;
                    }
                    break;
                case VelocityFormulas.InstantMovement:
                    _acceleration = -1;
                    break;
                case VelocityFormulas.IndependentVariables:
                    _acceleration = value;
                    break;
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
            switch (VelocityFormula)
            {
                case VelocityFormulas.TerminalVelocityForm:
                    _friction = value;
                    if (Acceleration != MaxSpeed * Friction)
                    {
                        Acceleration = MaxSpeed * Friction;
                    }
                    break;
                case VelocityFormulas.InstantMovement:
                    _friction = -1;
                    break;
                case VelocityFormulas.IndependentVariables:
                    _friction = value;
                    break;
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
            switch (VelocityFormula)
            {
                case VelocityFormulas.TerminalVelocityForm:
                    _brakingFriction = value;
                    break;
                case VelocityFormulas.InstantMovement:
                    _brakingFriction = -1;
                    break;
                case VelocityFormulas.IndependentVariables:
                    _brakingFriction = -1;
                    break;
            }
        }
    }

    private float _lastAcceleration;
    private float _lastFriction;
    private float _lastBreakingFriction;

    public VelocityIDResource()
    {
        VelocityType = VelocityType.Ground;
        MaxSpeed = 0f;
        Acceleration = 0f;
        Friction = 0f;
        BrakingFrictionMod = 1f;

        VelocityFormula = VelocityFormulas.IndependentVariables;
        _lastAcceleration = 0f;
        _lastFriction = 0f;
        _lastBreakingFriction = 0f;
    }

    public VelocityID GetVelocityID()
    {
        switch (VelocityFormula)
        {
            case VelocityFormulas.InstantMovement:
                return new VelocityID(MaxSpeed);
            case VelocityFormulas.TerminalVelocityForm:
                return new VelocityID(
                                MaxSpeed,
                                Acceleration,
                                Friction);
            case VelocityFormulas.IndependentVariables:
                return new VelocityID(
                                MaxSpeed,
                                Acceleration,
                                Friction,
                                BrakingFrictionMod);
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(VelocityFormula),
                    VelocityFormula,
                    "Invalid velocity formula.");
        }
    }
}

