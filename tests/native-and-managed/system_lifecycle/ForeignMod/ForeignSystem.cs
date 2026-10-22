using BunnyLoader.Source;
using System;

namespace BunnyLoader.IsolatedWorld;

public sealed class ForeignSystem : ModSystem
{
    public static int Loads;
    public static int WorldLoads;
    public static int WorldUnloads;
    public static int ModUnloads;
    public static int Unloads;
    public override void Load() => Loads++;
    public override void OnWorldLoad() => WorldLoads++;
    public override void OnWorldUnload() => WorldUnloads++;
    public override void OnModUnload() => ModUnloads++;
    public override void Unload() => Unloads++;
}

public sealed class BrokenSystem : ModSystem
{
    public BrokenSystem() => throw new InvalidOperationException("Teste de falha isolada");
}
