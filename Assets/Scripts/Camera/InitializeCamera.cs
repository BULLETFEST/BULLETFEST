using UnityEngine;
using UnityEngine.Video;


public class InitializeCamera : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {
    VideoPlayer v = Camera.main.gameObject.AddComponent<VideoPlayer>();
    v.clip = Resources.Load<VideoClip>("glitch");
    v.isLooping = true;
    v.playOnAwake = true;
    v.waitForFirstFrame = true;
    v.playbackSpeed = 1.75f;
    v.targetCameraAlpha = 0.222f;
    v.aspectRatio = VideoAspectRatio.FitInside;
    v.audioOutputMode = VideoAudioOutputMode.None;
    v.renderMode = VideoRenderMode.CameraFarPlane;

    v.Play();
  }
}
