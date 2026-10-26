# ExampleMod CSharp

Exemplos de autoria de mods C# para o Bunny Loader. As classes mostram o uso do SDK próprio, incluindo conteúdo, eventos, player, projéteis e testes controlados de hooks.

O pacote `Playable/` incluído neste snapshot está identificado como **ExampleMod CSharp 0.1.45** no `manifest.json`, destinado à QA dos hooks tipados no Terraria Android build `301720`. Não é um port completo de todos os exemplos do ExampleModBunny nem uma release final para jogar.

## Organização

| Caminho | Conteúdo |
| --- | --- |
| [`Playable/`](Playable/) | Fontes e assets do mod de teste importável em um Bunny Loader compatível |
| [`Playable/content/Common/`](Playable/content/Common/) | Comportamentos de jogador, eventos e classes auxiliares |
| [`Playable/content/Content/`](Playable/content/Content/) | Exemplos de itens, entidades, equipamentos e outros conteúdos |
| [`Examples/`](Examples/) | Exemplos adicionais de uso da API |
| [`Tests/`](Tests/) | Fontes de teste e regressão |
| [`Experimental/`](Experimental/) | Implementações experimentais, não destinadas a uso automático |
| [`Docs/`](Docs/) | Referências técnicas e histórico de desenvolvimento |

O ponto de entrada do mod é `Playable/content/main.cs`, conforme `Playable/manifest.json`. Fontes e sprites permanecem editáveis.

## Como utilizar os exemplos

1. Consulte as classes em `Playable/content/` e as APIs correspondentes em [`BunnyCsSdk`](../../src/managed/BunnyCsSdk/).
2. Use um Bunny Loader com suporte C# compatível com o manifesto e com os callbacks exigidos por cada exemplo.
3. Para testar hooks que alteram geração de mundo, aleatoriedade ou estado do Terraria, utilize um mundo de testes e habilite somente o comportamento desejado.
4. Diferencie sucesso de compilação, aceitação do hook e execução efetiva do callback. Cada etapa possui um contrato próprio.

Os detalhes da QA dos cinco hooks do pacote 0.1.45 estão em [`Playable/content/Documentation/HOOK_QA_ANDROID.md`](Playable/content/Documentation/HOOK_QA_ANDROID.md). As instruções antigas em [`Playable/content/LEIA_PRIMEIRO.md`](Playable/content/LEIA_PRIMEIRO.md) e `Docs/` documentam versões experimentais anteriores e não devem ser aplicadas automaticamente a outro APK.

## Referência de comportamento

A estrutura e os comportamentos originais para port Bunny vêm dos fontes do **ExampleModBunny**. Estes arquivos demonstram a API C# e suas pontes nativas; não exigem que um mod C# seja escrito em JavaScript e não utilizam o runtime do tModLoader.

Para entender a compilação, o MonoVM e a integração ao Terraria, consulte o [README do repositório](../../README.md).
