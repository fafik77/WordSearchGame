using UnityEngine;

public class LineRendereFound : MonoBehaviour
{
	[SerializeField] public Color lineColorNormal;
	[SerializeField] public float lineColorMaxHueShift;

	private float colorHsf_h, colorHsf_s, colorHsf_v;
	private LineRenderer lineRenderer;
	public LineRenderer LineRenderer
	{
		get
		{
			if (!lineRenderer)
				lineRenderer = this.gameObject.GetComponent<LineRenderer>();
			return lineRenderer;
		}
		private set => lineRenderer = value;
	}
	private void Awake()
	{
		LineRenderer.positionCount = 2;
	}
	private void Start()
	{
		Color.RGBToHSV(lineColorNormal, out colorHsf_h, out colorHsf_s, out colorHsf_v);

		var randomHueShift = Random.Range(-lineColorMaxHueShift, lineColorMaxHueShift);
		var newHueClamped = colorHsf_h + randomHueShift;
		if (newHueClamped >= 1f)
			newHueClamped -= 1f;
		else if (newHueClamped <= -1f)
			newHueClamped += 1f;

		var newRandomColor = Color.HSVToRGB(newHueClamped, colorHsf_s, colorHsf_v);

        LineRenderer.material.SetColor("_Color", newRandomColor);
	}

	public void SetPositions(Vector2 fromPos, Vector2 toPos)
	{
		LineRenderer.positionCount = 2;
		LineRenderer.SetPosition(0, fromPos);
		LineRenderer.SetPosition(1, toPos);
		LineRenderer.enabled = true;
	}
}
