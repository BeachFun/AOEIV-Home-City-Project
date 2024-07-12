using RTSEngine.EntityComponent;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskDisplayItem : MonoBehaviour
{
	[SerializeField]
    private TextMeshProUGUI actionText;
    [SerializeField]
    private TextMeshProUGUI targetText;

	public void Init(SetTargetInputData task)
	{
        actionText.text = task.componentCode;
        targetText.text = task.target.instance?.Code ?? "No target";
	}

    //TODO: Accomodate Icons
}
 