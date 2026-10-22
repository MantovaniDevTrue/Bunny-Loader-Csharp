#!/usr/bin/env python3
import argparse
import hashlib
import json
import shutil
import subprocess
from pathlib import Path, PurePosixPath

ROOT = Path(__file__).resolve().parents[1]
UPSTREAM_PREFIX = "Split/Bunny-Loader/"
DISALLOWED = {".js", ".mjs", ".apk", ".bmod", ".dll", ".so", ".a", ".jar", ".tmod", ".tl", ".keystore", ".jks", ".class", ".dex"}
MAX_FILE = 10 * 1024 * 1024
MAX_TOTAL = 100 * 1024 * 1024


def run(repo, *args, binary=False):
    command = ["git", "-C", str(repo), *args]
    result = subprocess.run(command, check=True, capture_output=True)
    return result.stdout if binary else result.stdout.decode("utf-8")


def destination(path):
    item = PurePosixPath(path)
    if item.is_absolute() or any(part in {"", ".", ".."} for part in item.parts):
        return None
    if item.suffix.lower() in DISALLOWED:
        return None
    rel = path[len(UPSTREAM_PREFIX):] if path.startswith(UPSTREAM_PREFIX) else path
    if rel.startswith("docs/"):
        return "docs/reference/" + rel[len("docs/"):]
    pairs = [
        ("experiments/csharp/", "src/managed/"),
        ("app/src/main/cpp/experiments/csharp/", "src/native/experiments/csharp/"),
        ("app/src/main/cpp/graphics/", "src/native/graphics/"),
        ("app/src/main/cpp/script/api/", "src/native/script/api/"),
        ("app/src/main/kotlin/dev/bunnyloader/csharp/", "src/android-interop/csharp/"),
        ("app/src/test/kotlin/dev/bunnyloader/csharp/", "tests/android-interop/"),
        ("samples/", "samples/"),
        ("tests/", "tests/native-and-managed/"),
        ("tools/", "tools/support/"),
        ("docs/", "docs/reference/"),
        (".github/workflows/", "integration/ci-reference/"),
    ]
    for source, target in pairs:
        if rel.startswith(source):
            suffix = rel[len(source):]
            if source == "app/src/main/cpp/script/api/" and not ("CSharp" in suffix or "Managed" in suffix):
                return None
            if source == "app/src/main/kotlin/dev/bunnyloader/csharp/" and item.suffix.lower() not in {".kt", ".java", ".md"}:
                return None
            return target + suffix
    cpp_prefix = "app/src/main/cpp/"
    if rel.startswith(cpp_prefix) and item.suffix.lower() in {".cpp", ".h", ".hpp", ".c", ".s", ".inl", ".txt", ".cmake"}:
        other = rel[len(cpp_prefix):]
        for group in ("core/", "il2cpp/", "hook/"):
            if other.startswith(group):
                return "src/native/host-interfaces/" + other
        return "src/native/host-patches/" + other
    android_prefix = "app/src/main/"
    if rel.startswith(android_prefix) and any(word in rel.lower() for word in ("csharp", "mono", "roslyn", "managed")):
        return "integration/android-host/" + rel[len(android_prefix):]
    return None


def fetch_refs(source, entry):
    number = entry["number"]
    run(source, "fetch", "--no-tags", "origin", "+refs/pull/%d/head:refs/remotes/origin/csharp-pr-%d" % (number, number))
    run(source, "fetch", "--no-tags", "origin", entry["base_sha"])
    head = entry["head_sha"]
    base = entry["base_sha"]
    run(source, "cat-file", "-e", head + "^{commit}")
    run(source, "cat-file", "-e", base + "^{commit}")
    current = run(source, "rev-parse", "refs/remotes/origin/csharp-pr-%d" % number).strip()
    if current != head:
        run(source, "merge-base", "--is-ancestor", head, current)
    return base, head, current


def import_entry(source, target, entry, total):
    base, head, current = fetch_refs(source, entry)
    paths = [p for p in run(source, "diff", "--name-only", "--diff-filter=ACMR", base + "..." + head, "--").splitlines() if p]
    number = entry["number"]
    folder = target / ("pr-%04d" % number)
    folder.mkdir(parents=True, exist_ok=True)
    records = []
    excluded = 0
    for old_path in paths:
        dest = destination(old_path)
        if dest is None:
            excluded += 1
            continue
        raw = run(source, "show", head + ":" + old_path, binary=True)
        if len(raw) > MAX_FILE:
            raise RuntimeError("PR #%d: arquivo excede limite: %s" % (number, old_path))
        total[0] += len(raw)
        if total[0] > MAX_TOTAL:
            raise RuntimeError("Limite total de 100 MiB excedido")
        new_path = folder / dest
        new_path.parent.mkdir(parents=True, exist_ok=True)
        new_path.write_bytes(raw)
        records.append({
            "original": old_path,
            "path": dest,
            "sha256": hashlib.sha256(raw).hexdigest(),
            "bytes": len(raw),
        })
    if not records:
        raise RuntimeError("PR #%d sem arquivos C# relevantes: verificar filtro de importacao" % number)
    meta = {
        "pr": number,
        "name": entry["name"],
        "kind": entry["kind"],
        "status": entry["status"],
        "runtime_enabled": False,
        "base_sha": base,
        "head_sha": head,
        "current_pr_head_when_fetched": current,
        "file_count": len(records),
        "excluded_paths": excluded,
        "files": records,
    }
    (folder / "SOURCE.json").write_text(json.dumps(meta, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    return {
        "pr": number,
        "name": entry["name"],
        "status": entry["status"],
        "file_count": len(records),
        "bytes": sum(file["bytes"] for file in records),
        "runtime_enabled": False,
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", required=True, type=Path)
    args = parser.parse_args()
    source = args.source.resolve()
    config = json.loads((ROOT / "loader-prs.json").read_text(encoding="utf-8"))
    assert config["mode"] == "isolated_source_overlays"
    assert config["activation"] == "none"
    assert config["distribution"] == "disabled"
    target = ROOT / "imports" / "loader-prs"
    if target.exists():
        shutil.rmtree(target)
    target.mkdir(parents=True)
    imported = []
    skipped = []
    total = [0]
    for entry in config["pull_requests"]:
        if entry["import"]:
            imported.append(import_entry(source, target, entry, total))
        else:
            skipped.append({"pr": entry["number"], "name": entry["name"], "status": entry["status"], "reason": "origem tModLoader exige auditoria antes de reutilizar fontes"})
    owners = {}
    for summary in imported:
        manifest_path = target / ("pr-%04d" % summary["pr"]) / "SOURCE.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        for record in manifest["files"]:
            owners.setdefault(record["path"], []).append({
                "pr": summary["pr"],
                "sha256": record["sha256"],
            })
    overlaps = []
    for path, entries in owners.items():
        variants = {entry["sha256"] for entry in entries}
        if len(variants) > 1:
            overlaps.append({
                "path": path,
                "prs": [entry["pr"] for entry in entries],
                "variants": len(variants),
            })
    overlaps.sort(key=lambda entry: (-len(entry["prs"]), entry["path"]))
    (target / "CONFLICTS.json").write_text(
        json.dumps({"schema": 1, "different_content_shared_paths": len(overlaps), "files": overlaps},
                   indent=2, ensure_ascii=False) + "\n",
        encoding="utf-8")
    inventory = {
        "schema": 1,
        "different_content_shared_paths": len(overlaps),
        "highest_collision_paths": overlaps[:30],
        "upstream": config["upstream"],
        "target_baseline": config["target_baseline"],
        "mode": "isolated_source_overlays",
        "runtime_enabled": False,
        "android_tested": False,
        "apk_generated": False,
        "bytes": total[0],
        "imported": imported,
        "quarantined": skipped,
        "reviewed_not_imported": config["reviewed_not_imported"],
    }
    (target / "INDEX.json").write_text(json.dumps(inventory, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print("CSHARP_LOADER_OVERLAYS=PASS", len(imported), "PRs", total[0], "bytes", len(skipped), "provenance blocks")


if __name__ == "__main__":
    main()
