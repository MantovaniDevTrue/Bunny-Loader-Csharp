using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BunnyLoader.Source;

/// <summary>
/// Contexto de receitas de ModSystem.AddRecipes. A geracao ocorre uma vez no
/// compilador de conteudo, antes dos descritores serem enviados ao Terraria.
/// </summary>
public static class BunnySystemRecipes
{
    [ThreadStatic] private static Dictionary<string, BunnyItem>? active;

    public static BunnyRecipe CreateRecipe(string resultItem, int stack = 1)
    {
        BunnyContentKey.Validate(resultItem, nameof(resultItem),
            "BunnySystemRecipes.CreateRecipe");
        Dictionary<string, BunnyItem> items = active ??
            throw new InvalidOperationException("CreateRecipe so pode ser usada em ModSystem.AddRecipes");
        if (resultItem.Contains('/'))
            throw new ArgumentException("Receita global deste mod deve usar nome local de BunnyItem",
                nameof(resultItem));
        if (!items.TryGetValue(resultItem, out BunnyItem? item))
            throw new ArgumentException("BunnyItem local nao encontrado: " + resultItem,
                nameof(resultItem));
        return item.CreateRecipe(stack);
    }

    public static int Generate(Assembly assembly, object[] content)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        if (content == null) throw new ArgumentNullException(nameof(content));
        if (active != null)
            throw new InvalidOperationException("Geracao de receitas reentrante");

        var items = new Dictionary<string, BunnyItem>(StringComparer.Ordinal);
        foreach (object entry in content)
        {
            if (entry is not BunnyItem item)
                throw new ArgumentException("Conteudo inclui tipo diferente de BunnyItem");
            if (!items.TryAdd(entry.GetType().Name, item))
                throw new InvalidOperationException("BunnyItem duplicado: " + entry.GetType().Name);
        }
        active = items;
        int executed = 0;
        try
        {
            Type[] types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters &&
                    typeof(ModSystem).IsAssignableFrom(t))
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToArray();
            if (types.Length > 256)
                throw new InvalidOperationException("ModSystem: mais de 256 sistemas");
            foreach (Type type in types)
            {
                MethodInfo? hook = type.GetMethod(nameof(ModSystem.AddRecipes),
                    BindingFlags.Public | BindingFlags.Instance);
                if (hook == null || hook.DeclaringType == typeof(ModSystem) ||
                    hook.GetBaseDefinition().DeclaringType != typeof(ModSystem))
                    continue;
                if (Activator.CreateInstance(type) is not ModSystem system)
                    throw new InvalidOperationException(type.FullName + " sem construtor publico");
                system.AddRecipes();
                executed++;
            }
        }
        finally
        {
            active = null;
        }
        return executed;
    }
}
