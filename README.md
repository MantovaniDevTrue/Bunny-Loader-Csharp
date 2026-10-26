# Bunny Loader C#

Subsistema C# do [Bunny Loader](https://github.com/JonataOliveiraa/Bunny-Loader) para Terraria Mobile. Reúne o runtime gerenciado, a compilação de mods, a API C#, os hooks nativos e os componentes necessários à integração com Android/IL2CPP.

Este repositório contém **código-fonte do suporte C#**, não o aplicativo Android inteiro. A execução dos mods C# usa o próprio runtime e as pontes nativas do Bunny; o projeto original pode continuar oferecendo suporte a JavaScript separadamente.

## Como funciona

O suporte C# é dividido em quatro partes:

1. **Roslyn:** analisa e compila os arquivos \`.cs\` dos mods, usando as referências do SDK e o cache de compilação.
2. **MonoVM:** carrega e executa o código gerenciado e recebe eventos do jogo por meio do worker Android.
3. **C++ / IL2CPP:** faz a ligação com o Terraria, registra conteúdo e conecta os callbacks, hooks e comportamentos nativos.
4. **SDK C#:** oferece os tipos utilizados pelos mods para declarar itens, projéteis, NPCs, jogadores, buffs, equipamentos, tiles e outros recursos.

Em termos de fluxo: o host descobre o mod, o compilador prepara as classes, o runtime carrega a montagem C# e as pontes nativas conectam o conteúdo e os eventos ao Terraria. Operações que dependem dos objetos Unity/IL2CPP precisam respeitar a thread, o ciclo de vida e os contratos do jogo.

## Estrutura

| Caminho | Responsabilidade |
| --- | --- |
| [\`src/managed/BunnyCsSdk/\`](src/managed/BunnyCsSdk/) | API C# de itens, entidades, equipamentos, eventos, hooks, visuais e acesso nativo |
| [\`src/managed/BunnySourceSdk/\`](src/managed/BunnySourceSdk/) | Assembly de referência usado para compilar mods-fonte |
| [\`src/managed/BunnySourceCompiler/\`](src/managed/BunnySourceCompiler/) | Compilador Roslyn, referências, validação e cache |
| [\`src/managed/BunnyManagedProbe/\`](src/managed/BunnyManagedProbe/) | Ponto de entrada gerenciado do worker |
| [\`src/native/experiments/csharp/\`](src/native/experiments/csharp/) | Runtime MonoVM, loaders, filas, registro, hooks e integração IL2CPP |
| [\`src/native/host-interfaces/\`](src/native/host-interfaces/) | Interfaces nativas necessárias para conectar o subsistema ao host |
| [\`src/native/graphics/\`](src/native/graphics/) | Renderização, ScreenFX e recursos gráficos usados pelo C# |
| [\`src/android-interop/\`](src/android-interop/) | Componentes Kotlin/Android ligados ao worker e ao compilador |
| [\`samples/\`](samples/) | Exemplos de mods e recursos C# |
| [\`tests/\`](tests/) | Testes de contratos, compilação e integração |
| [\`tools/support/\`](tools/support/) | Scripts de preparação, empacotamento e suporte ao runtime |

Arquivos C++, Kotlin e recursos gráficos fazem parte desta fonte porque são dependências reais do suporte C#, mesmo que não sejam escritos em C#.

## Recursos presentes

A fonte contém infraestrutura para:

- Compilação e carregamento de mods escritos em C#.
- Itens, armas, acessórios, armaduras, buffs, receitas, prefixos e tiles.
- Jogadores, NPCs, projéteis, IA, eventos de dano e combate.
- Hooks universais, chamadas nativas e comunicação entre C# e o Terraria.
- Texturas, efeitos visuais, ScreenFX e recursos de renderização.
- Controle de eventos, filas, cache e gerenciamento de estado.

**Importante:** a existência de uma API, classe ou exemplo não garante que todos os callbacks de cada loader estejam implementados ou validados no Terraria Android. O núcleo MonoVM/Roslyn e a cobertura dos loaders são etapas diferentes.

## Compilação e integração

Os projetos gerenciados estão em \`src/managed/\`. O SDK de autoria usa \`.NET 10\` em \`BunnyCsSdk.csproj\`; \`BunnySourceSdk\`, \`BunnySourceCompiler\` e \`BunnyManagedProbe\` usam \`.NET 8\`. O compilador utiliza \`Microsoft.CodeAnalysis.CSharp\` (Roslyn).

Para verificar a parte gerenciada num ambiente com os SDKs necessários:

\`\`\`bash
dotnet build src/managed/BunnyCsSdk/BunnyCsSdk.csproj -c Release
dotnet build src/managed/BunnySourceSdk/BunnySourceSdk.csproj -c Release
dotnet build src/managed/BunnySourceCompiler/BunnySourceCompiler.csproj -c Release
\`\`\`

Esses comandos compilam projetos C# individualmente; **não geram um APK** e não substituem a integração com o Bunny Loader Android.

A execução no jogo exige o host do Bunny, seu CMake/NDK, as bibliotecas Mono/.NET ARM64, JNI, registradores de conteúdo e interfaces IL2CPP. Os binários e assets proprietários do Terraria não estão incluídos aqui.

Leia [Integração com o Bunny Loader](docs/INTEGRACAO.md) para os pontos de ligação e [Dependências](docs/DEPENDENCIAS.md) para os requisitos do ambiente.

## Versão desta fonte

A árvore principal é uma extração rastreável da versão **\`0.1.10.16.90.29.87.1\`**, originada do commit \`dc6243850c1d7ac83eb528bc2d56cf585066fc85\`. O arquivo [\`SOURCE_MANIFEST.json\`](SOURCE_MANIFEST.json) registra os caminhos e checksums dos arquivos importados.

É um **snapshot do subsistema C#**, não uma declaração de que todas as alterações posteriores do desenvolvimento já estejam incorporadas. Os arquivos aqui foram separados do projeto Android para facilitar leitura, manutenção e reutilização da arquitetura.

## Créditos

Bunny Loader original por **Jonata Oliveira**. Desenvolvimento da extensão C# por **Mantovani**. A autoria e os termos dos componentes de terceiros são preservados; este repositório não distribui o jogo, APKs ou credenciais de assinatura.
