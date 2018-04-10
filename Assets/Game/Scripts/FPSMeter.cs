using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChemicalG {
public class FPSMeter : MonoBehaviour {

  [Header("Will be false in build. ")]
  [Header("Set on commandline with --showfps")]
  public bool show = true;

  // Use this for initialization
  void Start () 
  {
    if (!Application.isEditor)
    {
      show = false;
    }

    string[] args = System.Environment.GetCommandLineArgs();
    int result = System.Array.FindIndex(args, s => s.Equals("--showfps", System.StringComparison.OrdinalIgnoreCase));
    if (result >= 0) {
      show = true;
    }

    m_green = MakeTexture(new Color(0, 0.7f, 0));
    m_red = MakeTexture(Color.red);
    m_gray = MakeTexture(Color.gray);
    m_blue = MakeTexture(Color.blue);
  }

  Texture MakeTexture(Color color)
  {
    var texture = new Texture2D(1, 1);
    Color[] pixels = { color };
    texture.SetPixels(pixels);
    texture.Apply();
    return texture;
  }

  void InitHistory()
  {
    if (m_durationByFrame == null || m_durationByFrame.Length != m_numHistory) 
    {
      m_durationByFrame = new float[m_numHistory];
      m_processingTimeByFrame = new float[m_numHistory];
      m_totalTime = 0.0f;
      m_stopWatch.Start();
    }
  }

  void Update() 
  {
    InitHistory();
    m_startTime = m_stopWatch.ElapsedTicks;
    m_deltaTime = Time.unscaledDeltaTime;
    int ndx = Time.frameCount % m_durationByFrame.Length;
    m_totalTime += m_deltaTime - m_durationByFrame[ndx];
    m_durationByFrame[ndx] = m_deltaTime;
  }

  void OnGUI() 
  {
    if (!show) {
      return;
    }

    if (m_style == null)
    {
      m_style = new GUIStyle(GUI.skin.box);
      m_style.alignment = TextAnchor.UpperLeft;
    }

    InitHistory();

    // if (m_drawFrameId != Time.frameCount) {
    //  m_drawFrameId = Time.frameCount;

      float instantFps = 1.0f / m_deltaTime;
      float averageFps = 1.0f / (m_totalTime / m_durationByFrame.Length);
      GUI.BeginGroup(m_groupRect);
        GUI.Box(m_groupRect, string.Format("FPS: {0:0} - {1:0}", averageFps, instantFps), m_style);
        GUI.BeginGroup(m_fpsRect);
          GUI.DrawTexture(m_60FpsRect, m_gray);
          GUI.DrawTexture(m_30FpsRect, m_gray);
          GUI.DrawTexture(m_0FpsRect, m_gray);
          for (int i = 0; i < m_durationByFrame.Length; ++i)
          { 
            /*
            55 56 58 59  0
            57 58 59  0  1
            58 59  0  1  2 
            59  0  1  2  3
            */
            int ndx = (Time.frameCount + m_durationByFrame.Length + i - 1) % m_durationByFrame.Length;
            float dt = m_durationByFrame[ndx];
            int fps = (int)(1.0f / dt);
            GUI.DrawTexture(new Rect(i, 0, 1, m_processingTimeByFrame[ndx] * 60), m_blue);
            GUI.DrawTexture(new Rect(i, fps, 1, 2), fps < 50 ? m_red : m_green);
          }
        GUI.EndGroup();
      GUI.EndGroup();
    // }

    {
      int ndx = Time.frameCount % m_durationByFrame.Length;
      double ticksPerFrame = System.Diagnostics.Stopwatch.Frequency / 60.0;
      long deltaTicks = m_stopWatch.ElapsedTicks - m_startTime;
      m_processingTimeByFrame[ndx] = (float)((double)deltaTicks / ticksPerFrame);
    }
  }

  System.Diagnostics.Stopwatch m_stopWatch = new System.Diagnostics.Stopwatch();
  Texture m_green;
  Texture m_red;
  Texture m_blue;
  Texture m_gray;
  int m_drawFrameId = 0;
  float m_totalTime = 0.0f;
  uint m_numHistory = 100;
  float[] m_durationByFrame;
  float[] m_processingTimeByFrame;
  float m_deltaTime;
  long m_startTime;
  Rect m_groupRect = new Rect(0, 0, 100, 100);
  Rect m_fpsRect = new Rect(0, 30, 100, 100 - 30);
  Rect m_0FpsRect = new Rect(0, 0, 100, 1);
  Rect m_60FpsRect = new Rect(0, 30, 100, 1);
  Rect m_30FpsRect = new Rect(0, 60, 100, 1);
  GUIStyle m_style;
}

}  // namespace
