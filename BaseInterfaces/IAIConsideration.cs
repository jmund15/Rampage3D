using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAIConsideration<DirT>
{
    public Dictionary<DirT, float> GetConsiderationVector(IBlackboard bb);
}
