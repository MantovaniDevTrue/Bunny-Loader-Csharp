# Componentes gerenciados do Bunny Loader C#

Esta pasta contém os projetos .NET usados pelo subsistema C#. O caminho principal dos mods-fonte é composto pelo SDK de referência, pelo compilador Roslyn e pelo runtime MonoVM; protótipos NativeAOT e programas de benchmark permanecem separados.

## Projetos principais

| Projeto | Uso |
| --- | --- |
| [`BunnyCsSdk`](BunnyCsSdk/) | API pública de autoria: itens, hooks, jogadores, projéteis, entidades, tiles e visuais |
| [`BunnySourceSdk`](BunnySourceSdk/) | Montagem de referência utilizada na compilação de mods-fonte |
| [`BunnySourceCompiler`](BunnySourceCompiler/) | Compilação Roslyn, cache e verificação das fontes |
| [`BunnyManagedProbe`](BunnyManagedProbe/) | Código gerenciado executado pelo worker |
| [`BunnyCsPackager`](BunnyCsPackager/) | Ferramentas de empacotamento e experiências de autoria |

`BunnyCsSdk` utiliza `net10.0`; os projetos SourceSdk, SourceCompiler e ManagedProbe usam `net8.0`. Os requisitos e as diferenças entre compilação do SDK e execução no Android estão no [README principal](../../README.md).

## Código de pesquisa

Projetos como `BunnyCsPrototype`, `BunnyVmBenchmark67`, `BunnyVmGameplay69`, `ExamplePureCSharpMod` e testes de sintaxe guardam experiências específicas do desenvolvimento. Eles não devem ser confundidos com uma segunda implementação obrigatória do loader C# atual.

O histórico de protótipos NativeAOT não define o funcionamento do pipeline Roslyn + MonoVM dos mods-fonte. Scripts JavaScript usados em diagnósticos antigos também não significam que os callbacks C# sejam executados pela engine JavaScript.

## Referências

- [Arquitetura e integração](../../docs/INTEGRACAO.md)
- [Dependências](../../docs/DEPENDENCIAS.md)
- [Exemplos C#](../../samples/ExampleModCSharp/)
