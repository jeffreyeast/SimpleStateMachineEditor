using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleStateMachineEditor.Utility
{
    internal static class DrawingAids
    {
        internal static double Distance(Point p1, Point p2)
        {
            return Distance(p1.X, p1.Y, p2.X, p2.Y);
        }

        internal static double Distance(double p1X, double p1Y, double p2X, double p2Y)
        {
            return Math.Sqrt((p2X - p1X) * (p2X - p1X) + (p2Y - p1Y) * (p2Y - p1Y));
        }

        internal static double Distance(Point p1, Point p2, Point subject)
        {
            //  Return the distance from a point to a line between P1 and P2

            return Math.Abs((p2.X - p1.X) * (p1.Y - subject.Y) - (p1.X - subject.X) * (p2.Y - p1.Y)) / 
                Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
        }

        public static PathFigure DrawArrowHead(Point startPoint, Point endPoint, double arrowLength = 10)
        {
            PathFigure pathFigure = new PathFigure() { StartPoint = endPoint, };
            double cos30 = 0.866;
            double sin30 = 0.5;
            double w = arrowLength * cos30;
            double h = arrowLength * sin30;

            double d = Utility.DrawingAids.Distance(startPoint, endPoint);
            double Xn = (endPoint.X - startPoint.X) / d;
            double Yn = (endPoint.Y - startPoint.Y) / d;

            double X3 = endPoint.X - w * Xn - h * Yn;
            double Y3 = endPoint.Y - w * Yn + h * Xn;

            double X4 = endPoint.X - w * Xn + h * Yn;
            double Y4 = endPoint.Y - w * Yn - h * Xn;

            PolyLineSegment segment = new PolyLineSegment();
            pathFigure.Segments.Add(segment);

            segment.Points.Add(new Point(X3, Y3));
            segment.Points.Add(endPoint);
            segment.Points.Add(new Point(X4, Y4));
            segment.Points.Add(endPoint);

            return pathFigure;
        }

        internal static T FindAncestorOfType<T>(FrameworkElement root) where T : FrameworkElement
        {
            DependencyObject parent = root;

            while (parent != null && parent.GetType() != typeof(T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }

        internal static T FindChildOfSpecificType<T>(DependencyObject root) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child.GetType() == typeof(T))
                {
                    return child as T;
                }
                if (VisualTreeHelper.GetChildrenCount(child) > 0)
                {
                    if ((child = FindChildOfSpecificType<T>(child)) != null)
                    {
                        return child as T;
                    }
                }
            }
            return null;
        }

        internal static IEnumerable<T> FindOccludedIcons<T>(Visual root, Point targetPoint)
        {
            //  We're going to use the point at the center of the subject icon for the test. Only icons of the designated type are returned.

            List<T> occludedIcons = new List<T>();

            VisualTreeHelper.HitTest(root, null, new HitTestResultCallback((HitTestResult result) =>
            {
                if (result.VisualHit is FrameworkElement fe && fe.DataContext is T icon && icon.GetType() == typeof(T) && !occludedIcons.Contains(icon))
                {
                    occludedIcons.Add(icon);
                }
                return HitTestResultBehavior.Continue;
            }),
                new PointHitTestParameters(targetPoint));

            return occludedIcons;
        }

        public static double LengthOfFigure(PathFigure figure)
        {
            double length = 0;
            Point start = figure.StartPoint;
            foreach (PathSegment segment in figure.Segments)
            {
                if (segment is LineSegment)
                {
                    length += Utility.DrawingAids.Distance(start, (segment as LineSegment).Point);
                    start = (segment as LineSegment).Point;
                }
                else if (segment is PolyLineSegment)
                {
                    foreach (Point p in (segment as PolyLineSegment).Points)
                    {
                        length += Utility.DrawingAids.Distance(start, p);
                        start = p;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return length;
        }

        public static Geometry NormalizeGeometry(Panel outputParent, Panel inputParent, Geometry inputGeometry)
        {
            StreamGeometry outputGeometry = new StreamGeometry();
            PathGeometry flattenedInputGeometry = inputGeometry.GetFlattenedPathGeometry();

            using (StreamGeometryContext ctx = outputGeometry.Open())
            {
                foreach (PathFigure pathFigure in flattenedInputGeometry.Figures)
                {
                    Point start = pathFigure.StartPoint;
                    ctx.BeginFigure(NormalizePoint(outputParent, inputParent, start), pathFigure.IsFilled, pathFigure.IsClosed);

                    foreach (PathSegment segment in pathFigure.Segments)
                    {
                        if (segment is LineSegment lineSegment)
                        {
                            ctx.LineTo(NormalizePoint(outputParent, inputParent, lineSegment.Point), lineSegment.IsStroked, lineSegment.IsSmoothJoin);
                        }
                        else if (segment is PolyLineSegment polyLineSegment)
                        {
                            IList<Point> points = new List<Point>(polyLineSegment.Points.Count);
                            foreach (Point p in (segment as PolyLineSegment).Points)
                            {
                                points.Add(NormalizePoint(outputParent, inputParent, p));
                            }
                            ctx.PolyLineTo(points, polyLineSegment.IsStroked, polyLineSegment.IsSmoothJoin);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                    }
                }
            }

            return outputGeometry;
        }

        public static Point NormalizePoint(Visual outputParent, Visual inputParent, Point inputPoint)
        {
            return outputParent.PointFromScreen(inputParent.PointToScreen(inputPoint));
        }

        internal static double TangentalDistance(Geometry geometry, Point subject)
        {
            PathGeometry flattenedInputGeometry = geometry.GetFlattenedPathGeometry();
            double smallestDistance = double.MaxValue;

            foreach (PathFigure pathFigure in flattenedInputGeometry.Figures)
            {
                Point start = pathFigure.StartPoint;
                foreach (PathSegment segment in pathFigure.Segments)
                {
                    if (segment is LineSegment lineSegment)
                    {
                        smallestDistance = Math.Min(smallestDistance, Distance(start, lineSegment.Point, subject));
                        start = (segment as LineSegment).Point;
                    }
                    else if (segment is PolyLineSegment polyLineSegment)
                    {
                        foreach (Point p in polyLineSegment.Points)
                        {
                            smallestDistance = Math.Min(smallestDistance, Distance(start, p, subject));
                            start = p;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            return smallestDistance;
        }

        internal static bool Occludes(Point coveringRectableP1, Point coveringRectangleP2, Point testeeLeftTopPosition, Point testeeBottomRightPosition)
        {
            // Normalize the coverting rectangle

            double coveringRectangleLeft = Math.Min(coveringRectableP1.X, coveringRectangleP2.X);
            double coveringRectangleTop = Math.Min(coveringRectableP1.Y, coveringRectangleP2.Y);
            double coveringRectangleRight = Math.Max(coveringRectableP1.X, coveringRectangleP2.X);
            double coveringRectangleBottom = Math.Max(coveringRectableP1.Y, coveringRectangleP2.Y);

            return (coveringRectangleLeft <= testeeLeftTopPosition.X && coveringRectangleRight >= testeeBottomRightPosition.X &&
                    coveringRectangleTop <= testeeLeftTopPosition.Y && coveringRectangleBottom >= testeeBottomRightPosition.Y);
        }
    }
}
