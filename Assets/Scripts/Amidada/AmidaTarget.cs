using UnityEngine;

namespace Amidada
{
	[RequireComponent(typeof(RectTransform))]
	public class AmidaTarget : MonoBehaviour
	{
		[SerializeField] private RectTransform rectTransform;

		public Vector2 AnchoredPosition
		{
			get => rectTransform.anchoredPosition;
			private set => rectTransform.anchoredPosition = value;
		}

		/// <summary> 何番目の縦線に配置したか？ </summary>
		public int TateLineIndex { get; set; }

		public void SetPosition(Vector2 position, int tateLineIndex)
		{
			AnchoredPosition = position;
			TateLineIndex = tateLineIndex;
		}

		/// <summary>
		/// 削除する
		/// </summary>
		public void Destroy()
		{
			Destroy(this.gameObject);
		}
	}
}