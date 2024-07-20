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

public class ActionWheel : MonoBehaviour, IPostRunGameService
{
	//[SerializeField] private ActionWheelOption actionWheelOptionPrefab;
	[SerializeField] private Transform uiCanvas;

	protected IGameManager gameMgr { private set; get; }
	protected ISelectionManager selectionMgr { private set; get; }
	protected IGlobalEventPublisher globalEvent { private set; get; }
	protected IGameUITextDisplayManager gameUITextDisplayer { private set; get; }
	protected IGameLoggingService logger { private set; get; }

	public void Init(IGameManager gameMgr)
	{
		this.gameMgr = gameMgr;
		this.selectionMgr = gameMgr.GetService<ISelectionManager>();
		this.globalEvent = gameMgr.GetService<IGlobalEventPublisher>();
		//this.gameUITextDisplayer = gameMgr.GetService<IGameUITextDisplayManager>();
		this.logger = gameMgr.GetService<IGameLoggingService>();


	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		//TODO: Delete display panels that belong to units that are no longer active. Or consider making one single display panel rather than creating multiple new ones.
	}


	private IEntity GetCurrentSelectedUnit()
    {
		IEntity selectedEntity = selectionMgr.GetSingleSelectedEntity(EntityType.unit);
		return selectedEntity;
	}


	private void ShowActionWheel()
	{

		// Retrieve current active unit, and all actions available to them (a different script manages what actions are available to them).
		IEntity activeUnit = GetCurrentSelectedUnit();
		// Retrieve all actions available to currently selected unit.


		// Retrieve current selected target, and all potential actions that could be done unto the target (a different script manages what actions could be done on to it).
		// How to select the current target?

		// Update action wheel based on currently selected unit, and currently selected target. Only show actions that fall within both the selected unit's available actions, and the selected target's potential actions).

		// Animate Action wheel In

	}

	private void HideActionWheel()
	{
		// Animate Action Wheel Out


	}
}