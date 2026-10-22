#!/usr/bin/env python3
import json
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
FILE = ROOT / "loader-completion-matrix.json"
REQUIRED = (
    "source_contract", "managed_compiler", "native_binding",
    "complete_example", "host_tests", "android_abi", "android_gameplay",
    "multi_mod_lifecycle", "save_network", "fps_memory",
)
QUARANTINED = {134, 135, 138}

def validate(data):
    assert data["schema"] == 1
    assert data["rules"]["noTModLoaderSource"] is True
    assert data["rules"]["noJsExecutionForCSharp"] is True
    assert data["rules"]["requireFiveCompletePerApk"] is True
    assert data["release"] == "blocked" or data["release"] == "per_batch"
    items = data["loaders"]
    assert len(items) == data["rules"]["loaderCount"] == 50
    assert len({entry["name"] for entry in items}) == len(items)
    group_counts = Counter(entry["group"] for entry in items)
    assert len(group_counts) == data["rules"]["loaderPackages"] == 10
    assert set(group_counts.values()) == {5}
    delivery_batches = data.get("delivery_batches", [])
    assert isinstance(delivery_batches, list)
    delivery_ids = set()
    for batch in delivery_batches:
        assert batch["id"] not in delivery_ids
        delivery_ids.add(batch["id"])
        assert batch["state"] in ("planned", "validated")
        assert len(batch["names"]) == 5
        assert len(set(batch["names"])) == 5
        assert set(batch["names"]) <= {entry["name"] for entry in items}
    totals = Counter()
    blocked = []
    for entry in items:
        assert isinstance(entry["modules"], list)
        assert len(entry["modules"]) == len(set(entry["modules"]))
        assert tuple(entry["checks"].keys()) == REQUIRED
        assert all(isinstance(value, bool) for value in entry["checks"].values())
        assert entry["completion"] in ("not_certified", "certified")
        assert isinstance(entry["source_prs"], list)
        has_quarantined = set(entry["source_prs"]) & QUARANTINED
        if has_quarantined:
            assert entry["source_scope"] == "provenance_blocked"
            assert entry["completion"] != "certified"
            blocked.append(entry["name"])
        if entry["reference_example"] == "REPLACE_WITH_BUNNY_ORIGINAL":
            assert entry["completion"] != "certified"
            blocked.append(entry["name"])
        if entry["completion"] == "certified":
            assert all(entry["checks"].values()), entry["name"] + " missing certification gates"
            evidence = entry.get("evidence", {})
            assert all(evidence.get(key) for key in ("revision", "ci_run", "android_test")), (
                entry["name"] + " lacks source and Android evidence")
        totals[entry["completion"]] += 1
    if data["release"] == "per_batch":
        certified = {entry["name"] for entry in items if entry["completion"] == "certified"}
        assert any(set(batch["names"]) <= certified and batch["state"] == "validated"
                   for batch in delivery_batches), "No five-loader delivery batch is certified"
    return group_counts, totals, sorted(set(blocked))

def main():
    data = json.loads(FILE.read_text(encoding="utf-8"))
    groups, totals, quarantined = validate(data)
    print("CSHARP_LOADER_GATE=PASS")
    print("LOADERS=%d GROUPS=%d CERTIFIED=%d NOT_CERTIFIED=%d" % (
        len(data["loaders"]), len(groups), totals["certified"], totals["not_certified"]))
    print("PROVENANCE_BLOCKED=" + ",".join(quarantined))
    print("APK_POLICY=%s" % data["release"])

if __name__ == "__main__":
    main()
