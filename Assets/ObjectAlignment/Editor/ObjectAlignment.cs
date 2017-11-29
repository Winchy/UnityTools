using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ObjectAlignment : EditorWindow {

	bool alignX = true;
	bool alignY = true;
	bool alignZ = true;

	enum AlignAction
	{
		None,
		ToMinimum,
		ToMaximum,
		ToMiddle,
		Distribute
	}

    enum SortOption 
    {
        X,
        Y,
        Z
    }

	[MenuItem ("GameObject/Align")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow<ObjectAlignment> ();
	}

	void OnGUI()
	{
		GUILayout.Label ("Align Objects", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal (GUILayout.Width(200.0f));
		//EditorGUILayout.BeginHorizontal ();
		alignX = GUILayout.Toggle (alignX, "x");
		alignY = GUILayout.Toggle (alignY, "y");
		alignZ = GUILayout.Toggle (alignZ, "z");
		EditorGUILayout.EndHorizontal ();

		AlignAction action = AlignAction.None;

		if (GUILayout.Button("Align To the minimum")) action = AlignAction.ToMinimum;
        if (GUILayout.Button("Align To the maximum")) action = AlignAction.ToMaximum;
        if (GUILayout.Button("Align To the middle")) action = AlignAction.ToMiddle;
        if (GUILayout.Button("Distribute")) action = AlignAction.Distribute;

        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (Input.GetKey(KeyCode.LeftCommand))
            {
                Undo.PerformUndo();
            }
        }

        HandleAction(action);
	}

    void HandleAction(AlignAction action)
    {
        var selectedObjs = Selection.gameObjects;

        if (action == AlignAction.None || selectedObjs.Length == 1) 
            return;

        Vector3 min = selectedObjs[0].transform.localPosition;
        Vector3 max = min;
        Vector3 sum = Vector3.zero;
        Vector3 targetPos = min;
        foreach (var obj in selectedObjs)
        {
            var pos = obj.transform.localPosition;
            sum += pos;
            if (pos.x < min.x) min.x = pos.x;
            if (pos.y < min.y) min.y = pos.y;
            if (pos.z < min.z) min.z = pos.z;
            if (pos.x > max.x) max.x = pos.x;
            if (pos.y > max.y) max.y = pos.y;
            if (pos.z > max.z) max.z = pos.z;
        }
        sum /= selectedObjs.Length;

        switch (action)
        {
            case AlignAction.ToMinimum:
                targetPos = min;
                break;

            case AlignAction.ToMaximum:
                targetPos = max;
                break;

            case AlignAction.ToMiddle:
                targetPos = sum;
                break;

            case AlignAction.Distribute:
                break;
        }

        if (action == AlignAction.Distribute) {
            if (alignX) 
            {
                selectedObjs = Sort(selectedObjs, SortOption.X);
                min = selectedObjs[0].transform.localPosition;
                max = selectedObjs[selectedObjs.Length - 1].transform.localPosition;
                for (int i = 1; i < selectedObjs.Length - 1; ++i)
                {
                    Undo.RecordObject(selectedObjs[i].transform, "Align");
                    var iPos = selectedObjs[i].transform.localPosition;
                    selectedObjs[i].transform.localPosition = new Vector3(Mathf.Lerp(min.x, max.x, (float)i / (float)(selectedObjs.Length - 1)), iPos.y, iPos.z);
                }
            }
            if (alignY)
            {
                selectedObjs = Sort(selectedObjs, SortOption.Y);
                min = selectedObjs[0].transform.localPosition;
                max = selectedObjs[selectedObjs.Length - 1].transform.localPosition;
                for (int i = 1; i < selectedObjs.Length - 1; ++i)
                {
                    Undo.RecordObject(selectedObjs[i].transform, "Align");
                    var iPos = selectedObjs[i].transform.localPosition;
                    selectedObjs[i].transform.localPosition = new Vector3(iPos.x, Mathf.Lerp(min.y, max.y, (float)i / (float)(selectedObjs.Length - 1)), iPos.z);
                }
            }
            if (alignZ)
            {
                selectedObjs = Sort(selectedObjs, SortOption.Z);
                min = selectedObjs[0].transform.localPosition;
                max = selectedObjs[selectedObjs.Length - 1].transform.localPosition;
                for (int i = 1; i < selectedObjs.Length - 1; ++i)
                {
                    Undo.RecordObject(selectedObjs[i].transform, "Align");
                    var iPos = selectedObjs[i].transform.localPosition;
                    selectedObjs[i].transform.localPosition = new Vector3(iPos.x, iPos.y, Mathf.Lerp(min.z, max.z, (float)i / (float)(selectedObjs.Length - 1)));
                }
            }
        }
        else
        {
            for (int i = 0; i < selectedObjs.Length; ++i)
            {
                Undo.RecordObject(selectedObjs[i].transform, "Align");
                var pos = selectedObjs[i].transform.localPosition;
                if (alignX) pos.x = targetPos.x;
                if (alignY) pos.y = targetPos.y;
                if (alignZ) pos.z = targetPos.z;
                selectedObjs[i].transform.localPosition = pos;
            }
        }
    }

    GameObject[] Sort(GameObject[] objs, SortOption so = SortOption.X)
	{
        List<GameObject> sorted = new List<GameObject>();
        if (objs.Length == 0) return sorted.ToArray();

        sorted.Insert(0, objs[0]);
        for (var i = 1; i < objs.Length; ++i)
        {
            var pos = objs[i].transform.localPosition;
            for (var j = 0; j <= i; ++j)
            {
                var jPos = j < i ? sorted[j].transform.localPosition : Vector3.zero;
                if (j == i 
                    || (so == SortOption.X && pos.x < jPos.x)
                    || (so == SortOption.Y && pos.y < jPos.y)
                    || (so == SortOption.Z && pos.z < jPos.z)) 
                {
                    sorted.Insert(j, objs[i]);
                    break;
                }
            }
        }
        return sorted.ToArray();
	}

}
