using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileInfo
{
	public Texture texture;
}

public class TileItem : MonoBehaviour
{
	TileInfo tileInfo;
	UITexture texture;
	Ui_ItemScrollView scpParent;

	public void SetInit(Ui_ItemScrollView _scpParent, Texture _texture)
	{
		scpParent = _scpParent;

		if (texture == null)
			texture = GetComponent<UITexture>();
		if(tileInfo == null)
			tileInfo = new TileInfo();

		tileInfo.texture = _texture;
		texture.mainTexture = _texture;
	}

	public void Invoke_Select()
	{
		Ui_SelectTile.ins.SetSelectTileData(tileInfo);
	}
}
