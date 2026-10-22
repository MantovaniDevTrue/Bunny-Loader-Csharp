# ModSystem em C# — integração de ciclo de vida

Este módulo usa o nome e as assinaturas de eventos `ModSystem` do tModLoader, mas a execução é implementada diretamente no SDK C# do Bunny Loader, sem executar JavaScript.

## API

A classe `BunnyLoader.Source.ModSystem` oferece `Load`, `OnModLoad`, `OnWorldLoad`, `OnWorldUnload`, `OnModUnload` e `Unload`. O `ModSystemLoader` cria uma instância por classe concreta e por assembly, ordenadas pelo nome completo, e executa cada etapa somente na transição correspondente. Falhas de um sistema não bloqueiam outros. Limite: 256 sistemas por mod.

`Load` precede `OnModLoad`. `OnWorldLoad` ocorre somente quando o jogador realmente entra no mundo; `OnWorldUnload` é emitido antes de sair do mundo. `OnModUnload` e `Unload` são reservados para descarregamento real do mod, nunca para uma simples saída do mundo.

Exemplo didático: `samples/ExampleModCSharp/Playable/content/Common/Systems/ExampleLifecycleSystem.cs`.

## Critérios antes de liberar APK

- [x] Entrada de despacho incluida no compilador Roslyn; CI pendente.
- [x] Parser e descoberta implementados para mods so com ModSystem; CI pendente.
- [x] Codigo de ponte Mono por assembly e isolamento por sistema; teste Android pendente.
- [x] Conectado ao caminho de entrada/saida de mundo; teste Android pendente.
- [x] Hook terminal de QuitGame integrado no código-fonte, com isolamento e ordem inversa no Unload; validação no jogo pendente.
- [ ] Testes de host, CI e Android comprovam ordem, multi-mod, reentrada e ausência de regressões.
- [ ] Confirmação no Terraria Android do exemplo e FPS antes da conclusão.

A existência do código-fonte ou do teste gerenciado NÃO é validação no aparelho. Nenhum APK deve ser publicado enquanto houver itens pendentes.

## Segurança do dispatcher

O dispatcher mantém uma trava separada para serializar as transições dos sistemas. A trava global do runtime Mono é liberada antes da chamada gerenciada, permitindo que o mod utilize outras APIs sem bloqueio recursivo. As chamadas continuam eventuais, fora do ciclo por frame.

O hook C# de `Main.QuitGame()` executa `OnWorldUnload`, `OnModUnload` e `Unload` nessa ordem, conforme o ciclo terminal do loader JS de referência. A API não considera uma simples troca de mundo um descarregamento de mod. Se o método nativo não existir, o host informa a ausência e o APK continua bloqueado. Falta demonstrar no Android que o método nativo é realmente resolvido, que executa uma vez ao sair do jogo e que reentradas e múltiplos mods permanecem íntegros.
