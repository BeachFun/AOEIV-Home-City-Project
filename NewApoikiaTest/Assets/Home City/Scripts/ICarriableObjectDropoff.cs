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

		ErrorMessage CanDropOff(IEntity carriableObject);
		ErrorMessage DropOff(IEntity carriableObject);

		event CustomEventHandler<ICarriableObjectDropoff, CarriableObjectEventArgs> ObjectDroppedOff;
	}

	public class CarriableObjectEventArgs : EventArgs
	{
		public IEntity CarriableObject { get; }

		public CarriableObjectEventArgs(IEntity carriableObject)
		{
			CarriableObject = carriableObject;
		}
	}
}