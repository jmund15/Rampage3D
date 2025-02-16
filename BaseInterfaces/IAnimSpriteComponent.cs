using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IAnimSpriteComponent : IAnimComponent
{
    public void SetFrame(int frame);
    public void SetFrameAndProgress(int frame, float progress);

    public int GetFrame();
    public float GetFrameProgress();
}
