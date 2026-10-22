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

    public override void AddRecipes()
    {
        BunnySystemRecipes.CreateRecipe("ExampleItem", 1)
            .AddIngredient(BunnyVanillaItems.DirtBlock, 100)
            .AddTile(BunnyVanillaTiles.WorkBenches)
            .Register();
    }

    public override void PostAddRecipes()
    {
        Console.WriteLine("[ExampleMod C#] registro nativo de receitas concluido");
    }

    public override void PostSetupRecipes()
    {
        Console.WriteLine("[ExampleMod C#] receitas prontas para crafting");
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

    private int nearbyCount;

    private long lastUiTicks;

    public override void UpdateUI(BunnySystemGameTime gameTime)
    {
        lastUiTicks = gameTime.TotalGameTime.Ticks;
    }

    public override void TileCountsAvailable(int[] counts)
    {
        nearbyCount = counts.Length > 754 ? counts[754] : 0;
    }

    public override void ResetNearbyTileEffects()
    {
        // Recomeça a contagem de bioma apenas na varredura do jogador local.
        // Não guarde arrays de SceneMetrics e não faça scans do mundo.
        nearbyCount = 0;
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

    public override void ModifySunLightColor(ref BunnySystemColor tiles,
                                             ref BunnySystemColor background)
    {
        tiles = new BunnySystemColor(tiles.R, tiles.G,
            (byte)Math.Min(255, tiles.B + 5), tiles.A);
    }

    public override void ModifyScreenPosition()
    {
        BunnySystemVector2 pos = BunnySystemCamera.ScreenPosition;
        BunnySystemCamera.ScreenPosition = new BunnySystemVector2(pos.X, pos.Y - 1f);
    }

    public override void ModifyLightingBrightness(ref float brightness)
    {
        // O efeito multiplica a luminosidade somente no mundo.
        // O dispatcher rejeita NaN e valores negativos e nao aloca por quadro.
        brightness *= 1.03f;
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

    public override void PostWorldLoad()
    {
        // Após OnWorldLoad: mundo pronto para lógica de pós-entrada.
        // Carga de dados persistentes terá hook separado.
        Console.WriteLine("[ExampleMod C#] post-world inicializado");
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
