using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAIConsideration<VecT>
{
    public void InitializeResources(IBlackboard bb);

    //TODO: investigate, not all considerations will use a detector
    public Dictionary<VecT, float> GetConsiderationVector(IAIDetector3D detector);
}
