using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAV3 = Microsoft.Xna.Framework.Vector3;

namespace MonoScene.Graphics.Content
{
    /// <summary>
    /// Utility class for <see cref="ModelCollectionContent"/> post processing.
    /// </summary>
    /// <remarks>
    /// This class should be located in the MonoScene.Pipeline project, but we internally
    /// require <see cref="ModelInstance"/> to do the evalution, so until a better solution is
    /// found, this will stay here.
    /// </remarks>
    public static class PostProcessor
    {
        public static ModelCollectionContent Postprocess(ModelCollectionContent models)
        {
            for(int i=0; i < models._Models.Length; ++i)
            {
                var srcModel = new ModelTemplate(models, i).CreateInstance();
                var bounds = EvaluateBoundingSphere(srcModel, models._SharedMeshes);

                var dstModel = models._Models[i];
                dstModel.ModelBounds = bounds;
            }

            return models;
        }

        private static Microsoft.Xna.Framework.BoundingSphere EvaluateBoundingSphere(ModelInstance srcModel, MeshCollectionContent srcMeshes)
        {
            var triangles = EvaluateTriangles(srcModel, srcMeshes)
                .SelectMany(item => new[] { item.A, item.B, item.C });

            return Microsoft.Xna.Framework.BoundingSphere.CreateFromPoints(triangles);
        }

        private static IEnumerable<(XNAV3 A, XNAV3 B, XNAV3 C)> EvaluateTriangles(ModelInstance srcModel, MeshCollectionContent srcMeshes)
        {
            foreach (var drawable in srcModel.DrawableInstances)
            {
                var controller = drawable.Transform;

                XNAV3 transform(XNAV3 pos, Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences blend)
                {
                    return controller.TransformPosition(pos, null, blend);
                }

                foreach (var tri in srcMeshes.EvaluateTriangles(drawable.Template.MeshIndex, transform))
                {
                    yield return tri;
                }
            }
        }        
    }
}
