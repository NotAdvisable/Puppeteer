using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class FishManager : SingletonMonoBehaviour<FishManager>
{
	public void AddFish(GameObject _harvestedFish)
	{
		OnFishCountChanged?.Invoke(m_DesiredFishCount, ++m_FishCounter);

		if (_harvestedFish != null)
		{
			m_Unclaimed.Remove(_harvestedFish);
			m_Claimed.Remove(_harvestedFish);

			if (Application.isEditor)
			{
				DestroyImmediate(_harvestedFish);
			}
			else
			{
				Destroy(_harvestedFish);
			}
		}
	}

	public GameObject GetClosestUnclaimedFishToPosition(Vector3 _position)
	{
		GameObject closest = null;
		float distSquared = float.MaxValue;

		foreach (var spawnedFish in m_Unclaimed)
		{
			if (spawnedFish == null)
			{
				continue;
			}

			var currentMushroom = spawnedFish;
			float currentDistSquared = (currentMushroom.transform.position - _position).sqrMagnitude;
			if (currentDistSquared < distSquared && !m_Claimed.Contains(currentMushroom))
			{
				closest = currentMushroom;
				distSquared = currentDistSquared;
			}
		}

		if (closest != null)
		{
			m_Claimed.Add(closest);
			OnFishClaimed?.Invoke(m_Unclaimed.Count, m_Claimed.Count);
		}

		return closest;
	}

	public Vector3 GetCurrentFishingSpotPosition()
	{
		return m_ActieFishingSpot.position;
	}

	public int GetDesiredCount()
	{
		return m_DesiredFishCount;
	}

	public int GetHasCount()
	{
		return m_FishCounter;
	}

	public Transform GetHomeBaseTransform()
	{
		return m_HomeBaseTransform;
	}

	public float GetRandomFishingDuration()
	{
		return Random.Range(m_MinMaxFishingDurationInSeconds.x, m_MinMaxFishingDurationInSeconds.y);
	}

	public void ReturnFish(GameObject _returnedFish)
	{
		m_Claimed.Remove(_returnedFish);
		m_Unclaimed.Add(_returnedFish);

		OnFishSpawned?.Invoke(m_Unclaimed.Count, m_Claimed.Count);
	}

	public void SpawnFish(Vector3 _position)
	{
		if (m_FishPrefab == null)
		{
			return;
		}

		var spawnedFish = Instantiate(m_FishPrefab, _position, Quaternion.Euler(Random.Range(0, 361), Random.Range(0, 361), Random.Range(0, 361)));
		spawnedFish.transform.localScale *= Random.Range(0.5f, 1.5f);

		m_Unclaimed.Add(spawnedFish);
		OnFishSpawned?.Invoke(m_Unclaimed.Count, m_Claimed.Count);
	}

	protected override void Awake()
	{
		base.Awake();
		UpdateFishingSpot();
		ConsumeAfterWait();
	}

	async private void ConsumeAfterWait()
	{
		await Task.Delay(TimeSpan.FromSeconds(m_EatFishEvery));
		m_FishCounter = Math.Max(m_FishCounter - m_FishConsumptionPerMeal, 0);
		OnFishCountChanged?.Invoke(m_DesiredFishCount, m_FishCounter);
		ConsumeAfterWait();
	}
	private void FixedUpdate()
	{
		if (Time.time >= m_ChangeFishingSpotIn)
		{
			UpdateFishingSpot();
		}
	}

	private void UpdateFishingSpot()
	{
		m_ActieFishingSpot = transform.GetChild(Random.Range(0, transform.childCount));

		m_ChangeFishingSpotIn = Time.time + Random.Range(m_MinMaxFishingSpotChangeInSeconds.x, m_MinMaxFishingSpotChangeInSeconds.y);
	}

	public System.Action<int, int> OnFishClaimed;
	public System.Action<int, int> OnFishCountChanged;
	public System.Action<int, int> OnFishSpawned;
	private readonly List<GameObject> m_Claimed = new List<GameObject>();
	private readonly HashSet<GameObject> m_Unclaimed = new HashSet<GameObject>();

	private Transform m_ActieFishingSpot;
	private float m_ChangeFishingSpotIn;
	private int m_FishCounter = 0;

#pragma warning disable CS0649
	[SerializeField] private int m_DesiredFishCount = 5;
	[SerializeField] private float m_EatFishEvery = 20;
	[SerializeField] private int m_FishConsumptionPerMeal = 1;
	[SerializeField] private GameObject m_FishPrefab;
	[SerializeField] private Transform m_HomeBaseTransform;
	[SerializeField] private Vector2 m_MinMaxFishingDurationInSeconds;
	[SerializeField] private Vector2 m_MinMaxFishingSpotChangeInSeconds;
#pragma warning restore CS0649
}