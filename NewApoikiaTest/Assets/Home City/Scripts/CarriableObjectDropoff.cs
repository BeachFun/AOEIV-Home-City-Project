using System;
using System.Collections.Generic;
using UnityEngine;
using RTSEngine.Entities;
using RTSEngine.Event;
using RTSEngine.Game;
using RTSEngine.EntityComponent;
using RTSEngine.Custom;
using UnityEngine.Events;


namespace RTSEngine.Custom
{

	public class CarriableObjectDropoff : EntityComponentBase, ICarriableObjectDropoff
	{
		[SerializeField, Tooltip("Define the carriable objects that can be dropped off.")]
		private EntityTargetPicker targetPicker = new EntityTargetPicker();

		[SerializeField, Tooltip("The maximum amount of carriable objects that can be dropped off at the same time.")]
		private int capacity = 2;
		public int MaxAmount => capacity;
		public int CurrAmount { get { return carriedObjects.Count; } }
		public bool HasMaxAmount => CurrAmount >= MaxAmount;

		[SerializeField, Tooltip("What action to perform when an object is dropped off?")]
		private UnityEngine.Events.UnityEvent onObjectDroppedOff = new UnityEngine.Events.UnityEvent();

		public event CustomEventHandler<ICarriableObjectDropoff, CarriableObjectEventArgs> ObjectDroppedOff;

		private IGlobalEventPublisher globalEvent;
		private IBuilding building;

		private List<CarriableObject> carriedObjects = new List<CarriableObject>();


		public ErrorMessage CanDropOff(CarriableObject carriableObject)
		{
			if (!carriableObject.IsValid())
				return ErrorMessage.invalid;
			else if (!carriableObject.Entity.IsInteractable)
				return ErrorMessage.uninteractable;
			else if (CurrAmount >= MaxAmount)
				return ErrorMessage.dropOffMaxCapacityReached;
			else if (!targetPicker.IsValidTarget(carriableObject.Entity))
				return ErrorMessage.targetPickerUndefined;

			return ErrorMessage.none;
		}

		public ErrorMessage DropOff(CarriableObject carriableObject)
		{
			Debug.Log("DROP OFF: " + carriableObject.gameObject.name);
			if (globalEvent == null)
            {
				this.globalEvent = gameMgr.GetService<IGlobalEventPublisher>();
			}
			if (building == null)
            {
				this.building = Entity as IBuilding;
			}

			ErrorMessage errorMsg = CanDropOff(carriableObject);
			if (errorMsg != ErrorMessage.none)
				return errorMsg;

			carriedObjects.Add(carriableObject);

			// Perform the dropoff action
			onObjectDroppedOff.Invoke();

			// Raise the event
			RaiseObjectDroppedOff(new CarriableObjectEventArgs(carriableObject));

			return ErrorMessage.none;
		}

		private void RaiseObjectDroppedOff(CarriableObjectEventArgs args)
		{
			Debug.Log("EVENT DROPPED OFF: " + args.CarriableObject.gameObject.name);

			var handler = ObjectDroppedOff;
			handler?.Invoke(this, args);

			RaiseInventoryFullEvent();
		}

		


		//Hack Inventory system

		private void RaiseInventoryFullEvent()
		{
			if (HasMaxAmount)
            {
				Debug.Log("CAPACITY REACHED! ====");
				globalEvent.RaiseBuildingInventoryFull(building);
			}
		}



	}
}

/*
//Some kind of list of acceptable object types it can receive
public List<Object> validObjects;

public UnityEvent<CarriableObject> DroppedOffObjectEvent;

void Start()
{

}

void Update()
{

}

// Checks if you can drop off an object here. Only accepts certain types of objects?
public bool IsValidDropoffObject()
{
	//Checks against a list of acceptable object types




	return true;
}


// Performs the drop off of the object here
public void DoDropoffObject(CarriableObject obj)
{





	DroppedOffObjectEvent.Invoke(obj);
}


// Emits some kind of UnityEvent when resources are dropped into it. This interacts with other scripts on the CarriableObject.*/