using TMPro;
using UnityEngine;

public class CanvasCounterHelper : MonoBehaviour
{
	private void CountChanged(TextMeshProUGUI _textMesh, int _desiredCount, int _hasCount)
	{
		_textMesh.SetText(string.Format(m_CounterFormat, _hasCount, _desiredCount));
	}

	private void Start()
	{
		var fishManager = FishManager.Instance;
		CountChanged(m_FishCounterText, fishManager.GetDesiredCount(), fishManager.GetHasCount());
		fishManager.OnFishCountChanged += (_desired, _has) => CountChanged(m_FishCounterText, _desired, _has);

		var mushroomManager = MushroomManager.Instance;
		CountChanged(m_MushroomCounterText, mushroomManager.GetDesiredCount(), mushroomManager.GetHasCount());
		mushroomManager.OnMushroomCountChanged += (_desired, _has) => CountChanged(m_MushroomCounterText, _desired, _has);
	}

	private const string m_CounterFormat = "{0}/{1}";

#pragma warning disable CS0649
	[SerializeField] private TextMeshProUGUI m_FishCounterText;
	[SerializeField] private TextMeshProUGUI m_MushroomCounterText;
#pragma warning restore CS0649
}