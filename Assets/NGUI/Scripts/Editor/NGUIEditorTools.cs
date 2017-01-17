//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2016 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Tools for the editor
/// </summary>

public static class NGUIEditorTools
{
	static Texture2D mBackdropTex;
	static Texture2D mContrastTex;
	static Texture2D mGradientTex;
	static GameObject mPrevious;

	/// <summary>
	/// Returns a blank usable 1x1 white texture.
	/// </summary>

	static public Texture2D blankTexture
	{
		get
		{
			return EditorGUIUtility.whiteTexture;
		}
	}

	/// <summary>
	/// Returns a usable texture that looks like a dark checker board.
	/// </summary>

	static public Texture2D backdropTexture
	{
		get
		{
			if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
				new Color(0.1f, 0.1f, 0.1f, 0.5f),
				new Color(0.2f, 0.2f, 0.2f, 0.5f));
			return mBackdropTex;
		}
	}

	/// <summary>
	/// Returns a usable texture that looks like a high-contrast checker board.
	/// </summary>

	static public Texture2D contrastTexture
	{
		get
		{
			if (mContrastTex == null) mContrastTex = CreateCheckerTex(
				new Color(0f, 0.0f, 0f, 0.5f),
				new Color(1f, 1f, 1f, 0.5f));
			return mContrastTex;
		}
	}

	/// <summary>
	/// Gradient texture is used for title bars / headers.
	/// </summary>

	static public Texture2D gradientTexture
	{
		get
		{
			if (mGradientTex == null) mGradientTex = CreateGradientTex();
			return mGradientTex;
		}
	}

	/// <summary>
	/// Create a white dummy texture.
	/// </summary>

	static Texture2D CreateDummyTex ()
	{
		Texture2D tex = new Texture2D(1, 1);
		tex.name = "[Generated] Dummy Texture";
		tex.hideFlags = HideFlags.DontSave;
		tex.filterMode = FilterMode.Point;
		tex.SetPixel(0, 0, Color.white);
		tex.Apply();
		return tex;
	}

	/// <summary>
	/// Create a checker-background texture
	/// </summary>

	static Texture2D CreateCheckerTex (Color c0, Color c1)
	{
		Texture2D tex = new Texture2D(16, 16);
		tex.name = "[Generated] Checker Texture";
		tex.hideFlags = HideFlags.DontSave;

		for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
		for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
		for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
		for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

		tex.Apply();
		tex.filterMode = FilterMode.Point;
		return tex;
	}

	/// <summary>
	/// Create a gradient texture
	/// </summary>

	static Texture2D CreateGradientTex ()
	{
		Texture2D tex = new Texture2D(1, 16);
		tex.name = "[Generated] Gradient Texture";
		tex.hideFlags = HideFlags.DontSave;

		Color c0 = new Color(1f, 1f, 1f, 0f);
		Color c1 = new Color(1f, 1f, 1f, 0.4f);

		for (int i = 0; i < 16; ++i)
		{
			float f = Mathf.Abs((i / 15f) * 2f - 1f);
			f *= f;
			tex.SetPixel(0, i, Color.Lerp(c0, c1, f));
		}

		tex.Apply();
		tex.filterMode = FilterMode.Bilinear;
		return tex;
	}

	/// <summary>
	/// Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
	/// </summary>

	static public void DrawTiledTexture (Rect rect, Texture tex)
	{
		GUI.BeginGroup(rect);
		{
			int width  = Mathf.RoundToInt(rect.width);
			int height = Mathf.RoundToInt(rect.height);

			for (int y = 0; y < height; y += tex.height)
			{
				for (int x = 0; x < width; x += tex.width)
				{
					GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
				}
			}
		}
		GUI.EndGroup();
	}

	/// <summary>
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = contrastTexture;
			GUI.color = Color.white;
			DrawTiledTexture(new Rect(rect.xMin, rect.yMax, 1f, -rect.height), tex);
			DrawTiledTexture(new Rect(rect.xMax, rect.yMax, 1f, -rect.height), tex);
			DrawTiledTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			DrawTiledTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
		}
	}

	/// <summary>
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			// Calculate where the outer rectangle would be
			float x = rect.xMin + rect.width * relative.xMin;
			float y = rect.yMax - rect.height * relative.yMin;
			float width = rect.width * relative.width;
			float height = -rect.height * relative.height;
			relative = new Rect(x, y, width, height);

			// Draw the selection
			DrawOutline(relative, color);
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative)
	{
		if (Event.current.type == EventType.Repaint)
		{
			// Calculate where the outer rectangle would be
			float x = rect.xMin + rect.width * relative.xMin;
			float y = rect.yMax - rect.height * relative.yMin;
			float width = rect.width * relative.width;
			float height = -rect.height * relative.height;
			relative = new Rect(x, y, width, height);

			// Draw the selection
			DrawOutline(relative);
		}
	}

	/// <summary>
	/// Draw a 9-sliced outline.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect outer, Rect inner)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Color green = new Color(0.4f, 1f, 0f, 1f);

			DrawOutline(rect, new Rect(outer.x, inner.y, outer.width, inner.height));
			DrawOutline(rect, new Rect(inner.x, outer.y, inner.width, outer.height));
			DrawOutline(rect, outer, green);
		}
	}

	/// <summary>
	/// Draw a checkered background for the specified texture.
	/// </summary>

	static public Rect DrawBackground (Texture2D tex, float ratio)
	{
		Rect rect = GUILayoutUtility.GetRect(0f, 0f);
		rect.width = Screen.width - rect.xMin;
		rect.height = rect.width * ratio;
		GUILayout.Space(rect.height);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D blank = blankTexture;
			Texture2D check = backdropTexture;

			// Lines above and below the texture rectangle
			GUI.color = new Color(0f, 0f, 0f, 0.2f);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin - 1, rect.width, 1f), blank);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), blank);
			GUI.color = Color.white;

			// Checker background
			DrawTiledTexture(rect, check);
		}
		return rect;
	}

	/// <summary>
	/// Draw a visible separator in addition to adding some padding.
	/// </summary>

	static public void DrawSeparator ()
	{
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Convenience function that displays a list of sprites and returns the selected value.
	/// </summary>

	static public string DrawList (string field, string[] list, string selection, params GUILayoutOption[] options)
	{
		if (list != null && list.Length > 0)
		{
			int index = 0;
			if (string.IsNullOrEmpty(selection)) selection = list[0];

			// We need to find the sprite in order to have it selected
			if (!string.IsNullOrEmpty(selection))
			{
				for (int i = 0; i < list.Length; ++i)
				{
					if (selection.Equals(list[i], System.StringComparison.OrdinalIgnoreCase))
					{
						index = i;
						break;
					}
				}
			}

			// Draw the sprite selection popup
			index = string.IsNullOrEmpty(field) ?
				EditorGUILayout.Popup(index, list, options) :
				EditorGUILayout.Popup(field, index, list, options);

			return list[index];
		}
		return null;
	}

	/// <summary>
	/// Convenience function that displays a list of sprites and returns the selected value.
	/// </summary>

	static public string DrawAdvancedList (string field, string[] list, string selection, params GUILayoutOption[] options)
	{
		if (list != null && list.Length > 0)
		{
			int index = 0;
			if (string.IsNullOrEmpty(selection)) selection = list[0];

			// We need to find the sprite in order to have it selected
			if (!string.IsNullOrEmpty(selection))
			{
				for (int i = 0; i < list.Length; ++i)
				{
					if (selection.Equals(list[i], System.StringComparison.OrdinalIgnoreCase))
					{
						index = i;
						break;
					}
				}
			}

			// Draw the sprite selection popup
			index = string.IsNullOrEmpty(field) ?
				DrawPrefixList(index, list, options) :
				DrawPrefixList(field, index, list, options);

			return list[index];
		}
		return null;
	}

	/// <summary>
	/// Helper function that checks to see if this action would break the prefab connection.
	/// </summary>

	static public bool WillLosePrefab (GameObject root)
	{
		if (root == null) return false;

		if (root.transform != null)
		{
			// Check if the selected object is a prefab instance and display a warning
			PrefabType type = PrefabUtility.GetPrefabType(root);

			if (type == PrefabType.PrefabInstance)
			{
				return EditorUtility.DisplayDialog("Losing prefab",
					"This action will lose the prefab connection. Are you sure you wish to continue?",
					"Continue", "Cancel");
			}
		}
		return true;
	}

	/// <summary>
	/// Change the import settings of the specified texture asset, making it readable.
	/// </summary>

	static public bool MakeTextureReadable (string path, bool force)
	{
		if (string.IsNullOrEmpty(path)) return false;
		TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
		if (ti == null) return false;

		TextureImporterSettings settings = new TextureImporterSettings();
		ti.ReadTextureSettings(settings);

		if (force || !settings.readable || settings.npotScale != TextureImporterNPOTScale.None || settings.alphaIsTransparency)
		{
			settings.readable = true;
			if (NGUISettings.trueColorAtlas) settings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			settings.npotScale = TextureImporterNPOTScale.None;
			settings.alphaIsTransparency = false;
			ti.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
		}
		return true;
	}

	/// <summary>
	/// Change the import settings of the specified texture asset, making it suitable to be used as a texture atlas.
	/// </summary>

	static bool MakeTextureAnAtlas (string path, bool force, bool alphaTransparency)
	{
		if (string.IsNullOrEmpty(path)) return false;
		TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
		if (ti == null) return false;

		TextureImporterSettings settings = new TextureImporterSettings();
		ti.ReadTextureSettings(settings);

		if (force ||
			settings.readable ||
			settings.maxTextureSize < 4096 ||
			settings.wrapMode != TextureWrapMode.Clamp ||
			settings.npotScale != TextureImporterNPOTScale.ToNearest)
		{
			settings.readable = false;
			settings.maxTextureSize = 4096;
			settings.wrapMode = TextureWrapMode.Clamp;
			settings.npotScale = TextureImporterNPOTScale.ToNearest;

			if (NGUISettings.trueColorAtlas)
			{
				settings.textureFormat = TextureImporterFormat.ARGB32;
				settings.filterMode = FilterMode.Trilinear;
			}

			settings.aniso = 4;
			settings.alphaIsTransparency = alphaTransparency;
			ti.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
		}
		return true;
	}

	/// <summary>
	/// Fix the import settings for the specified texture, re-importing it if necessary.
	/// </summary>

	static public Texture2D ImportTexture (string path, bool forInput, bool force, bool alphaTransparency)
	{
		if (!string.IsNullOrEmpty(path))
		{
			if (forInput) { if (!MakeTextureReadable(path, force)) return null; }
			else if (!MakeTextureAnAtlas(path, force, alphaTransparency)) return null;
			//return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;

			Texture2D tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			return tex;
		}
		return null;
	}

	/// <summary>
	/// Fix the import settings for the specified texture, re-importing it if necessary.
	/// </summary>

	static public Texture2D ImportTexture (Texture tex, bool forInput, bool force, bool alphaTransparency)
	{
		if (tex != null)
		{
			string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
			return ImportTexture(path, forInput, force, alphaTransparency);
		}
		return null;
	}

	/// <summary>
	/// Helper function that returns the folder where the current selection resides.
	/// </summary>

	static public string GetSelectionFolder ()
	{
		if (Selection.activeObject != null)
		{
			string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());

			if (!string.IsNullOrEmpty(path))
			{
				int dot = path.LastIndexOf('.');
				int slash = Mathf.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
				if (slash > 0) return (dot > slash) ? path.Substring(0, slash + 1) : path + "/";
			}
		}
		return "Assets/";
	}

	/// <summary>
	/// Struct type for the integer vector field below.
	/// </summary>

	public struct IntVector
	{
		public int x;
		public int y;
	}

	/// <summary>
	/// Integer vector field.
	/// </summary>

	static public IntVector IntPair (string prefix, string leftCaption, string rightCaption, int x, int y)
	{
		GUILayout.BeginHorizontal();

		if (string.IsNullOrEmpty(prefix))
		{
			GUILayout.Space(82f);
		}
		else
		{
			GUILayout.Label(prefix, GUILayout.Width(74f));
		}

		NGUIEditorTools.SetLabelWidth(48f);

		IntVector retVal;
		retVal.x = EditorGUILayout.IntField(leftCaption, x, GUILayout.MinWidth(30f));
		retVal.y = EditorGUILayout.IntField(rightCaption, y, GUILayout.MinWidth(30f));

		NGUIEditorTools.SetLabelWidth(80f);

		GUILayout.EndHorizontal();
		return retVal;
	}

	/// <summary>
	/// Integer rectangle field.
	/// </summary>

	static public Rect IntRect (string prefix, Rect rect)
	{
		int left	= Mathf.RoundToInt(rect.xMin);
		int top		= Mathf.RoundToInt(rect.yMin);
		int width	= Mathf.RoundToInt(rect.width);
		int height	= Mathf.RoundToInt(rect.height);

		NGUIEditorTools.IntVector a = NGUIEditorTools.IntPair(prefix, "Left", "Top", left, top);
		NGUIEditorTools.IntVector b = NGUIEditorTools.IntPair(null, "Width", "Height", width, height);

		return new Rect(a.x, a.y, b.x, b.y);
	}

	/// <summary>
	/// Integer vector field.
	/// </summary>

	static public Vector4 IntPadding (string prefix, Vector4 v)
	{
		int left	= Mathf.RoundToInt(v.x);
		int top		= Mathf.RoundToInt(v.y);
		int right	= Mathf.RoundToInt(v.z);
		int bottom	= Mathf.RoundToInt(v.w);

		NGUIEditorTools.IntVector a = NGUIEditorTools.IntPair(prefix, "Left", "Top", left, top);
		NGUIEditorTools.IntVector b = NGUIEditorTools.IntPair(null, "Right", "Bottom", right, bottom);

		return new Vector4(a.x, a.y, b.x, b.y);
	}

	/// <summary>
	/// Find all scene components, active or inactive.
	/// </summary>

	static public List<T> FindAll<T> () where T : Component
	{
		T[] comps = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];

		List<T> list = new List<T>();

		foreach (T comp in comps)
		{
			if (comp.gameObject.hideFlags == 0)
			{
				string path = AssetDatabase.GetAssetPath(comp.gameObject);
				if (string.IsNullOrEmpty(path)) list.Add(comp);
			}
		}
		return list;
	}

	static public bool DrawPrefixButton (string text)
	{
		return GUILayout.Button(text, "DropDown", GUILayout.Width(76f));
	}

	static public bool DrawPrefixButton (string text, params GUILayoutOption[] options)
	{
		return GUILayout.Button(text, "DropDown", options);
	}

	static public int DrawPrefixList (int index, string[] list, params GUILayoutOption[] options)
	{
		return EditorGUILayout.Popup(index, list, "DropDown", options);
	}

	static public int DrawPrefixList (string text, int index, string[] list, params GUILayoutOption[] options)
	{
		return EditorGUILayout.Popup(text, index, list, "DropDown", options);
	}

	/// <summary>
	/// Draw the specified sprite.
	/// </summary>

	public static void DrawTexture (Texture2D tex, Rect rect, Rect uv, Color color)
	{
		DrawTexture(tex, rect, uv, color, null);
	}

	/// <summary>
	/// Draw the specified sprite.
	/// </summary>

	public static void DrawTexture (Texture2D tex, Rect rect, Rect uv, Color color, Material mat)
	{
		int w = Mathf.RoundToInt(tex.width * uv.width);
		int h = Mathf.RoundToInt(tex.height * uv.height);

		// Create the texture rectangle that is centered inside rect.
		Rect outerRect = rect;
		outerRect.width = w;
		outerRect.height = h;

		if (outerRect.width > 0f)
		{
			float f = rect.width / outerRect.width;
			outerRect.width *= f;
			outerRect.height *= f;
		}

		if (rect.height > outerRect.height)
		{
			outerRect.y += (rect.height - outerRect.height) * 0.5f;
		}
		else if (outerRect.height > rect.height)
		{
			float f = rect.height / outerRect.height;
			outerRect.width *= f;
			outerRect.height *= f;
		}

		if (rect.width > outerRect.width) outerRect.x += (rect.width - outerRect.width) * 0.5f;

		// Draw the background
		NGUIEditorTools.DrawTiledTexture(outerRect, NGUIEditorTools.backdropTexture);

		// Draw the sprite
		GUI.color = color;
		
		if (mat == null)
		{
			GUI.DrawTextureWithTexCoords(outerRect, tex, uv, true);
		}
		else
		{
			// NOTE: There is an issue in Unity that prevents it from clipping the drawn preview
			// using BeginGroup/EndGroup, and there is no way to specify a UV rect... le'suq.
			UnityEditor.EditorGUI.DrawPreviewTexture(outerRect, tex, mat);
		}
		GUI.color = Color.white;

		// Draw the lines around the sprite
		Handles.color = Color.black;
		Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMin, outerRect.yMax));
		Handles.DrawLine(new Vector3(outerRect.xMax, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMax));
		Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMin));
		Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMax), new Vector3(outerRect.xMax, outerRect.yMax));

		// Sprite size label
		string text = string.Format("Texture Size: {0}x{1}", w, h);
		EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, 18f), text);
	}

	static string mEditedName = null;
	static string mLastSprite = null;

	/// <summary>
	/// Select the specified game object and remember what was selected before.
	/// </summary>

	static public void Select (GameObject go)
	{
		mPrevious = Selection.activeGameObject;
		Selection.activeGameObject = go;
	}
	
	/// <summary>
	/// Select the previous game object.
	/// </summary>

	static public void SelectPrevious ()
	{
		if (mPrevious != null)
		{
			Selection.activeGameObject = mPrevious;
			mPrevious = null;
		}
	}

	/// <summary>
	/// Previously selected game object.
	/// </summary>

	static public GameObject previousSelection { get { return mPrevious; } }

	/// <summary>
	/// Helper function that checks to see if the scale is uniform.
	/// </summary>

	static public bool IsUniform (Vector3 scale)
	{
		return Mathf.Approximately(scale.x, scale.y) && Mathf.Approximately(scale.x, scale.z);
	}

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawMinimalisticHeader (string text) { return DrawHeader(text, text, false, true); }

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text) { return DrawHeader(text, text, false, NGUISettings.minimalisticLook); }

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text, string key) { return DrawHeader(text, key, false, NGUISettings.minimalisticLook); }

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text, bool detailed) { return DrawHeader(text, text, detailed, !detailed); }

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text, string key, bool forceOn, bool minimalistic)
	{
		bool state = EditorPrefs.GetBool(key, true);

		if (!minimalistic) GUILayout.Space(3f);
		if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
		GUILayout.BeginHorizontal();
		GUI.changed = false;

		if (minimalistic)
		{
			if (state) text = "\u25BC" + (char)0x200a + text;
			else text = "\u25BA" + (char)0x200a + text;

			GUILayout.BeginHorizontal();
			GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
			if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
			GUI.contentColor = Color.white;
			GUILayout.EndHorizontal();
		}
		else
		{
			text = "<b><size=11>" + text + "</size></b>";
			if (state) text = "\u25BC " + text;
			else text = "\u25BA " + text;
			if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
		}

		if (GUI.changed) EditorPrefs.SetBool(key, state);

		if (!minimalistic) GUILayout.Space(2f);
		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		if (!forceOn && !state) GUILayout.Space(3f);
		return state;
	}

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents () { BeginContents(NGUISettings.minimalisticLook); }

	static bool mEndHorizontal = false;

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents (bool minimalistic)
	{
		if (!minimalistic)
		{
			mEndHorizontal = true;
			GUILayout.BeginHorizontal();
			EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
		}
		else
		{
			mEndHorizontal = false;
			EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
			GUILayout.Space(10f);
		}
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}

	/// <summary>
	/// End drawing the content area.
	/// </summary>

	static public void EndContents ()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		if (mEndHorizontal)
		{
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();
		}

		GUILayout.Space(3f);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty (this SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(null, serializedObject, property, false, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty (this SerializedObject serializedObject, string property, string label, params GUILayoutOption[] options)
	{
		return DrawProperty(label, serializedObject, property, false, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty (string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(label, serializedObject, property, false, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawPaddedProperty (this SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(null, serializedObject, property, true, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawPaddedProperty (string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(label, serializedObject, property, true, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty (string label, SerializedObject serializedObject, string property, bool padding, params GUILayoutOption[] options)
	{
		SerializedProperty sp = serializedObject.FindProperty(property);

		if (sp != null)
		{
			if (NGUISettings.minimalisticLook) padding = false;

			if (padding) EditorGUILayout.BeginHorizontal();

			if (sp.isArray && sp.type != "string") DrawArray(serializedObject, property, label ?? property);
			else if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
			else EditorGUILayout.PropertyField(sp, options);

			if (padding)
			{
				NGUIEditorTools.DrawPadding();
				EditorGUILayout.EndHorizontal();
			}
		}
		else Debug.LogWarning("Unable to find property " + property);
		return sp;
	}

	/// <summary>
	/// Helper function that draws an array property.
	/// </summary>

	static public void DrawArray (this SerializedObject obj, string property, string title)
	{
		SerializedProperty sp = obj.FindProperty(property + ".Array.size");

		if (sp != null && NGUIEditorTools.DrawHeader(title))
		{
			NGUIEditorTools.BeginContents();
			int size = sp.intValue;
			int newSize = EditorGUILayout.IntField("Size", size);
			if (newSize != size) obj.FindProperty(property + ".Array.size").intValue = newSize;

			EditorGUI.indentLevel = 1;

			for (int i = 0; i < newSize; i++)
			{
				SerializedProperty p = obj.FindProperty(string.Format("{0}.Array.data[{1}]", property, i));
				if (p != null) EditorGUILayout.PropertyField(p);
			}
			EditorGUI.indentLevel = 0;
			NGUIEditorTools.EndContents();
		}
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public void DrawProperty (string label, SerializedProperty sp, params GUILayoutOption[] options)
	{
		DrawProperty(label, sp, true, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public void DrawProperty (string label, SerializedProperty sp, bool padding, params GUILayoutOption[] options)
	{
		if (sp != null)
		{
			if (padding) EditorGUILayout.BeginHorizontal();

			if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
			else EditorGUILayout.PropertyField(sp, options);

			if (padding)
			{
				NGUIEditorTools.DrawPadding();
				EditorGUILayout.EndHorizontal();
			}
		}
	}

	/// <summary>
	/// Helper function that draws a compact Vector4.
	/// </summary>

	static public void DrawBorderProperty (string name, SerializedObject serializedObject, string field)
	{
		if (serializedObject.FindProperty(field) != null)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(name, GUILayout.Width(75f));

				NGUIEditorTools.SetLabelWidth(50f);
				GUILayout.BeginVertical();
				NGUIEditorTools.DrawProperty("Left", serializedObject, field + ".x", GUILayout.MinWidth(80f));
				NGUIEditorTools.DrawProperty("Bottom", serializedObject, field + ".y", GUILayout.MinWidth(80f));
				GUILayout.EndVertical();

				GUILayout.BeginVertical();
				NGUIEditorTools.DrawProperty("Right", serializedObject, field + ".z", GUILayout.MinWidth(80f));
				NGUIEditorTools.DrawProperty("Top", serializedObject, field + ".w", GUILayout.MinWidth(80f));
				GUILayout.EndVertical();

				NGUIEditorTools.SetLabelWidth(80f);
			}
			GUILayout.EndHorizontal();
		}
	}

	/// <summary>
	/// Helper function that draws a compact Rect.
	/// </summary>

	static public void DrawRectProperty (string name, SerializedObject serializedObject, string field)
	{
		DrawRectProperty(name, serializedObject, field, 56f, 18f);
	}

	/// <summary>
	/// Helper function that draws a compact Rect.
	/// </summary>

	static public void DrawRectProperty (string name, SerializedObject serializedObject, string field, float labelWidth, float spacing)
	{
		if (serializedObject.FindProperty(field) != null)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(name, GUILayout.Width(labelWidth));

				NGUIEditorTools.SetLabelWidth(20f);
				GUILayout.BeginVertical();
				NGUIEditorTools.DrawProperty("X", serializedObject, field + ".x", GUILayout.MinWidth(50f));
				NGUIEditorTools.DrawProperty("Y", serializedObject, field + ".y", GUILayout.MinWidth(50f));
				GUILayout.EndVertical();

				NGUIEditorTools.SetLabelWidth(50f);
				GUILayout.BeginVertical();
				NGUIEditorTools.DrawProperty("Width", serializedObject, field + ".width", GUILayout.MinWidth(80f));
				NGUIEditorTools.DrawProperty("Height", serializedObject, field + ".height", GUILayout.MinWidth(80f));
				GUILayout.EndVertical();

				NGUIEditorTools.SetLabelWidth(80f);
				if (spacing != 0f) GUILayout.Space(spacing);
			}
			GUILayout.EndHorizontal();
		}
	}

	/// <summary>
	/// Unity 4.3 changed the way LookLikeControls works.
	/// </summary>

	static public void SetLabelWidth (float width)
	{
		EditorGUIUtility.labelWidth = width;
	}

	/// <summary>
	/// Create an undo point for the specified objects.
	/// </summary>

	static public void RegisterUndo (string name, params Object[] objects)
	{
		if (objects != null && objects.Length > 0)
		{
			UnityEditor.Undo.RecordObjects(objects, name);

			foreach (Object obj in objects)
			{
				if (obj == null) continue;
				EditorUtility.SetDirty(obj);
			}
		}
	}

	/// <summary>
	/// Convenience function that replaces the specified MonoBehaviour with one of specified class ID.
	/// </summary>

	static public SerializedObject ReplaceClass (MonoBehaviour mb, int classID)
	{
		SerializedObject ob = new SerializedObject(mb);
		ob.Update();
		ob.FindProperty("m_Script").objectReferenceInstanceIDValue = classID;
		ob.ApplyModifiedProperties();
		ob.Update();
		return ob;
	}

	/// <summary>
	/// Convenience function that replaces the specified MonoBehaviour with one of specified class ID.
	/// </summary>

	static public void ReplaceClass (SerializedObject ob, int classID)
	{
		ob.FindProperty("m_Script").objectReferenceInstanceIDValue = classID;
		ob.ApplyModifiedProperties();
		ob.Update();
	}

	class MenuEntry
	{
		public string name;
		public GameObject go;
		public MenuEntry (string name, GameObject go) { this.name = name; this.go = go; }
	}

	/// <summary>
	/// Load the asset at the specified path.
	/// </summary>

	static public Object LoadAsset (string path)
	{
		if (string.IsNullOrEmpty(path)) return null;
		return AssetDatabase.LoadMainAssetAtPath(path);
	}

	/// <summary>
	/// Convenience function to load an asset of specified type, given the full path to it.
	/// </summary>

	static public T LoadAsset<T> (string path) where T: Object
	{
		Object obj = LoadAsset(path);
		if (obj == null) return null;

		T val = obj as T;
		if (val != null) return val;

		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			if (obj.GetType() == typeof(GameObject))
			{
				GameObject go = obj as GameObject;
				return go.GetComponent(typeof(T)) as T;
			}
		}
		return null;
	}

	/// <summary>
	/// Get the specified object's GUID.
	/// </summary>

	static public string ObjectToGUID (Object obj)
	{
		string path = AssetDatabase.GetAssetPath(obj);
		return (!string.IsNullOrEmpty(path)) ? AssetDatabase.AssetPathToGUID(path) : null;
	}

	static MethodInfo s_GetInstanceIDFromGUID;

	/// <summary>
	/// Convert the specified GUID to an object reference.
	/// </summary>

	static public Object GUIDToObject (string guid)
	{
		if (string.IsNullOrEmpty(guid)) return null;
		
		if (s_GetInstanceIDFromGUID == null)
			s_GetInstanceIDFromGUID = typeof(AssetDatabase).GetMethod("GetInstanceIDFromGUID", BindingFlags.Static | BindingFlags.NonPublic);

		int id = (int)s_GetInstanceIDFromGUID.Invoke(null, new object[] { guid });
		if (id != 0) return EditorUtility.InstanceIDToObject(id);
		string path = AssetDatabase.GUIDToAssetPath(guid);
		if (string.IsNullOrEmpty(path)) return null;
		return AssetDatabase.LoadAssetAtPath(path, typeof(Object));
	}

	/// <summary>
	/// Convert the specified GUID to an object reference of specified type.
	/// </summary>

	static public T GUIDToObject<T> (string guid) where T : Object
	{
		Object obj = GUIDToObject(guid);
		if (obj == null) return null;

		System.Type objType = obj.GetType();
		if (objType == typeof(T) || objType.IsSubclassOf(typeof(T))) return obj as T;

		if (objType == typeof(GameObject) && typeof(T).IsSubclassOf(typeof(Component)))
		{
			GameObject go = obj as GameObject;
			return go.GetComponent(typeof(T)) as T;
		}
		return null;
	}

	/// <summary>
	/// Add a soft shadow to the specified color buffer.
	/// The buffer must have some padding around the edges in order for this to work properly.
	/// </summary>

	static public void AddShadow (Color32[] colors, int width, int height, Color shadow)
	{
		Color sh = shadow;
		sh.a = 1f;

		for (int y2 = 0; y2 < height; ++y2)
		{
			for (int x2 = 0; x2 < width; ++x2)
			{
				int index = x2 + y2 * width;
				Color32 uc = colors[index];
				if (uc.a == 255) continue;

				Color original = uc;
				float val = original.a;
				int count = 1;
				float div1 = 1f / 255f;
				float div2 = 2f / 255f;
				float div3 = 3f / 255f;

				// Left
				if (x2 != 0)
				{
					val += colors[x2 - 1 + y2 * width].a * div1;
					count += 1;
				}

				// Top
				if (y2 + 1 != height)
				{
					val += colors[x2 + (y2 + 1) * width].a * div2;
					count += 2;
				}

				// Top-left
				if (x2 != 0 && y2 + 1 != height)
				{
					val += colors[x2 - 1 + (y2 + 1) * width].a * div3;
					count += 3;
				}

				val /= count;

				Color c = Color.Lerp(original, sh, shadow.a * val);
				colors[index] = Color.Lerp(c, original, original.a);
			}
		}
	}

	/// <summary>
	/// Add a visual depth effect to the specified color buffer.
	/// The buffer must have some padding around the edges in order for this to work properly.
	/// </summary>

	static public void AddDepth (Color32[] colors, int width, int height, Color shadow)
	{
		Color sh = shadow;
		sh.a = 1f;

		for (int y2 = 0; y2 < height; ++y2)
		{
			for (int x2 = 0; x2 < width; ++x2)
			{
				int index = x2 + y2 * width;
				Color32 uc = colors[index];
				if (uc.a == 255) continue;

				Color original = uc;
				float val = original.a * 4f;
				int count = 4;
				float div1 = 1f / 255f;
				float div2 = 2f / 255f;

				if (x2 != 0)
				{
					val += colors[x2 - 1 + y2 * width].a * div2;
					count += 2;
				}

				if (x2 + 1 != width)
				{
					val += colors[x2 + 1 + y2 * width].a * div2;
					count += 2;
				}

				if (y2 != 0)
				{
					val += colors[x2 + (y2 - 1) * width].a * div2;
					count += 2;
				}

				if (y2 + 1 != height)
				{
					val += colors[x2 + (y2 + 1) * width].a * div2;
					count += 2;
				}

				if (x2 != 0 && y2 != 0)
				{
					val += colors[x2 - 1 + (y2 - 1) * width].a * div1;
					++count;
				}

				if (x2 != 0 && y2 + 1 != height)
				{
					val += colors[x2 - 1 + (y2 + 1) * width].a * div1;
					++count;
				}

				if (x2 + 1 != width && y2 != 0)
				{
					val += colors[x2 + 1 + (y2 - 1) * width].a * div1;
					++count;
				}

				if (x2 + 1 != width && y2 + 1 != height)
				{
					val += colors[x2 + 1 + (y2 + 1) * width].a * div1;
					++count;
				}

				val /= count;

				Color c = Color.Lerp(original, sh, shadow.a * val);
				colors[index] = Color.Lerp(c, original, original.a);
			}
		}
	}

	/// <summary>
	/// Draw 18 pixel padding on the right-hand side. Used to align fields.
	/// </summary>

	static public void DrawPadding ()
	{
		if (!NGUISettings.minimalisticLook)
			GUILayout.Space(18f);
	}
}
