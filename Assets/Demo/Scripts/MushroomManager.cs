using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MushroomManager : SingletonMonoBehaviour<MushroomManager>
{
	public void AddMushroom(GameObject _harvestedMushroom)
	{
		OnMushroomCountChanged?.Invoke(m_DesiredMushroomCount, ++m_MushroomCounter);

		if (_harvestedMushroom != null)
		{
			m_Claimed.Remove(_harvestedMushroom);

			if (Application.isEditor)
			{
				DestroyImmediate(_harvestedMushroom);
			}
			else
			{
				Destroy(_harvestedMushroom);
			}
		}

		foreach (var spawnedMushroomPair in m_SpawnedMushrooms)
		{
			if (spawnedMushroomPair.Value != null)
			{
				continue;
			}

			int countForTransform = m_RespawnQueue.Count(_entry => _entry.Item2 == spawnedMushroomPair.Key);
			if (countForTransform != 0)
			{
				continue;
			}

			float randomRespawnTimer = Time.time + UnityEngine.Random.Range(m_MinMaxRespawnTimerInSeonds.x, m_MinMaxRespawnTimerInSeonds.y);

			m_RespawnQueue.Add(new Tuple<float, Transform>(randomRespawnTimer, spawnedMushroomPair.Key));
		}
	}

	public GameObject GetClosestUnclaimedMushroomToPosition(Vector3 _position)
	{
		GameObject closest = null;
		float distSquared = float.MaxValue;
		bool wasClosestReturned = false;

		foreach (var returnedMushroom in m_Returned)
		{
			if (returnedMushroom == null)
			{
				continue;
			}

			float currentDistSquared = (returnedMushroom.transform.position - _position).sqrMagnitude;
			if (currentDistSquared < distSquared && !m_Claimed.Contains(returnedMushroom))
			{
				closest = returnedMushroom;
				distSquared = currentDistSquared;
				wasClosestReturned = true;
			}
		}

		foreach (var spawnedMushroomPair in m_SpawnedMushrooms)
		{
			if (spawnedMushroomPair.Value == null)
			{
				continue;
			}

			var currentMushroom = spawnedMushroomPair.Value;
			float currentDistSquared = (currentMushroom.transform.position - _position).sqrMagnitude;
			if (currentDistSquared < distSquared && !m_Claimed.Contains(currentMushroom))
			{
				closest = currentMushroom;
				distSquared = currentDistSquared;
				wasClosestReturned = false;
			}
		}

		if (closest != null)
		{
			m_Claimed.Add(closest);
		}

		if (wasClosestReturned)
		{
			m_Returned.Remove(closest);
		}

		return closest;
	}

	public int GetDesiredCount()
	{
		return m_DesiredMushroomCount;
	}
	public int GetHasCount()
	{
		return m_MushroomCounter;
	}

	public Transform GetHomeBaseTransform()
	{
		return m_HomeBaseTransform;
	}

	public void ReturnMushroom(GameObject _returnedMushroom)
	{
		m_Claimed.Remove(_returnedMushroom);
		m_Returned.Add(_returnedMushroom);
	}
	async private void ConsumeAfterWait()
	{
		await Task.Delay(TimeSpan.FromSeconds(m_EatMushroomsEvery));
		m_MushroomCounter = Math.Max(m_MushroomCounter - m_MushroomConsumptionPerMeal, 0);
		OnMushroomCountChanged?.Invoke(m_DesiredMushroomCount, m_MushroomCounter);
		ConsumeAfterWait();
	}

	private void FixedUpdate()
	{
		if (m_RespawnQueue.Count == 0)
		{
			return;
		}

		for (int i = m_RespawnQueue.Count - 1; i >= 0; --i)
		{
			Tuple<float, Transform> currentTuple = m_RespawnQueue[i];
			if (Time.time >= currentTuple.Item1)
			{
				SpawnRandomMushroomAtTransform(currentTuple.Item2);
				m_RespawnQueue.Remove(currentTuple);
			}
		}
	}

	private void SpawnRandomMushroomAtTransform(Transform _transform)
	{
		int randomIndex = UnityEngine.Random.Range(0, m_MushroomPrefabs.Length);
		GameObject spawnedMushroom = Instantiate(m_MushroomPrefabs[randomIndex], _transform, false);
		spawnedMushroom.transform.localPosition = Vector3.zero;

		m_SpawnedMushrooms[_transform] = spawnedMushroom;
	}

	private void Start()
	{
		foreach (Transform childTransform in transform)
		{
			SpawnRandomMushroomAtTransform(childTransform);
		}

		ConsumeAfterWait();
	}
	public Action<int, int> OnMushroomCountChanged;

	private readonly List<GameObject> m_Claimed = new List<GameObject>();
	private readonly List<Tuple<float, Transform>> m_RespawnQueue = new List<Tuple<float, Transform>>();
	private readonly List<GameObject> m_Returned = new List<GameObject>();
	private readonly Dictionary<Transform, GameObject> m_SpawnedMushrooms = new Dictionary<Transform, GameObject>();

#pragma warning disable CS0649
	[SerializeField] private int m_DesiredMushroomCount = 5;
	[SerializeField] private float m_EatMushroomsEvery = 30;
	[SerializeField] private Transform m_HomeBaseTransform;
	[SerializeField] private Vector2 m_MinMaxRespawnTimerInSeonds;
	[SerializeField] private int m_MushroomConsumptionPerMeal = 5;
	[SerializeField] private int m_MushroomCounter = 0;
	[SerializeField] private GameObject[] m_MushroomPrefabs;
#pragma warning restore CS0649
}