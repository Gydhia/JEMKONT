using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DownBelow.Inventory
{
    public class Inventory : MonoBehaviour
    {   
        //Properties
        public List<InventoryElement> InventoryElements => inventoryElements;

        //Fields
        [SerializeField] private List<InventoryElement> inventoryElements;

        public Inventory()
        {
            inventoryElements = new List<InventoryElement>();
        }

        public void AddElementToInventory(InventoryElement element)
        {
            inventoryElements.Add(element);
        }

        public void RemoveElementFromInventory(InventoryElement element)
        {
            inventoryElements.Remove(element);
        }
        public void RemoveElementAtIndex(int index)
        {
            inventoryElements.RemoveAt(index);
        }

        public void SwapElements(InventoryElement baseElement, InventoryElement destinationElement)
        {
            InventoryElement firstElement, secondElement;
            bool hasFoundFirstElement = false;
            bool hasFoundSecondElement = false;
            int count = 0;

            while (!hasFoundFirstElement)
            {
                if(InventoryElements[count] == baseElement)
                {
                    firstElement = InventoryElements[count];
                    hasFoundFirstElement = true;
                }
                count++;
            }

            count = 0;

            while (!hasFoundSecondElement)
            {
                if (InventoryElements[count] == destinationElement)
                {
                    secondElement = InventoryElements[count];
                    hasFoundSecondElement = true;
                }
                count++;
            }



        }

    }
    
}
