using UnityEngine;
using RTSEngine.Entities;
using RTSEngine.ResourceExtension;
using RTSEngine.Game;

public class CarriableItem : Resource
{
	[SerializeField, Tooltip("The weight of this item, affecting unit movement speed.")]
	private float weight = 1f;
	public float Weight => weight;

	[SerializeField, Tooltip("Offset position when carried on unit's head")]
	private Vector3 carryOffset = new Vector3(0, 1.5f, 0);

	public void AttachToUnit(Transform unitTransform)
	{
		transform.SetParent(unitTransform);
		transform.localPosition = carryOffset;
		transform.localRotation = Quaternion.identity;
	}

	public void Drop(Vector3 position)
	{
		transform.position = position;
		gameObject.SetActive(true);
	}
}