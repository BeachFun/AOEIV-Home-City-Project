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

public class SelectedUnitTaskListDisplay : MonoBehaviour, IPostRunGameService
{
	[SerializeField] private TaskDisplayPanel taskDisplayPrefab;
	[SerializeField] private Transform uiCanvas;
	private TaskDisplayPanel currentTaskPanel;

	protected IGameManager gameMgr { private set; get; }
	protected ISelectionManager selectionMgr { private set; get; }
	protected IGlobalEventPublisher globalEvent { private set; get; }
	protected IGameUITextDisplayManager gameUITextDisplayer { private set; get; }
	protected IGameLoggingService logger { private set; get; }

	[SerializeField]
	private Dictionary<IUnit, TaskDisplayPanel> taskDisplayPanelDictionary = new Dictionary<IUnit, TaskDisplayPanel>();

	public void Init(IGameManager gameMgr)
	{
		this.gameMgr = gameMgr;
		//this.selectionMgr = gameMgr.GetService<ISelectionManager>();
		this.globalEvent = gameMgr.GetService<IGlobalEventPublisher>();
		//this.gameUITextDisplayer = gameMgr.GetService<IGameUITextDisplayManager>();
		this.logger = gameMgr.GetService<IGameLoggingService>();

		if (!logger.RequireValid(taskDisplayPrefab,
			$"[{GetType().Name}] The 'Task Panel Prefab' field must be assigned!")
			|| !logger.RequireValid(uiCanvas,
			$"[{GetType().Name}] The 'UI Canvas' field must be assigned!"))
			return;

		globalEvent.EntitySelectedGlobal += HandleEntitySelectedGlobal;
		globalEvent.EntityDeselectedGlobal += HandleEntityDeselectedGlobal;
	}

	private void OnDestroy()
	{
		if (globalEvent != null)
		{
			globalEvent.EntitySelectedGlobal -= HandleEntitySelectedGlobal;
			globalEvent.EntityDeselectedGlobal -= HandleEntityDeselectedGlobal;
		}
	}

	private void Update()
	{
		//TODO: Delete display panels that belong to units that are no longer active. Or consider making one single display panel rather than creating multiple new ones.
	}

	private void HandleEntitySelectedGlobal(IEntity entity, EntitySelectionEventArgs args)
	{
		if (entity.IsValid() && entity is IUnit unit)
		{
			if (!taskDisplayPanelDictionary.ContainsKey(unit))
			{
				SetupTaskDisplay(unit);
			}

			currentTaskPanel = taskDisplayPanelDictionary[unit];
			ShowTaskPanel();
		}
	}

	private void HandleEntityDeselectedGlobal(IEntity entity, EventArgs args)
	{
		if (entity.IsValid() && entity is IUnit unit)
		{
			HideTaskPanel();
			currentTaskPanel = null;
		}
	}

	private void SetupTaskDisplay(IUnit unit)
	{
		TaskDisplayPanel taskDisplayPanel = Instantiate(taskDisplayPrefab, uiCanvas);
		RectTransform rectTransform = taskDisplayPanel.GetComponent<RectTransform>();
		rectTransform.anchoredPosition = new Vector2(633f, 566f);

		taskDisplayPanel.Init(unit);
		taskDisplayPanelDictionary.Add(unit, taskDisplayPanel);
	}

	private void ShowTaskPanel()
	{
		if (currentTaskPanel != null)
		{
			currentTaskPanel.gameObject.SetActive(true);
		}
	}

	private void HideTaskPanel()
	{
		if (currentTaskPanel != null)
		{
			currentTaskPanel.gameObject.SetActive(false);
		}
	}
}