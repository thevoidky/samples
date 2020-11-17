using CUtility.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//--------------------------------------------------------------------------------------------------------------------
//	imSceneIntro
//
//--------------------------------------------------------------------------------------------------------------------
public class imSceneIntro : CSceneBase
{
	//--------------------------------------------------------------------------------------------------------------------
	//	변수
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [변수]

	[SerializeField] private Slider _loadingProgress;
	[SerializeField] private imUiVersionCut _uiVersionCut;

	private float _unitProgress = 0f;
	private float _minProgress, _maxProgress;

	#endregion


	//--------------------------------------------------------------------------------------------------------------------
	//	property
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [property]

	#endregion


	//--------------------------------------------------------------------------------------------------------------------
	//	Message Receiver
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [Update]
	//--------------------------------------------------------------------------------------------------------------------
	private void Update()
	{
		_loadingProgress.value = Mathf.Lerp(_minProgress, _maxProgress, _unitProgress);
	}
	#endregion


	//--------------------------------------------------------------------------------------------------------------------
	//	추상 함수 구현
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [Scene 초기화 동작]
	//--------------------------------------------------------------------------------------------------------------------
	protected override IEnumerator init()
	{
		yield break;
	}
	#endregion

	#region [Scene이 시작된 뒤 일련의 동작]
	//--------------------------------------------------------------------------------------------------------------------
	protected override IEnumerator sequence()
	{
		var isComplete = false;

		UltimateMobileWrapper.Instance.Init();

		//	서버 주소 불러오기
		_minProgress = 0f; _maxProgress = 0.2f; _unitProgress = 0f;
		yield return StartCoroutine(loadUrl());

		//	서버 데이터 불러오기
		_minProgress = _maxProgress; _maxProgress = 0.45f; _unitProgress = 0f;
		yield return StartCoroutine(loadServerData());

		//	서버 저장된 데이터 불러오기
		yield return StartCoroutine(loadDatabase());

		//	파이어베이스
		_minProgress = _maxProgress; _maxProgress = 0.5f; _unitProgress = 0f;
		yield return StartCoroutine(initFirebase());

		//	리소스 불러오기
		_minProgress = _maxProgress; _maxProgress = 0.8f; _unitProgress = 0f;
		yield return StartCoroutine(loadResources());

		//	결제 모듈 접속
		_minProgress = _maxProgress; _maxProgress = 1f; _unitProgress = 0f;
		UltimateMobileWrapper.Instance.ConnectBilling(res => isComplete = true);
		yield return CachedYield.WaitUntil(() => isComplete);

		_unitProgress = _maxProgress;
		yield return CachedYield.WaitForSeconds(0.3f);
		CSceneManager.LoadSceneAsync("Game", LoadSceneMode.Single, null, null);

		yield break;
	}
	#endregion

	#region [Scene 벗어날 때의 동작]
	//--------------------------------------------------------------------------------------------------------------------
	protected override void release()
	{
	}
	#endregion


	//--------------------------------------------------------------------------------------------------------------------
	//	내부 함수
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [파이어베이스 초기화]
	//--------------------------------------------------------------------------------------------------------------------
	private IEnumerator initFirebase()
	{
		var isComplete = false;

		Firebase.FirebaseApp.CheckAndFixDependenciesAsync()?.ContinueWith(task =>
		{
			var dependencyStatus = task.Result;
			if (Firebase.DependencyStatus.Available == dependencyStatus)
			{
				// Create and hold a reference to your FirebaseApp,
				// where app is a Firebase.FirebaseApp property of your application class.
				//   app = Firebase.FirebaseApp.DefaultInstance;

				// Set a flag here to indicate whether Firebase is ready to use by your app.
			}
			else
			{
				Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
				// Firebase Unity SDK is not safe to use here.
			}

			isComplete = true;
		});

		yield return CachedYield.WaitUntil(() => isComplete);

		_unitProgress = _maxProgress;
	}
	#endregion

	#region [서버 데이터 불러오기]
	//--------------------------------------------------------------------------------------------------------------------
	private IEnumerator loadServerData()
	{
		var request = CWebWrapper.Get(CWebWrapper.urlServerData, null, CWebWrapper.useCertificate);
		var async = request.SendWebRequest();

		while (!async.isDone)
		{
			yield return CachedYield.WaitForEndOfFrame;
			_unitProgress = async.progress;
		}

		_unitProgress = 1f;

		//----------------------------------------------------------------------------------------------------------------
		//	Detail is not contained in sample code
		//----------------------------------------------------------------------------------------------------------------
	}
	#endregion

	#region [서버 주소 불러오기]
	//--------------------------------------------------------------------------------------------------------------------
	private IEnumerator loadUrl()
	{
		var request = CWebWrapper.Get(CWebWrapper.urlUrlData);
		var async = request.SendWebRequest();

		while (!async.isDone)
		{
			yield return CachedYield.WaitForEndOfFrame;
			_unitProgress = async.progress;
		}

		_unitProgress = 1f;

		//----------------------------------------------------------------------------------------------------------------
		//	Detail is not contained in sample code
		//----------------------------------------------------------------------------------------------------------------

		yield break;
	}
	#endregion

	#region [서버 저장된 데이터 불러오기]
	//--------------------------------------------------------------------------------------------------------------------
	private IEnumerator loadDatabase()
	{
		//----------------------------------------------------------------------------------------------------------------
		//	Detail is not contained in sample code
		//----------------------------------------------------------------------------------------------------------------

		yield break;
	}
	#endregion

	#region [리소스 불러오기]
	//--------------------------------------------------------------------------------------------------------------------
	private IEnumerator loadResources()
	{
		//----------------------------------------------------------------------------------------------------------------
		//	Detail is not contained in sample code
		//----------------------------------------------------------------------------------------------------------------

		yield break;
	}
	#endregion



	//--------------------------------------------------------------------------------------------------------------------
	//	메서드
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [스토어 열기]
	//--------------------------------------------------------------------------------------------------------------------
	public void OpenStore()
	{
#if UNITY_ANDROID
		Application.OpenURL($"market://details?id={Application.identifier}");
#endif
	}
	#endregion
}
