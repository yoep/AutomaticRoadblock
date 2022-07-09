using System.Diagnostics.CodeAnalysis;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class PropUtils
    {
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

        #endregion

        #region Street items

        public static Object CreateSmallBlankCone(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_cone_03"), position);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateSmallConeWithStripes(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_cone_02"), position);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateBigConeWithStripes(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_cone_01"), position);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateLargeThinConeWithStripes(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_cone_04"), position);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateConeWithLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_air_conelight"), position, heading);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateBarrier(Vector3 position, float heading = 0f)
        {
            return new Object(new Model("prop_ld_barrier_01"), position, heading);
        }

        public static Object CreateWorkerBarrierArrowRight(Vector3 position)
        {
            return new Object(new Model("prop_mp_arrow_barrier_01"), position);
        }

        public static Object RedirectTrafficArrowLeft(Vector3 position, float heading)
        {
            return new Object(new Model("prop_trafficdiv_01"), position, heading);
        }

        public static Object RedirectTrafficArrowBoth(Vector3 position, float heading)
        {
            return new Object(new Model("prop_trafficdiv_02"), position, heading);
        }

        public static Object StoppedVehiclesSign(Vector3 position, float heading)
        {
            var instance = new Object(new Model("prop_consign_02a"), position, heading);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreatePoliceDoNotCrossBarrier(Vector3 position, float heading = 0f)
        {
            var barrier = new Object(new Model("prop_barrier_work05"), position, heading);
            PlaceCorrectlyOnGround(barrier);
            return barrier;
        }

        public static Object CreateGeneratorWithLights(Vector3 position, float heading = 0f)
        {
            return new Object(new Model("prop_generator_03b"), position, heading);
        }

        public static Object CreateFloodLight(Vector3 position, float heading = 0f)
        {
            return new Object(new Model("prop_worklight_03a"), position, heading);
        }

        public static Object CreateFloodLights(Vector3 position, float heading = 0f)
        {
            return new Object(new Model("prop_worklight_03b"), position, heading);
        }

        public static Object CreateGroundFloodLight(Vector3 position, float heading = 0f)
        {
            return new Object(new Model("prop_worklight_02a"), position, heading);
        }

        public static Object CreateRedGroundLight(Vector3 position)
        {
            var instance = new Object(new Model("prop_air_lights_02b"), position);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateMedKit(Vector3 position)
        {
            return new Object(new Model("prop_ld_health_pack"), position);
        }

        #endregion

        #region Methods

        public static bool PlaceCorrectlyOnGround(Object instance)
        {
            Assert.NotNull(instance, "instance cannot be null");
            return NativeFunction.Natives.PLACE_OBJECT_ON_GROUND_PROPERLY<bool>(instance);
        }

        public static void SetVisibility(Object instance, bool isVisible)
        {
            Assert.NotNull(instance, "instance cannot be null");
            if (!instance.IsValid())
                return;

            instance.Opacity = isVisible ? 1f : 0f;
        }

        public static void Remove(Object entity)
        {
            EntityUtils.Remove(entity);
        }

        #endregion
    }
}