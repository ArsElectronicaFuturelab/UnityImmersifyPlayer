using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeepSpace;

public class LoadVideoByConfig : MonoBehaviour
{
	private ImmersifyCmdConfigMgr _config;
	
	[SerializeField]
	private ImmersifyPlugin _videoPlugin = null;

	[SerializeField]
	private bool _pauseAtStart = false;
	[SerializeField]
	private bool _loopVideo = false;

	private IEnumerator Start()
	{
		_config = ImmersifyCmdConfigMgr.Instance as ImmersifyCmdConfigMgr;

		if(string.IsNullOrEmpty(_config.audioFile) == false)
		{
			while(_config.AudioClipIsLoading == true)
			{
				yield return null;
			}
		}

		Debug.Log("Loading Video via Plugin " + (_config.AudioClip != null ? "with" : "without") + " audio.");
        if (System.IO.File.Exists(_config.videoFile))
        {
		    _videoPlugin.InitializePlayer(_config.videoFile, _config.videoPathType, _config.videoFramerate, _config.AudioClip, _config.maxQueue, _config.stereoMode, _config.invertLeftRight, _config.videoIsUpsideDown, _pauseAtStart, _loopVideo);
            _videoPlugin.Play();
        }
        else
        {
            Debug.Log("The file \"" + _config.videoFile + "\" does not exist");
        }
    }
}
