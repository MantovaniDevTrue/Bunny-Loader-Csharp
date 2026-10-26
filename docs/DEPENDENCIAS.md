# Dependências do suporte C#

Este repositório contém as fontes do subsistema C# do Bunny Loader. As dependências variam conforme a tarefa: estudar o SDK, compilar os projetos gerenciados ou integrar o runtime no Terraria Android.

## Projetos .NET

Os projetos principais usam:

| Projeto | Target framework | Função |
| --- | --- | --- |
| [`BunnyCsSdk`](../src/managed/BunnyCsSdk/BunnyCsSdk.csproj) | `net10.0` | API pública de autoria C# |
| [`BunnySourceSdk`](../src/managed/BunnySourceSdk/BunnySourceSdk.csproj) | `net8.0` | Referências usadas por mods-fonte |
| [`BunnySourceCompiler`](../src/managed/BunnySourceCompiler/BunnySourceCompiler.csproj) | `net8.0` | Compilação Roslyn |
| [`BunnyManagedProbe`](../src/managed/BunnyManagedProbe/BunnyManagedProbe.csproj) | `net8.0` | Componentes gerenciados do worker |

`BunnySourceCompiler` declara o pacote `Microsoft.CodeAnalysis.CSharp` versão `4.11.0` no projeto. Um ambiente com SDK .NET 10 (e suporte a projetos .NET 8) atende à leitura e compilação dessas fontes, desde que os pacotes necessários possam ser restaurados.

A execução no aparelho é uma etapa separada da compilação desses projetos.

## Integração Android

A aplicação que hospeda o C# utiliza:

- Ferramentas Android SDK, NDK, CMake e Gradle compatíveis com o Bunny Loader.
- Runtime Mono/.NET e bibliotecas nativas para Android ARM64.
- Montagens gerenciadas do worker, compilador Roslyn, SDK e dependências do framework distribuídas com o aplicativo.
- JNI e o serviço Android necessários para comunicar o runtime ao host.
- Resolução IL2CPP, mecanismos de hook, registradores de conteúdo, ciclo de vida do jogo e integração com Unity.
- Assets, rotas de áudio e renderização quando o mod utilizar efeitos visuais.

`src/native/host-interfaces/` reúne apenas os pontos de interface necessários ao subsistema C#. O código host completo é responsabilidade do Bunny Loader original. Os arquivos extraídos mantêm seus includes e contratos, que precisam ser reconciliados com a versão do host usada na integração.

## O que não é distribuído

Este repositório não fornece APK pronto, binários proprietários do Terraria/Unity, assets do jogo, chaves privadas ou segredos de build. DLLs e bibliotecas geradas por ferramentas externas também não são incluídas como uma instalação completa do runtime.

Os shaders e texturas em `samples/` pertencem aos exemplos do C# e são preservados como material de fonte; não representam dependência de uma engine de mods separada.

## Reprodução e conferência

O [`SOURCE_MANIFEST.json`](../SOURCE_MANIFEST.json) registra os arquivos extraídos, sua versão de origem e hashes SHA-256. Ele permite conferir a procedência do snapshot `0.1.10.16.90.29.87.1`, mas não substitui uma build ou um teste em dispositivo.

Para conferir os projetos .NET, veja os comandos no [README](../README.md). Para integrar as fontes ao aplicativo Android, consulte [INTEGRACAO.md](INTEGRACAO.md).

As licenças e a autoria de componentes de terceiros devem ser preservadas quando estas fontes forem redistribuídas ou incorporadas a outra árvore.
