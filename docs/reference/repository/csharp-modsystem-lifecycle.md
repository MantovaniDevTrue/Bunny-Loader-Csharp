# ModSystem em C# — integração de ciclo de vida

> **Auditoria de escopo:** ciclo de vida parcial, NÃO o ModSystem completo. Dos 58 métodos originais, 36 estão declarados, 22 faltam; `Unload` é adicional. Ver [matriz de cobertura](csharp-modsystem-coverage.md). Nunca anunciar o loader como concluído nem gerar APK pela aprovação do CI isolado.


Este módulo usa o nome e as assinaturas de eventos `ModSystem` do tModLoader, mas a execução é implementada diretamente no SDK C# do Bunny Loader, sem executar JavaScript.

## API

A classe `BunnyLoader.Source.ModSystem` já declara **36 dos 58 callbacks** do ModSystem original; `Unload` permanece um método adicional de limpeza. As categorias implementadas em fonte são: ciclo de vida, limpeza e geração básica do mundo, conclusão de registro de conteúdo, eventos de atualização e estágios ARM64 de jogadores/NPCs/gore/itens. Veja a [matriz auditável](csharp-modsystem-coverage.md) para os 22 ausentes, sem confundir declaração com validação Android.

O `ModSystemLoader` cria uma instância por classe concreta e por assembly, ordenadas pelo nome completo; callbacks de atualização só são chamados nas classes que efetivamente sobrescrevem o método. Falhas de uma classe não bloqueiam outras; máximo de 256 sistemas por mod.

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

## Hooks de atualizacao em lote

O SDK possui os 12 métodos `PreUpdateWorld`, `PostUpdateWorld`, `PreUpdateTime`, `PostUpdateTime`, `PreUpdateEntities`, `PostUpdateEverything`, `PreUpdateProjectiles`, `PreUpdateDusts`, `PostUpdateDusts`, `PreUpdateInvasions`, `PostUpdateInvasions`, `PostUpdateInput`. A ponte nativa intercepta `WorldGen.UpdateWorld`, `Main.UpdateTime`, `Main.DoUpdateInWorld`, `Main.PreUpdateAllProjectiles`, `Dust.UpdateDust`, `Main.UpdateInvasion` e `Main.DoUpdate_HandleInput`, conservando a ordem antes/depois do método original do Bunny. Os hooks nativos são instalados somente se uma classe ModSystem sobrescreveu o callback; a máscara de 58 bits impede invocações gerenciadas desnecessárias por frame. O gate pré-mundo não permite entrar se uma instalação requerida falhar. Os métodos que o Bunny implementa por system stages internos (Players/NPCs/Items/Gores) continuam pendentes, assim como todo o grupo de rede e persistência. Sem benchmark Android, não declarar 60 FPS nem lote fechado.

## Ponto de execução ARM64 dos grupos de entidades

Usamos os sete pontos nativos de `DoUpdateInWorld` documentados no `SystemHooks.cpp` do Bunny original (build Terraria 301720): `PlayersBegin`, `PlayersEnd`, `NPCsBegin`, `NPCsEnd`, `GoresEnd`, `ItemsBegin`, `ItemsEnd`. São 9 callbacks, pois dois pontos executam dois métodos em sequência. Um trampolim ARM64 guarda/restaura registradores inteiros, flags NZCV e registradores SIMD; a instalação confere quatro instruções originais no endereço/offset antes de alterar o fluxo. **Não depende de JS**, mas a coexistência de dois motores instalando o mesmo patchpoint ainda precisa de testes nativos. Se a assinatura falhar, o gate do mundo recusa a ativação em vez de executar hook inválido. Os exemplos de Pre/PostUpdatePlayers não alocam nem geram logs por quadro.

## Conclusão do registro C#

O `ResizeArrays` executa uma vez por mod após o registro dos tipos C# e o carregamento das assemblies Mono, usando a fase 31. A fase 32, `PostSetupContent`, exige todos os mods C# registrados, receitas/objetos/visuais nativos sem pendências e o gate de conteúdo preparado. O retorno gerenciado é isolado por classe; o sistema evita duplicações ao reentrar no menu. O gatilho de `ResizeArrays` não substitui o conteúdo do pipeline de grupos/receitas do tModLoader, ainda ausente; é uma implementação inicial de ciclo de vida que precisa de teste real da ordem de registro e das transições. Se o preload falhar, a entrada no mundo continua bloqueada, em vez de executar callbacks cedo demais.

## Auditoria de ABI do Main.QuitGame

A implementação JS original do Bunny invoca `original(self)` em `Main.QuitGame()`; a ponte C# agora define `QuitGameFn(Il2CppObject* self, const MethodInfo* method)`, preserva o primeiro argumento e o encaminha para a função original após as notificações de unload. O contrato é verificado pela suíte de CI; somente um teste no binário ARM64 real poderá confirmar a chamada em tempo de execução.

## Compatibilidade e critérios de aceite

- A implementação mantém o C# independente do JavaScript em runtime, mas os pontos de modificação do código nativo ARM64 ainda precisam ser testados quando um mod JS usa o mesmo endereço.
- O código atual de entrada no mundo é focalizado em modo individual (`netMode == 0`); o evento de mundo no cliente multiplayer do Bunny original não pode ser considerado validado.
- CI verde significa compilação gerenciada, testes contratuais, sintaxe C++/assembly ARM64 e auditoria de fontes. Não cobre ligação do app, execução no aparelho, integridade do save, FPS ou teste real do trampolim.
- Nunca gerar APK ou liberar PR enquanto faltarem os 22 métodos, a compatibilidade de hooks e a validação nativa no Terraria 301720.

## Localização e contagem de biomas

`OnLocalizationsLoaded` é disparado após a preparação inicial do conteúdo C# e depois de cada execução de `LanguageManager.LoadFilesForCulture(GameCulture)` via hook nativo IL2CPP de instância. A implementação é habilitada somente quando um sistema sobrescreve o hook; os textos JSON específicos do C# ainda precisam de validação de aplicação em jogo e de integração com o registro nativo de localizações.

`ResetNearbyTileEffects` usa `SceneMetrics.Reset()` original, compara a instância com `Main.get_PlayerSceneMetrics()` e não chama sistemas para contagens da câmera ou de outros painéis. Também limpa o cache quando `clearWorld` conclui e antes de `SaveAndQuit`. São callbacks do ciclo do jogo, sem varredura adicional de blocos. A assinatura, a ordem e a coexistência com hooks JS exigem teste no Terraria 301720 em Android ARM64.

## PostWorldLoad e despacho rápido

`PostWorldLoad` é enviado após o `OnWorldLoad` no caminho de mundo local, uma vez por sessão, com sinalizador reiniciado por `OnWorldUnload`. **A ordem definitiva com `LoadWorldData` ainda não foi implementada**: o callback atualmente serve para inicialização pós-mundo, não para afirmar persistência pronta. No multiplayer, entrada/saída continua pendente.

Para os hooks repetidos, o host mantém um snapshot imutável dos IDs, caminhos de DLL e máscaras de sobrescrita dos mods. O snapshot é republicado apenas quando o status de um mod muda, evitando percorrer o registro e reconstruir caminhos de arquivos em cada quadro. O runtime Mono agora chama `stat()` para validar uma DLL no primeiro carregamento, não por callback. Mantemos as verificações de segurança, isolamento e a mesma ordem dos sistemas. Performance real ainda não foi medida em Android.

## ModifyLightingBrightness — ponte com retorno nativo

O hook C# recebe `ref float brightness` logo após `Lighting.UpdateGlobalBrightness()`. A entrada `BunnySystemEntry.Brightness(float)` é compilada pelo Roslyn; a ponte Mono chama esse método com float nativo, lê o float retornado e aplica `Lighting.GlobalBrightness` pelo campo estático IL2CPP. Os overrides são executados em ordem, preservando a última intensidade válida e ignorando NaN/infinito ou valor negativo. Só há chamada Mono para mod que realmente implementa o hook; os demais não pagam custo por quadro. CI cobre o cálculo gerenciado e a montagem ARM64; ainda falta testar efeito visual/FPS no Android e coexistência com hooks JS.
