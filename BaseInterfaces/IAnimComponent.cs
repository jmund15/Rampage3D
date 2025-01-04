using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeRobbers.Interfaces
{
    public interface IAnimComponent
    {  
        public void StartAnimation(string animName);
        public void StopAnimation();
        public string GetCurrAnimationName();
        public string GetNextRecoilDirectionAnimationName();
    }
}
