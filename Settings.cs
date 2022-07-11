using BepInEx.Configuration;

namespace MapScale
{
    class Settings
    {
        public static ConfigEntry<float> WorldSize;
        public static ConfigEntry<float> LocationDensity;

        public static void SetConfig(string name, ConfigFile config)
        {
            var worldSizeDescription = new ConfigDescription(
                "An ordinary world has a size of 10000. " +
                "3333 is 1/9 area. 5000 is 1/4 area. " +
                "20000 is 4x area. 30000 is 9x area.",
                new AcceptableValueRange<float>(2500f, 100000f));
            WorldSize = config.Bind(name + ".Global", "world_size", 10000f, worldSizeDescription);


            var locationDensityDescription = new ConfigDescription(
                "This mod automatically scales the amount of key locations in the game to fit the world size, " +
                "but you can reduce it here if you're making a particularly large world. You can also increase " +
                "it a little but the world is already close to the maximum density of these.",
                new AcceptableValueRange<float>(0f, 2f));
            LocationDensity = config.Bind(name + ".Global", "location_density", 1f, locationDensityDescription);
        }
    }
}
