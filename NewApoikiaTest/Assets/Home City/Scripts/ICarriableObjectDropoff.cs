using RTSEngine.Entities;
using RTSEngine.EntityComponent;
using RTSEngine.Event;
using RTSEngine;
using System;

namespace RTSEngine.Custom
{
	public interface ICarriableObjectDropoff : IEntityComponent
	{
		int MaxAmount { get; }
		int CurrAmount { get; }
		bool HasMaxAmount { get; }

		ErrorMessage CanDropOff(CarriableObject carriableObject);
		ErrorMessage DropOff(CarriableObject carriableObject);

		event CustomEventHandler<ICarriableObjectDropoff, CarriableObjectEventArgs> ObjectDroppedOff;
	}

	public class CarriableObjectEventArgs : EventArgs
	{
		public CarriableObject CarriableObject { get; }

		public CarriableObjectEventArgs(CarriableObject carriableObject)
		{
			CarriableObject = carriableObject;
		}
	}
}