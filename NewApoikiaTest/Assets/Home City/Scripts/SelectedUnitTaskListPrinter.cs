using UnityEngine;
using RTSEngine.Entities;
using RTSEngine.Selection;
using RTSEngine.Task;
using RTSEngine.Event;
using RTSEngine.Game;
using System.Text;
using RTSEngine;

public class SelectedUnitTaskListPrinter : MonoBehaviour, IPostRunGameService
{
	protected IGameManager gameMgr { private set; get; }
	protected ISelectionManager selectionMgr { private set; get; }
	protected IGlobalEventPublisher globalEvent { private set; get; }

	private void Start()
	{
		IGameManager gm = FindObjectOfType<GameManager>();
		Init(gm);
	}

	public void Init(IGameManager gameMgr)
	{
		this.gameMgr = gameMgr;

		this.selectionMgr = gameMgr.GetService<ISelectionManager>();
		this.globalEvent = gameMgr.GetService<IGlobalEventPublisher>();

		// Subscribe to the entity selected event
		globalEvent.EntitySelectedGlobal += HandleEntitySelectedGlobal;
	}

	private void OnDestroy()
	{
		if (globalEvent != null)
			globalEvent.EntitySelectedGlobal -= HandleEntitySelectedGlobal;
	}

	private void HandleEntitySelectedGlobal(IEntity entity, EntitySelectionEventArgs args)
	{
		if (entity.IsValid() && entity is IUnit unit)
		{
			PrintTaskList(unit);
		}
	}

	private void PrintTaskList(IUnit unit)
	{
		var tasksQueueHandler = unit.TasksQueue;
		if (tasksQueueHandler == null)
		{
			Debug.Log($"Unit {unit.Code} does not have a TasksQueue.");
			return;
		}

		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"Task list for unit: {unit.Code}");
		sb.AppendLine($"Total tasks in queue: {tasksQueueHandler.QueueCount}");

		if (tasksQueueHandler.QueueCount > 0)
		{
			int index = 0;
			foreach (var task in tasksQueueHandler.Queue)
			{
				sb.AppendLine($"Task {index}: Component: {task.componentCode}, Target: {task.target.instance?.Code ?? "No target"}");
				index++;
			}
		}
		else
		{
			sb.AppendLine("No tasks in queue.");
		}
		if (tasksQueueHandler.IsRunningQueueTask)
		{
			sb.AppendLine($"Currently running task component: {tasksQueueHandler.RunningQueueTaskCompCode}");
		}
		else
		{
			sb.AppendLine("No task currently running.");
		}
	}
}