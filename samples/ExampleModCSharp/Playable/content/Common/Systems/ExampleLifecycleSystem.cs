using System;
using BunnyLoader.Source;

namespace ExampleModCSharp.Common.Systems;

public sealed class ExampleLifecycleSystem : ModSystem
{
    private int visits;
    private bool insideWorld;

    public override void Load()
    {
        // Uma vez por carregamento do mod: inicialize dados globais aqui.
        visits = 0;
    }

    public override void OnModLoad()
    {
        Console.WriteLine("[ExampleMod C#] sistemas carregados");
    }

    public override void ClearWorld()
    {
        // Executado depois de WorldGen.clearWorld; invalide caches de tiles.
        Console.WriteLine("[ExampleMod C#] mundo limpo");
    }

    public override void PreSaveAndQuit()
    {
        // Executado antes de WorldGen.SaveAndQuit; prepare o encerramento.
        Console.WriteLine("[ExampleMod C#] preparando save e saida");
    }

    public override void OnWorldLoad()
    {
        // Um evento por entrada efetiva no mundo, nao por frame.
        visits++;
        insideWorld = true;
        Console.WriteLine("[ExampleMod C#] entrada no mundo #" + visits);
    }

    public override void OnWorldUnload()
    {
        // Nao mantenha referencias para entidades ou tiles de um mundo anterior.
        insideWorld = false;
        Console.WriteLine("[ExampleMod C#] saida do mundo");
    }

    public override void OnModUnload()
    {
        Console.WriteLine("[ExampleMod C#] mod encerrando; mundo ativo=" + insideWorld);
    }

    public override void Unload()
    {
        visits = 0;
        insideWorld = false;
    }
}
