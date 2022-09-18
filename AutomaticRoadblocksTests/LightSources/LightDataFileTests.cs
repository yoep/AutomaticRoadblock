using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AutomaticRoadblocks.LightSources
{
    public class LightDataFileTests
    {
        public LightDataFileTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        [Fact]
        public void TestLightsDeserialization()
        {
            var expectedFlares = new Light("Flares", Light.FlaresScriptName, "weapon_flare", 1.0, ELightSourceFlags.Lane);
            var expectedSpots = new Light("Spots", Light.SpotsScriptName, "prop_generator_03b", ELightSourceFlags.RoadLeft | ELightSourceFlags.RoadRight);
            var expectedGroundSpots = new Light("Ground spots", Light.GroundSpotsScriptName, "prop_worklight_02a", 0.0, 180.0, ELightSourceFlags.RoadLeft | ELightSourceFlags.RoadRight);
            var modelData = IoC.Instance.GetInstance<ILightSourceData>();

            var result = modelData.Lights;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.True(result.Items.Count > 6);
            Xunit.Assert.Equal(expectedFlares, result.Items.First(x => x.ScriptName.Equals(Light.FlaresScriptName)));
            Xunit.Assert.Equal(expectedSpots, result.Items.First(x => x.ScriptName.Equals(Light.SpotsScriptName)));
            Xunit.Assert.Equal(expectedGroundSpots, result.Items.First(x => x.ScriptName.Equals(Light.GroundSpotsScriptName)));
        }
    }
}