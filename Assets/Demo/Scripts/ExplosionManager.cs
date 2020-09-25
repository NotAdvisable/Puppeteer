using Puppeteer.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ExplosionManager : SingletonMonoBehaviour<ExplosionManager>
{
	private void Start()
	{
		foreach(var agent in PuppeteerManager.Instance.GetAgents())
		{
			agent.AddOrUpdateWorkingMemory("IsInDanger", false);
		}
	}

	public float GetDangerRange()
	{
		return m_DangerRange;
	}

	public Vector3? GetLastExplosionPositionIfAny()
	{
		return m_LastExplosionPosition;
	}

	public float GetRandomHideDuration()
	{
		return UnityEngine.Random.Range(m_MinMaxHideDurationInSeconds.x, m_MinMaxHideDurationInSeconds.y);
	}

	private async void ClearLastExplosionPositionAfterWait()
	{
		await Task.Delay(TimeSpan.FromSeconds(0.5f));
		m_LastExplosionPosition = null;
	}

	private async void DestroyExplosionInstanceAfterWait(GameObject _instance)
	{
		await Task.Delay(TimeSpan.FromSeconds(m_DestroyExplosionInstanceAfter));
		if (Application.isEditor)
		{
			DestroyImmediate(_instance);
		}
		else
		{
			Destroy(_instance);
		}
	}
	private void Update()
	{
		if (!Input.GetMouseButtonDown(0))
		{
			return;
		}

		if (m_ExplosionPrefab == null)
		{
			return;
		}

		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hit, 1000.0f, m_ExplosionRaycastMask))
		{
			DestroyExplosionInstanceAfterWait(Instantiate(m_ExplosionPrefab, hit.point, Quaternion.identity));
			m_LastExplosionPosition = hit.point;
			ClearLastExplosionPositionAfterWait();
		}
	}

#pragma warning disable CS0649
	[SerializeField] private float m_DangerRange = 5.0f;
	[SerializeField] private float m_DestroyExplosionInstanceAfter = 1;
	[SerializeField] private GameObject m_ExplosionPrefab;
	[SerializeField] private LayerMask m_ExplosionRaycastMask;
	[SerializeField] private Vector2 m_MinMaxHideDurationInSeconds;
#pragma warning restore CS0649

	private Vector3? m_LastExplosionPosition = null;
}