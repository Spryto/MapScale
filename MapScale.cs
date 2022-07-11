using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MapScale
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [HarmonyPatch]
    public class MapScale : BaseUnityPlugin
    {
        private const string GUID = "spryto.map-scale";
        private const string NAME = "Map Scale";
        private const string VERSION = "1.0.0";

        static ManualLogSource logger;
        private const float original_world_radius = 10000;
 
        void Awake()
        {
            logger = Logger;
            Settings.SetConfig(NAME, Config);

            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Ship), "ApplyEdgeForce")]
        public static IEnumerable<CodeInstruction> ApplyEdgeForce(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyPatch(typeof(Player), "EdgeOfWorldKill")]
        public class PlayerPatches
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return ReplaceAll(instructions);
            }

            public static bool Prefix(ref Player __instance) 
            {
                if(__instance.transform.position.y > 4000)
                {
                    return false; // skip method, we're in a dungeon
                }
                return true;
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(TerrainComp), "LevelTerrain")]
        public static IEnumerable<CodeInstruction> LevelTerrain(IEnumerable<CodeInstruction> instructions)
        {
            Dictionary<float, float> map = new Dictionary<float, float>();
            map.Add(8f, 1000f);
            map.Add(-8f, -1000f);
            return ReplaceMany(instructions, map);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(TerrainComp), "RaiseTerrain")]
        public static IEnumerable<CodeInstruction> RaiseTerrain(IEnumerable<CodeInstruction> instructions)
        {
            Dictionary<float, float> map = new Dictionary<float, float>();
            map.Add(8f, 1000f);
            map.Add(-8f, -1000f);
            return ReplaceMany(instructions, map);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(TerrainComp), "ApplyToHeightmap")]
        public static IEnumerable<CodeInstruction> ApplyToHeightmap(IEnumerable<CodeInstruction> instructions)
        {
            Dictionary<float, float> map = new Dictionary<float, float>();
            map.Add(8f, 1000f);
            map.Add(-8f, -1000f);
            return ReplaceMany(instructions, map);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WaterVolume), "GetWaterSurface")]
        public static IEnumerable<CodeInstruction> SpawnZone(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyPatch(typeof(WaterVolume), "SetupMaterial")]
        public class SetupMaterial
        {
            public static void Prefix(WaterVolume __instance)
            {
                var obj = __instance;
                obj.m_waterSurface.material.SetFloat("_WaterEdge", Settings.WorldSize.Value + 500f);
            }

            public static void Refresh()
            {
                var objects = FindObjectsOfType<WaterVolume>();
                foreach (var water in objects)
                {
                    water.m_waterSurface.material.SetFloat("_WaterEdge", Settings.WorldSize.Value + 500f);
                }
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WaterVolume), "GetWaterSurface")]
        public static IEnumerable<CodeInstruction> GetWaterSurface(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(EnvMan), "UpdateWind")]
        public static IEnumerable<CodeInstruction> UpdateWind(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WorldGenerator), "FindStreamStartPoint")]
        public static IEnumerable<CodeInstruction> FindStreamStartPoint(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WorldGenerator), "GetBaseHeight")]
        public static IEnumerable<CodeInstruction> GetBaseHeight(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WorldGenerator), "GetEdgeHeight")]
        public static IEnumerable<CodeInstruction> GetEdgeHeight(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WorldGenerator), "FindLakes")]
        public static IEnumerable<CodeInstruction> FindLakes(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WorldGenerator), "FindMountains")]
        public static IEnumerable<CodeInstruction> FindMountains(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZoneSystem), "GenerateLocations", new[] { typeof(ZoneSystem.ZoneLocation) })]
        public static void GenerateLocationsPrefix(ZoneSystem.ZoneLocation __0)
        {
            var area_scale = Math.Pow(Settings.WorldSize.Value / original_world_radius, 2);
            var original = __0.m_quantity;
            var min = 5;
            if (__0.m_prefabName == "StartTemple")
            {
                return;
            }

            if (area_scale <= 0.5)
            {
                // make more attempts to spawn some problematic bosses 
                if (__0.m_prefabName == "Bonemass")
                {
                    min = 20;
                }
                if (__0.m_prefabName == "GoblinKing")
                {
                    min = 10;
                }
                if (__0.m_prefabName == "GDKing")
                {
                    min = 10;
                }
            }

            __0.m_quantity = Math.Max((int)Math.Ceiling(__0.m_quantity * area_scale * Settings.LocationDensity.Value), min);
            logger.LogInfo($"Generating {__0.m_quantity} (originally {original}) {__0.m_prefabName}(s)");
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ZoneSystem), "GenerateLocations", new[] { typeof(ZoneSystem.ZoneLocation) })]
        public static IEnumerable<CodeInstruction> GenerateLocations(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceOne(ReplaceAll(instructions), 20, 50);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ZoneSystem), "GetRandomZone")]
        public static IEnumerable<CodeInstruction> GetRandomZone(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Minimap), "GenerateWorldMap")]
        public static void GenerateWorldMap(ref Minimap __instance)
        {
            var scale = Settings.WorldSize.Value / original_world_radius;
            __instance.m_pixelSize = 12f * scale;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ZoneSystem), "GetRandomPointInZone", new [] {typeof(float)})]
        public static IEnumerable<CodeInstruction> GetRandomPointInZone(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceAll(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(WorldGenerator), "GetBiome", new[] { typeof(float), typeof(float) })]
        public static IEnumerable<CodeInstruction> GetBiome(IEnumerable<CodeInstruction> instructions)
        {
            Dictionary<float, float> map = new Dictionary<float, float>();
            var scale = Settings.WorldSize.Value / original_world_radius;
            
            map.Add(12000f, 12000f * scale);
            map.Add(10000f, 10000f * scale);
            map.Add(8000f, 8000f * scale);
            map.Add(6000f, 6000f * scale);
            map.Add(5000f, 5000f * scale);
            map.Add(4000f, 4000f * scale);
            map.Add(3000f, 3000f * scale);
            map.Add(2000f, 2000f * scale);

            map.Add(600f, Math.Max(600f * scale, 300f));
            map.Add(-4000f, -4000f * scale);

            return ReplaceMany(instructions, map);
        }

        private static IEnumerable<CodeInstruction> ReplaceAll(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand >= 10000 && (float)instruction.operand <= 10500)
                {
                    var offset = (float)instruction.operand - original_world_radius;
                    instruction.operand = Settings.WorldSize.Value + offset;
                }
                if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == -10000 )
                {
                    instruction.operand = -Settings.WorldSize.Value;
                }
                yield return instruction;
            }
        }

        private static IEnumerable<CodeInstruction> ReplaceMany(IEnumerable<CodeInstruction> instructions, Dictionary<float, float> replacementMap)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4)
                {
                    if (replacementMap.ContainsKey((float)instruction.operand))
                    {
                        instruction.operand = replacementMap[(float)instruction.operand];
                    }
                }
                yield return instruction;
            }
        }

        private static IEnumerable<CodeInstruction> ReplaceOne(IEnumerable<CodeInstruction> instructions, float from, float to)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4)
                {
                    if ((float)instruction.operand == from)
                    {
                        instruction.operand = to;
                    }
                }
                yield return instruction;
            }
        }

        private static void LogInstructions(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions) 
            {
                logger.LogInfo($"{instruction}");
            }
        }
    }
}