#!/usr/bin/env python3
import json
import re
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[3]
data = json.loads((root / "docs/reference/repository/csharp-modsystem-coverage.json").read_text())
sdk = (root / "src/managed/BunnyCsSdk/ModSystem.cs").read_text()
declared = set(data["declaredOriginalHooks"])
missing = set(data["missingOriginalHooks"])
assert data["originalHookCount"] == 58, "Original snapshot changed; audit required"
assert declared.isdisjoint(missing), "Hook cannot be declared and missing"
assert len(declared | missing) == 58, "Incomplete or duplicated source catalog"
for name in declared:
    assert re.search(r"public virtual void\s+" + re.escape(name) + r"\s*\(", sdk), (
        name + " listed without CSharp SDK method")
for name in missing:
    assert not re.search(r"public virtual void\s+" + re.escape(name) + r"\s*\(", sdk), (
        name + " available in SDK but not audited; update matrix")
assert data["androidValidated"] is False or len(missing) == 0, (
    "Android completion must not be claimed with missing hooks")
if "--require-complete" in sys.argv:
    if missing or not data["androidValidated"] or not data["scopeComplete"]:
        raise SystemExit("MODSYSTEM_RELEASE_BLOCKED: missing hooks or Android QA incomplete")
print(f"MODSYSTEM_COVERAGE_OK original=58 declared={len(declared)} missing={len(missing)} android={data['androidValidated']}")
