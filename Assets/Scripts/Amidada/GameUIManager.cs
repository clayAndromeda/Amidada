using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Amidada
{
	public class GameUIManager : MonoBehaviour
	{
		[SerializeField] private Image speedUpImage;
		[SerializeField] private Color redColor;

		private CompositeDisposable speedUpEventDisposables;
		
		public void RegisterGameEvents(AmidaGameSystem gameSystem)
		{
			// IsSpeedUpがtrueなら、0.1秒ごとに赤白に点滅。falseなら白色のまま
			gameSystem.IsSpeedUp
				.Subscribe(isSpeedUp =>
				{
					if (isSpeedUp)
					{
						speedUpEventDisposables = new();
						Observable.Interval(TimeSpan.FromSeconds(0.2))
							.Subscribe(_ => speedUpImage.color = speedUpImage.color == redColor ? Color.white : redColor)
							.AddTo(speedUpEventDisposables);
					}
					else
					{
						speedUpEventDisposables.Dispose();
						speedUpEventDisposables = null;
						speedUpImage.color = Color.white;
					}
				}).AddTo(this);
		}
	}
}