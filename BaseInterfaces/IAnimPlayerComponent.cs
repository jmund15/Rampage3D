using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public interface IAnimPlayerComponent : IAnimComponent
{  
    public void SeekPos(float time, bool updateNow = true);
    public void FastForward(float time);

    public float GetCurrAnimationPosition();
    public float GetCurrAnimationLength();
}
