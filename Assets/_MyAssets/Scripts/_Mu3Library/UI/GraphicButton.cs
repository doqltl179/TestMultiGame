using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.UI.Button;

[RequireComponent(typeof(CanvasRenderer))]
public class GraphicButton : Graphic, IUIRaycaster {
    public Vector3[] Corners { get; private set; } = new Vector3[4];

    [Space(20)]
    [SerializeField, Range(2, 100)] private int quality = 8;

    [Space(20)]
    public UnityEvent OnClick;



    protected override void OnPopulateMesh(VertexHelper vh) {
        base.OnPopulateMesh(vh);
        vh.Clear();

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        rectTransform.GetLocalCorners(Corners);
        Vector3 lb = Corners[0];
        Vector3 lt = Corners[1];
        Vector3 rt = Corners[2];
        Vector3 rb = Corners[3];
        Vector3 center = (lb + rt) * 0.5f;

        vertex.position = center;
        vh.AddVert(vertex);

        int addedTriCount = 0;
        void AddVert(Vector3 start, Vector3 end, Vector3 point) {
            float l;
            Vector3 vertPos;

            float scaleOffset;
            float diagonalLength = Vector3.Distance(center, rt);

            for(int i = 0; i < quality; i++) {
                l = (float)i / (quality - 1);
                vertPos = UtilFunc.BezierCurve(start, end, point, l);
                scaleOffset = Mathf.InverseLerp(0.5f, 0.0f, Mathf.Abs(l - 0.5f));

                vertex.position = vertPos.normalized * Mathf.Lerp(vertPos.magnitude, diagonalLength, scaleOffset);
                vh.AddVert(vertex);
            }

            for(int i = 0; i < quality - 1; i++) {
                vh.AddTriangle(0, addedTriCount + 1, addedTriCount + 2);
                addedTriCount++;
            }

            addedTriCount++;
        }

        AddVert(Vector3.up * rt.y, Vector3.right * lt.x, lt);
        AddVert(Vector3.right * lt.x, Vector3.up * lb.y, lb);
        AddVert(Vector3.up * lb.y, Vector3.right * rb.x, rb);
        AddVert(Vector3.right * rb.x, Vector3.up * rt.y, rt);
    }

    public void OnEnter(PointerEventData point) {
        throw new System.NotImplementedException();
    }

    public void OnExit(PointerEventData point) {
        throw new System.NotImplementedException();
    }

    public void OnMove(PointerEventData point) {
        throw new System.NotImplementedException();
    }
}
