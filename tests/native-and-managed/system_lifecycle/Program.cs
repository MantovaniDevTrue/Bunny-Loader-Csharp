using System;
using System.Collections.Generic;
using System.Reflection;
using BunnyLoader.Source;
using BunnyLoader.IsolatedWorld;

namespace BunnyLoader.SystemLifecycleTests;

internal static class Events
{
    internal static readonly List<string> Items = new();
    internal static void Add(string value) => Items.Add(value);
}

public sealed class AlphaSystem : ModSystem
{
    public override void Load() => Events.Add("A.Load");
    public override void OnModLoad() => Events.Add("A.OnModLoad");
    public override void OnWorldLoad() => Events.Add("A.OnWorldLoad");
    public override void ClearWorld() => Events.Add("A.ClearWorld");
    public override void PreWorldGen() => Events.Add("A.PreWorldGen");
    public override void PostWorldGen() => Events.Add("A.PostWorldGen");
    public override void PreSaveAndQuit() => Events.Add("A.PreSaveAndQuit");
    public override void OnWorldUnload() => Events.Add("A.OnWorldUnload");
    public override void OnModUnload() => Events.Add("A.OnModUnload");
    public override void Unload() => Events.Add("A.Unload");
}

public sealed class BetaSystem : ModSystem
{
    public override void Load() => Events.Add("B.Load");
    public override void OnModLoad() => Events.Add("B.OnModLoad");
    public override void OnWorldLoad()
    {
        Events.Add("B.OnWorldLoad");
        throw new InvalidOperationException("Falha proposital: outros sistemas devem continuar");
    }
    public override void ClearWorld() => Events.Add("B.ClearWorld");
    public override void PreWorldGen() => Events.Add("B.PreWorldGen");
    public override void PostWorldGen() => Events.Add("B.PostWorldGen");
    public override void PreSaveAndQuit() => Events.Add("B.PreSaveAndQuit");
    public override void OnWorldUnload() => Events.Add("B.OnWorldUnload");
    public override void OnModUnload() => Events.Add("B.OnModUnload");
    public override void Unload() => Events.Add("B.Unload");
}

internal static class Program
{
    private static void Expect(string expected)
    {
        string actual = string.Join(",", Events.Items);
        if (actual != expected)
            throw new Exception("Sequencia diferente. Esperado=" + expected + " Obtido=" + actual);
    }

    private static int Main()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        if (ModSystemLoader.Dispatch(assembly, 0) != 0) return 1;
        if (ModSystemLoader.Dispatch(assembly, 0) != 0) return 2;
        ulong expectedMask = 0;
        foreach (int bit in new[] {0, 1, 2, 10, 17, 19, 21, 22, 23})
            expectedMask |= 1UL << bit;
        if ((ModSystemLoader.HookMask(assembly) & expectedMask) != expectedMask) return 30;
        Expect("A.Load,B.Load,A.OnModLoad,B.OnModLoad");
        if (ModSystemLoader.Dispatch(assembly, 6) != 0) return 21;
        if (ModSystemLoader.Dispatch(assembly, 8) != 0) return 26;
        if (ModSystemLoader.Dispatch(assembly, 9) != 0) return 27;
        if (ModSystemLoader.Dispatch(assembly, 2) != 1) return 3;
        if (ModSystemLoader.Dispatch(assembly, 7) != 0) return 22;
        if (ModSystemLoader.Dispatch(assembly, 2) != 0) return 4;
        if (ModSystemLoader.Dispatch(assembly, 3) != 0) return 5;
        if (ModSystemLoader.Dispatch(assembly, 7) != 0) return 23;
        if (ModSystemLoader.Dispatch(assembly, 3) != 0) return 6;
        if (ModSystemLoader.Dispatch(assembly, 6) != 0) return 24;
        if (ModSystemLoader.Dispatch(assembly, 8) != 0) return 28;
        if (ModSystemLoader.Dispatch(assembly, 9) != 0) return 29;
        if (ModSystemLoader.Dispatch(assembly, 2) != 1) return 7;
        if (ModSystemLoader.Dispatch(assembly, 7) != 0) return 25;
        if (ModSystemLoader.Dispatch(assembly, 5) != 0) return 8;
        Expect("A.Load,B.Load,A.OnModLoad,B.OnModLoad," +
            "A.ClearWorld,B.ClearWorld,A.PreWorldGen,B.PreWorldGen," +
            "A.PostWorldGen,B.PostWorldGen,A.OnWorldLoad,B.OnWorldLoad," +
            "A.PreSaveAndQuit,B.PreSaveAndQuit,A.OnWorldUnload,B.OnWorldUnload," +
            "A.ClearWorld,B.ClearWorld,A.PreWorldGen,B.PreWorldGen," +
            "A.PostWorldGen,B.PostWorldGen,A.OnWorldLoad,B.OnWorldLoad," +
            "A.PreSaveAndQuit,B.PreSaveAndQuit,A.OnWorldUnload,B.OnWorldUnload," +
            "A.OnModUnload,B.OnModUnload,B.Unload,A.Unload");
        if (ModSystemLoader.Dispatch(assembly, 3) != 0) return 9;

        // Outra DLL representa outro mod, com erros isolados da primeira.
        Assembly foreign = typeof(ForeignSystem).Assembly;
        if (ModSystemLoader.HookMask(foreign) != 0) return 31;
        if (ModSystemLoader.Dispatch(foreign, 2) != 1) return 10;
        ulong foreignMask = ModSystemLoader.HookMask(foreign);
        if ((foreignMask & (1UL << 21)) != 0) return 32;
        if ((foreignMask & (1UL << 17)) != 0) return 33;
        if (ForeignSystem.Loads != 1 || ForeignSystem.WorldLoads != 1) return 11;
        if (ModSystemLoader.Dispatch(foreign, 2) != 0) return 12;
        if (ForeignSystem.WorldLoads != 1) return 13;
        if (ModSystemLoader.Dispatch(foreign, 3) != 0) return 14;
        if (ModSystemLoader.Dispatch(foreign, 5) != 0) return 15;
        if (ForeignSystem.WorldUnloads != 1 ||
            ForeignSystem.ModUnloads != 1 || ForeignSystem.Unloads != 1) return 16;
        if (ModSystemLoader.Dispatch(foreign, 2) != 1) return 17;
        if (ForeignSystem.Loads != 2 || ForeignSystem.WorldLoads != 2) return 18;
        if (ModSystemLoader.Dispatch(foreign, 5) != 0) return 19;
        if (ForeignSystem.WorldUnloads != 2 ||
            ForeignSystem.ModUnloads != 2 || ForeignSystem.Unloads != 2) return 20;
        Console.WriteLine("MODSYSTEM_LIFECYCLE_HOST=PASS");
        Console.WriteLine("MODSYSTEM_LIFECYCLE_MULTIMOD=PASS");
        return 0;
    }
}
