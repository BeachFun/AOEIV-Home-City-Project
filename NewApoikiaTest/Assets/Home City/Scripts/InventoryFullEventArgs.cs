using System;
using RTSEngine.Entities;

namespace RTSEngine.Event
{
    public class InventoryFullEventArgs : EventArgs
    {
        public IBuilding Building { get; private set; }

        public InventoryFullEventArgs(IBuilding building)
        {
            Building = building;
        }
    }
}