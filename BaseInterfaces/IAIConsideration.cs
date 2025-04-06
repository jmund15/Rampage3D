using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAIConsideration<VecT>
{
    public Dictionary<VecT, float> GetConsiderationVector(IBlackboard bb);
}
