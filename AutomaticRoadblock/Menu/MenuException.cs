using System;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Menu
{
    public class MenuException : Exception
    {
        public MenuException(string message) : base(message)
        {
        }

        public MenuException(string message, UIMenuItem item) : base(message)
        {
            Item = item;
        }

        public UIMenuItem Item { get; }
    }
}