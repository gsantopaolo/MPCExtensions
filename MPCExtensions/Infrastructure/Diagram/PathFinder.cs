// -----------------------------------------------------------------------
// <copyright file="PathFinder.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-06-08 @ 15:40
//  edited: 2017-06-08 @ 16:00
// -----------------------------------------------------------------------

#region Using

using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using CollaBoard.Infrastructure.Helpers;

#endregion

namespace MPCExtensions.Infrastructure.Diagram
{
    //https://stackoverflow.com/questions/38625268/how-can-i-detect-the-control-under-a-point-in-uwp

    // Note: I couldn't find a useful open source library that does
    // orthogonal routing so started to write something on my own.
    // Categorize this as a quick and dirty short term solution.
    // I will keep on searching.

    // Helper class to provide an orthogonal connection path
    public class PathFinder
    {
        private const int Margin = 20;

        /// <summary>
        /// Gets or sets the container margin.
        /// </summary>
        /// <value>
        /// The container margin.
        /// </value>
        public int ContainerMargin { get; set; } = 1;

        /// <summary>
        /// Gets the connection line.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sink">The sink.</param>
        /// <param name="showLastLine">if set to <c>true</c> [show last line].</param>
        /// <returns></returns>
        public List<Point> GetConnectionLine(ConnectorInfo source, ConnectorInfo sink, bool showLastLine)
        {
            List<Point> linePoints = new List<Point>();

            Rect rectSource = this.GetRectWithMargin(source, Margin);
            Rect rectSink = this.GetRectWithMargin(sink, Margin);

            Point startPoint = this.GetOffsetPoint(source, rectSource);
            Point endPoint = this.GetOffsetPoint(sink, rectSink);
            
            linePoints.Add(startPoint);
            Point currentPoint = startPoint;

            if (!rectSink.Contains(currentPoint) && !rectSource.Contains(endPoint))
            {
                while (true)
                {
                    #region source node

                    if (this.IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource, rectSink }))
                    {
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }

                    Point neighbour = this.GetNearestVisibleNeighborSink(currentPoint, endPoint, sink, rectSource, rectSink);
                    if (!double.IsNaN(neighbour.X))
                    {
                        linePoints.Add(neighbour);
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }

                    if (currentPoint == startPoint)
                    {
                        bool flag;
                        Point n = this.GetNearestNeighborSource(source, endPoint, rectSource, rectSink, out flag);
                        linePoints.Add(n);
                        currentPoint = n;

                        if (!this.IsRectVisible(currentPoint, rectSink, new Rect[] { rectSource }))
                        {
                            Point n1, n2;
                            this.GetOppositeCorners(source.Orientation, rectSource, out n1, out n2);
                            if (flag)
                            {
                                linePoints.Add(n1);
                                currentPoint = n1;
                            }
                            else
                            {
                                linePoints.Add(n2);
                                currentPoint = n2;
                            }
                            if (!this.IsRectVisible(currentPoint, rectSink, new Rect[] { rectSource }))
                            {
                                if (flag)
                                {
                                    linePoints.Add(n2);
                                    currentPoint = n2;
                                }
                                else
                                {
                                    linePoints.Add(n1);
                                    currentPoint = n1;
                                }
                            }
                        }
                    }

                    #endregion

                    #region sink node

                    else // from here on we jump to the sink node
                    {
                        Point n1, n2; // neighbour corner
                        Point s1, s2; // opposite corner
                        this.GetNeighborCorners(sink.Orientation, rectSink, out s1, out s2);
                        this.GetOppositeCorners(sink.Orientation, rectSink, out n1, out n2);

                        bool n1Visible = this.IsPointVisible(currentPoint, n1, new Rect[] { rectSource, rectSink });
                        bool n2Visible = this.IsPointVisible(currentPoint, n2, new Rect[] { rectSource, rectSink });

                        if (n1Visible && n2Visible)
                        {
                            if (rectSource.Contains(n1))
                            {
                                linePoints.Add(n2);
                                if (rectSource.Contains(s2))
                                {
                                    linePoints.Add(n1);
                                    linePoints.Add(s1);
                                }
                                else
                                    linePoints.Add(s2);

                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }

                            if (rectSource.Contains(n2))
                            {
                                linePoints.Add(n1);
                                if (rectSource.Contains(s1))
                                {
                                    linePoints.Add(n2);
                                    linePoints.Add(s2);
                                }
                                else
                                    linePoints.Add(s1);

                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }

                            if ((this.Distance(n1, endPoint) <= this.Distance(n2, endPoint)))
                            {
                                linePoints.Add(n1);
                                if (rectSource.Contains(s1))
                                {
                                    linePoints.Add(n2);
                                    linePoints.Add(s2);
                                }
                                else
                                    linePoints.Add(s1);
                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }
                            else
                            {
                                linePoints.Add(n2);
                                if (rectSource.Contains(s2))
                                {
                                    linePoints.Add(n1);
                                    linePoints.Add(s1);
                                }
                                else
                                    linePoints.Add(s2);
                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }
                        }
                        else if (n1Visible)
                        {
                            linePoints.Add(n1);
                            if (rectSource.Contains(s1))
                            {
                                linePoints.Add(n2);
                                linePoints.Add(s2);
                            }
                            else
                                linePoints.Add(s1);
                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }
                        else
                        {
                            linePoints.Add(n2);
                            if (rectSource.Contains(s2))
                            {
                                linePoints.Add(n1);
                                linePoints.Add(s1);
                            }
                            else
                                linePoints.Add(s2);
                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }
                    }

                    #endregion
                }
            }
            else
            {
                linePoints.Add(endPoint);
            }

            linePoints = this.OptimizeLinePoints(linePoints, new Rect[] { rectSource, rectSink }, source.Orientation, sink.Orientation);

            this.CheckPathEnd(source, sink, showLastLine, linePoints);
            return linePoints;
        }

        /// <summary>
        /// Gets the connection line.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sinkPoint">The sink point.</param>
        /// <param name="preferredOrientation">The preferred orientation.</param>
        /// <returns></returns>
        public List<Point> GetConnectionLine(ConnectorInfo source, Point sinkPoint, ConnectorOrientation preferredOrientation)
        {
            List<Point> linePoints = new List<Point>();
            Rect rectSource = this.GetRectWithMargin(source, 1);
            Point startPoint = this.GetOffsetPoint(source, rectSource);
            Point endPoint = sinkPoint;

            linePoints.Add(startPoint);

            //linePoints.Add(endPoint);
            //return linePoints;



            Point currentPoint = startPoint;

            if (!rectSource.Contains(endPoint))
            {
                while (true)
                {
                    if (this.IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource }))
                    {
                        linePoints.Add(endPoint);
                        break;
                    }

                    bool sideFlag;
                    Point n = this.GetNearestNeighborSource(source, endPoint, rectSource, out sideFlag);
                    linePoints.Add(n);
                    currentPoint = n;

                    if (this.IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource }))
                    {
                        linePoints.Add(endPoint);
                        break;
                    }
                    else
                    {
                        Point n1, n2;
                        this.GetOppositeCorners(source.Orientation, rectSource, out n1, out n2);
                        if (sideFlag)
                            linePoints.Add(n1);
                        else
                            linePoints.Add(n2);

                        linePoints.Add(endPoint);
                        break;
                    }
                }
            }
            else
            {
                linePoints.Add(endPoint);
            }

            if (preferredOrientation != ConnectorOrientation.None)
                linePoints = this.OptimizeLinePoints(linePoints, new Rect[] { rectSource }, source.Orientation, preferredOrientation);
            else
                linePoints = this.OptimizeLinePoints(linePoints, new Rect[] { rectSource }, source.Orientation, this.GetOpositeOrientation(source.Orientation));

            return linePoints;
        }

        private void CheckPathEnd(ConnectorInfo source, ConnectorInfo sink, bool showLastLine, List<Point> linePoints)
        {
            if (showLastLine)
            {
                Point startPoint = new Point(0, 0);
                Point endPoint = new Point(0, 0);
                double marginPath = 15;
                switch (source.Orientation)
                {
                    case ConnectorOrientation.Left:
                        startPoint = new Point(source.Position.X - marginPath, source.Position.Y);
                        break;
                    case ConnectorOrientation.Top:
                        startPoint = new Point(source.Position.X, source.Position.Y - marginPath);
                        break;
                    case ConnectorOrientation.Right:
                        startPoint = new Point(source.Position.X + marginPath, source.Position.Y);
                        break;
                    case ConnectorOrientation.Bottom:
                        startPoint = new Point(source.Position.X, source.Position.Y + marginPath);
                        break;
                    default:
                        break;
                }

                switch (sink.Orientation)
                {
                    case ConnectorOrientation.Left:
                        endPoint = new Point(sink.Position.X - marginPath, sink.Position.Y);
                        break;
                    case ConnectorOrientation.Top:
                        endPoint = new Point(sink.Position.X, sink.Position.Y - marginPath);
                        break;
                    case ConnectorOrientation.Right:
                        endPoint = new Point(sink.Position.X + marginPath, sink.Position.Y);
                        break;
                    case ConnectorOrientation.Bottom:
                        endPoint = new Point(sink.Position.X, sink.Position.Y + marginPath);
                        break;
                    default:
                        break;
                }
                linePoints.Insert(0, startPoint);
                linePoints.Add(endPoint);
            }
            else
            {
                linePoints.Insert(0, source.Position);
                linePoints.Add(sink.Position);
            }
        }

        private double Distance(Point p1, Point p2)
        {
            return RectHelper.Subtract(p1, p2).Length();
        }

        private Point GetNearestNeighborSource(ConnectorInfo source, Point endPoint, Rect rectSource, Rect rectSink, out bool flag)
        {
            Point n1, n2; // neighbors
            this.GetNeighborCorners(source.Orientation, rectSource, out n1, out n2);

            if (rectSink.Contains(n1))
            {
                flag = false;
                return n2;
            }

            if (rectSink.Contains(n2))
            {
                flag = true;
                return n1;
            }

            if ((this.Distance(n1, endPoint) <= this.Distance(n2, endPoint)))
            {
                flag = true;
                return n1;
            }
            else
            {
                flag = false;
                return n2;
            }
        }

        private Point GetNearestNeighborSource(ConnectorInfo source, Point endPoint, Rect rectSource, out bool flag)
        {
            Point n1, n2; // neighbors
            this.GetNeighborCorners(source.Orientation, rectSource, out n1, out n2);

            if ((this.Distance(n1, endPoint) <= this.Distance(n2, endPoint)))
            {
                flag = true;
                return n1;
            }
            else
            {
                flag = false;
                return n2;
            }
        }

        private Point GetNearestVisibleNeighborSink(Point currentPoint, Point endPoint, ConnectorInfo sink, Rect rectSource, Rect rectSink)
        {
            Point s1, s2; // neighbors on sink side
            this.GetNeighborCorners(sink.Orientation, rectSink, out s1, out s2);

            bool flag1 = this.IsPointVisible(currentPoint, s1, new Rect[] { rectSource, rectSink });
            bool flag2 = this.IsPointVisible(currentPoint, s2, new Rect[] { rectSource, rectSink });

            if (flag1) // s1 visible
            {
                if (flag2) // s1 and s2 visible
                {
                    if (rectSink.Contains(s1))
                        return s2;

                    if (rectSink.Contains(s2))
                        return s1;

                    if ((this.Distance(s1, endPoint) <= this.Distance(s2, endPoint)))
                        return s1;
                    else
                        return s2;
                }
                else
                {
                    return s1;
                }
            }
            else // s1 not visible
            {
                if (flag2) // only s2 visible
                {
                    return s2;
                }
                else // s1 and s2 not visible
                {
                    return new Point(double.NaN, double.NaN);
                }
            }
        }

        private void GetNeighborCorners(ConnectorOrientation orientation, Rect rect, out Point n1, out Point n2)
        {
            switch (orientation)
            {
                case ConnectorOrientation.Left:
                    n1 = rect.TopLeft();
                    n2 = rect.BottomLeft();
                    break;
                case ConnectorOrientation.Top:
                    n1 = rect.TopLeft();
                    n2 = rect.TopRight();
                    break;
                case ConnectorOrientation.Right:
                    n1 = rect.TopRight();
                    n2 = rect.BottomRight();
                    break;
                case ConnectorOrientation.Bottom:
                    n1 = rect.BottomLeft();
                    n2 = rect.BottomRight();
                    break;
                default:
                    throw new Exception("No neighour corners found!");
            }
        }

        private Point GetOffsetPoint(ConnectorInfo connector, Rect rect)
        {
            Point offsetPoint = new Point();

            switch (connector.Orientation)
            {
                case ConnectorOrientation.Left:
                    offsetPoint = new Point(rect.Left, connector.Position.Y);
                    break;
                case ConnectorOrientation.Top:
                    offsetPoint = new Point(connector.Position.X, rect.Top);
                    break;
                case ConnectorOrientation.Right:
                    offsetPoint = new Point(rect.Right, connector.Position.Y);
                    break;
                case ConnectorOrientation.Bottom:
                    offsetPoint = new Point(connector.Position.X, rect.Bottom);
                    break;
                default:
                    break;
            }

            return offsetPoint;
        }

        private ConnectorOrientation GetOpositeOrientation(ConnectorOrientation connectorOrientation)
        {
            switch (connectorOrientation)
            {
                case ConnectorOrientation.Left:
                    return ConnectorOrientation.Right;
                case ConnectorOrientation.Top:
                    return ConnectorOrientation.Bottom;
                case ConnectorOrientation.Right:
                    return ConnectorOrientation.Left;
                case ConnectorOrientation.Bottom:
                    return ConnectorOrientation.Top;
                default:
                    return ConnectorOrientation.Top;
            }
        }

        private void GetOppositeCorners(ConnectorOrientation orientation, Rect rect, out Point n1, out Point n2)
        {
            switch (orientation)
            {
                case ConnectorOrientation.Left:
                    n1 = rect.TopRight();
                    n2 = rect.BottomRight();
                    break;
                case ConnectorOrientation.Top:
                    n1 = rect.BottomLeft();
                    n2 = rect.BottomRight();
                    break;
                case ConnectorOrientation.Right:
                    n1 = rect.TopLeft();
                    n2 = rect.BottomLeft();
                    break;
                case ConnectorOrientation.Bottom:
                    n1 = rect.TopLeft();
                    n2 = rect.TopRight();
                    break;
                default:
                    throw new Exception("No opposite corners found!");
            }
        }

        private ConnectorOrientation GetOrientation(Point p1, Point p2)
        {
            if (p1.X == p2.X)
            {
                if (p1.Y >= p2.Y)
                    return ConnectorOrientation.Bottom;
                else
                    return ConnectorOrientation.Top;
            }
            else if (p1.Y == p2.Y)
            {
                if (p1.X >= p2.X)
                    return ConnectorOrientation.Right;
                else
                    return ConnectorOrientation.Left;
            }
            throw new Exception("Failed to retrieve orientation");
        }

        private Orientation GetOrientation(ConnectorOrientation sourceOrientation)
        {
            switch (sourceOrientation)
            {
                case ConnectorOrientation.Left:
                    return Orientation.Horizontal;
                case ConnectorOrientation.Top:
                    return Orientation.Vertical;
                case ConnectorOrientation.Right:
                    return Orientation.Horizontal;
                case ConnectorOrientation.Bottom:
                    return Orientation.Vertical;
                default:
                    throw new Exception("Unknown ConnectorOrientation");
            }
        }

        private Rect GetRectWithMargin(ConnectorInfo connectorThumb, double margin)
        {
            Rect rect = new Rect(connectorThumb.HostLeft,
                connectorThumb.HostTop,
                connectorThumb.HostSize.Width,
                connectorThumb.HostSize.Height);

            rect = rect.Inflate(margin, margin);
            return rect;
        }

        private bool IsPointVisible(Point fromPoint, Point targetPoint, Rect[] rectangles)
        {
            foreach (Rect rect in rectangles)
            {
                if (this.RectangleIntersectsLine(rect, fromPoint, targetPoint))
                    return false;
            }
            return true;
        }

        private bool IsRectVisible(Point fromPoint, Rect targetRect, Rect[] rectangles)
        {
            if (this.IsPointVisible(fromPoint, targetRect.TopLeft(), rectangles))
                return true;

            if (this.IsPointVisible(fromPoint, targetRect.TopRight(), rectangles))
                return true;

            if (this.IsPointVisible(fromPoint, targetRect.BottomLeft(), rectangles))
                return true;

            if (this.IsPointVisible(fromPoint, targetRect.BottomRight(), rectangles))
                return true;

            return false;
        }

        private  List<Point> OptimizeLinePoints(List<Point> linePoints, Rect[] rectangles, ConnectorOrientation sourceOrientation, ConnectorOrientation sinkOrientation)
        {
            List<Point> points = new List<Point>();
            int cut = 0;

            for (int i = 0; i < linePoints.Count; i++)
            {
                if (i >= cut)
                {
                    for (int k = linePoints.Count - 1; k > i; k--)
                    {
                        if (this.IsPointVisible(linePoints[i], linePoints[k], rectangles))
                        {
                            cut = k;
                            break;
                        }
                    }
                    points.Add(linePoints[i]);
                }
            }

            #region Line

            for (int j = 0; j < points.Count - 1; j++)
            {
                if (points[j].X != points[j + 1].X && points[j].Y != points[j + 1].Y)
                {
                    ConnectorOrientation orientationFrom;
                    ConnectorOrientation orientationTo;

                    // orientation from point
                    if (j == 0)
                        orientationFrom = sourceOrientation;
                    else
                        orientationFrom = this.GetOrientation(points[j], points[j - 1]);

                    // orientation to pint 
                    if (j == points.Count - 2)
                        orientationTo = sinkOrientation;
                    else
                        orientationTo = this.GetOrientation(points[j + 1], points[j + 2]);


                    if ((orientationFrom == ConnectorOrientation.Left || orientationFrom == ConnectorOrientation.Right) &&
                        (orientationTo == ConnectorOrientation.Left || orientationTo == ConnectorOrientation.Right))
                    {
                        double centerX = Math.Min(points[j].X, points[j + 1].X) + Math.Abs(points[j].X - points[j + 1].X) / 2;
                        points.Insert(j + 1, new Point(centerX, points[j].Y));
                        points.Insert(j + 2, new Point(centerX, points[j + 2].Y));
                        if (points.Count - 1 > j + 3)
                            points.RemoveAt(j + 3);
                        return points;
                    }

                    if ((orientationFrom == ConnectorOrientation.Top || orientationFrom == ConnectorOrientation.Bottom) &&
                        (orientationTo == ConnectorOrientation.Top || orientationTo == ConnectorOrientation.Bottom))
                    {
                        double centerY = Math.Min(points[j].Y, points[j + 1].Y) + Math.Abs(points[j].Y - points[j + 1].Y) / 2;
                        points.Insert(j + 1, new Point(points[j].X, centerY));
                        points.Insert(j + 2, new Point(points[j + 2].X, centerY));
                        if (points.Count - 1 > j + 3)
                            points.RemoveAt(j + 3);
                        return points;
                    }

                    if ((orientationFrom == ConnectorOrientation.Left || orientationFrom == ConnectorOrientation.Right) &&
                        (orientationTo == ConnectorOrientation.Top || orientationTo == ConnectorOrientation.Bottom))
                    {
                        points.Insert(j + 1, new Point(points[j + 1].X, points[j].Y));
                        return points;
                    }

                    if ((orientationFrom == ConnectorOrientation.Top || orientationFrom == ConnectorOrientation.Bottom) &&
                        (orientationTo == ConnectorOrientation.Left || orientationTo == ConnectorOrientation.Right))
                    {
                        points.Insert(j + 1, new Point(points[j].X, points[j + 1].Y));
                        return points;
                    }
                }
            }

            #endregion

            return points;
        }

        private bool RectangleIntersectsLine(Rect rect, Point startPoint, Point endPoint)
        {
            rect= rect.Inflate(-1, -1);
            bool intersect = rect.IntersectsWith(new Rect(startPoint, endPoint));
            return intersect;
        }
    }
}