using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public interface IAnimStyleComponent<StyleType> : IAnimComponent
{
    //public void PlayStyledAnim(string baseAnimName, StyleType style);
    public StyleType GetStyle();
    public void SetStyle(StyleType style);
    public void ResetStyle();
}
