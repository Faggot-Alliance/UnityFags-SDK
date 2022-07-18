using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

#if UNITY_EDITOR
public class BetterKeyframingEditor : EditorWindow
{

    public float currentTime;
    private float lastTime, lastLastTime; // ;) 
    private bool isPlaying;
    private bool hasStopped;
    AudioClip audioClip;

    Texture2D audioWaveform;
    bool generatedAudioWaveform;
    bool isPlayMode = false;
    bool instantReplay = false;

    Color graphColor = Color.red;
    Color bgColor = new Color(0.761f, 0.761f, 0.761f, 1);

    Color lastGraphColor, lastBgColor;

    [MenuItem("/Window/Better Keyframer")]
    static void ShowWaveformWindow()
    {
        BetterKeyframingEditor window = (BetterKeyframingEditor)GetWindow<BetterKeyframingEditor>("Audio Keyframer", true, typeof(EditorWindow));
        window.minSize = new Vector2(500, 75);
        window.maxSize = new Vector2(10000, 75);
        window.Show();
    }

    void OnEnable()
    {
        graphColor = new Color(EditorPrefs.GetFloat("GraphColor_R"), EditorPrefs.GetFloat("GraphColor_G"), EditorPrefs.GetFloat("GraphColor_B"), EditorPrefs.GetFloat("GraphColor_A"));
        bgColor = new Color(EditorPrefs.GetFloat("BGColor_R"), EditorPrefs.GetFloat("BGColor_G"), EditorPrefs.GetFloat("BGColor_B"), EditorPrefs.GetFloat("BGColor_A"));

        if (EditorPrefs.GetFloat("GraphColor_A") == 0 && EditorPrefs.GetFloat("BGColor_A") == 0)
        {
            graphColor = Color.red;
            bgColor = new Color(0.761f, 0.761f, 0.761f, 1);
        }

        audioClip = (AudioClip)AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("clipPath", null), typeof(AudioClip));
        if (audioClip != null)
        {
            audioWaveform = AudioWaveform(audioClip, 16000, 100, graphColor);
            generatedAudioWaveform = true;
        }
    }

    void Update()
    {
        if (audioClip == null)
        {
            isPlaying = false;
            return;
        }

        if (EditorApplication.isPlaying)
        {
            isPlayMode = true;
            return;
        }
        else
        {
            if (audioWaveform == null) audioWaveform = AudioWaveform(audioClip, 16000, 100, graphColor);
            isPlayMode = false;
        }

        if (generatedAudioWaveform) Repaint();

        lastLastTime = lastTime;
        lastTime = currentTime;
        currentTime = AnimWindowGetTime();
        if (AnimWindowState("playing") && lastTime != currentTime && !isPlaying && currentTime <= audioClip.length)
        {
            isPlaying = true;
            hasStopped = false;
            int sampleStart = (int)System.Math.Ceiling(audioClip.samples * (currentTime / audioClip.length));
            AudioUtility.PlayClip(audioClip);
            AudioUtility.SetClipSamplePosition(audioClip, sampleStart);
            return;
        }
        else if (lastTime != currentTime && !AnimWindowState("playing") && currentTime <= audioClip.length && instantReplay)
        {
            if (!isPlaying)
            {
                hasStopped = false;
                AudioUtility.PlayClip(audioClip);
                isPlaying = true;
            }
            int sampleStart = (int)System.Math.Ceiling(audioClip.samples * (currentTime / audioClip.length));
            AudioUtility.SetClipSamplePosition(audioClip, sampleStart);
            return;
        }
        else if (!AnimWindowState("playing") && lastLastTime == currentTime || lastTime > currentTime)
        {
            if (!hasStopped)
            {
                AudioUtility.StopAllClips();
            }
            isPlaying = false;
            hasStopped = true;
        }
    }

    void OnGUI()
    {
        Rect bgRect = new Rect(0, 0, position.width, position.height);
        EditorGUI.DrawRect(bgRect, bgColor);

        if (audioClip == null)
        {
            generatedAudioWaveform = false;
        }
        else if (generatedAudioWaveform)
        {

            Rect shownArea = GetShownArea();
            float offset = GetHierarchyWidth() / GetContentWidth();
            Rect audioWaveformRect = new Rect(GetRect().x + GetTranslation().x, 5, ((audioClip.length * GetRect().width) / shownArea.width), position.height - 10); // shownArea.width / 4

            Rect timeFrame = new Rect(audioWaveformRect.x + ((currentTime / (audioClip.length)) * audioWaveformRect.width), 0, 1, position.height);
            timeFrame.width = 1;
            timeFrame.y = 0;

            timeFrame.x = (audioWaveformRect.x + ((currentTime / (audioClip.length)) * audioWaveformRect.width));

            GUI.DrawTexture(audioWaveformRect, audioWaveform);
            EditorGUI.DrawRect(timeFrame, new Color(1f, 1f, 1f, 1));
            Rect block = new Rect(GetHierarchyWidth() - GetRect().width, 0, GetRect().width, position.height);
            EditorGUI.DrawRect(block, bgColor);
        }

        instantReplay = EditorGUILayout.Toggle("Instant Audio Playback", instantReplay);
        EditorGUILayout.BeginHorizontal();
        graphColor = EditorGUILayout.ColorField("Graph", graphColor, GUILayout.MaxWidth(200));
        if (GUILayout.Button("Update", GUILayout.MaxWidth(50)))
        {
            audioWaveform = AudioWaveform(audioClip, 16000, 100, graphColor);
        }
        EditorGUILayout.EndHorizontal();
        bgColor = EditorGUILayout.ColorField("Background", bgColor, GUILayout.MaxWidth(200));
        if (GUILayout.Button("Reset Colors", GUILayout.MaxWidth(100)))
        {
            graphColor = Color.red;
            bgColor = new Color(0.761f, 0.761f, 0.761f, 1);
            audioWaveform = AudioWaveform(audioClip, 16000, 100, graphColor);
        }
        EditorGUILayout.LabelField("Script by surfknasen.");

        if (lastBgColor != bgColor || lastGraphColor != graphColor) SaveColors();

        lastBgColor = bgColor;
        lastGraphColor = graphColor;

        DropAudioClip();

    }

    void SaveColors()
    {
        EditorPrefs.SetFloat("GraphColor_R", graphColor.r);
        EditorPrefs.SetFloat("GraphColor_G", graphColor.g);
        EditorPrefs.SetFloat("GraphColor_B", graphColor.b);
        EditorPrefs.SetFloat("GraphColor_A", graphColor.a);

        EditorPrefs.SetFloat("BGColor_R", bgColor.r);
        EditorPrefs.SetFloat("BGColor_G", bgColor.g);
        EditorPrefs.SetFloat("BGColor_B", bgColor.b);
        EditorPrefs.SetFloat("BGColor_A", bgColor.a);
    }

    public void DropAudioClip()
    {
        Event evt = Event.current;

        Rect dropArea = GUILayoutUtility.GetRect(0.0f, position.height, GUILayout.ExpandWidth(true));
        Rect labelRect = new Rect(position.width / 2.2f, position.height / 2.2f, 200, 50);
        if (audioClip == null) GUI.Label(labelRect, "Drag Audio Clip");
        try
        {
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            if (dragged_object.GetType() == typeof(AudioClip))
                            {
                                generatedAudioWaveform = false;
                                audioClip = (AudioClip)dragged_object;
                                EditorPrefs.SetString("clipPath", AssetDatabase.GetAssetPath(audioClip));
                                audioWaveform = AudioWaveform(audioClip, 16000, 100, graphColor);
                                generatedAudioWaveform = true;
                            }

                        }
                    }
                    break;
            }
        }
        catch
        {
            return;
        }

    }

    public static Texture2D AudioWaveform(AudioClip aud, int width, int height, Color color)
    {
        int step = Mathf.CeilToInt((aud.samples * aud.channels) / width); // 4
        float[] samples = new float[aud.samples * aud.channels];
		
		aud.GetData(samples, 0);

        Texture2D img = new Texture2D(width, height, TextureFormat.RGBA32, false);
 
        Color[] xy = new Color[width * height];
        for (int x = 0; x < width * height; x++)
        {
            xy[x] = new Color(0, 0, 0, 0);
        }
 
        img.SetPixels(xy);
 
        int i = 0;
        while (i < width)
        {
            int barHeight = Mathf.CeilToInt(Mathf.Clamp(Mathf.Abs(samples[i * step]) * height, 0, height));
            int add = samples[i * step] > 0 ? 1 : -1;
            for (int j = 0; j < barHeight; j++)
            {
                img.SetPixel(i, Mathf.FloorToInt(height / 2) - (Mathf.FloorToInt(barHeight / 2) * add) + (j * add), color);
            }
            ++i;
        }
		img.filterMode = FilterMode.Point;
        img.Apply();
        return img;
    }

	bool AnimWindowState (string property)
	{
		Assembly editorAssembly = typeof (Editor).Assembly;
		
		Type windowType = editorAssembly.GetType
			("UnityEditorInternal.AnimationWindowState");
		
		PropertyInfo isRecordingProp = windowType.GetProperty
			(property, BindingFlags.Instance |
			BindingFlags.Public);
		
		UnityEngine.Object [] windowInstances = Resources.FindObjectsOfTypeAll (windowType);
		
		for (int i = 0; i < windowInstances.Length; i++)
		{
			bool isRecording = (bool) isRecordingProp.GetValue
				(windowInstances [i], null);
		
			return isRecording;
		}
		
		return false;
	}

	float AnimWindowGetTime ()
	{
		Assembly editorAssembly = typeof (Editor).Assembly;
		
		Type windowType = editorAssembly.GetType
			("UnityEditorInternal.AnimationWindowState");
		
		PropertyInfo frameProp = windowType.GetProperty
			("currentTime", BindingFlags.Instance | BindingFlags.Public);
		
		UnityEngine.Object [] windowInstances = Resources.FindObjectsOfTypeAll (windowType);
		
		for (int i = 0; i < windowInstances.Length; i++)
		{
			float frame = (float)frameProp.GetValue
				(windowInstances [i], null);
		
			return frame;
		}
		
		return 0;
	}

	public Rect GetShownArea ()
	{
		Assembly editorAssembly = typeof (Editor).Assembly;
		
		Type windowType = editorAssembly.GetType
			("UnityEditor.AnimEditor"); // AnimEditor contentWidth
		
		MethodInfo contentWidthProp = windowType.GetMethod
			("get_dopeSheetEditor", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

		UnityEngine.Object [] windowInstances = Resources.FindObjectsOfTypeAll (windowType);
		
		for (int i = 0; i < windowInstances.Length; i++)
		{
			object obj = contentWidthProp.Invoke(windowInstances[i], null);
			Type type = editorAssembly.GetType
			("UnityEditorInternal.DopeSheetEditor");
			PropertyInfo dopeSheetProp = type.GetProperty
				("shownArea", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			
		//	print(dopeSheetProp.GetValue(obj, null));
			return (Rect)dopeSheetProp.GetValue(obj, null);
		}	
		return new Rect();
	} 

	public float GetHierarchyWidth()
	{
		Assembly editorAssembly = typeof (Editor).Assembly;
		
		Type windowType = editorAssembly.GetType
			("UnityEditor.AnimEditor"); // AnimEditor contentWidth
		
		MethodInfo hierarchyWidthProp = windowType.GetMethod
			("get_hierarchyWidth", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

		UnityEngine.Object [] windowInstances = Resources.FindObjectsOfTypeAll (windowType);
		
		for (int i = 0; i < windowInstances.Length; i++)
		{
			object obj = hierarchyWidthProp.Invoke(windowInstances[i], null);
			//print((float)obj);
			return (float)obj;
		}	
		return 0;
	}

	public float GetContentWidth()
	{
		Assembly editorAssembly = typeof (Editor).Assembly;
		
		Type windowType = editorAssembly.GetType
			("UnityEditor.AnimEditor"); // AnimEditor contentWidth
		
		MethodInfo contentWidthProp = windowType.GetMethod
			("get_contentWidth", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

		UnityEngine.Object [] windowInstances = Resources.FindObjectsOfTypeAll (windowType);
		
		for (int i = 0; i < windowInstances.Length; i++)
		{
			object obj = contentWidthProp.Invoke(windowInstances[i], null);
			//print((float)obj);
			return (float)obj;
		}	
		return 0;
	}

	public Vector2 GetTranslation ()
	{
		Assembly editorAssembly = typeof (Editor).Assembly;
		
		Type windowType = editorAssembly.GetType
			("UnityEditor.AnimEditor"); // AnimEditor contentWidth
		
		MethodInfo contentWidthProp = windowType.GetMethod
			("get_dopeSheetEditor", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

		UnityEngine.Object [] windowInstances = Resources.FindObjectsOfTypeAll (windowType);
		
		for (int i = 0; i < windowInstances.Length; i++)
		{
			object obj = contentWidthProp.Invoke(windowInstances[i], null);
			Type type = editorAssembly.GetType
			("UnityEditorInternal.DopeSheetEditor");
			PropertyInfo dopeSheetProp = type.GetProperty
				("translation", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			
			return (Vector2)dopeSheetProp.GetValue(obj, null);
		}	
		return Vector2.zero;
	} 

	public Rect GetRect ()
	{
		Assembly editorAssembly = typeof (Editor).Assembly;
		
		Type windowType = editorAssembly.GetType
			("UnityEditor.AnimEditor"); // AnimEditor contentWidth
		
		MethodInfo contentWidthProp = windowType.GetMethod
			("get_dopeSheetEditor", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

		UnityEngine.Object [] windowInstances = Resources.FindObjectsOfTypeAll (windowType);
		
		for (int i = 0; i < windowInstances.Length; i++)
		{
			object obj = contentWidthProp.Invoke(windowInstances[i], null);
			Type type = editorAssembly.GetType
			("UnityEditorInternal.DopeSheetEditor");
			PropertyInfo dopeSheetProp = type.GetProperty
				("rect", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			
			return (Rect)dopeSheetProp.GetValue(obj, null);
		//	Debug.Log(dopeSheetProp.GetValue(obj, null));
		}	
		return new Rect();
	} 


	
}
#endif