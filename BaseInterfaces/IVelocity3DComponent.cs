using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
public interface IVelocity3DComponent
{
    public void ApplyJump();
    public void SetRunning();
    public void SetWalking();
    public void SetDirection(Vector3 direction);
    public float GetVelMult();
}
