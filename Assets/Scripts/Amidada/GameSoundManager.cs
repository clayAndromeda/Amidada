using R3;
using UnityEngine;

namespace Amidada
{
	public class GameSoundManager : MonoBehaviour
	{
		public void RegisterSoundEvents(AmidaGameSystem gameSystem)
		{
			gameSystem.OnTurn.Subscribe(_ =>
			{
				// プレイヤーが曲がった
				Debug.Log("OnTurn");
			}).AddTo(this);
			
			gameSystem.GamePoint
				.Pairwise()
				.Where(pair => pair.Previous < pair.Current)
				.Subscribe(_ =>
				{
					// ポイントゲット！
					Debug.Log("PointUp");
				}).AddTo(this);
			
			gameSystem.State
				.Subscribe(_ =>
				{
					// ゲーム状態変更
					Debug.Log("StateChange");
				}).AddTo(this);	
		}
	}
}