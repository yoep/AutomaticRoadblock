using AutomaticRoadblocks.Menu;
using LSPD_First_Response.Mod.API;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class EndCalloutComponent : IMenuComponent<UIMenuItem>
    {
        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AutomaticRoadblocksPlugin.EndCallout);

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Debug;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            Functions.StopCurrentCallout();
        }
    }
}