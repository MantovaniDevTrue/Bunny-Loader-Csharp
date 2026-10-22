# Fast-track para completar os 50 loaders C# do Bunny

## Situação verificada

A matriz inicial `docs/csharp-loader-batches.json` do repositório fonte define dez grupos de cinco sistemas, 88 módulos do Bunny e o objetivo de um exemplo original completo por loader. O inventário dedicado tem 23 PRs importadas isoladamente (1.451 arquivos, 23.879.788 bytes). A auditoria de conflitos encontrou 137 caminhos divergentes; entre os mais disputados: `AutoSourceRuntime.cpp`, `ManagedRuntime.cpp`, `BunnySourceSdk.csproj`, `SourceLoader.cpp`.

Esses números representam *fontes disponíveis*, não 23 loaders completos. O módulo C# dedicado continua com a baseline .87.1 intacta. O mapa inicial está em `loader-completion-matrix.json`; cada situação será reavaliada contra o fonte real antes de declarar paridade.

## Como acelerar sem quebrar o runtime

**1. Resolver o núcleo compartilhado uma única vez.** Um integrador mantém `SourceParser`, `SourceLoader`, `AutoSourceRuntime`, `ManagedRuntime`, MonoVM e registros do SDK. Implementações das PRs são reconciliadas por função/contrato, nunca copiadas integralmente por último escritor. O núcleo oferece propriedade de tipo por mod, registro ordenado, rollback, lifecycle e dispatch tipado usando implementações já existentes no Bunny Loader. Não incorporar o `LoaderManager` adaptado de código tModLoader na PR #138.

**2. Implementar cinco loaders simultaneamente em áreas isoladas.** Para cada loader, dividir as tarefas entre API/compilador C#, ponte nativa C++/IL2CPP, exemplo original/assets e regressões/testes. Só o integrador toca os arquivos compartilhados. Não rodar JS como fallback para executar mods C#; não adicionar varreduras massivas.

**3. Usar contratos de origem, não apenas classes que compilam.** Fazer mapa de cada membro no loader do Bunny e do ExampleModBunny: declaração C#, registro, hook nativo, thread, save/network, renderização e comportamento. Callback desconhecido deve ser explicitamente recusado. Um item de exemplo ou uma flag não certifica um loader inteiro.

**4. Criar uma régua de QA automatizada.** Em cada push: compilador/SDK Roslyn, testes C++ nativos e NDK arm64, regressões de concorrência/IDs/memória, manifesto de assets e ExampleMod de teste. Todos os gates precisam passar no mesmo SHA. Build Android de integração apenas quando os cinco sistemas do lote estiverem completos nos fontes e sem conflito; execução real no dispositivo é requisito separado para certificação.

**5. Só liberar lote de cinco loaders validados.** A matriz guarda dez requisitos por loader: contrato, compilador, hook nativo, exemplo, testes host, ABI Android, gameplay Android, multi-mod/lifecycle, persistência/rede e FPS/memória. `tools/check_loader_completion.py` recusa o status `certified` sem os requisitos e provas de CI/dispositivo.

## Primeiro lote de menor dependência estrutural

| Loader | PRs de referência | Principal trabalho que falta |
| --- | --- | --- |
| CommandLoader | #117 | Confirmar todos os comandos, permissões/rede e hooks reais Android |
| GoreLoader | #121 | Testar NewGore/texturas, ChildSafety, vida útil, ABI e dois mods |
| AnglerQuestLoader | #114 | Validar captura/entrega/persistência, UI e rede real |
| RarityLoader | #102 | Retestar Disco animado, item/popup/inventário, salvamento e JS misto |
| DustLoader | #115, #127, #128, #132 | Resolver `Dust.NewDust`/registro real, callbacks de desenho, slot reuse e FPS |

Este lote é **candidato**, não promessa de conclusão imediata. Pode ser reorganizado conforme impedimentos reais; só contará cinco após fechar contratos e testes. Não produzir APK de release com apenas cinco exemplos que escondam callbacks faltando.

## Lotes seguintes

- Aproveitar código já existente de Item, Projectile, NPC, Player, Buff e receitas depois da auditoria de contrato; unificar o SDK ao invés de reescrever a base estável.
- Finalizar separadamente as dependências de mundo Tile/Wall/Tree/TileEntity/GlobalTile: desenho `SpriteBatch`, objetos em posição, save/rede, frames e hooks globais. Elas devem ter testes intensivos de mapa, persistência e performance.
- Completar equipamentos (Equip, ArmorSet, Wing, Prefix), fundo/água/bioma, UI, mundo, menus e multiplayer conforme o grafo de dependências. SystemUpdate 19/19 **não** equivale a SystemLoader inteiro; Sound de efeitos **não** equivale a SoundLoader + música.
- Se um loader estiver ausente do inventário das PRs, buscar primeiro a implementação nativa já disponível em Bunny/ExampleModBunny, reutilizar o host existente e registrar sua lacuna na matriz. Não inventar interfaces nem copiar tModLoader.

## Proveniência obrigatória

As PRs #133 (StatModifier), #134 (Hair), #135 (DamageClass), #136 (Random utilities) e #138 (Loader core) fazem referência explícita a código/API do tModLoader. Para o objetivo deste projeto, elas são **material de diagnóstico**, não fonte de implementação. Recriar essas funcionalidades a partir das APIs reais Bunny Loader/ExampleModBunny, mantendo a fidelidade possível; se não existir equivalente, documentar a incompatibilidade.

`GlobalTileLoader` tem referência PC no plano anterior, mas a implementação não pode ser copiada daquela fonte. Usar o GlobalTile já presente no Bunny original como ponto de partida.

## Entrega e desempenho

C# executa pelo MonoVM e C++ nativo, sem QuickJS na cadeia de execução. Pré-calcular registro e tabelas, fazer snapshots de leitura imutáveis, filas de eventos limitadas, invalidação na troca de mundo e cache de draw atualizado por frame quando possível. Não chamar MonoVM por pixel/tile nem eliminar efeitos para ganhar FPS. No Xiaomi de QA, medir FPS, frame-time, heap, logs, troca de mundo, reload, multi-mod com JS e uso prolongado.

**Regra de status:** `source available` ≠ `source complete` ≠ `CI passed` ≠ `Android verified` ≠ `certified`. Nenhum arquivo importado em `imports/loader-prs` habilita gameplay automaticamente.
