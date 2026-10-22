using System;
using BunnyLoader.Source;

namespace ExampleModCSharp.Common.Systems;

public sealed class ExampleLifecycleSystem : ModSystem
{
    private int visits;
    private int worldTicks;
    private int playerBatches;
    private bool insideWorld;

    public override void Load()
    {
        // Uma vez por carregamento do mod: inicialize dados globais aqui.
        visits = 0;
        worldTicks = 0;
        playerBatches = 0;
    }

    public override void ResizeArrays()
    {
        // Os tipos nativos estao registrados: redimensione caches proprios.
        Console.WriteLine("[ExampleMod C#] arrays de conteudo prontas");
    }

    public override void PostSetupContent()
    {
        // Uma vez apos instalar defaults, receitas e visuais de todos os mods.
        Console.WriteLine("[ExampleMod C#] conteudo C# totalmente preparado");
    }

    public override void OnLocalizationsLoaded()
    {
        // O Bunny chama novamente após trocar a cultura ativa do jogo.
        // Recupere textos localizados aqui; não releia todos os arquivos por frame.
        Console.WriteLine("[ExampleMod C#] idioma aplicado");
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

    public override void PreWorldGen()
    {
        // Antes do gerador nativo. Nao guarde referencias do mundo anterior.
        Console.WriteLine("[ExampleMod C#] inicio da geracao");
    }

    public override void PostWorldGen()
    {
        // Somente apos o gerador nativo retornar sucesso.
        Console.WriteLine("[ExampleMod C#] geracao concluida");
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

    public override void PreUpdateWorld()
    {
        // Chamado antes de WorldGen.UpdateWorld; sem alocacao nem log por frame.
        worldTicks++;
    }

    public override void PreUpdatePlayers()
    {
        // Uma vez antes do grupo de jogadores, sem fazer varreduras por jogador.
        playerBatches++;
    }

    public override void PostUpdatePlayers()
    {
        // A atualização vanilla do grupo terminou; sem log por frame.
    }

    public override void PostUpdateWorld()
    {
        // Chamado depois da atualizacao vanilla do mundo.
        // A contagem e local ao mundo e serve como exemplo de estado persistente em RAM.
        if (!insideWorld) worldTicks = 0;
    }

    public override void OnWorldUnload()
    {
        // Nao mantenha referencias para entidades ou tiles de um mundo anterior.
        insideWorld = false;
        Console.WriteLine("[ExampleMod C#] saida do mundo; lotes de jogadores=" + playerBatches);
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
