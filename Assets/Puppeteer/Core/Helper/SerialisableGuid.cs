using System;
using UnityEngine;

[Serializable]
public struct SerialisableGuid : ISerializationCallbackReceiver
{
	public SerialisableGuid(Guid _guidValue)
	{
		m_SerialisedGuid = string.Empty;
		m_Guid = _guidValue;
	}

	public Guid Value => m_Guid;

	public static implicit operator SerialisableGuid(Guid _guid)
	{
		return new SerialisableGuid(_guid);
	}
	public int CompareTo(Guid _guid)
	{
		return Value.CompareTo(_guid);
	}

	public int CompareTo(SerialisableGuid _serialisableGuid)
	{
		return Value.CompareTo(_serialisableGuid.Value);
	}

	public int CompareTo(object _object)
	{
		return Value.CompareTo(_object);
	}

	public bool Equals(Guid _guid)
	{
		return Value.Equals(_guid);
	}

	public bool Equals(SerialisableGuid _serialisableGuid)
	{
		return Value.Equals(_serialisableGuid.Value);
	}

	public override bool Equals(object _object)
	{
		return (_object is SerialisableGuid serialisableGuid) && Value.Equals(serialisableGuid.Value);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public void OnAfterDeserialize()
	{
		m_Guid = Guid.Parse(m_SerialisedGuid);
	}

	public void OnBeforeSerialize()
	{
		m_SerialisedGuid = m_Guid.ToString();
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	private Guid m_Guid;
	[SerializeField] private string m_SerialisedGuid;
}