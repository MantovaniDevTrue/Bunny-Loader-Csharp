#include "experiments/csharp/SourceParser.h"
#include "experiments/csharp/SourceContentIdentity.h"
#include <cassert>
#include <string>
#include <vector>

using namespace bl::csharp;

int main() {
    std::vector<ParsedSourceType> parsed;
    std::string error;
    const std::string source =
        "namespace LifecycleDemo; public sealed class WorldState : ModSystem {"
        "public override void OnWorldLoad() { } "
        "public override void OnWorldUnload() { } }";
    assert(parseCSharpSource(source, &parsed, &error, true));
    assert(parsed.size() == 1);
    assert(parsed[0].base == "ModSystem");
    assert(parsed[0].name == "WorldState");
    assert(parsed[0].namespaceName == "LifecycleDemo");
    assert(sourceContentDomain("ModSystem") == "SYSTEM");
    assert(!sourceRequiresNativeRegistration("ModSystem"));
    parsed.clear();
    assert(parseCSharpSource("public abstract class BaseState : ModSystem { }",
                            &parsed, &error, false));
    assert(parsed.empty());
    return 0;
}
