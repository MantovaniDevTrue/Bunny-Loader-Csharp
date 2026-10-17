#!/usr/bin/env python3
from pathlib import Path
import argparse
import hashlib
import json
import re
import shutil

TARGET = Path(__file__).resolve().parents[1]
VERSION = "0.1.10.16.90.29.87.1"
SOURCE_SHA = "dc6243850c1d7ac83eb528bc2d56cf585066fc85"
SAMPLE_NAMES = {
    "ExampleModCSharp", "ArmorPoliciesCSharp", "LightsAndShadowsCSharp",
    "SourceCSharpMod", "NativePlacementProbe", "NativeAccessProbe",
    "NativeAccessoryCSharp", "ScreenFxProbeCSharp", "SecondExampleModCSharp",
    "StarterModCSharp", "DeclaredContentCSharp", "ExampleCSharpOnlyMod"
}
TEST_FOLDERS = {
    "basic_items", "prefix_v89", "visual_queue", "universal_hooks",
    "csharp_scale", "native_access", "native_recipes", "screenfx_stubs",
    "combat_batch", "compiler_references", "entity_lifecycle",
    "entity_source", "hooks", "screenfx_probe", "sdk_foundation", "startup"
}
TOOLS_FOLDERS = {
    "mono", "sdk_reference", "api_discovery", "example_completion"
}
TOOL_FILES = {
    "build_compiler_aot.py", "check_compiler_delivery.py",
    "create_csharp_mod.py", "csharp_api_compile_batch.py",
    "csharp_api_discovery.py", "csharp_api_gap_map.py",
    "csharp_api_negative_compile.py", "example_completion_audit.py",
    "package_native_access_probe.py", "stage_compiler_references.py",
    "stage_mono_runtime.py", "check_lot_a_delivery.py"
}
TEXT_EXTS = {".md", ".txt", ".rst"}
FORBIDDEN_EXTS = {".js", ".mjs", ".apk", ".so", ".dll", ".jar", ".tmod", ".tl"}
source_files = []
manifest = []
def add(source: Path, destination: Path):
    if not source.is_file() or source.suffix.lower() in FORBIDDEN_EXTS:
        return
    destination = TARGET / destination
    destination.parent.mkdir(parents=True, exist_ok=True)
    data = source.read_bytes()
    if destination.suffix.lower() in TEXT_EXTS:
        text = data.decode("utf-8", errors="replace")
        text = text.replace("Split/Bunny-Loader/", "BunnyLoader/")
        text = re.sub(r"\bTLPro\b|\bExMod\b", "BunnyLoader", text, flags=re.IGNORECASE)
        text = re.sub(r"(?<!\w)Split(?!\w)", "BunnyLoader", text)
        data = text.encode("utf-8")
    destination.write_bytes(data)
    source_files.append(destination)
    manifest.append({"path": str(destination.relative_to(TARGET)), "sha256": hashlib.sha256(data).hexdigest()})
def tree(root: Path, dst: Path, predicate=None):
    if not root.is_dir():
        return
    for file in sorted(root.rglob("*")):
        if file.is_file() and (predicate is None or predicate(file)):
            add(file, dst / file.relative_to(root))
def select_tests(file: Path, base: Path):
    rel = file.relative_to(base)
    name = file.name.lower()
    allowed = rel.parts[0] in TEST_FOLDERS if len(rel.parts) > 1 else False
    allowed = allowed or any(w in name for w in (
        "csharp", "mono", "managed", "compiler", "source_", "universal",
        "screenfx", "frame_composer", "native_", "hook_qa", "world_entry",
        "example_reference", "visual_", "signal_", "run_native_", "run_universal"))
    return allowed and file.suffix.lower() not in FORBIDDEN_EXTS
def select_docs(file: Path, base: Path):
    rel = file.relative_to(base)
    if len(rel.parts) > 1:
        return rel.parts[0] == "generated" and (
            file.name.startswith("csharp_") or file.name.startswith("sdk_"))
    return bool(re.search(r"csharp|mono|managed|sdk|universal|native|screenfx|acesso", file.name, re.I))
def main(source: Path):
    candidates = [p for p in source.rglob("Bunny-Loader") if (p / "experiments/csharp/BunnyCsSdk").is_dir()]
    if len(candidates) != 1:
        raise SystemExit("Não foi possível identificar unicamente o checkout da versão fixada")
    up = candidates[0]
    tree(up / "experiments/csharp", Path("src/managed"))
    tree(up / "app/src/main/cpp/experiments/csharp", Path("src/native/experiments/csharp"))
    tree(up / "app/src/main/cpp/graphics", Path("src/native/graphics"))
    for base in ("il2cpp", "hook", "core"):
        tree(up / "app/src/main/cpp" / base, Path("src/native/host-interfaces") / base)
    for name in ("CSharpPrototypeApi.cpp", "CSharpPrototypeApi.h"):
        add(up / "app/src/main/cpp/script/api" / name, Path("src/native/script/api") / name)
    tree(up / "app/src/main/kotlin/dev/bunnyloader/csharp", Path("src/android-interop/csharp"))
    tree(up / "app/src/test/kotlin/dev/bunnyloader/csharp", Path("tests/android-interop"))
    for sample in sorted(SAMPLE_NAMES):
        tree(up / "samples" / sample, Path("samples") / sample)
    tree(up / "tests", Path("tests/native-and-managed"), lambda p: select_tests(p, up / "tests"))
    for folder in TOOLS_FOLDERS:
        tree(up / "tools" / folder, Path("tools/support") / folder)
    for name in TOOL_FILES:
        add(up / "tools" / name, Path("tools/support") / name)
    tree(up / "docs", Path("docs/reference"), lambda p: select_docs(p, up / "docs"))
    outside = source / "docs"
    tree(outside, Path("docs/reference/repository"), lambda p: select_docs(p, outside))
    check = [
        "src/managed/BunnyCsSdk/BunnyUniversalHooks.cs",
        "src/managed/BunnySourceCompiler/Compiler.cs",
        "src/managed/BunnySourceSdk/BunnySourceSdk.csproj",
        "src/native/experiments/csharp/ManagedRuntime.cpp",
        "src/native/experiments/csharp/UniversalHooks.cpp",
        "src/native/experiments/csharp/MonoWorkerJni.cpp",
        "src/native/graphics/CSharpScreenEffects.cpp",
        "src/native/graphics/CSharpScreenGraph.cpp",
        "samples/LightsAndShadowsCSharp/Playable/content/main.cs",
        "samples/ExampleModCSharp/Playable/content/Common/Live/BunnyLiveEvents.cs",
    ]
    missing = [x for x in check if not (TARGET / x).is_file()]
    if missing:
        raise SystemExit("Fontes obrigatórios ausentes: " + ", ".join(missing))
    if len(source_files) < 370:
        raise SystemExit(f"Importação incompleta: {len(source_files)} arquivos")
    result = {"version": VERSION, "source_commit": SOURCE_SHA, "files": len(source_files),
              "csharp": sum(x.suffix == ".cs" for x in source_files),
              "native_cpp": sum(x.suffix in {".cpp", ".h"} for x in source_files),
              "entries": sorted(manifest, key=lambda x: x["path"])}
    (TARGET / "SOURCE_MANIFEST.json").write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "entries"}, indent=2))
if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, required=True)
    main(parser.parse_args().source.resolve())
