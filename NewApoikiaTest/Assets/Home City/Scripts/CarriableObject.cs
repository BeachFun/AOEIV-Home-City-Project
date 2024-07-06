using UnityEngine;
using RTSEngine.Entities;
using RTSEngine.Game;
using RTSEngine.EntityComponent;

namespace RTSEngine.Custom
{
    public class CarriableObject : EntityComponentBase, IEntityComponent
    {
        protected IEntity entity { private set; get; }

        protected override void OnInit()
        {
            this.entity = Entity;

            if (!logger.RequireValid(entity,
                $"[{GetType().Name}] This component must be attached to an entity object!"))
                return;
        }

        private Carrier currentCarrier = null;

        public void OnPickedUp(Carrier carrier)
        {
            Debug.Log($"[CarriableObject] OnPickedUp called by Carrier: {carrier}");
            currentCarrier = carrier;
            // Disable physics, colliders, etc.
            if (entity.MovementComponent.IsValid())
            {
                Debug.Log("[CarriableObject] Disabling movement component");
                entity.MovementComponent.SetActiveLocal(false, false);
            }
        }

        public void OnPutDown()
        {
            Debug.Log("[CarriableObject] OnPutDown called");
            currentCarrier = null;
            // Re-enable physics, colliders, etc.
            if (entity.MovementComponent.IsValid())
            {
                Debug.Log("[CarriableObject] Enabling movement component");
                entity.MovementComponent.SetActiveLocal(true, false);
            }
        }
    }
}