using System;
using System.Collections.Generic;
using System.Reflection;
using BunnyLoader.Source;
using BunnyLoader.IsolatedWorld;
using BunnyLoader.SourceCompiler;

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
    public override void OnLocalizationsLoaded() => Events.Add("A.OnLocalizationsLoaded");
    public override void ModifySunLightColor(ref BunnySystemColor tiles, ref BunnySystemColor background)
    {
        tiles = new BunnySystemColor(10, 20, 30, tiles.A);
        background = new BunnySystemColor(background.R, 80, 90, background.A);
    }
    public override void ModifyScreenPosition()
    {
        BunnySystemVector2 p = BunnySystemCamera.ScreenPosition;
        BunnySystemCamera.ScreenPosition = new BunnySystemVector2(p.X + 16, p.Y - 8);
    }
    public override void ModifyLightingBrightness(ref float brightness)
    {
        brightness = brightness * 0.8f;
    }
    public override void ResetNearbyTileEffects() => Events.Add("A.ResetNearbyTileEffects");
    public override void ResizeArrays() => Events.Add("A.ResizeArrays");
    public override void PostSetupContent() => Events.Add("A.PostSetupContent");
    public override void PostAddRecipes() => Events.Add("A.PostAddRecipes");
    public override void PostSetupRecipes() => Events.Add("A.PostSetupRecipes");
    public override void OnWorldLoad() => Events.Add("A.OnWorldLoad");
    public override void PostWorldLoad() => Events.Add("A.PostWorldLoad");
    public override void ClearWorld() => Events.Add("A.ClearWorld");
    public override void PreWorldGen() => Events.Add("A.PreWorldGen");
    public override void PreUpdatePlayers() => Events.Add("A.PreUpdatePlayers");
    public override void PostUpdatePlayers() => Events.Add("A.PostUpdatePlayers");
    public override void PreUpdateNPCs() => Events.Add("A.PreUpdateNPCs");
    public override void PostUpdateNPCs() => Events.Add("A.PostUpdateNPCs");
    public override void PreUpdateGores() => Events.Add("A.PreUpdateGores");
    public override void PostUpdateGores() => Events.Add("A.PostUpdateGores");
    public override void PostUpdateProjectiles() => Events.Add("A.PostUpdateProjectiles");
    public override void PreUpdateItems() => Events.Add("A.PreUpdateItems");
    public override void PostUpdateItems() => Events.Add("A.PostUpdateItems");
    public override void PreUpdateWorld() => Events.Add("A.PreUpdateWorld");
    public override void PostUpdateWorld() => Events.Add("A.PostUpdateWorld");
    public override void PreUpdateTime() => Events.Add("A.PreUpdateTime");
    public override void PostUpdateTime() => Events.Add("A.PostUpdateTime");
    public override void PreUpdateEntities() => Events.Add("A.PreUpdateEntities");
    public override void PostUpdateEverything() => Events.Add("A.PostUpdateEverything");
    public override void PreUpdateProjectiles() => Events.Add("A.PreUpdateProjectiles");
    public override void PreUpdateDusts() => Events.Add("A.PreUpdateDusts");
    public override void PostUpdateDusts() => Events.Add("A.PostUpdateDusts");
    public override void PreUpdateInvasions() => Events.Add("A.PreUpdateInvasions");
    public override void PostUpdateInvasions() => Events.Add("A.PostUpdateInvasions");
    public override void PostUpdateInput() => Events.Add("A.PostUpdateInput");
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
    public override void ModifySunLightColor(ref BunnySystemColor tile, ref BunnySystemColor sky)
    {
        tile = new BunnySystemColor((byte)(tile.R + 1), tile.G, tile.B, tile.A);
    }
    public override void ModifyScreenPosition()
    {
        BunnySystemCamera.ScreenPosition = new BunnySystemVector2(float.NaN, 0);
    }
    public override void OnWorldUnload() => Events.Add("B.OnWorldUnload");
    public override void OnModUnload() => Events.Add("B.OnModUnload");
    public override void Unload() => Events.Add("B.Unload");
}

public sealed class RecipeTrialItem : BunnyItem
{
    public override void SetDefaults()
    {
        DisplayName = "Recipe Trial";
        Texture = "Content/Items/RecipeTrialItem.png";
        MaxStack = 99;
    }

    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient(BunnyVanillaItems.DirtBlock).Register();
    }
}

public sealed class RecipeTrialSystem : ModSystem
{
    public override void AddRecipes()
    {
        BunnySystemRecipes.CreateRecipe(nameof(RecipeTrialItem), 2)
            .AddIngredient(BunnyVanillaItems.DirtBlock, 3)
            .AddTile(BunnyVanillaTiles.WorkBenches)
            .Register();
    }
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
        var item = new RecipeTrialItem();
        item.SetDefaults();
        if (BunnySystemRecipes.Generate(assembly, new object[] {item}) != 1) return 51;
        if (item.RecipeDefinitions.Count != 2) return 52;
        string serialized = BasicItemWriter.Write(item);
        if (!serialized.Contains("1,-1,v2:1;2,18,v2:3", StringComparison.Ordinal)) return 53;
        if (item.RecipeDefinitions.Count != 2) return 54;
        try
        {
            BunnySystemRecipes.CreateRecipe(nameof(RecipeTrialItem));
            return 55;
        }
        catch (InvalidOperationException) { }

        if (ModSystemLoader.Dispatch(assembly, 0) != 0) return 1;
        if (ModSystemLoader.Dispatch(assembly, 0) != 0) return 2;
        ulong expectedMask = 0;
        foreach (int bit in new[] {0, 1, 2, 10, 17, 19, 21, 22, 23})
            expectedMask |= 1UL << bit;
        if ((ModSystemLoader.HookMask(assembly) & expectedMask) != expectedMask) return 30;
        Expect("A.Load,B.Load,A.OnModLoad,B.OnModLoad");
        if (ModSystemLoader.ModifyLightingBrightness(assembly, 2f) != 2f) return 48;
        if (ModSystemLoader.Dispatch(assembly, 31) != 0) return 38;
        if (ModSystemLoader.Dispatch(assembly, 31) != 0) return 39;
        if (ModSystemLoader.Dispatch(assembly, 32) != 0) return 40;
        if (ModSystemLoader.Dispatch(assembly, 32) != 0) return 41;
        if (ModSystemLoader.Dispatch(assembly, 36) != 0) return 56;
        if (ModSystemLoader.Dispatch(assembly, 37) != 0) return 57;
        if (ModSystemLoader.Dispatch(assembly, 37) != 0) return 58;
        if (ModSystemLoader.Dispatch(assembly, 33) != 0) return 42;
        if (ModSystemLoader.Dispatch(assembly, 33) != 0) return 43;
        if (ModSystemLoader.Dispatch(assembly, 6) != 0) return 21;
        if (ModSystemLoader.Dispatch(assembly, 34) != 0) return 44;
        if (ModSystemLoader.Dispatch(assembly, 8) != 0) return 26;
        if (ModSystemLoader.Dispatch(assembly, 9) != 0) return 27;
        if (ModSystemLoader.Dispatch(assembly, 2) != 1) return 3;
        if (Math.Abs(ModSystemLoader.ModifyLightingBrightness(assembly, 2f) - 1.6f) > 0.0001f) return 49;
        ulong originalSun = 0xff324650UL << 32 | 0xff112233UL;
        ulong updatedSun = ModSystemLoader.ModifySunLightColor(assembly, originalSun);
        var tile = new BunnySystemColor((uint)updatedSun);
        var sky = new BunnySystemColor((uint)(updatedSun >> 32));
        if (tile.R != 11 || tile.G != 20 || tile.B != 30 || tile.A != 255 ||
            sky.R != 80 || sky.G != 80 || sky.B != 90 || sky.A != 255) return 59;
        ulong cameraRaw = (uint)BitConverter.SingleToInt32Bits(100f) |
            ((ulong)(uint)BitConverter.SingleToInt32Bits(60f) << 32);
        ulong cameraChanged = ModSystemLoader.ModifyScreenPosition(assembly, cameraRaw);
        ulong invalidCamera = 0x7fc0000000000000UL | (uint)BitConverter.SingleToInt32Bits(0f);
        if (ModSystemLoader.ModifyScreenPosition(assembly, invalidCamera) != invalidCamera) return 62;
        if (BitConverter.Int32BitsToSingle((int)cameraChanged) != 116f ||
            BitConverter.Int32BitsToSingle((int)(cameraChanged >> 32)) != 52f) return 60;
        try { _ = BunnySystemCamera.ScreenPosition; return 61; }
        catch (InvalidOperationException) { }
        if (!float.IsNaN(ModSystemLoader.ModifyLightingBrightness(assembly, float.NaN))) return 50;
        if (ModSystemLoader.Dispatch(assembly, 35) != 0) return 45;
        if (ModSystemLoader.Dispatch(assembly, 35) != 0) return 46;
        for (int phase = 10; phase <= 21; phase++)
            if (ModSystemLoader.Dispatch(assembly, phase) != 0) return 34;
        for (int phase = 22; phase <= 30; phase++)
            if (ModSystemLoader.Dispatch(assembly, phase) != 0) return 36;
        if (ModSystemLoader.Dispatch(assembly, 7) != 0) return 22;
        if (ModSystemLoader.Dispatch(assembly, 2) != 0) return 4;
        if (ModSystemLoader.Dispatch(assembly, 3) != 0) return 5;
        for (int phase = 10; phase <= 20; phase++)
            if (ModSystemLoader.Dispatch(assembly, phase) != 0) return 35;
        if (ModSystemLoader.Dispatch(assembly, 7) != 0) return 23;
        if (ModSystemLoader.Dispatch(assembly, 3) != 0) return 6;
        if (ModSystemLoader.Dispatch(assembly, 6) != 0) return 24;
        if (ModSystemLoader.Dispatch(assembly, 8) != 0) return 28;
        if (ModSystemLoader.Dispatch(assembly, 9) != 0) return 29;
        if (ModSystemLoader.Dispatch(assembly, 2) != 1) return 7;
        if (ModSystemLoader.Dispatch(assembly, 35) != 0) return 47;
        for (int phase = 22; phase <= 30; phase++)
            if (ModSystemLoader.Dispatch(assembly, phase) != 0) return 37;
        if (ModSystemLoader.Dispatch(assembly, 7) != 0) return 25;
        if (ModSystemLoader.Dispatch(assembly, 5) != 0) return 8;
        Expect("A.Load,B.Load,A.OnModLoad,B.OnModLoad," +
            "A.ResizeArrays,A.PostSetupContent,A.PostAddRecipes,A.PostSetupRecipes," +
            "A.OnLocalizationsLoaded,A.OnLocalizationsLoaded," +
            "A.ClearWorld,B.ClearWorld,A.ResetNearbyTileEffects,A.PreWorldGen,B.PreWorldGen," +
            "A.PostWorldGen,B.PostWorldGen,A.OnWorldLoad,B.OnWorldLoad," +
            "A.PostWorldLoad," +
            "A.PreUpdateWorld," +
            "A.PostUpdateWorld," +
            "A.PreUpdateTime," +
            "A.PostUpdateTime," +
            "A.PreUpdateEntities," +
            "A.PostUpdateEverything," +
            "A.PreUpdateProjectiles," +
            "A.PreUpdateDusts," +
            "A.PostUpdateDusts," +
            "A.PreUpdateInvasions," +
            "A.PostUpdateInvasions," +
            "A.PostUpdateInput," +
            "A.PreUpdatePlayers," +
            "A.PostUpdatePlayers," +
            "A.PreUpdateNPCs," +
            "A.PostUpdateNPCs," +
            "A.PreUpdateGores," +
            "A.PostUpdateGores," +
            "A.PostUpdateProjectiles," +
            "A.PreUpdateItems," +
            "A.PostUpdateItems," +
            "A.PreSaveAndQuit,B.PreSaveAndQuit,A.OnWorldUnload,B.OnWorldUnload," +
            "A.ClearWorld,B.ClearWorld,A.PreWorldGen,B.PreWorldGen," +
            "A.PostWorldGen,B.PostWorldGen,A.OnWorldLoad,B.OnWorldLoad," +
            "A.PostWorldLoad," +
            "A.PreUpdatePlayers," +
            "A.PostUpdatePlayers," +
            "A.PreUpdateNPCs," +
            "A.PostUpdateNPCs," +
            "A.PreUpdateGores," +
            "A.PostUpdateGores," +
            "A.PostUpdateProjectiles," +
            "A.PreUpdateItems," +
            "A.PostUpdateItems," +
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
