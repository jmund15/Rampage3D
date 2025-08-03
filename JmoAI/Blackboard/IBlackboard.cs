using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IBlackboard
{
    public void SetParent(IBlackboard parent);
    public T GetVar<T>(BBDataSig bbVar) where T : class;
    public Error SetVar<T>(BBDataSig bbVar, T value) where T : class;

    public T? GetPrimVar<T>(BBDataSig bbPrimVar) where T : struct;
    public Error SetPrimVar<T>(BBDataSig bbPrimVar, T val) where T : struct;
}
/*
public interface IBlackboard<TKey> where TKey : Enum
{
    public T GetVar<T>(TKey bbVar) where T : class;
    public Error SetVar<T>(TKey bbVar, T value) where T : class;
    public T? GetPrimVar<T>(TKey bbVar) where T : struct;
    public Error SetPrimVar<T>(TKey bbVar, T val) where T : struct;
} */
