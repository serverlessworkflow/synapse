using Neuroglia.Blazor.Dagre.Models;

namespace Neuroglia.Blazor.Dagre
{
    public static class PathBuilder
    {
        public static string GetLinearPath(ICollection<IPosition> points)
        {
            var path = new SvgPath();
            foreach (var point in points)
            {
                if (string.IsNullOrEmpty(path.GetPath()))
                {
                    path.MoveTo(point);
                }
                else
                {
                    path.LineTo(point);
                }
            }
            return path.GetPath();
        }

        public static string GetBSplinePath(ICollection<IPosition> points)
        {
            throw new NotImplementedException(); // todo
        }
    }
}
