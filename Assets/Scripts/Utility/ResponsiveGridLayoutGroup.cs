using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kalank.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(GridLayoutGroup))]
    public class ResponsiveGridLayoutGroup : MonoBehaviour
    {
        public enum ResponsiveGridLayoutGroupOption
        {
            ScreenPercent,
            ViewportPercent,
        }

        [Header("Responsive option")]
        [SerializeField] private ResponsiveGridLayoutGroupOption responsiveOption = ResponsiveGridLayoutGroupOption.ScreenPercent;
        [SerializeField] private RectTransform viewport;
        [Header("Cell size")]
        [Range(0f, 1f)]
        [SerializeField] private float cellWidthPercent;
        [Range(0f, 1f)]
        [SerializeField] private float cellHeightPercent;
        [Header("Cell size (ratio)")]
        [SerializeField] private bool cellsUseRatio = false;
        [Tooltip("cellSizeRatio = width / height")]
        [SerializeField] private float cellSizeRatio = 1;
        [SerializeField] private RectTransform.Axis cellSizeRatioMainAxis = RectTransform.Axis.Horizontal;
        [Header("Padding")]
        [SerializeField] private bool responsivePaddingLeft = false;
        [Range(0f, 0.5f)]
        [SerializeField] private float paddingLeftPercent = 0;
        [SerializeField] private bool responsivePaddingRight = false;
        [Range(0f, 0.5f)]
        [SerializeField] private float paddingRightPercent = 0;
        [SerializeField] private bool responsivePaddingTop = false;
        [Range(0f, 0.5f)]
        [SerializeField] private float paddingTopPercent = 0;
        [SerializeField] private bool responsivePaddingBottom = false;
        [Range(0f, 0.5f)]
        [SerializeField] private float paddingBottomPercent = 0;
        [Header("Spacing")]
        [SerializeField] private bool responsiveSpacingHorizontal = false;
        [Range(0f, 0.5f)]
        [SerializeField] private float spacingHorizontalPercent = 0;
        [SerializeField] private bool responsiveSpacingVertical = false;
        [Range(0f, 0.5f)]
        [SerializeField] private float spacingVerticalPercent = 0;
        [Header("Spacing ratio")]
        [SerializeField] private bool spacingUseRatio = false;
        [Tooltip("spacingRatio = width / height")]
        [SerializeField] private float spacingRatio = 1;
        [SerializeField] private RectTransform.Axis spacingRatioMainAxis = RectTransform.Axis.Horizontal;

        private GridLayoutGroup m_GridLayoutGroup;
        private RectTransform m_RectTransform;

        protected void OnRectTransformDimensionsChange()
        {
            ResponsiveCalculations();
        }

        protected void OnValidate()
        {
            ResponsiveCalculations();
        }

        protected void OnEnable()
        {
            ResponsiveCalculations();
        }

        protected void OnDisable()
        {
            ResponsiveCalculations();
        }

        private void ResponsiveCalculations()
        {
            if (!m_GridLayoutGroup)
                m_GridLayoutGroup = GetComponent<GridLayoutGroup>();
            if (!m_RectTransform)
                m_RectTransform = GetComponent<RectTransform>();
            float responsiveWidth = GetResponsiveWidth();
            float responsiveHeight = GetResponsiveHeight();
            float cellWidth = cellWidthPercent * responsiveWidth;
            float cellHeight = cellHeightPercent * responsiveHeight;
            if (cellsUseRatio)
            {
                if (cellSizeRatioMainAxis == RectTransform.Axis.Horizontal)
                {
                    cellHeight = cellWidth / cellSizeRatio;
                }
                else
                {
                    cellWidth = cellHeight * cellSizeRatio;
                }
            }

            float paddingLeft = responsivePaddingLeft ? (paddingLeftPercent * responsiveWidth) : m_GridLayoutGroup.padding.left;
            float paddingRight = responsivePaddingRight ? (paddingRightPercent * responsiveWidth) : m_GridLayoutGroup.padding.right;
            float paddingTop = responsivePaddingTop ? (paddingTopPercent * responsiveHeight) : m_GridLayoutGroup.padding.top;
            float paddingBottom = responsivePaddingBottom ? (paddingBottomPercent * responsiveHeight) : m_GridLayoutGroup.padding.bottom;

            float spacingHorizontal = responsiveSpacingHorizontal ? (spacingHorizontalPercent * responsiveWidth) : m_GridLayoutGroup.spacing.x;
            float spacingVertical = responsiveSpacingVertical ? (spacingVerticalPercent * responsiveHeight) : m_GridLayoutGroup.spacing.y;
            if (spacingUseRatio)
            {
                if (spacingRatioMainAxis == RectTransform.Axis.Horizontal)
                {
                    spacingVertical = spacingHorizontal / spacingRatio;
                }
                else
                {
                    spacingHorizontal = spacingVertical * spacingRatio;
                }
            }

            m_GridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
            m_GridLayoutGroup.padding = new RectOffset((int)paddingLeft, (int)paddingRight, (int)paddingTop, (int)paddingBottom);
            m_GridLayoutGroup.spacing = new Vector2(spacingHorizontal, spacingVertical);
            LayoutRebuilder.MarkLayoutForRebuild(m_RectTransform);
        }

        private float GetResponsiveWidth()
        {
            return (responsiveOption == ResponsiveGridLayoutGroupOption.ScreenPercent) ? Screen.width : viewport.rect.width;
        }

        private float GetResponsiveHeight()
        {
            return (responsiveOption == ResponsiveGridLayoutGroupOption.ScreenPercent) ? Screen.height : viewport.rect.height;
        }
    }
}