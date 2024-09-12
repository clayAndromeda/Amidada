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
		public AmidaPlayerPointData PointData { get; private set; }
		
		public void SetPointData(AmidaPlayerPointData pointData)
		{
			PointData = pointData;
			UpdatePointData();
		}

		public void UpdatePointData()
		{
			AnchoredPosition = PointData.Position;
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