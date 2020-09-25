using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance
	{
		get
		{
			lock (INSTANCE_LOCK)
			{
				if (m_Quitting)
				{
					return null;
				}

				if (m_InstanceValue != null)
				{
					return m_InstanceValue;
				}

				m_InstanceValue = FindObjectOfType<T>();
				if (m_InstanceValue == null)
				{
					GameObject go = new GameObject(typeof(T).ToString());
					m_InstanceValue = go.AddComponent<T>();

					DontDestroyOnLoad(m_InstanceValue.gameObject);
				}

				return m_InstanceValue;
			}
		}
		set
		{
			m_InstanceValue = value;
		}
	}

	protected virtual void Awake()
	{
		if (Instance == null)
		{
#pragma warning disable UNT0014 // T is not a Unity Component
			Instance = gameObject.GetComponent<T>();
#pragma warning restore UNT0014 // T is not a Unity Component
		}
		else if (Instance.GetInstanceID() != GetInstanceID())
		{
			Debug.LogWarning(string.Format("There should only be one instance of {0}", typeof(T).ToString()));
			Destroy(gameObject);
		}
	}

	protected virtual void OnApplicationQuit()
	{
		m_Quitting = true;
	}

	protected virtual void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private static readonly object INSTANCE_LOCK = new object();
	private static T m_InstanceValue = null;
	private static bool m_Quitting = false;
}