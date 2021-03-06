﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingUtil  {



    public static void DrawText(GUISkin guiSkin, string text, Vector3 position, Color? color = null, int fontSize = 0, float yOffset = 0)
    {
#if UNITY_EDITOR
        var prevSkin = GUI.skin;
        if (guiSkin == null)
            Debug.LogWarning("editor warning: guiSkin parameter is null");
        else
            GUI.skin = guiSkin;

        GUIContent textContent = new GUIContent(text);

        GUIStyle style = (guiSkin != null) ? new GUIStyle(guiSkin.GetStyle("Label")) : new GUIStyle();
        if (color != null)
            style.normal.textColor = (Color)color;
        if (fontSize > 0)
            style.fontSize = fontSize;

        Vector2 textSize = style.CalcSize(textContent);
        Vector3 screenPoint = Camera.current.WorldToScreenPoint(position);

        if (screenPoint.z > 0) // checks necessary to the text is not visible when the camera is pointed in the opposite direction relative to the object
        {
            var worldPosition = Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f + yOffset, screenPoint.z));
            UnityEditor.Handles.Label(worldPosition, textContent, style);
        }
        GUI.skin = prevSkin;
#endif
    }


    public static void DrawText(string text, Vector3 position, Color? color = null, int fontSize = 0, float yOffset = 0)
    {
		GUISkin guiSkin = GUI.skin;
#if UNITY_EDITOR
        var prevSkin = GUI.skin;
        if (guiSkin == null)
            Debug.LogWarning("editor warning: guiSkin parameter is null");
        else
            GUI.skin = guiSkin;

        GUIContent textContent = new GUIContent(text);

        GUIStyle style = (guiSkin != null) ? new GUIStyle(guiSkin.GetStyle("Label")) : new GUIStyle();
        if (color != null)
            style.normal.textColor = (Color)color;
        if (fontSize > 0)
            style.fontSize = fontSize;

        Vector2 textSize = style.CalcSize(textContent);
        Vector3 screenPoint = Camera.current.WorldToScreenPoint(position);

        if (screenPoint.z > 0) // checks necessary to the text is not visible when the camera is pointed in the opposite direction relative to the object
        {
            var worldPosition = Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f + yOffset, screenPoint.z));
            UnityEditor.Handles.Label(worldPosition, textContent, style);
        }
        GUI.skin = prevSkin;
#endif
    }

    public static void DrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3,  Color color)
    {
        #if UNITY_EDITOR
            Color temp = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawLine(v1, v2 );
			Gizmos.DrawLine(v2, v3 );
			Gizmos.DrawLine(v3, v1 );
            Gizmos.color = temp;
        #endif
    }

    public static void DrawTriangle(Triangle tri,  Color color)
    {
        #if UNITY_EDITOR
            Color temp = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawLine(tri.Vertex1, tri.Vertex2 );
			Gizmos.DrawLine(tri.Vertex2, tri.Vertex3 );
			Gizmos.DrawLine(tri.Vertex1, tri.Vertex3 );
            Gizmos.color = temp;
        #endif
    }

    public static void DrawBoundingBox(Bounds bounds, Color col) 
    {
        Vector3 upperLeft = bounds.center + bounds.extents;
        //bounds.extents
        Debug.DrawLine( bounds.center, upperLeft, col);
        //Debug.DrawLine(leftUpper, new Vector3( rightLower.x - leftUpper.x , rightLower.y, rightLower.));
        /*Debug.DrawLine(bounds.max, bounds.min, col);
        Debug.DrawLine(bounds.max, bounds.min, col);
        Debug.DrawLine(bounds.max, bounds.min, col);
        Debug.DrawLine(bounds.max, bounds.min, col);
        Debug.DrawLine(bounds.max, bounds.min, col);
        Debug.DrawLine(bounds.max, bounds.min, col);*/
    }

	public static void DrawGizmoBox(Bounds bounds, Color color) {
		//Vector3 vCenter = bounds.center;
		//Vector3 vExtents = bounds.extents;
		Color prevColor = Gizmos.color;
		Gizmos.color = color;
		Gizmos.DrawWireCube( bounds.center, bounds.extents);
		Gizmos.color = prevColor;
	}


}
