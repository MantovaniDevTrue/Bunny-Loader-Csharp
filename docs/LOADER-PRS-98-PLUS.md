# Importação C# a partir da PR #98

O repositório dedicado continua usando a base ativa `.87.1`. As branches de implementação do repositório `MantovaniDevTrue/Bunnyloader-Csharp` têm bases divergentes (`main`, `v1.0` e branches empilhadas), muitos arquivos compartilhados e pendências de integração Android. Não é correto concatená-las nem fazer merge automático.

## Organização

- `loader-prs.json`: inventário revisado, categoria e SHA exato de base/HEAD de cada PR.
- `tools/import_loader_prs.py`: importa somente arquivos relevantes do subsistema C#, exemplos, testes e pontes nativas, sem dependência de execução JS.
- `imports/loader-prs/pr-NNNN/`: sobreposição **isolada** dos arquivos modificados pela PR, mantendo o caminho organizado do repositório C#.
- `imports/loader-prs/pr-NNNN/SOURCE.json`: hashes SHA-256 e caminhos de origem de todos os arquivos copiados.
- `imports/loader-prs/INDEX.json`: inventário gerado, quantidades, PRs bloqueadas e status de ativação.

Cada sobreposição é um *delta* em relação ao `base_sha` da PR, **não é um loader independente executável**. A base `.87.1` pode não satisfazer as dependências de PRs posteriores. Para integrar um loader, é preciso reconciliar o código compartilhado do SDK, compilador, SourceLoader, MonoVM, hooks, assets e testes; revisar a API do ExampleModBunny e comprovar a ABI Android.

## Grupos e estado observado em 22/10/2026

| PR | Escopo | Estado |
| --- | --- | --- |
| #98 | Tile, Wall, Tree, TileEntity, GlobalTile | Lote heterogêneo; ainda há contratos/QA pendentes |
| #100 | Prefix | Parcial, sem paridade universal |
| #101 | Localization | Parcial |
| #102 | Rarity | Aguardando reteste do Disco |
| #114, #115, #116 | AnglerQuest, Dust, Invasion | Fontes prontas para QA; Android não certificado |
| #117, #118 | Command, Sound | Fontes prontas para QA; Android não certificado |
| #120 | Keybind/TouchButton | Parcial |
| #121 | Gore | Fontes prontas para QA; Android não certificado |
| #122 | WorldSave | Integração parcial |
| #125 | Cloud e base Biome | Parcial; ativação bloqueada no SourceLoader |
| #126 | SystemUpdate | 19/19 callbacks nos fontes; Android pendente |
| #130, #131 | Fundos, água e SceneEffect | Parcial, não são 15 loaders completos |
| #134, #135 | Hair, DamageClass | Em quarentena: revisar proveniência relacionada ao tModLoader antes de aproveitar qualquer código |

As PRs #119, #123, #124, #127, #128, #129 e #132 são integrações, correções e testes que auxiliam esses loaders, mas não contam como novos loaders. A #133 está em quarentena por dependência de implementação de utilitários derivada do tModLoader. A #136 não é loader. As PRs #103–#113 são testes temporários de Prefix, não onze implementações de loaders.

## Regras de integração

1. Preservar o runtime/SDK atual; importar sobreposições como referência, **sem conectar hooks automaticamente**.
2. Reaproveitar somente a API nativa do Bunny Loader e do ExampleModBunny. Não incorporar código do tModLoader nem usar JS para executar os mods C#.
3. Reconciliar PRs correlatas e remover implementações duplicadas antes de promover qualquer fonte de `imports` para `src`.
4. Exigir teste de fonte + NDK/ARM64 + ABI IL2CPP + testes reais do Terraria Android, multi-mod e FPS antes de declarar um loader concluído.
5. Não produzir APK, `.bmod`, merge na `main` ou release a partir desta importação isolada.

### Reprodução

A Action `Stage PR98+ CSharp loader sources` faz checkout do repositório upstream, busca os commits imutáveis catalogados, importa os arquivos e registra checksums. Pode ser repetida depois de atualizar propositalmente os SHAs do catálogo. Uma PR avançar não atualiza silenciosamente o snapshot.

`python3 tools/import_loader_prs.py --source _upstream`

O resultado aqui é **organização e preservação dos fontes**, não implementação final conjunta de todos os loaders no host.
