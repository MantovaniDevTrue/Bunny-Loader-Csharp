#!/usr/bin/env python3
from pathlib import Path
import re
root=Path(__file__).resolve().parents[3]
stage=(root/"src/native/experiments/csharp/ModSystemStages.cpp").read_text()
runtime=(root/"src/native/experiments/csharp/AutoSourceRuntime.cpp").read_text()
expected=[
    ("PlayersBegin","0x1b8",(22,),(33,),("0xd00100bd","0xd001011a","0xf00100f9","0xd000ffdc")),
    ("PlayersEnd","0x57c",(23,),(34,),("0xd0010028","0xf940b108","0x2a1f03f5","0xf9400108")),
    ("NPCsBegin","0x884",(24,),(35,),("0xaa1f03e0","0x3900811f","0x94063b0a","0xaa1f03e0")),
    ("NPCsEnd","0xf40",(25,26),(36,37),("0xd0010028","0xf940b108","0x120002c9","0xd00100f5")),
    ("GoresEnd","0x114c",(27,),(38,),("0x2a1f03f5","0xf94003a0","0xb940e008","0x35000068")),
    ("ItemsBegin","0x1740",(28,29),(40,41),("0xd000feb7","0xf000fed8","0xd000ff99","0x9001017c")),
    ("ItemsEnd","0x1a14",(30,),(42,),("0x9000ffc8","0xf9437908","0xb0010037","0xd00100b6")),
]
for idx,(name,offset,phases,bits,signature) in enumerate(expected):
    table=r'\{"'+name+r'",\s*'+offset+r',\s*\{'+r',\s*'.join(signature)+r'\},'
    assert re.search(table,stage), "Native original instruction mismatch: "+name
    case=re.search(r'case\s+'+str(idx)+r':(.*?)break;',stage,re.S)
    assert case, "Missing native stage "+name
    assert tuple(map(int,re.findall(r'dispatchAutoSourceSystems\((\d+)\)',case.group(1))))==phases, "Wrong phase ordering "+name
    assert "blCsSystemStageBridge"+str(idx) in stage
    for phase,bit in zip(phases,bits):
        assert re.search(r'(?:if|else if)\s*\(phase\s*==\s*'+str(phase)+r'\)\s*hookBit\s*=\s*'+str(bit)+r';',runtime), "Mask/phase mismatch"
assert re.search(r'using QuitGameFn\s*=\s*void\s*\(\*\)\(Il2CppObject\*,\s*const MethodInfo\*\)',runtime), "QuitGame must receive Main self"
assert re.search(r'void hkCSharpQuitGame\(Il2CppObject\* self, const MethodInfo\* method\)',runtime), "QuitGame hook ABI changed"
assert 'g_originalQuitGame(self, method)' in runtime, "QuitGame must forward Main instance"
assert 'LoadFilesForCulture' in runtime and 'hkCSharpModSystemLanguage' in runtime, "Missing native reload event"
assert re.search(r'else if\s*\(phase\s*==\s*33\)\s*hookBit\s*=\s*3',runtime), "Wrong localization bit"
assert re.search(r'else if\s*\(phase\s*==\s*34\)\s*hookBit\s*=\s*52',runtime), "Wrong nearby tile bit"
assert 'self == playerMetrics' in runtime and 'get_PlayerSceneMetrics' in runtime, "Scene reset must only use player metrics"
assert 'dispatchAutoSourceSystems(34);' in runtime, "Missing world change reset"
assert 'else if (phase == 35) hookBit = 12;' in runtime, "PostWorldLoad bit missing"
assert 'dispatchAutoSourceSystems(2);\n        dispatchAutoSourceSystems(35);' in runtime, "PostWorldLoad must follow world start"
assert 'refreshSystemDispatchSnapshotLocked();' in runtime, "Dispatch snapshot must refresh"
assert 'std::atomic_load_explicit(' in runtime, "Callbacks must reuse cached target snapshot"
native=(root/"src/native/experiments/csharp/ManagedRuntime.cpp").read_text()
sdk=(root/"src/managed/BunnyCsSdk/ModSystem.cs").read_text()
native_boundary=re.search(r'bool dispatchManagedSourceSystem\(.*?phase\s*>\s*(\d+)',native,re.S)
sdk_boundary=re.search(r'public static int Dispatch\(Assembly assembly, int phase\).*?phase\s*>\s*(\d+)',sdk,re.S)
assert native_boundary and sdk_boundary, "Dispatcher phase boundaries not found"
assert native_boundary.group(1)==sdk_boundary.group(1)=="37", "SDK/native dispatch phases differ"
assert "if (g_withinModSystemCallback)" in native and "scope(g_withinModSystemCallback)" in native, "Nested native callbacks could deadlock"
assert "if (!validAssembly(assemblyPath))" in native, "DLL must be validated on first load"
for phase, bit in ((33,3),(34,52),(35,12),(36,7),(37,8)):
    assert re.search(r'else if\s*\(phase\s*==\s*'+str(phase)+r'\)\s*hookBit\s*=\s*'+str(bit),runtime), "Missing native hook bit"
assert 'ModifyLightingBrightness' in sdk, "SDK brightness hook missing"
assert 'modifyManagedSourceSystemBrightness' in native, "Mono scalar bridge missing"
assert 'UpdateGlobalBrightness' in runtime and 'GlobalBrightness' in runtime, "Native field brightness missing"
assert 'modifyManagedSourceSystemBrightness(target.modId' in runtime, "Brightness must forward float"
probe = (root/"src/managed/BunnySourceCompiler/CompiledAssemblyProbe.cs").read_text()
assert '"BunnyLoader.Source.BunnySystemRecipes"' in probe and 'GetMethod("Generate"' in probe, "No recipe probe bridge"
assert 'PrepareRecipes' in (root/"src/managed/BunnySourceCompiler/BasicItemWriter.cs").read_text(), "Recipe item serialization may be duplicated"
assert 'ModifySunLightColor' in sdk and 'ModifyScreenPosition' in sdk, "Color/camera API missing"
assert 'modifyManagedSourceSystemPackedValue' in native, "64-bit Mono native bridge missing"
assert 'ApplyColorOfTheSkiesToTiles' in runtime, "Sunlight native hook missing"
assert 'DoDraw_UpdateCameraPosition' in runtime, "Camera native hook missing"
assert 'field_static_set_value(g_modSystemCameraPosition' in runtime, "Camera result not applied"
assert 'field_static_set_value(g_modSystemTileColor' in runtime, "Sunlight result not applied"
assert 'field_static_set_value(g_modSystemSkyColor' in runtime, "Sky result not applied"
assert 'isModSystemNativeStructField(g_modSystemTileColor, 4)' in runtime, "Tile Color must be 4 bytes"
assert 'isModSystemNativeStructField(g_modSystemSkyColor, 4)' in runtime, "Sky Color must be 4 bytes"
assert 'isModSystemNativeStructField(g_modSystemCameraPosition, 8)' in runtime, "Camera Vector2 must be 8 bytes"
assert 'TileCountsAvailable' in sdk and 'tileCounts.Length > 8192' in sdk, "C# tile counts contract missing"
assert 'AggregateTileCounts' in runtime and 'isModSystemLocalPlayerMetrics' in runtime, "Local metrics native hook missing"
assert 'g_modSystemTileCountsOffset' in runtime and 'std::strcmp(name, "Int32")' in runtime, "Tile count ABI validation absent"
assert 'dispatchManagedSourceSystemTileCounts' in native, "Mono tile counts dispatch missing"
assert 'mono_array_new' in native and 'mono_array_addr_with_size' in native, "Mono int[] allocation/copy unavailable"
assert 'if (!g_modSystemTileCountsOffset)' not in runtime, "Invalid guard"
assert len(expected)==7
print("MODSYSTEM_NATIVE_STAGES=PASS edges=7 hooks=9 version=301720")
