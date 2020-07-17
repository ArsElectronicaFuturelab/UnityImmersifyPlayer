using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeepSpace.JsonProtocol;

using ChromaSubsampling = ImmersifyPlugin.ChromaSubsampling;
using PathType = ImmersifyPlugin.PathType;
using StereoVideoMode = ImmersifyPlugin.StereoVideoMode;

[System.Serializable]
public class VideoLoadAsset : AssetIdType
{
	public VideoLoadAsset()
	{
		EventIdentifier = EventIdentifierVideo.VIDEO_LOAD;
	}

	[SerializeField]
	private string _path;
	[SerializeField]
	private ImmersifyPlugin.PathType _pathType;
	//[SerializeField]
	//private int _width;
	//[SerializeField]
	//private int _height;
	[SerializeField]
	private float _framerate;
	//[SerializeField]
	//private ChromaSubsampling _chromaSubsampling;
	[SerializeField]
	private int maxQueue;
	[SerializeField]
	private StereoVideoMode stereoMode = StereoVideoMode.MONO;
	[SerializeField]
	private bool invertLeftRight = false;
	[SerializeField]
	private bool videoIsUpsideDown = false;

	public string Path
	{
		get { return _path; }
		set { _path = value; }
	}

	public PathType PathType
	{
		get { return _pathType; }
		set { _pathType = value; }
	}

	//public int Width
	//{
	//	get { return _width; }
	//	set { _width = value; }
	//}

	//public int Height
	//{
	//	get { return _height; }
	//	set { _height = value; }
	//}

	public float Framerate
	{
		get { return _framerate; }
		set { _framerate = value; }
	}

	//public ChromaSubsampling ChromaSubsampling
	//{
	//	get { return _chromaSubsampling; }
	//	set { _chromaSubsampling = value; }
	//}

	public int MaxQueue
	{
		get { return maxQueue; }
		set { maxQueue = value; }
	}

	public StereoVideoMode StereoMode
	{
		get { return stereoMode; }
		set { stereoMode = value; }
	}

	public bool InvertLeftRight
	{
		get { return invertLeftRight; }
		set { invertLeftRight = value; }
	}

	public bool VideoIsUpsideDown
	{
		get { return videoIsUpsideDown; }
		set { videoIsUpsideDown = value; }
	}
}
