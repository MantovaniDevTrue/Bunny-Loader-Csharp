using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BunnyLoader.Source;

/// <summary>
/// Sistema global de um mod C#. Hooks de ciclo de vida ocorrem nas transicoes;
/// hooks de atualizacao podem executar em todos os quadros quando sobrescritos.
/// </summary>
public abstract class ModSystem
{
    public virtual void Load() { }
    public virtual void OnModLoad() { }
    public virtual void ResizeArrays() { }
    public virtual void PostSetupContent() { }
    public virtual void OnWorldLoad() { }
    public virtual void ClearWorld() { }
    public virtual void PreWorldGen() { }
    public virtual void PostWorldGen() { }
    public virtual void PreSaveAndQuit() { }
    public virtual void PreUpdateWorld() { }
    public virtual void PostUpdateWorld() { }
    public virtual void PreUpdateTime() { }
    public virtual void PostUpdateTime() { }
    public virtual void PreUpdateEntities() { }
    public virtual void PostUpdateEverything() { }
    public virtual void PreUpdateProjectiles() { }
    public virtual void PreUpdateDusts() { }
    public virtual void PostUpdateDusts() { }
    public virtual void PreUpdateInvasions() { }
    public virtual void PostUpdateInvasions() { }
    public virtual void PostUpdateInput() { }
    public virtual void PreUpdatePlayers() { }
    public virtual void PostUpdatePlayers() { }
    public virtual void PreUpdateNPCs() { }
    public virtual void PostUpdateNPCs() { }
    public virtual void PreUpdateGores() { }
    public virtual void PostUpdateGores() { }
    public virtual void PostUpdateProjectiles() { }
    public virtual void PreUpdateItems() { }
    public virtual void PostUpdateItems() { }
    public virtual void OnWorldUnload() { }
    public virtual void OnModUnload() { }
    public virtual void Unload() { }
}

/// <summary>
/// Despacha hooks para classes ModSystem da assembly que realmente contem o mod.
/// A ponte nativa chama este metodo somente nas transicoes correspondentes.
/// </summary>
public static class ModSystemLoader
{
    private sealed class Entry
    {
        public readonly ModSystem System;
        public readonly ulong OverrideMask;
        public bool Loaded;
        public Entry(ModSystem system, ulong mask)
        {
            System = system;
            OverrideMask = mask;
        }
    }

    private sealed class Registry
    {
        public readonly Entry[] Entries;
        public bool ModLoaded;
        public bool WorldLoaded;
        public bool ArraysResized;
        public bool ContentSetup;
        public bool Unloaded;
        public int ConstructionFailures;
        public Registry(Entry[] entries, int failures)
        {
            Entries = entries;
            ConstructionFailures = failures;
        }
    }

    private static readonly object Sync = new();
    private static readonly Dictionary<Assembly, Registry> Registries = new();
    private const int MaxSystemsPerMod = 256;
    private static readonly int[] UpdateBitIndexes =
    {
        24, 25, 26, 27, 32, 28, 39, 43, 44, 45, 46, 31,
        33, 34, 35, 36, 37, 38, 40, 41, 42
    };
    private static readonly (string Name, Action<ModSystem> Call)[] UpdateHooks =
    {
        ("PreUpdateWorld", x => x.PreUpdateWorld()),
        ("PostUpdateWorld", x => x.PostUpdateWorld()),
        ("PreUpdateTime", x => x.PreUpdateTime()),
        ("PostUpdateTime", x => x.PostUpdateTime()),
        ("PreUpdateEntities", x => x.PreUpdateEntities()),
        ("PostUpdateEverything", x => x.PostUpdateEverything()),
        ("PreUpdateProjectiles", x => x.PreUpdateProjectiles()),
        ("PreUpdateDusts", x => x.PreUpdateDusts()),
        ("PostUpdateDusts", x => x.PostUpdateDusts()),
        ("PreUpdateInvasions", x => x.PreUpdateInvasions()),
        ("PostUpdateInvasions", x => x.PostUpdateInvasions()),
        ("PostUpdateInput", x => x.PostUpdateInput()),
        ("PreUpdatePlayers", x => x.PreUpdatePlayers()),
        ("PostUpdatePlayers", x => x.PostUpdatePlayers()),
        ("PreUpdateNPCs", x => x.PreUpdateNPCs()),
        ("PostUpdateNPCs", x => x.PostUpdateNPCs()),
        ("PreUpdateGores", x => x.PreUpdateGores()),
        ("PostUpdateGores", x => x.PostUpdateGores()),
        ("PostUpdateProjectiles", x => x.PostUpdateProjectiles()),
        ("PreUpdateItems", x => x.PreUpdateItems()),
        ("PostUpdateItems", x => x.PostUpdateItems())
    };
    private static readonly string[] OriginalHookNames =
    {
        "Load",
        "OnModLoad",
        "OnModUnload",
        "OnLocalizationsLoaded",
        "ResizeArrays",
        "AddRecipeGroups",
        "AddRecipes",
        "PostAddRecipes",
        "PostSetupRecipes",
        "PostSetupContent",
        "OnWorldLoad",
        "LoadWorldData",
        "PostWorldLoad",
        "SaveWorldData",
        "SaveWorldHeader",
        "CanWorldBePlayed",
        "WorldCanBePlayedRejectionMessage",
        "PreWorldGen",
        "ModifyWorldGenTasks",
        "PostWorldGen",
        "ModifyHardmodeTasks",
        "ClearWorld",
        "PreSaveAndQuit",
        "OnWorldUnload",
        "PreUpdateWorld",
        "PostUpdateWorld",
        "PreUpdateTime",
        "PostUpdateTime",
        "PostUpdateEverything",
        "UpdateUI",
        "ModifyInterfaceLayers",
        "PostUpdateInput",
        "PreUpdateEntities",
        "PreUpdatePlayers",
        "PostUpdatePlayers",
        "PreUpdateNPCs",
        "PostUpdateNPCs",
        "PreUpdateGores",
        "PostUpdateGores",
        "PreUpdateProjectiles",
        "PostUpdateProjectiles",
        "PreUpdateItems",
        "PostUpdateItems",
        "PreUpdateDusts",
        "PostUpdateDusts",
        "PreUpdateInvasions",
        "PostUpdateInvasions",
        "ModifyTimeRate",
        "ModifySunLightColor",
        "ModifyLightingBrightness",
        "ModifyScreenPosition",
        "ModifyTransformMatrix",
        "ResetNearbyTileEffects",
        "TileCountsAvailable",
        "NetSend",
        "NetReceive",
        "HijackGetData",
        "HijackSendData"
    };

    /// <summary>
    /// Mapa de overrides dos 58 hooks originais, em ordem fixada no
    /// ModSystem.js do Bunny. Um bit so entra se uma instancia ativa o sobrescreve.
    /// Usado pelo host para nao consultar Mono em hooks sem assinantes.
    /// </summary>
    private static ulong ResolveHookMask(Type type)
    {
        ulong mask = 0;
        for (int index = 0; index < OriginalHookNames.Length; index++)
        {
            MethodInfo? original = typeof(ModSystem).GetMethod(
                OriginalHookNames[index], BindingFlags.Public | BindingFlags.Instance);
            if (original == null) continue;
            MethodInfo? method = type.GetMethod(OriginalHookNames[index],
                BindingFlags.Public | BindingFlags.Instance);
            if (method != null && method.DeclaringType != typeof(ModSystem) &&
                method.GetBaseDefinition().Equals(original))
                mask |= 1UL << index;
        }
        return mask;
    }

    public static ulong HookMask(Assembly assembly)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        lock (Sync)
        {
            if (!Registries.TryGetValue(assembly, out Registry? registry) ||
                !registry.ModLoaded) return 0;
            ulong mask = 0;
            foreach (Entry entry in registry.Entries)
                if (entry.Loaded) mask |= entry.OverrideMask;
            return mask;
        }
    }

    private static Registry Create(Assembly assembly)
    {
        Type[] classes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters &&
                typeof(ModSystem).IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToArray();
        if (classes.Length > MaxSystemsPerMod)
            throw new InvalidOperationException("ModSystem: limite de 256 classes por mod");
        var entries = new List<Entry>(classes.Length);
        int failures = 0;
        foreach (Type type in classes)
        {
            try
            {
                if (type.GetConstructor(Type.EmptyTypes) == null)
                    throw new InvalidOperationException("Construtor publico sem parametros obrigatorio");
                entries.Add(new Entry((ModSystem)Activator.CreateInstance(type)!,
                    ResolveHookMask(type)));
            }
            catch (Exception error)
            {
                failures++;
                Console.Error.WriteLine("[CSharp][ModSystem] " + type.FullName +
                    " construcao falhou: " + error);
            }
        }
        return new Registry(entries.ToArray(), failures);
    }

    private static int Invoke(Entry entry, Action<ModSystem> callback, string phase)
    {
        try
        {
            callback(entry.System);
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("[CSharp][ModSystem] " +
                entry.System.GetType().FullName + "." + phase + " falhou: " + error);
            return 1;
        }
    }

    /// <summary>
    /// 0=Load, 1=OnModLoad, 2=OnWorldLoad, 3=OnWorldUnload,
    /// 4=OnModUnload, 5=Unload, 6=ClearWorld, 7=PreSaveAndQuit,
    /// 8=PreWorldGen, 9=PostWorldGen, 10..30=hooks de atualizacao.
    /// 31=ResizeArrays, 32=PostSetupContent.
    /// Retorna o numero de falhas isoladas.
    /// </summary>
    public static int Dispatch(Assembly assembly, int phase)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        if ((uint)phase > 32) throw new ArgumentOutOfRangeException(nameof(phase));
        lock (Sync)
        {
            if (!Registries.TryGetValue(assembly, out Registry? registry))
            {
                if (phase >= 3) return 0;
                registry = Create(assembly);
                Registries.Add(assembly, registry);
            }
            if (registry.Unloaded) return 0;
            int failures = registry.ConstructionFailures;
            registry.ConstructionFailures = 0;

            if (!registry.ModLoaded && phase <= 2)
            {
                registry.ModLoaded = true;
                foreach (Entry entry in registry.Entries)
                {
                    int failed = Invoke(entry, x => x.Load(), nameof(ModSystem.Load));
                    entry.Loaded = failed == 0;
                    failures += failed;
                }
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded)
                        failures += Invoke(entry, x => x.OnModLoad(), nameof(ModSystem.OnModLoad));
            }

            if (phase == 31 && registry.ModLoaded && !registry.ArraysResized)
            {
                registry.ArraysResized = true;
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded && (entry.OverrideMask & (1UL << 4)) != 0)
                        failures += Invoke(entry, x => x.ResizeArrays(), nameof(ModSystem.ResizeArrays));
            }
            if (phase == 32 && registry.ModLoaded && !registry.ContentSetup)
            {
                registry.ContentSetup = true;
                if (!registry.ArraysResized)
                {
                    registry.ArraysResized = true;
                    foreach (Entry entry in registry.Entries)
                        if (entry.Loaded && (entry.OverrideMask & (1UL << 4)) != 0)
                            failures += Invoke(entry, x => x.ResizeArrays(), nameof(ModSystem.ResizeArrays));
                }
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded && (entry.OverrideMask & (1UL << 9)) != 0)
                        failures += Invoke(entry, x => x.PostSetupContent(), nameof(ModSystem.PostSetupContent));
            }

            if (phase >= 10 && phase <= 30 && registry.ModLoaded &&
                (registry.WorldLoaded || phase == 21))
            {
                var hook = UpdateHooks[phase - 10];
                ulong hookBit = 1UL << UpdateBitIndexes[phase - 10];
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded && (entry.OverrideMask & hookBit) != 0)
                        failures += Invoke(entry, hook.Call, hook.Name);
            }
            if (phase == 6 && registry.ModLoaded)
            {
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded)
                        failures += Invoke(entry, x => x.ClearWorld(), nameof(ModSystem.ClearWorld));
            }
            if (phase == 8 && registry.ModLoaded)
            {
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded)
                        failures += Invoke(entry, x => x.PreWorldGen(), nameof(ModSystem.PreWorldGen));
            }
            if (phase == 9 && registry.ModLoaded)
            {
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded)
                        failures += Invoke(entry, x => x.PostWorldGen(), nameof(ModSystem.PostWorldGen));
            }
            if (phase == 7 && registry.WorldLoaded)
            {
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded)
                        failures += Invoke(entry, x => x.PreSaveAndQuit(), nameof(ModSystem.PreSaveAndQuit));
            }
            if (phase == 2 && registry.ModLoaded && !registry.WorldLoaded)
            {
                registry.WorldLoaded = true;
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded)
                        failures += Invoke(entry, x => x.OnWorldLoad(), nameof(ModSystem.OnWorldLoad));
            }
            if ((phase == 3 || phase == 4 || phase == 5) && registry.WorldLoaded)
            {
                registry.WorldLoaded = false;
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded)
                        failures += Invoke(entry, x => x.OnWorldUnload(), nameof(ModSystem.OnWorldUnload));
            }
            if ((phase == 4 || phase == 5) && registry.ModLoaded)
            {
                registry.ModLoaded = false;
                foreach (Entry entry in registry.Entries)
                    if (entry.Loaded)
                        failures += Invoke(entry, x => x.OnModUnload(), nameof(ModSystem.OnModUnload));
            }
            if (phase == 5)
            {
                registry.Unloaded = true;
                for (int i = registry.Entries.Length - 1; i >= 0; i--)
                    if (registry.Entries[i].Loaded)
                        failures += Invoke(registry.Entries[i], x => x.Unload(), nameof(ModSystem.Unload));
                Registries.Remove(assembly);
            }
            return failures;
        }
    }
}
