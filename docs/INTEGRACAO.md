# Integração com o Bunny Loader

O código desta pasta é o **subsistema C#** separado do aplicativo Android. Ele reúne o compilador, o MonoVM, o SDK e suas pontes nativas, mas depende do Bunny Loader para inicialização, JNI, gerenciamento de mods e acesso ao Terraria.

Esta documentação descreve a arquitetura do snapshot `0.1.10.16.90.29.87.1`, não um procedimento automático de atualização de versões mais recentes.

## Componentes principais

| Componente | Arquivos |
| --- | --- |
| API e contratos públicos dos mods | [`BunnyCsSdk`](../src/managed/BunnyCsSdk/) e [`BunnySourceSdk`](../src/managed/BunnySourceSdk/) |
| Compilação dos arquivos C# | [`BunnySourceCompiler`](../src/managed/BunnySourceCompiler/) |
| Entrada gerenciada | [`BunnyManagedProbe`](../src/managed/BunnyManagedProbe/) |
| Descoberta de mods C# e conteúdo | [`CSharpModLoader.cpp`](../src/native/experiments/csharp/CSharpModLoader.cpp), [`AutoSourceRuntime.cpp`](../src/native/experiments/csharp/AutoSourceRuntime.cpp) |
| Execução Mono e callbacks | [`ManagedRuntime.cpp`](../src/native/experiments/csharp/ManagedRuntime.cpp) e [`MonoServiceDispatch.cpp`](../src/native/experiments/csharp/MonoServiceDispatch.cpp) |
| Interfaces com o host e o jogo | [`host-interfaces`](../src/native/host-interfaces/), [`script/api`](../src/native/script/api/) |
| Comunicação Android | [`src/android-interop/csharp`](../src/android-interop/csharp/) |
| Visuais e renderização | [`src/native/graphics`](../src/native/graphics/) |

## Fluxo da execução

1. O Bunny Loader descobre os mods e inicializa a infraestrutura C# junto dos subsistemas existentes do aplicativo.
2. Os mods-fonte são preparados pelo compilador Roslyn, usando as referências do SDK e as regras de cache.
3. O MonoVM recebe e executa as classes gerenciadas. O worker mantém a execução C# separada das operações diretas na memória do jogo.
4. O host C++ resolve registros e contratos de conteúdo e associa os callbacks aos hooks necessários no Terraria Android.
5. Eventos de NPC, projétil, jogador e outros componentes atravessam os mecanismos de despacho nativos e gerenciados. As operações de Unity/IL2CPP são coordenadas com o contexto de jogo apropriado.
6. Troca de mundo, saída, recarga ou remoção de um mod exigem invalidar callbacks, identidades e registros pertencentes àquela sessão; cada loader implementa seu próprio contrato de ciclo de vida.

Essa separação permite que o comportamento de mods C# seja executado sem depender de um script JavaScript para processar cada callback. O sistema JavaScript do Bunny original permanece um subsistema diferente e pode compartilhar infraestrutura do host.

## Integração num projeto Android

Para incorporar essas fontes, a aplicação hospedeira precisa providenciar:

- Inicialização e encerramento do runtime C# na sequência correta do jogo.
- Inclusão dos arquivos C++ de produção no CMake e ligação das bibliotecas necessárias.
- Contexto JNI válido, serviço Android do worker e acesso aos diretórios de mods/cache.
- Publicação das referências gerenciadas, bibliotecas Mono/.NET ARM64 e compilador no pacote.
- Registradores nativos de conteúdo, sistema de hooks e resolução IL2CPP compatíveis com a versão do Terraria.
- Integração de áudio, texturas, renderização e assets quando exigidos pelo conteúdo.
- Tratamento de falhas, revogação dos registros e isolamento entre mods e sessões.

A pasta `src/native/host-interfaces/` contém interfaces usadas pelo C#, **não uma implementação integral do host**. Alguns arquivos incluem cabeçalhos de módulos do Bunny original que não estão neste repositório. Por isso, copiar apenas `src/native/` para um projeto vazio não produz uma biblioteca executável por si só.

## Compilar versus executar

É possível compilar os projetos C# com os SDKs .NET adequados, de forma independente do APK, para conferir interfaces e referências. A compilação nativa completa requer Android NDK, CMake, JNI, o ambiente do Bunny Loader e as bibliotecas externas correspondentes. Um teste compilado no computador não confirma automaticamente o contrato IL2CPP no Android.

As pastas de `samples/` e `tests/` incluem material de desenvolvimento de versões diferentes. Para adaptar um exemplo ao jogo, utilize a API disponível no snapshot e confira os métodos/assinaturas reais dos loaders. Os comportamentos originais devem seguir os fontes do Bunny Loader e do ExampleModBunny, sem importar o runtime do tModLoader.

## Limites deste repositório

O repositório **não inclui** o launcher completo, o catálogo de mods JavaScript, os recursos proprietários de Terraria/Unity, as bibliotecas externas já compiladas nem chaves de assinatura. Também não é uma distribuição de APK.

Para dependências do ambiente, consulte [DEPENDENCIAS.md](DEPENDENCIAS.md). Para identificar a proveniência exata dos arquivos importados, consulte [SOURCE_MANIFEST.json](../SOURCE_MANIFEST.json).
