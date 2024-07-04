using UnityEngine;
using RTSEngine.Entities;
using RTSEngine.Event;
using RTSEngine.Game;
using RTSEngine.Logging;
using RTSEngine.UI;
using RTSEngine.Task;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using RTSEngine.Selection;
using RTSEngine;
using System.Linq;
using Unity.VisualScripting;
using RTSEngine.EntityComponent;
using static UnityEditor.Progress;

public class TaskDisplayPanel : MonoBehaviour
{
	[SerializeField] private float updateInterval = 0.5f; // How often to check for updates
	private float lastUpdateTime;
	private IEntityTasksQueueHandler tasksQueueHandler;
	[SerializeField]
	private List<TaskDisplayItem> taskDisplayItemList = new List<TaskDisplayItem>();

	[SerializeField] private TaskDisplayItem displayItemPrefab;
	[SerializeField] private TextMeshProUGUI queueSize;

	public void Init(RTSEngine.Entities.IUnit unit)
	{
		tasksQueueHandler = unit.TasksQueue;
	}

	private void Update()
	{
		if (Time.time - lastUpdateTime >= updateInterval)
		{
			UpdateTaskDisplay();
			lastUpdateTime = Time.time;
		}
	}

	private void UpdateTaskDisplay()
	{
		queueSize.text = tasksQueueHandler.QueueCount.ToString();
		for (int i = 0; i < tasksQueueHandler.QueueCount; i++)
		{
			SetTargetInputData task = tasksQueueHandler.Queue.ElementAt(i);

			TaskDisplayItem item;
			if (i >= taskDisplayItemList.Count)
			{
				// If we don't have enough items in our list, create a new one
				item = Instantiate(displayItemPrefab, transform);
				taskDisplayItemList.Add(item);
				Debug.Log("adding task display item for: " + task.componentCode);
			}
			else
			{
				// Use the existing item
				item = taskDisplayItemList[i];
			}

			item.Init(task);
		}

		// Disable any extra items
		for (int i = tasksQueueHandler.QueueCount; i < taskDisplayItemList.Count; i++)
		{
			DestroyImmediate(taskDisplayItemList[i].gameObject);
			taskDisplayItemList.RemoveAt(i);
		}
	}
}
