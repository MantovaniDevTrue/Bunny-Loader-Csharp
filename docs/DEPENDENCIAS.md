# Dependências externas do subsistema C#

O repositório contém **todos os fontes selecionados do subsistema C# .87.1**, incluindo pontes nativas e scripts de integração; não contém os binários externos usados pelo APK.

- MonoVM arm64 e bibliotecas .NET/Mono geradas ou fornecidas pelo pipeline.
- Roslyn e referências .NET obtidas por ferramentas de build e gerenciadores de pacotes.
- Módulos AOT compilados para o pacote do runtime C#.
- Android NDK/CMake/Gradle/SDK e bibliotecas externas dos respectivos fornecedores.
- Unity/IL2CPP, `libil2cpp.so` e assets do Terraria obtidos legalmente, fora deste repositório.
- Estruturas do host BunnyLoader existentes no aplicativo original.
- Chaves privadas de assinatura, APKs e quaisquer arquivos temporários de build.

A compilação `.csproj` dos projetos de SDK/compilador é distinta da integração do worker no APK Android. A existência dos fontes nativos não torna o módulo um aplicativo standalone.

Os exemplos de shader contêm `.frag`, `.screenfx` e recursos de textura necessários às demonstrações; esses arquivos são complementos do C#, não um motor de mods alternativo.
