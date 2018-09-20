using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleVisualizer : MonoBehaviour {

    LineRenderer m_lineRenderer;
    Vector3[] linePos;
    public AnimationCurve curve;
    public GameObject teleEndPoint;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        HideTeleportFX();
    }

    public void ShowTeleportFX()
    {
        teleEndPoint.SetActive(true);
    }

    public void SetTeleport(Vector3 startPoint, Vector3 endPoint)
    {
        m_lineRenderer.SetPosition(0, startPoint);
        m_lineRenderer.SetPosition(1, endPoint);
        teleEndPoint.transform.position = endPoint;
    }

    public void HideTeleportFX()
    {
        m_lineRenderer.SetPosition(0, Vector3.zero);
        m_lineRenderer.SetPosition(1, Vector3.zero);
        teleEndPoint.SetActive(false);
    }
}
