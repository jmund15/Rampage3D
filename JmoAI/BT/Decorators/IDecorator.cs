using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IDecorator
{
    public abstract void DecoratorEnter();
    public abstract void DecoratorPostProcess(TaskStatus postProcStatus);
    public abstract void DecoratorOnChangeStatus(TaskStatus newStatus);
    public abstract void DecoratorExit(); 
}
