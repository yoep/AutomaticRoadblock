using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils
{
    public static class PropUtils
    {
        #region Models

        public static class Models
        {
            public static Model TrafficArrowLeft => new("prop_trafficdiv_01");
            public static Model TrafficArrowBoth => new("prop_trafficdiv_01");
            public static Model SpikeStrip => new("p_ld_stinger_s");
        }

        #endregion

        #region Ped items

        public static Object CreateWand()
        {
            var model = new Model("prop_parking_wand_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateOpenNotebook()
        {
            var model = new Model("prop_notepad_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateNotebook()
        {
            var model = new Model("prop_notepad_02");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreatePencil()
        {
            var model = new Model("prop_pencil_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateProtestSign1()
        {
            var model = new Model("prop_cs_protest_sign_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateProtestSign2()
        {
            var model = new Model("prop_cs_protest_sign_02");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateProtestSign2B()
        {
            var model = new Model("prop_cs_protest_sign_02b");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateProtestSign3()
        {
            var model = new Model("prop_cs_protest_sign_03");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateFlareVertical()
        {
            var model = new Model("prop_flare_01");
            return new Object(model, Vector3.Zero);
        }

        #endregion

        #region Street items

        public static Object CreateWorkerBarrierArrowRight(Vector3 position, float heading)
        {
            var instance = new Object(new Model("prop_mp_arrow_barrier_01"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object RedirectTrafficArrowLeft(Vector3 position, float heading)
        {
            var instance = new Object(Models.TrafficArrowLeft, position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object RedirectTrafficArrowBoth(Vector3 position, float heading)
        {
            var instance = new Object(Models.TrafficArrowBoth, position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object StoppedVehiclesSign(Vector3 position, float heading)
        {
            var instance = new Object(new Model("prop_consign_02a"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateGroundFloodLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_worklight_02a"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateMedKit(Vector3 position)
        {
            return new Object(new Model("prop_ld_health_pack"), position);
        }

        public static Object CreateSpikeStrip(Vector3 position, float heading)
        {
            var instance = new Object(Models.SpikeStrip, position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        #endregion

        #region Methods

        public static Object PlaceCorrectlyOnGround(Object instance)
        {
            Assert.NotNull(instance, "instance cannot be null");
            NativeFunction.Natives.PLACE_OBJECT_ON_GROUND_PROPERLY<bool>(instance);
            return instance;
        }

        #endregion
    }
}