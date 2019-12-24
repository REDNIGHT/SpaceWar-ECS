using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class TextGradientEffect : BaseMeshEffect
    {
        public Color bottom = Color.red;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!this.IsActive())
                return;


            //
            var index = 0;
            for (var i = 0; i < vh.currentVertCount; ++i)
            {
                if (index >= 2)
                {
                    UIVertex v = new UIVertex();
                    vh.PopulateUIVertex(ref v, i);
                    v.color = bottom;
                    vh.SetUIVertex(v, i);
                }


                //
                ++index;
                if (index >= 4)
                    index = 0;
            }
        }
        
    }
}