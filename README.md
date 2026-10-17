# BunnyLoader C# — Runtime, SDK e complementos

Repositório dedicado **somente ao subsistema C# do BunnyLoader para Terraria Mobile**. Código-fonte preservado da versão **0.1.10.16.90.29.87.1** (Android `versionCode 249`), base `dc6243850c1d7ac83eb528bc2d56cf585066fc85`.

Este repositório reúne todos os componentes C# relevantes e os adaptadores nativos indispensáveis, **sem clonar o aplicativo Android completo, o launcher, as interfaces gerais ou o mecanismo de mods JavaScript**.

## Organização

| Pasta | Conteúdo |
| --- | --- |
| `src/managed/BunnyCsSdk/` | API de mods C#, itens, projéteis, NPCs, player, equipamentos, hooks, acesso nativo, visuais |
| `src/managed/BunnySourceSdk/` | Projeto de montagem do SDK C# |
| `src/managed/BunnySourceCompiler/` | Roslyn, análise, cache, referência, compilação de fontes |
| `src/managed/` | Packager, probes e outros projetos gerenciados |
| `src/native/experiments/csharp/` | MonoVM, worker, filas, RPC, callbacks, políticas de conteúdo, IA, combates, hooks universais, AOT |
| `src/native/graphics/` | ScreenFX, shader registry, grafo multipass, composição e renderização |
| `src/native/host-interfaces/` | Adaptadores C++ necessários à integração com o host (IL2CPP, hooks e infraestrutura básica) |
| `src/native/script/api/` | Ponte de entrada C# do host, mantida para integração |
| `src/android-interop/` | Serviços Kotlin estritamente ligados ao compilador e worker C# |
| `samples/` | ExampleMod C#, armaduras, acessórios, NPCs, shaders e outras demonstrações C# |
| `tests/` | Testes focados no C#, runtime, hooks, gráficos, compilador e políticas |
| `tools/support/` | Scripts exclusivos para C#, Mono, AOT, API e empacotamento |
| `docs/` | Contratos de funcionamento e referências do SDK |
| `SOURCE_MANIFEST.json` | Lista de arquivos importados e checksums SHA-256 |

O projeto preserva arquivos nativos `.cpp/.h`, Kotlin de integração, Python de build e assets dos exemplos porque **sem eles as funcionalidades C# não ficariam completas**. Eles não representam o aplicativo inteiro.

## Funcionalidades

- Mods C# com arquivos fonte `.cs`, registro de conteúdo e compilações Roslyn/MonoVM.
- Equipamentos, acessórios, bônus de armadura, buffs, receitas, prefixos e tiles.
- Hooks de combate e eventos gerenciados, callbacks universais, proteção contra travamentos e filas.
- Player, projéteis, NPCs, comportamento/IA, vida/morte, dano e movimento.
- Acesso nativo tipado, JNI/IL2CPP e integração com o runtime do BunnyLoader.
- Visuais, desenho, ScreenFX, shaders GLSL, passes e efeitos de tela.
- Exemplos editáveis, ferramentas de empacotamento e testes relevantes.

Essas são **implementações existentes na fonte**; não significa que todos os contratos da API tenham cobertura integral nem que esta extração tenha sido testada como um APK separado.

## Integração e limites

Este é um **módulo de fontes para integração no BunnyLoader**, não um APK e nem um jogo executável independente. Exige um host compatível para thread do jogo, JNI, callbacks IL2CPP, sistema de mods, draw hooks, Android e assets de Terraria. Consultar `docs/INTEGRACAO.md` e `docs/DEPENDENCIAS.md`.

O snapshot está fixado na .87.1. Funcionalidades criadas somente em branches posteriores ou divergentes não foram misturadas com esta versão. O foco permanece na API C# própria do BunnyLoader, sem adaptadores JS para execução dos mods C#.

Referência do BunnyLoader original: https://github.com/JonataOliveiraa/Bunny-Loader

Licenças e autoria dos arquivos preservados permanecem de seus titulares. O repositório não distribui binários, dados do jogo nem segredos de assinatura.
