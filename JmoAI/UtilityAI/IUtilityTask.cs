using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

namespace TimeRobbers.JmoAI.UtilityAI
{
    public interface IUtilityTask
    {
        public UtilityConsideration Consideration { get; protected set; }

        //// after how much time can the action be interrupted
        //protected Timer InterruptibleTimer { get; set; }
        public bool Interruptible { get; protected set; }

    }
}
