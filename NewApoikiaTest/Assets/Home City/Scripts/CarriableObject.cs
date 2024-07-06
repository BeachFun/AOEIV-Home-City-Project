using UnityEngine;
using RTSEngine.Entities;
using RTSEngine.Game;
using RTSEngine.EntityComponent;
using UnityEngine.AI;

namespace RTSEngine.Custom
{
    public class CarriableObject : EntityComponentBase, IEntityComponent
    {
        protected IEntity entity { private set; get; }
        private NavMeshObstacle navMeshObstacle;

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
            navMeshObstacle = entity.gameObject.GetComponent<NavMeshObstacle>();
            if (navMeshObstacle != null)
            {
                Debug.Log("[CarriableObject] Disabling movement component");
                navMeshObstacle.enabled = false;
            }
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
            if (navMeshObstacle != null)
            {
                navMeshObstacle.enabled = true;
            }
        }
    }
}