using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ui_SelectTile : MonoBehaviour
{
	public static Ui_SelectTile ins;
	[HideInInspector]public TileInfo tileInfo;
	[SerializeField] UITexture uiTexture;
	[SerializeField] Ui_ItemScrollView uiItemScrollView;
	[SerializeField] GoogleARCore.Examples.HelloAR.HelloARController2 controller;


	private void Awake()
	{
		ins = this;
		//gameObject.SetActive(false);
	}

	public void SetSelectTileData(TileInfo _tileInfo)
	{
		//if(uiTexture == null)
		//	uiTexture = GetComponent<UITexture>();
		
		tileInfo = _tileInfo;
		uiTexture.mainTexture = _tileInfo.texture;
		controller.SetMaterial(_tileInfo.texture);

		gameObject.SetActive(true);
	}

	//public void Invoke_InVisiable()
	//{
	//	gameObject.SetActive(false);
	//}

	public void Invoke_Show_ItemScrollView()
	{
		uiItemScrollView.gameObject.SetActive(true);
		uiItemScrollView.InitData();
		//gameObject.SetActive(false);
	}
}
