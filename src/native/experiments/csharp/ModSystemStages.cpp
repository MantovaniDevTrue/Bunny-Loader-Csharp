#include "experiments/csharp/ModSystemStages.h"
#include "experiments/csharp/ManagedRuntime.h"
#include "il2cpp/Resolver.h"
#include "il2cpp/Signature.h"
#include "hook/HookManager.h"
#include "core/Log.h"
#include <array>
#include <atomic>
#include <cstdint>
#include <cstring>

namespace bl::csharp {
void dispatchAutoSourceSystems(int phase);
}

#if defined(__aarch64__)
extern "C" {
__attribute__((visibility("hidden"))) void* g_blCsSystemStageOriginal0 = nullptr;
__attribute__((visibility("hidden"))) void* g_blCsSystemStageOriginal1 = nullptr;
__attribute__((visibility("hidden"))) void* g_blCsSystemStageOriginal2 = nullptr;
__attribute__((visibility("hidden"))) void* g_blCsSystemStageOriginal3 = nullptr;
__attribute__((visibility("hidden"))) void* g_blCsSystemStageOriginal4 = nullptr;
__attribute__((visibility("hidden"))) void* g_blCsSystemStageOriginal5 = nullptr;
__attribute__((visibility("hidden"))) void* g_blCsSystemStageOriginal6 = nullptr;
}

namespace {
struct alignas(16) StageFrame {
    uint64_t general[31];
    uint64_t flags;
    alignas(16) uint8_t vectors[32][16];
};
static_assert(sizeof(StageFrame) == 768);
extern "C" __attribute__((visibility("hidden")))
void blCsSystemStageDispatch(unsigned stageIndex, StageFrame*) {
    switch (stageIndex) {
        case 0:
            bl::csharp::dispatchAutoSourceSystems(22);
            break;
        case 1:
            bl::csharp::dispatchAutoSourceSystems(23);
            break;
        case 2:
            bl::csharp::dispatchAutoSourceSystems(24);
            break;
        case 3:
            bl::csharp::dispatchAutoSourceSystems(25);
            bl::csharp::dispatchAutoSourceSystems(26);
            break;
        case 4:
            bl::csharp::dispatchAutoSourceSystems(27);
            break;
        case 5:
            bl::csharp::dispatchAutoSourceSystems(28);
            bl::csharp::dispatchAutoSourceSystems(29);
            break;
        case 6:
            bl::csharp::dispatchAutoSourceSystems(30);
            break;
        default: break;
    }
}

#define BL_CS_SYSTEM_STAGE_SAVE \
    "sub sp, sp, #768\n" \
    "stp x0, x1, [sp, #0]\n" \
    "stp x2, x3, [sp, #16]\n" \
    "stp x4, x5, [sp, #32]\n" \
    "stp x6, x7, [sp, #48]\n" \
    "stp x8, x9, [sp, #64]\n" \
    "stp x10, x11, [sp, #80]\n" \
    "stp x12, x13, [sp, #96]\n" \
    "stp x14, x15, [sp, #112]\n" \
    "stp x16, x17, [sp, #128]\n" \
    "stp x18, x19, [sp, #144]\n" \
    "stp x20, x21, [sp, #160]\n" \
    "stp x22, x23, [sp, #176]\n" \
    "stp x24, x25, [sp, #192]\n" \
    "stp x26, x27, [sp, #208]\n" \
    "stp x28, x29, [sp, #224]\n" \
    "str x30, [sp, #240]\n" \
    "mrs x9, nzcv\n" \
    "str x9, [sp, #248]\n" \
    "stp q0, q1, [sp, #256]\n" \
    "stp q2, q3, [sp, #288]\n" \
    "stp q4, q5, [sp, #320]\n" \
    "stp q6, q7, [sp, #352]\n" \
    "stp q8, q9, [sp, #384]\n" \
    "stp q10, q11, [sp, #416]\n" \
    "stp q12, q13, [sp, #448]\n" \
    "stp q14, q15, [sp, #480]\n" \
    "stp q16, q17, [sp, #512]\n" \
    "stp q18, q19, [sp, #544]\n" \
    "stp q20, q21, [sp, #576]\n" \
    "stp q22, q23, [sp, #608]\n" \
    "stp q24, q25, [sp, #640]\n" \
    "stp q26, q27, [sp, #672]\n" \
    "stp q28, q29, [sp, #704]\n" \
    "stp q30, q31, [sp, #736]\n"
#define BL_CS_SYSTEM_STAGE_RESTORE \
    "ldp q0, q1, [sp, #256]\n" \
    "ldp q2, q3, [sp, #288]\n" \
    "ldp q4, q5, [sp, #320]\n" \
    "ldp q6, q7, [sp, #352]\n" \
    "ldp q8, q9, [sp, #384]\n" \
    "ldp q10, q11, [sp, #416]\n" \
    "ldp q12, q13, [sp, #448]\n" \
    "ldp q14, q15, [sp, #480]\n" \
    "ldp q16, q17, [sp, #512]\n" \
    "ldp q18, q19, [sp, #544]\n" \
    "ldp q20, q21, [sp, #576]\n" \
    "ldp q22, q23, [sp, #608]\n" \
    "ldp q24, q25, [sp, #640]\n" \
    "ldp q26, q27, [sp, #672]\n" \
    "ldp q28, q29, [sp, #704]\n" \
    "ldp q30, q31, [sp, #736]\n" \
    "ldr x9, [sp, #248]\n" \
    "msr nzcv, x9\n" \
    "ldp x0, x1, [sp, #0]\n" \
    "ldp x2, x3, [sp, #16]\n" \
    "ldp x4, x5, [sp, #32]\n" \
    "ldp x6, x7, [sp, #48]\n" \
    "ldp x8, x9, [sp, #64]\n" \
    "ldp x10, x11, [sp, #80]\n" \
    "ldp x12, x13, [sp, #96]\n" \
    "ldp x14, x15, [sp, #112]\n" \
    "ldp x16, x17, [sp, #128]\n" \
    "ldp x18, x19, [sp, #144]\n" \
    "ldp x20, x21, [sp, #160]\n" \
    "ldp x22, x23, [sp, #176]\n" \
    "ldp x24, x25, [sp, #192]\n" \
    "ldp x26, x27, [sp, #208]\n" \
    "ldp x28, x29, [sp, #224]\n" \
    "ldr x30, [sp, #240]\n" \
    "add sp, sp, #768\n"
#define BL_CS_SYSTEM_STAGE_BRIDGE(name, index, target) \
__attribute__((naked)) void name() { \
    __asm__ volatile(BL_CS_SYSTEM_STAGE_SAVE \
        "mov x1, sp\n" "mov w0, #" #index "\n" "bl blCsSystemStageDispatch\n" \
        BL_CS_SYSTEM_STAGE_RESTORE \
        "adrp x16, " #target "\n" "ldr x16, [x16, :lo12:" #target "]\n" "br x16\n"); \
}
BL_CS_SYSTEM_STAGE_BRIDGE(blCsSystemStageBridge0, 0, g_blCsSystemStageOriginal0)
BL_CS_SYSTEM_STAGE_BRIDGE(blCsSystemStageBridge1, 1, g_blCsSystemStageOriginal1)
BL_CS_SYSTEM_STAGE_BRIDGE(blCsSystemStageBridge2, 2, g_blCsSystemStageOriginal2)
BL_CS_SYSTEM_STAGE_BRIDGE(blCsSystemStageBridge3, 3, g_blCsSystemStageOriginal3)
BL_CS_SYSTEM_STAGE_BRIDGE(blCsSystemStageBridge4, 4, g_blCsSystemStageOriginal4)
BL_CS_SYSTEM_STAGE_BRIDGE(blCsSystemStageBridge5, 5, g_blCsSystemStageOriginal5)
BL_CS_SYSTEM_STAGE_BRIDGE(blCsSystemStageBridge6, 6, g_blCsSystemStageOriginal6)
#undef BL_CS_SYSTEM_STAGE_BRIDGE
#undef BL_CS_SYSTEM_STAGE_SAVE
#undef BL_CS_SYSTEM_STAGE_RESTORE

struct Stage {
    const char* name;
    size_t offset;
    std::array<uint32_t, 4> signature;
    uint64_t hookBits;
    void (*bridge)();
    void** original;
    bool failed = false;
};
Stage stages[] = {
    {"PlayersBegin", 0x1b8, {0xd00100bd, 0xd001011a, 0xf00100f9, 0xd000ffdc}, (1ULL << 33), blCsSystemStageBridge0, &g_blCsSystemStageOriginal0, false},
    {"PlayersEnd", 0x57c, {0xd0010028, 0xf940b108, 0x2a1f03f5, 0xf9400108}, (1ULL << 34), blCsSystemStageBridge1, &g_blCsSystemStageOriginal1, false},
    {"NPCsBegin", 0x884, {0xaa1f03e0, 0x3900811f, 0x94063b0a, 0xaa1f03e0}, (1ULL << 35), blCsSystemStageBridge2, &g_blCsSystemStageOriginal2, false},
    {"NPCsEnd", 0xf40, {0xd0010028, 0xf940b108, 0x120002c9, 0xd00100f5}, (1ULL << 36) | (1ULL << 37), blCsSystemStageBridge3, &g_blCsSystemStageOriginal3, false},
    {"GoresEnd", 0x114c, {0x2a1f03f5, 0xf94003a0, 0xb940e008, 0x35000068}, (1ULL << 38), blCsSystemStageBridge4, &g_blCsSystemStageOriginal4, false},
    {"ItemsBegin", 0x1740, {0xd000feb7, 0xf000fed8, 0xd000ff99, 0x9001017c}, (1ULL << 40) | (1ULL << 41), blCsSystemStageBridge5, &g_blCsSystemStageOriginal5, false},
    {"ItemsEnd", 0x1a14, {0x9000ffc8, 0xf9437908, 0xb0010037, 0xd00100b6}, (1ULL << 42), blCsSystemStageBridge6, &g_blCsSystemStageOriginal6, false}
};

bool installStage(Stage& stage, const MethodInfo* method) {
    if (*stage.original) return true;
    if (stage.failed) return false;
    auto* entry = method
        ? static_cast<const uint8_t*>(bl::il2cpp::methodPointer(method))
        : nullptr;
    if (!entry || std::memcmp(entry + stage.offset,
            stage.signature.data(), sizeof(stage.signature)) != 0 ||
        !bl::hook::install(const_cast<uint8_t*>(entry + stage.offset),
            reinterpret_cast<void*>(stage.bridge), stage.original)) {
        stage.failed = true;
        BL_WARN("[CSharp][ModSystem] stage %s: assinatura Terraria incompatível ou hook recusado",
                stage.name);
        return false;
    }
    return true;
}
} // namespace
#endif

namespace bl::csharp {
bool installNativeModSystemStageHooks(uint64_t hookMask) {
#if defined(__aarch64__)
    bool ready = true;
    const MethodInfo* updateInWorld = nullptr;
    for (Stage& stage : stages) {
        if (!(hookMask & stage.hookBits)) continue;
        if (!updateInWorld) {
            auto* cls = bl::il2cpp::findClass({"Terraria", "Main", {}});
            updateInWorld = cls
                ? bl::il2cpp::findMethodBySignature(cls,
                    bl::il2cpp::parseSignature("void DoUpdateInWorld()"))
                : nullptr;
        }
        if (!installStage(stage, updateInWorld)) ready = false;
    }
    return ready;
#else
    return hookMask == 0;
#endif
}
} // namespace bl::csharp
