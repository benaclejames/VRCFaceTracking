using System;
using UnityEngine;
using UnityEngine.UI;

namespace VRCFaceTracking.QuickMenu.EyeTracking
{
	public class XYVisualizer
	{
		private readonly RectTransform _xLine, _yLine;
		private readonly Text _xValue, _yValue;
		private const float Range = 330;

		public XYVisualizer(Transform x, Transform y)
		{
			_xLine = x.GetComponent<RectTransform>();
			_yLine = y.GetComponent<RectTransform>();

			_xValue = x.transform.Find("Text").GetComponent<Text>();
			_yValue = y.transform.Find("Text").GetComponent<Text>();
		}

		public void ReceiveXY(float x, float y)
		{
			_xLine.localPosition = new Vector3(x*Range, 0);
			_yLine.localPosition = new Vector3(0, y*Range);

			_xValue.text = Math.Round(x, 2).ToString();
			_yValue.text = Math.Round(y, 2).ToString();
		}
	}
}