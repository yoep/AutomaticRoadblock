using System.Diagnostics.CodeAnalysis;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class PropUtils
    {
        #region Models

        public static class Models
        {
            public static Model BigCone => new("prop_roadcone01c");
            public static Model BigConeWithStrips => new("prop_mp_cone_01");
            public static Model ConeWithLight => new("prop_air_conelight");
            public static Model PoliceDoNotCross => new("prop_barrier_work05");
            public static Model SmallCone => new("prop_mp_cone_03");
            public static Model SmallConeWithStrips => new("prop_mp_cone_02");
            public static Model WorkBarrierHigh => new("prop_mp_barrier_02b");
            public static Model WorkBarrierLarge => new("prop_barrier_work06a");
            public static Model WorkBarrierSmall => new("prop_barrier_work01a");
            public static Model WorkBarrierAHeadSign => new("prop_barrier_work04a");
            public static Model WorkBarrierAHeadSignLights => new("prop_barrier_work06b");
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

        public static Object CreateSmallBlankCone(Vector3 position)
        {
            var instance = new Object(Models.SmallCone, position);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateSmallConeWithStripes(Vector3 position)
        {
            var instance = new Object(Models.SmallConeWithStrips, position);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateBigCone(Vector3 position)
        {
            var instance = new Object(Models.BigCone, position);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateBigConeWithStripes(Vector3 position)
        {
            var instance = new Object(Models.BigConeWithStrips, position);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateLargeThinConeWithStripes(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_cone_04"), position);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateConeWithLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(Models.ConeWithLight, position, heading);
            PlaceCorrectlyOnGround(instance);
            instance.Position += Vector3.WorldDown * 0.1f;
            return instance;
        }

        public static Object CreateWorkBarrierLarge(Vector3 position, float heading = 0f)
        {
            var instance = new Object(Models.WorkBarrierLarge, position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateWorkBarrierSmall(Vector3 position, float heading = 0f)
        {
            var instance = new Object(Models.WorkBarrierSmall, position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateBarrierWithWorkAhead(Vector3 position, float heading = 0f)
        {
            var instance = new Object(Models.WorkBarrierAHeadSign, position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        /// <summary>
        /// https://gta-objects.xyz/gallery/objects/prop_mp_barrier_02b.jpg
        /// </summary>
        public static Object CreateWorkBarrierHigh(Vector3 position, float heading = 0f)
        {
            var instance = new Object(Models.WorkBarrierHigh, position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateBarrierWithWorkAheadWithLights(Vector3 position, float heading = 0f)
        {
            var instance = new Object(Models.WorkBarrierAHeadSignLights, position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateBarrierWorkWithLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_barrier_work02a"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateBarrierWorkWithLightAlternative(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_barrier_work01b"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateWorkerBarrierArrowRight(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_arrow_barrier_01"), position);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object RedirectTrafficArrowLeft(Vector3 position, float heading)
        {
            var instance = new Object(new Model("prop_trafficdiv_01"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object RedirectTrafficArrowBoth(Vector3 position, float heading)
        {
            var instance = new Object(new Model("prop_trafficdiv_02"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object StoppedVehiclesSign(Vector3 position, float heading)
        {
            var instance = new Object(new Model("prop_consign_02a"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreatePoliceDoNotCrossBarrier(Vector3 position, float heading = 0f)
        {
            var barrier = new Object(Models.PoliceDoNotCross, position, heading);
            return PlaceCorrectlyOnGround(barrier);
        }

        public static Object CreateGeneratorWithLights(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_generator_03b"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateFloodLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_worklight_03a"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateFloodLights(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_worklight_03b"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateGroundFloodLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_worklight_02a"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateRedStandingGroundLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_air_lights_02b"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateBlueStandingGroundLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_air_lights_02a"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateBlueGroundLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_runlight_b"), position, heading);
            return instance;
        }

        public static Object CreateRedGroundLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_runlight_r"), position, heading);
            return instance;
        }

        public static Object CreateGreenGroundLight(Vector3 position)
        {
            var instance = new Object(new Model("prop_runlight_g"), position);
            return instance;
        }

        public static Object CreateYellowGroundLight(Vector3 position)
        {
            var instance = new Object(new Model("prop_runlight_y"), position);
            return instance;
        }

        /// <summary>
        /// https://gta-objects.xyz/gallery/objects/prop_warninglight_01.jpg
        /// </summary>
        public static Object CreateWarningLight(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_warninglight_01"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        /// <summary>
        /// https://gta-objects.xyz/gallery/objects/prop_barrier_wat_03b.jpg
        /// </summary>
        public static Object CreateBarrelTrafficCatcher(Vector3 position, float heading = 0f)
        {
            var instance = new Object(new Model("prop_barrier_wat_03b"), position, heading);
            return PlaceCorrectlyOnGround(instance);
        }

        public static Object CreateMedKit(Vector3 position)
        {
            return new Object(new Model("prop_ld_health_pack"), position);
        }

        public static Object CreateHorizontalFlare(Vector3 position, float heading)
        {
            var instance = PlaceCorrectlyOnGround(new Weapon(new WeaponAsset("weapon_flare"), position, -1)
            {
                Heading = heading
            });
            instance.Rotation = new Rotator(heading, 90f, 0f);
            return instance;
        }

        public static Object CreateVerticalFlare(Vector3 position, float heading)
        {
            var instance = new Weapon(new WeaponAsset("weapon_flare"), position, -1)
            {
                Heading = heading
            };
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