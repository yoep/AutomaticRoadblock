using System;
using System.Drawing;
using AutomaticRoadblocks.AbstractionLayer;
using Rage;

namespace AutomaticRoadblockTests.AbstractionLayer.Implementation
{
    public class TestGame : IGame
    {
        public uint GameTime { get; set; }
        public Vector3 PlayerPosition { get; set; }
        public float PlayerHeading { get; set; }
        public Vehicle PlayerVehicle { get; set; }

        public void NewSafeFiber(Action action, string name)
        {
            action.Invoke();
        }

        public void FiberYield()
        {
            // no-op
        }

        public void DisplayPluginNotification(string message)
        {
            Console.WriteLine($"[NOTIFICATION] {message}");
        }

        public void DisplayNotification(string message)
        {
            Console.WriteLine($"[NOTIFICATION] {message}");
        }

        public void DrawSphere(Vector3 position, float radius, Color color)
        {
            // no-op
        }

        public void DrawArrow(Vector3 position, Vector3 direction, Rotator rotationOffset, float scale, Color color)
        {
            // no-op
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            // no-op
        }
    }
}