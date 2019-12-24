using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Outline2", 15)]
    public class Outline2 : Shadow
    {
        protected Outline2()
        { }


		static List<UIVertex> output = new List<UIVertex>();
		public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

			output.Clear();
			vh.GetUIVertexStream(output);

            var start = 0;
            var end = output.Count;
            ApplyShadow(output, effectColor, start, output.Count, effectDistance.x, effectDistance.y);

            start = end;
            end = output.Count;
            ApplyShadow(output, effectColor, start, output.Count, effectDistance.x, -effectDistance.y);

            start = end;
            end = output.Count;
            ApplyShadow(output, effectColor, start, output.Count, -effectDistance.x, effectDistance.y);

            start = end;
            end = output.Count;
            ApplyShadow(output, effectColor, start, output.Count, -effectDistance.x, -effectDistance.y);


            var s = 1.4142135623730951f;
            start = end;
            end = output.Count;
            ApplyShadow(output, effectColor, start, output.Count, 0, effectDistance.y * s);

            start = end;
            end = output.Count;
            ApplyShadow(output, effectColor, start, output.Count, effectDistance.x * s, 0);

            start = end;
            end = output.Count;
            ApplyShadow(output, effectColor, start, output.Count, -effectDistance.x * s, 0);

            start = end;
            end = output.Count;
            ApplyShadow(output, effectColor, start, output.Count, 0, -effectDistance.y * s);


			vh.Clear();
			vh.AddUIVertexTriangleStream(output);
        }
    }
}
