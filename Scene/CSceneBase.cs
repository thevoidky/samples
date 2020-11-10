using UnityEngine;
using System.Collections;
using System.Text;


namespace CUtility.Scene
{
	//--------------------------------------------------------------------------------------------------------------------
	//	CSceneBase
	//
	//--------------------------------------------------------------------------------------------------------------------
	public abstract class CSceneBase : MonoBehaviour
	{
		//--------------------------------------------------------------------------------------------------------------------
		//	변수
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [변수]

		private bool _isLoaded = false;
		public bool isLoaded { get { return _isLoaded; } }

		private bool _isSequenceComplete = false;
		public bool isSequenceComplete { get { return _isSequenceComplete; } }

		private Coroutine _coroutine_playing = null;

		protected CSceneArgsBase _arguments = null;

		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	property
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [property]

		public string SceneName
		{ get { return GetType().ToString().Substring(CRoot.Instance._sceneHeader.Length); } }

		public string SceneFullname
		{ get { return GetType().ToString(); } }

		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	기본 함수
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [초기화]
		//--------------------------------------------------------------------------------------------------------------------
		protected virtual IEnumerator Start()
		{
			Debug.LogFormat("{0} : Start", GetType().ToString());

			_arguments = CSceneManager.Arguments;

			yield return StartCoroutine(init());

			_isLoaded = true;

			yield break;
		}
		#endregion

		#region [소멸]
		//--------------------------------------------------------------------------------------------------------------------
		protected virtual void OnDestroy()
		{
			Debug.LogFormat("{0} : {1}", GetType().ToString(), System.Reflection.MethodBase.GetCurrentMethod().ToString());

			release();
		}
		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	추상 함수
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [Scene 초기화 동작]
		//--------------------------------------------------------------------------------------------------------------------
		protected abstract IEnumerator init();
		#endregion

		#region [Scene이 시작된 뒤 일련의 동작]
		//--------------------------------------------------------------------------------------------------------------------
		protected abstract IEnumerator sequence();
		#endregion

		#region [Scene 벗어날 때의 동작]
		//--------------------------------------------------------------------------------------------------------------------
		protected abstract void release();
		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	메서드
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [Scene 동작 시작]
		//--------------------------------------------------------------------------------------------------------------------
		internal IEnumerator Play()
		{
			if (null != _coroutine_playing)
			{
				StopCoroutine(_coroutine_playing);
				_coroutine_playing = null;
			}

			float timeout = 15f;
			while (!isLoaded)
			{
				yield return CachedYield.WaitForEndOfFrame;

				timeout -= Time.unscaledDeltaTime;
				if (0f > timeout)
				{
					Debug.LogErrorFormat("{0} : Scene 시작 실패 ({1})", GetType().ToString(), SceneName);
					yield break;
				}
			}

			_coroutine_playing = StartCoroutine(sequence());
			yield return _coroutine_playing;

			_isSequenceComplete = true;
		}
		#endregion
	}



	//--------------------------------------------------------------------------------------------------------------------
	//	CSceneArgs
	//	Scene 초기화 시 사용할 인자를 구성한다.
	//
	//--------------------------------------------------------------------------------------------------------------------

	public abstract class CSceneArgsBase { }
}
