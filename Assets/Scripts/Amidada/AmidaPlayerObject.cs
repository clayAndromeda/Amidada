using UnityEngine;

namespace Amidada
{
	/// <summary>
	/// あみだくじの線上を動くプレイヤーを表現するコンポーネント
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	public class AmidaPlayerObject : MonoBehaviour
	{
		[SerializeField] private RectTransform rectTransform;

		public Vector2 AnchoredPosition
		{
			get => rectTransform.anchoredPosition;
			private set => rectTransform.anchoredPosition = value;
		}
		
		public void SetPointData(AmidaPlayerPoint playerPoint)
		{
			AnchoredPosition = playerPoint.Position;
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