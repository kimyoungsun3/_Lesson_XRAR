using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.HelloAR;

public class Ui_SelectTile : MonoBehaviour
{
	public static Ui_SelectTile ins;
	[HideInInspector]public TileItem tileItem;
	[SerializeField] UITexture uiTexture;
	//[SerializeField] GoogleARCore.Examples.HelloAR.HelloARController2 controller;


	private void Awake()
	{
		ins = this;
	}

	public void SetSelectTileData(TileItem _tileItem)
	{
		tileItem = _tileItem;
		uiTexture.mainTexture = tileItem.tileInfo.texture;
		HelloARController2.ins.SetMaterial(tileItem.tileInfo.texture);
		//Debug.Log(tileItem.tileInfo.xxx);

		gameObject.SetActive(true);
	}
	public void Invoke_Show_ItemScrollView()
	{
		Ui_ItemScrollView.ins.gameObject.SetActive(true);
		Ui_ItemScrollView.ins.InitData();
	}
}
