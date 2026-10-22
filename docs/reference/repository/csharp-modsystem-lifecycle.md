# ModSystem em C# — integração de ciclo de vida

Este módulo usa o nome e as assinaturas de eventos `ModSystem` do tModLoader, mas a execução é implementada diretamente no SDK C# do Bunny Loader, sem executar JavaScript.

## API

A classe `BunnyLoader.Source.ModSystem` oferece `Load`, `OnModLoad`, `OnWorldLoad`, `OnWorldUnload`, `OnModUnload` e `Unload`. O `ModSystemLoader` cria uma instância por classe concreta e por assembly, ordenadas pelo nome completo, e executa cada etapa somente na transição correspondente. Falhas de um sistema não bloqueiam outros. Limite: 256 sistemas por mod.

`Load` precede `OnModLoad`. `OnWorldLoad` ocorre somente quando o jogador realmente entra no mundo; `OnWorldUnload` é emitido antes de sair do mundo. `OnModUnload` e `Unload` são reservados para descarregamento real do mod, nunca para uma simples saída do mundo.

Exemplo didático: `samples/ExampleModCSharp/Playable/content/Common/Systems/ExampleLifecycleSystem.cs`.

## Critérios antes de liberar APK

- [ ] Compilador Roslyn incorpora a entrada de despacho sem custo por frame.
- [ ] Parser e descoberta do conteúdo aceitam também mods só com ModSystem.
- [ ] Mono executa hooks por mod, sem confundir assemblies, com tratamento de falhas.
- [ ] Host aciona `OnWorldLoad` e `OnWorldUnload` exatamente uma vez em cada ciclo, inclusive troca/reentrada.
- [ ] Descarregamento real do mod aciona `OnModUnload` e `Unload`, em ordem inversa e com isolamento.
- [ ] Testes de host, CI e Android comprovam ordem, multi-mod, reentrada e ausência de regressões.
- [ ] Confirmação no Terraria Android do exemplo e FPS antes da conclusão.

A existência do código-fonte ou do teste gerenciado NÃO é validação no aparelho. Nenhum APK deve ser publicado enquanto houver itens pendentes.
