using UnityEngine;
using RTSEngine.EntityComponent;
using RTSEngine.Entities;
using RTSEngine.Event;
using RTSEngine.UI;
using RTSEngine.Movement;
using RTSEngine.Game;
using RTSEngine.Determinism;
using System.Collections.Generic;
using RTSEngine.Logging;

namespace RTSEngine.Custom
{
    public class Carrier : FactionEntityTargetComponent<IEntity>
    {
       #region Attributes
       public IUnit Unit { private set; get; }

       [SerializeField, Tooltip("UI data for the pick up action")]
       private EntityComponentTaskUIAsset pickUpTaskUI = null;

       [SerializeField, Tooltip("UI data for the put down action")]
       private EntityComponentTaskUIAsset putDownTaskUI = null;

       [SerializeField, Tooltip("Maximum distance to pick up or put down an object")]
       private float interactionRange = 2f;

       [SerializeField, Tooltip("Time it takes to pick up or put down an object")]
       private float interactionDuration = 1f;

       private CarriableObject carriedObject = null;
       public bool IsCarrying => carriedObject != null;

       // Carrier states
       private enum CarrierState { Idle, MovingToPickup, PickingUp, Carrying, MovingToPutdown, PuttingDown }
       private CarrierState currentState = CarrierState.Idle;

       private float interactionTimer = 0f;

       public override bool IsIdle => currentState == CarrierState.Idle || currentState == CarrierState.Carrying;

       // Game services
       protected IMovementManager mvtMgr { private set; get; }
       protected IInputManager inputMgr { private set; get; }

       [SerializeField, Tooltip("UI data for the carry action")]
       private EntityComponentTaskUIAsset carryTaskUI = null;

        private float stoppingDistance = 2.0f;

        #endregion

        #region Initializing/Terminating
        protected override void OnTargetInit()
       {
           this.Unit = Entity as IUnit;
           this.mvtMgr = gameMgr.GetService<IMovementManager>();
           this.inputMgr = gameMgr.GetService<IInputManager>();

           if (!logger.RequireValid(Unit,
             $"[{GetType().Name}] This component must be initialized with a valid instance of {typeof(IUnit).Name}!"))
               return;
       }
       #endregion

       #region Handling Carrier Actions
       // ... (PickUp and PutDown methods remain the same)

       public override ErrorMessage SetTargetLocal(SetTargetInputData input)
       {
           Debug.Log($"[Carrier] SetTargetLocal called. IsCarrying: {IsCarrying}, Target: {input.target.instance}");

           if (!factionEntity.CanLaunchTask)
           {
               Debug.Log("!factionEntity.CanLaunchTask");
               return ErrorMessage.taskSourceCanNotLaunch;
           }

           IsTargetValid(input, out ErrorMessage errorMsg);
           if (errorMsg != ErrorMessage.none)
           {
               OnSetTargetError(input, errorMsg);
               if (input.playerCommand && RTSHelper.IsLocalPlayerFaction(factionEntity))
                   playerMsgHandler.OnErrorMessage(new PlayerErrorMessageWrapper
                   {
                       message = errorMsg,

                       source = Entity,
                       target = input.target.instance
                   });
               Debug.Log("errorMsg != ErrorMessage.none");

               return errorMsg;
           }

           if (HasTarget)
               Stop();

           bool sameTarget = input.target.instance == Target.instance as IEntity && input.target.instance.IsValid();

           // If this component requires the entity to be idle to run then set the entity to idle before assigning the new target
           if (RequireIdleEntity)
               factionEntity.SetIdle(sameTarget ? this : null);

           OnTargetPreLocked(input.playerCommand, input.target, sameTarget);

           TargetInputData = input;
           Target = input.target;

           if (input.playerCommand && Target.instance.IsValid() && factionEntity.IsLocalPlayerFaction())
               selector.FlashSelection(Target.instance, factionEntity.IsFriendlyFaction(Target.instance));

           Debug.Log("OnTargetPostLocked: " + Target.instance.gameObject);

           OnTargetPostLocked(input, sameTarget);

           RaiseTargetUpdated();

           return ErrorMessage.none;
       }

        protected override void OnTargetPostLocked(SetTargetInputData input, bool sameTarget)
        {
            base.OnTargetPostLocked(input, sameTarget);

            Debug.Log($"[Carrier] OnTargetPostLocked called. IsCarrying: {IsCarrying}, Target: {Target.instance}, SameTarget: {sameTarget}");

            // Check if the target is in range
            if (!IsTargetInRange(Unit.transform.position, Target))
            {
                // Set movement target
                Unit.MovementComponent.SetTarget(
                    Target,
                    stoppingDistance,
                    new MovementSource
                    {
                        sourceTargetComponent = this,
                        playerCommand = input.playerCommand
                    });

                currentState = IsCarrying ? CarrierState.MovingToPutdown : CarrierState.MovingToPickup;
            }
            else
            {
                // Target is in range, start pickup or putdown immediately
                currentState = IsCarrying ? CarrierState.PuttingDown : CarrierState.PickingUp;
                interactionTimer = interactionDuration;
            }

            Debug.Log($"[Carrier] State set to {currentState}");

            // Check if movement was set successfully
            if (!Unit.MovementComponent.HasTarget && (currentState == CarrierState.MovingToPickup || currentState == CarrierState.MovingToPutdown))
            {
                Debug.LogWarning("[Carrier] Failed to set movement target.");
                Stop();
                return;
            }

            globalEvent.RaiseEntityComponentTargetLockedGlobal(this, new TargetDataEventArgs(Target));
        }

        /*
        protected override void OnTargetPostLocked(SetTargetInputData input, bool sameTarget)
       {
           base.OnTargetPostLocked(input, sameTarget);

           if (!IsCarrying)
           {
               //currentState = CarrierState.MovingToPickup;
               PickUpObject();
               Debug.Log("[Carrier] State set to MovingToPickup");
           }
           else
           {
               //currentState = CarrierState.MovingToPutdown;
               PutDownObject();
               Debug.Log("[Carrier] State set to MovingToPutdown");
           }

           Unit.MovementComponent.SetTarget(
               Target,
               interactionRange,
               new MovementSource { playerCommand = input.playerCommand });
       }*/

        public override bool OnAwaitingTaskTargetSet(EntityComponentTaskUIAttributes taskAttributes, TargetData<IEntity> target)
       {
           if (carryTaskUI.IsValid() && taskAttributes.data.code == carryTaskUI.Data.code)
           {
               Debug.Log($"[Carrier] OnAwaitingTaskTargetSet called. IsCarrying: {IsCarrying}, Target: {target.instance}");

               ErrorMessage errorMsg = SetTarget(target, playerCommand: true);
               if (errorMsg != ErrorMessage.none)
               {
                   Debug.LogWarning($"[Carrier] SetTarget failed with error: {errorMsg}");
                   // You might want to display this error to the player
               }

               return true;
           }

           return base.OnAwaitingTaskTargetSet(taskAttributes, target);
       }
       #endregion

       #region Update Logic
       // Use Unity's Update method instead of OnActiveUpdate
       private void Update()
       {
           if (!IsInitialized || Unit.Health.IsDead)
               return;

           switch (currentState)
           {
               case CarrierState.MovingToPickup:
                   Debug.Log("Unit: " + Unit.gameObject);
                   Debug.Log("Unit.transform.position: " + Unit.transform.position);
                   Debug.Log("Target: " + Target.instance);
                   if (IsTargetInRange(Unit.transform.position, Target))
                   {
                       Debug.Log("[Carrier] Target in range, switching to PickingUp state");
                       currentState = CarrierState.PickingUp;
                       interactionTimer = interactionDuration;
                   }
                   break;

               case CarrierState.PickingUp:
                   interactionTimer -= Time.deltaTime;
                   if (interactionTimer <= 0)
                   {
                       Debug.Log("[Carrier] Picking up object");
                       PickUpObject();
                       currentState = CarrierState.Carrying;
                   }
                   break;

               case CarrierState.MovingToPutdown:
                   if (IsTargetInRange(Unit.transform.position, Target))
                   {
                       currentState = CarrierState.PuttingDown;
                       interactionTimer = interactionDuration;
                   }
                   break;

               case CarrierState.PuttingDown:
                   interactionTimer -= Time.deltaTime;
                   if (interactionTimer <= 0)
                   {
                       PutDownObject();
                       currentState = CarrierState.Idle;
                   }
                   break;
           }
       }
       #endregion

       #region Interaction Methods
       private void PickUpObject()
       {
            if (carriedObject != null)
            {
                return;
            }
           Debug.Log($"[Carrier] PickUpObject called. Target: {Target.instance}");
           CarriableObject carriable = Target.instance.GetComponent<CarriableObject>();
           if (carriable != null)
           {
               Debug.Log("[Carrier] CarriableObject found, picking up");
               carriedObject = carriable;
               carriable.OnPickedUp(this);
               carriable.transform.SetParent(Unit.transform);
               carriable.transform.localPosition = new Vector3(0, Unit.Radius, 0);
           }
           else
           {
               Debug.LogError("[Carrier] No CarriableObject component found on target");
           }
           Stop();
       }

       private void PutDownObject()
       {
           if (carriedObject != null)
           {
               carriedObject.OnPutDown();
               carriedObject.transform.SetParent(null);
               carriedObject.transform.position = Target.position;
               carriedObject = null;
           }
           Stop();
       }
       #endregion

       #region Task UI
       protected override bool OnTaskUICacheUpdate(List<EntityComponentTaskUIAttributes> taskUIAttributesCache, List<string> disabledTaskCodesCache)
       {
           if (!base.OnTaskUICacheUpdate(taskUIAttributesCache, disabledTaskCodesCache))
               return false;

           if (carryTaskUI.IsValid())
           {
               taskUIAttributesCache.Add(new EntityComponentTaskUIAttributes
               {
                   data = carryTaskUI.Data,
                   title = IsCarrying ? "Put Down Object" : "Pick Up Object",
                   tooltipText = IsCarrying
                       ? "Select a location to put down the carried object"
                       : "Select an object to pick up",
               });
           }

           return true;
       }

       public override bool OnTaskUIClick(EntityComponentTaskUIAttributes taskAttributes)
       {
           if (carryTaskUI.IsValid() && taskAttributes.data.code == carryTaskUI.Data.code)
           {
               Debug.Log($"[Carrier] Carry task clicked. IsCarrying: {IsCarrying}");

               // The carry task was clicked, so we're either picking up or putting down
               if (IsCarrying)
               {
                   Debug.Log("[Carrier] Initiating put down action");
                   // We're carrying something, so we're putting it down
                   // The actual target selection will happen when the player clicks on the ground
                   taskMgr.AwaitingTask.Enable(taskAttributes);
               }
               else
               {
                   Debug.Log("[Carrier] Initiating pick up action");
                   // We're not carrying anything, so we're picking up
                   // The actual target selection will happen when the player clicks on a carriable object
                   taskMgr.AwaitingTask.Enable(taskAttributes);
               }

               return true;
           }

           return base.OnTaskUIClick(taskAttributes);
       }
       #endregion

       #region Handling Target
       public override bool CanSearch => true;

       public override ErrorMessage IsTargetValid(SetTargetInputData input)
       {
           Debug.Log("IsTargetValid: " + input.ToString());
           if (!IsCarrying)
           {
               // When not carrying, we're looking for an entity with a CarriableObject component
               if (!input.target.instance.IsValid())
               {
                   Debug.Log($"[Carrier] IsTargetValid called. IsCarrying: {IsCarrying}, Target: {input.target.instance}" + ", !input.target.instance.IsValid(): " + !input.target.instance.IsValid());
                   return ErrorMessage.invalid;
               }
               if (input.target.instance.GetComponent<CarriableObject>() == null)
               {
                   Debug.Log($"[Carrier] IsTargetValid called. IsCarrying: {IsCarrying}, Target: {input.target.instance}" + ", input.target.instance.GetComponent<CarriableObject>() == null: " + input.target.instance.GetComponent<CarriableObject>() == null);
                   return ErrorMessage.invalid;
               }
               if (!input.target.instance.IsInteractable)
               {
                   Debug.Log($"[Carrier] IsTargetValid called. IsCarrying: {IsCarrying}, Target: {input.target.instance}" + ", !input.target.instance.IsInteractable: " + !input.target.instance.IsInteractable);
                   return ErrorMessage.uninteractable;
               }
           }
           else
           {
               // When carrying, we're looking for a valid position to put down the object
               if (!IsValidPosition(input.target.position))
                   return ErrorMessage.invalid;
           }

           // Check if the target is within range
           if (!IsTargetInRange(Entity.transform.position, input.target))
           {
               Debug.Log("!IsTargetInRange(Entity.transform.position, input.target)");
               //return ErrorMessage.targetOutOfRange;
           }

           return ErrorMessage.none;
       }

       private bool IsValidPosition(Vector3 position)
       {
           // Implement your own logic to determine if a position is valid
           // This could involve checking if it's within the map boundaries,
           // on valid terrain, not obstructed, etc.
           // For now, we'll just check if it's not a zero vector as a placeholder
           return position != Vector3.zero;
       }
        /*
       public override bool IsTargetInRange(Vector3 sourcePosition, TargetData<IEntity> target)
       {
           return true;

           Debug.Log("target: " + target.instance.gameObject);
           if (IsCarrying)
           {
               return Vector3.Distance(sourcePosition, target.position) <= interactionRange;
           }
           else if (target.instance.gameObject.GetComponent<CarriableObject>())
           {
               Debug.Log("distance: " + Vector3.Distance(sourcePosition, target.instance.transform.position) + ", interaction range: " + interactionRange);
               return Vector3.Distance(sourcePosition, target.instance.transform.position) <= interactionRange;
           }
           Debug.Log("target.instance is CarriableObject: " + target.instance.gameObject.GetComponent<CarriableObject>());
           return false;
       }*/

        public override bool IsTargetInRange(Vector3 sourcePosition, TargetData<IEntity> target)
        {
            float range = IsCarrying ? stoppingDistance : stoppingDistance + (target.instance?.Radius ?? 0f);
            return Vector3.Distance(sourcePosition, target.position) <= range;
        }

        protected override void OnStop()
       {
           base.OnStop();

           if (IsCarrying)
           {
             //  PutDownObject();
           }

           Unit.SetIdle(true);
       }
       #endregion*/



}
}