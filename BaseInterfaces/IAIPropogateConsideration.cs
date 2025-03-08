using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAIPropogateConsideration<DirT>
{
    public Dictionary<DirT, float> PropogateConsideration(Dictionary<DirT, float> considerations);
}
