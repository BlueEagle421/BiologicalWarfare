using HarmonyLib;
using Verse;

namespace BiologicalWarfare
{
    public class BiologicalWarfareMod : Mod
    {
        public BiologicalWarfareMod(ModContentPack content) : base(content)
        {
            Harmony.DEBUG = false;
            Harmony harmony = new Harmony("biologicalwarfare");

            harmony.PatchAll();
        }
    }
}