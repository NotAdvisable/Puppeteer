using Puppeteer.Core;
using UnityEngine;

public class CarryHelper : MonoBehaviour
{
	private void Awake()
	{
		if (gameObject.TryGetComponent<PuppeteerAgent>(out var puppeteerAgent))
		{
			m_Agent = puppeteerAgent;
			m_Agent.OnPlanInterrupted += ReturnObjectToManagerAndDrop;
		}
	}

	private void ReturnObjectToManagerAndDrop()
	{
		m_Agent.GetWorkingMemory().TryGetValue("Carries", out var value);

		if (value == null)
		{
			RemoveClaim();
			return;
		}
		switch (value as string)
		{
			case "Fish":
				FishManager.Instance.ReturnFish(m_CarriedGameObject);
				break;

			case "Mushroom":
				MushroomManager.Instance.ReturnMushroom(m_CarriedGameObject);
				break;

			default:
				RemoveClaim();
				break;
		}
		m_Agent.AddOrUpdateWorkingMemory("Carries", "-");

		DropCarriedGameObject();
	}

	private void RemoveClaim()
	{
		m_Agent.GetWorkingMemory().TryGetValue("TargetIs", out var targetType);
		if (targetType == null)
		{
			return;
		}

		m_Agent.GetWorkingMemory().TryGetValue("PickupTarget", out var targetObject);
		if (targetObject == null)
		{
			return;
		}

		switch (targetType as string)
		{
			case "Fish":
				FishManager.Instance.ReturnFish(targetObject as GameObject);
				break;

			case "Mushroom":
				MushroomManager.Instance.ReturnMushroom(targetObject as GameObject);
				break;
		}

		m_Agent.AddOrUpdateWorkingMemory("TargetIs", "-");
	}

	public void ReplaceCarriedObject(GameObject _replacement)
	{
		if (m_CarriedGameObject != null)
		{
			DropCarriedGameObject();
		}

		m_CarriedGameObject = _replacement;

		if (m_CarriedGameObject == null)
		{
			return;
		}

		m_CarriedGameObject.transform.parent = m_CarryPositionObject.transform;
		m_CarriedGameObject.transform.localPosition = Vector3.zero;
		m_CarriedGameObject.transform.localRotation = Quaternion.identity;

		if (m_CarriedGameObject.TryGetComponent<Rigidbody>(out var rigidbody))
		{
			rigidbody.isKinematic = true;
		}
	}

	public void DropCarriedGameObject()
	{
		if (m_CarriedGameObject == null)
		{
			return;
		}

		m_CarriedGameObject.transform.parent = null;
		m_CarriedGameObject.transform.localPosition = gameObject.transform.position;
		m_CarriedGameObject.transform.localRotation = Quaternion.identity;

		if (m_CarriedGameObject.TryGetComponent<Rigidbody>(out var rigidbody))
		{
			rigidbody.isKinematic = false;
		}

		m_CarriedGameObject = null;
	}

	private GameObject m_CarriedGameObject;
	private PuppeteerAgent m_Agent;

#pragma warning disable CS0649
	[SerializeField] private GameObject m_CarryPositionObject;
#pragma warning restore CS0649
}