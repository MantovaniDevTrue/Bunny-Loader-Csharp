# Auditoria de cobertura do loader ModSystem C#

Fonte de referência: `JonataOliveiraa/Bunny-Loader/app/src/main/cpp/script/js/mod/ModSystem.js` consultado em 22/10/2026. Convenção C# autorizada: mesmo nome dos hooks do Bunny/tModLoader quando aplicável, sem copiar o interpretador JS nem considerar método vazio como implementado.

**Status: INCOMPLETO, APK PROIBIDO.** Existem 58 hooks originais; 7 estão declarados no SDK C# como integração de ciclo de vida/eventos, e 51 estão ausentes. A API também tem `Unload`, que não integra os 58 métodos do `ModSystem` original. Mesmo os 5 declarados não estão certificados no Android.

## Inicialização e conteúdo (10)

| Hook do original | C# no PR #1 | Teste Android |
|---|---|---|
| `Load` | Declarado / integração parcial | Pendente |
| `OnModLoad` | Declarado / integração parcial | Pendente |
| `OnModUnload` | Declarado / integração parcial | Pendente |
| `OnLocalizationsLoaded` | Ausente | Pendente |
| `ResizeArrays` | Ausente | Pendente |
| `AddRecipeGroups` | Ausente | Pendente |
| `AddRecipes` | Ausente | Pendente |
| `PostAddRecipes` | Ausente | Pendente |
| `PostSetupRecipes` | Ausente | Pendente |
| `PostSetupContent` | Ausente | Pendente |

## Mundo, persistência e geração (14)

| Hook do original | C# no PR #1 | Teste Android |
|---|---|---|
| `OnWorldLoad` | Declarado / integração parcial | Pendente |
| `LoadWorldData` | Ausente | Pendente |
| `PostWorldLoad` | Ausente | Pendente |
| `SaveWorldData` | Ausente | Pendente |
| `SaveWorldHeader` | Ausente | Pendente |
| `CanWorldBePlayed` | Ausente | Pendente |
| `WorldCanBePlayedRejectionMessage` | Ausente | Pendente |
| `PreWorldGen` | Ausente | Pendente |
| `ModifyWorldGenTasks` | Ausente | Pendente |
| `PostWorldGen` | Ausente | Pendente |
| `ModifyHardmodeTasks` | Ausente | Pendente |
| `ClearWorld` | Declarado / hook nativo sem teste Android | Pendente |
| `PreSaveAndQuit` | Declarado / hook nativo sem teste Android | Pendente |
| `OnWorldUnload` | Declarado / integração parcial | Pendente |

## Atualização do jogo (20)

| Hook do original | C# no PR #1 | Teste Android |
|---|---|---|
| `PreUpdateWorld` | Ausente | Pendente |
| `PostUpdateWorld` | Ausente | Pendente |
| `PreUpdateTime` | Ausente | Pendente |
| `PostUpdateTime` | Ausente | Pendente |
| `PostUpdateEverything` | Ausente | Pendente |
| `PreUpdateEntities` | Ausente | Pendente |
| `PreUpdatePlayers` | Ausente | Pendente |
| `PostUpdatePlayers` | Ausente | Pendente |
| `PreUpdateNPCs` | Ausente | Pendente |
| `PostUpdateNPCs` | Ausente | Pendente |
| `PreUpdateGores` | Ausente | Pendente |
| `PostUpdateGores` | Ausente | Pendente |
| `PreUpdateProjectiles` | Ausente | Pendente |
| `PostUpdateProjectiles` | Ausente | Pendente |
| `PreUpdateItems` | Ausente | Pendente |
| `PostUpdateItems` | Ausente | Pendente |
| `PreUpdateDusts` | Ausente | Pendente |
| `PostUpdateDusts` | Ausente | Pendente |
| `PreUpdateInvasions` | Ausente | Pendente |
| `PostUpdateInvasions` | Ausente | Pendente |

## Interface, iluminação, entrada e biomas (10)

| Hook do original | C# no PR #1 | Teste Android |
|---|---|---|
| `UpdateUI` | Ausente | Pendente |
| `ModifyInterfaceLayers` | Ausente | Pendente |
| `PostUpdateInput` | Ausente | Pendente |
| `ModifyTimeRate` | Ausente | Pendente |
| `ModifySunLightColor` | Ausente | Pendente |
| `ModifyLightingBrightness` | Ausente | Pendente |
| `ModifyScreenPosition` | Ausente | Pendente |
| `ModifyTransformMatrix` | Ausente | Pendente |
| `ResetNearbyTileEffects` | Ausente | Pendente |
| `TileCountsAvailable` | Ausente | Pendente |

## Multiplayer (4)

| Hook do original | C# no PR #1 | Teste Android |
|---|---|---|
| `NetSend` | Ausente | Pendente |
| `NetReceive` | Ausente | Pendente |
| `HijackGetData` | Ausente | Pendente |
| `HijackSendData` | Ausente | Pendente |

## Bloqueios de lançamento

1. Implementar todos os 51 hooks ausentes, com contratos C# adequados para cada parâmetro, retorno e objeto utilizado pelo Terraria. Para métodos com semântica diferente ou API móvel indisponível, documentar equivalência funcional demonstrável ou bloqueio real; não usar `stub`.
2. Fazer `Load` e `OnModLoad` realmente executarem antes de entrar no mundo, mesmo que o Mono não esteja pronto no primeiro despacho. Uma simples mensagem de erro sem retry não basta.
3. Fazer `OnWorldLoad`, `LoadWorldData`, `PostWorldLoad`, `SaveWorldData`, `OnWorldUnload` e `Unload` obedecerem à ordem correta, sem duplicação em mundos locais, save/quit e novo mundo.
4. Inspecionar ABI do hook de `Main.QuitGame` no Android; a assinatura e o vínculo nativo precisam ser comprovados e não podem ser presumidos com base no JS.
5. Compilar e vincular `ManagedRuntime.cpp`, `SourceLoader.cpp` e `AutoSourceRuntime.cpp` no host Android; testes de parser isolados e testes .NET de DLL não comprovam a integração completa.
6. Testar dois mods C# separados, saída/retorno, falha isolada, reentrada, saves e rede, sem dependência do motor JS.
7. Medir memória, tempo de carregamento, tempo dos hooks frequentes e FPS no Android; confirmar que logs não foram excessivos e efeitos não foram removidos.
8. Só depois de todas essas verificações, atualizar evidências, concluir a PR e considerar gerar um APK. Nenhuma checklist deve ser marcada como concluída exclusivamente por existência de código.

**Critério de fidelidade:** consulta ao código-fonte do `SystemLoader`, `SystemWorldHooks`, `SystemUpdateHooks`, `SystemDrawHooks`, `SystemNetworkHooks` e documentação correspondente do tModLoader; a implementação C# usa o host nativo do Bunny, não QuickJS.
