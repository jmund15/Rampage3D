using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public interface IAnimDirectionalComponent<DirectionType>
{
    public DirectionType GetDirection();
    public void SetDirection(DirectionType style);
}
