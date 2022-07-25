using System.Collections.Generic;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils.Road;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblock : IPreviewSupport
    {
        private readonly Road _road;
        private readonly BarrierType _type;

        internal ManualRoadblock(Road road, BarrierType type)
        {
            _road = road;
            _type = type;

            Init();
        }

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            
        }

        #endregion

        #region Funtions

        private void Init()
        {
            
        }

        #endregion
    }
}