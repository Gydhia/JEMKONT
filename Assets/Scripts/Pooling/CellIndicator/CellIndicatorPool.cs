using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Pools
{
    public class CellIndicatorPool : ObjectPool<CellIndicator>
    {
        private Dictionary<EntityAction, List<CellIndicator>> _indicatorsRef;

        public readonly static Color GreenColor = new Color(0.1709f, 0.6320f, 0.1709f, .8f);
        public readonly static Color BlueColor = new Color(0.1725f, 0.4182f, 0.6313f, .8f);

        public readonly static Color GreenColorTransparent = new Color(0.1709f, 0.6320f, 0.1709f, .5f);
        public readonly static Color BlueColorTransparent = new Color(0.1725f, 0.4182f, 0.6313f, .5f);

        private void Awake()
        {
            this._indicatorsRef = new Dictionary<EntityAction, List<CellIndicator>>();
        }

        public void DisplayIndicators(EntityAction action, Color? color = null)
        {
            color ??= this._getColorFromAction(action);

            CellIndicator indicator = GetPooled();

            indicator.gameObject.SetActive(true);
            indicator.transform.position = action.TargetCell.transform.position;
            indicator.CellRenderer.material.color = color.Value;

            this._indicatorsRef.Add(action, new List<CellIndicator> { indicator });
        }

        private Color _getColorFromAction(EntityAction action)
        {
            if (action is MovementAction) 
                return action.RefBuffer.Count == 1 ? GreenColor : GreenColorTransparent;
            else
                return action.RefBuffer.Count == 1 ? BlueColor : BlueColorTransparent;
        }

        public void HideIndicators(EntityAction action)
        {
            // It may have already be removed
            if (!this._indicatorsRef.ContainsKey(action))
                return;

            foreach (var actionCells in this._indicatorsRef[action])
                actionCells.TryReleaseToPool();

            // Maybe useless, but to indicate to the GC that we doesn't need the list anymore
            this._indicatorsRef[action] = null;
            this._indicatorsRef.Remove(action);             
        }
    }

}