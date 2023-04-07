using UnityEngine;

namespace Utility.SLayout
{
    [AddComponentMenu("Layout/SHorizontal Layout Group", 150)]
    public class SHorizontalLayoutGroup : SHorizontalOrVerticalLayoutGroup
    {
        protected SHorizontalLayoutGroup()
        {}

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, false);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, false);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, false);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, false);
        }
        
        public void ForceUpdate() {
            SetDirty();
        }
    }
}