using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IConfigAnimComponent
{
    public event EventHandler<string> ConfigChanged;
    public void SetConfig(string type);
    public void IterateConfig();
    public string GetConfig();
    public List<string> GetConfigOptions();
    public void ResetConfig();
} 
