# Integração no host BunnyLoader

Os arquivos aqui foram separados **por responsabilidade**, preservando a implementação funcional da .87.1 sem importar o código do aplicativo inteiro.

## Pontos de integração

- `src/native/experiments/csharp/`: runtime gerenciado, parser, loader de fontes e instaladores nativos.
- `src/native/script/api/CSharpPrototypeApi.*`: ponte com as funções de registro e diagnóstico do host.
- `src/native/graphics/`: render hooks, ScreenFX, alvos multipass e gestão de shaders.
- `src/native/host-interfaces/`: interfaces que os componentes C# incluem para trabalhar com IL2CPP, hooks e infraestrutura do processo.
- `src/android-interop/csharp/`: agendador de precompilação e serviços do worker em Kotlin.
- `src/managed/BunnySourceSdk/`, `src/managed/BunnyCsSdk/` e `src/managed/BunnySourceCompiler/`: compilador, API e contratos gerenciados.

## Não incluído

Launcher Android, catálogo de mods, visual geral do aplicativo, código do subsistema de mods JavaScript, APK e conteúdo do Terraria. Ao incorporar o subsistema em uma aplicação host, é necessário ligar os pontos de inicialização, contexto de jogo, JNI, CMake, assets, logs, render pipeline e lifecycle do host existente.

Os caminhos neste repositório não são caminhos de Gradle/APK prontos. O host precisa atualizar includes e regras de montagem na integração. **Não foi realizada compilação Android independente deste repositório.**

## Identidade

Fonte original: `dc6243850c1d7ac83eb528bc2d56cf585066fc85`.
Versão: `0.1.10.16.90.29.87.1-csharp-lights-scene`.
A cópia não altera a lógica dos arquivos `.cs`, `.cpp`, `.h` e `.kt` importados. Veja `SOURCE_MANIFEST.json` para conferir os arquivos.
