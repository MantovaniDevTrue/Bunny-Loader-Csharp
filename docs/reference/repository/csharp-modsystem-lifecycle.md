# ModSystem em C# — integração de ciclo de vida

> **Auditoria de escopo:** ciclo de vida parcial, NÃO o ModSystem completo. Dos 58 métodos originais, 9 estão declarados, 49 faltam; `Unload` é adicional. Ver [matriz de cobertura](csharp-modsystem-coverage.md). Nunca anunciar o loader como concluído nem gerar APK pela aprovação do CI isolado.


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

## Teste isolado de dois mods

`system_lifecycle/ForeignMod` é uma segunda DLL gerenciada, compilada como projeto próprio. Os testes criam duas assemblies separadas, forçam falha de construtor em um dos mods, verificam que os outros continuam e conferem a reentrada após `Unload` com estado por assembly. O teste Android continua obrigatório.

## Gate do ModSystem antes do mundo

Mods C# com pelo menos um ModSystem agora preparam Mono e exigem despacho confirmado de Load/OnModLoad antes da entrada no mundo; falhas temporárias de inicialização retornam à fila leve de retry, sem reinvocar Load após sucesso. Mods sem ModSystem não pagam o custo dessa etapa. A integração utiliza o estado systemsLoaded protegido pelo mutex de status e não o FPS de jogo para executar Load no mundo. **Ainda exige validação Android do gate e dos possíveis estados de falha, e não substitui o teste da ABI nativa.**

## ClearWorld e PreSaveAndQuit

O `ClearWorld` C# executa depois do `WorldGen.clearWorld()` original, e `PreSaveAndQuit` antes de `WorldGen.SaveAndQuit()` original quando a sessão está ativa, conforme os hooks existentes no SystemLoader.js. O primeiro ensaio de regressão encontrou a condição `phase >= 4` descarregando indevidamente o mod durante PreSaveAndQuit; a condição foi corrigida para aceitar somente as fases de unload (3/4/5). Ambos exigem validação de ABI, compilação Android e QA real.

## Eventos reais de geracao do mundo

A ponte em C++ usa `Terraria.WorldBuilding.WorldGenerator.GenerateWorld()` do mesmo ponto de extensão do `SystemWorldHooks.js` original. `PreWorldGen` é chamado antes da função original e `PostWorldGen` somente se ela retorna `true`. O método permanece sem suporte a `ModifyWorldGenTasks` porque esse requer representação C# real de GenPass, lista mutável e aplicação segura na thread do gerador. Sem essa API, não declarar worldgen completo. Falta confirmar a assinatura nativa e a ordem de callbacks num mundo novo no Android.

## Threads do Mono em hooks nativos

No despacho de `ModSystem`, a ponte C++ exige `mono_thread_current` e `mono_thread_detach`, detecta se a thread já pertence ao domínio e desanexa somente as threads anexadas por esta própria chamada. Isso evita deixar threads temporárias de worldgen anexadas indefinidamente ao Mono. O lock global de métodos é solto antes de chamar a função do mod. É necessário testar especificamente criação de mundo e cancelamento da geração no Android; a implementação em fonte não prova ausência de crash em ARM64.

## Máscara de overrides

O compilador gera uma entrada gerenciada por assembly que informa uma máscara de 58 bits dos hooks originais. Apenas `override` verdadeiros (`GetBaseDefinition`) de instâncias carregadas contam; métodos com o mesmo nome que apenas escondem a base não são registrados. O host guarda a máscara em cache e ignora `ClearWorld`, `PreSaveAndQuit`, `PreWorldGen` e `PostWorldGen` quando nenhum mod os sobrescreve. Isso elimina chamadas Mono desnecessárias sem remover callbacks existentes. A integração de fases de alto custo por frame depende desse mecanismo e de benchmark Android.

O `Unload` também atualiza a máscara agregada de hooks: quando um mod sai, os bits que não pertencem mais a nenhum outro mod são retirados. Nenhum hook de outro mod é desligado por causa desse descarregamento.
