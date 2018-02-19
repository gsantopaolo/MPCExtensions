// -----------------------------------------------------------------------
// <copyright file="DiagramConnection.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-10-20 @ 12:35
//  edited: 2017-10-20 @ 15:20
// -----------------------------------------------------------------------

#define UseBezier


#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using MPCExtensions.Controls;
using MPCExtensions.Helpers;

#endregion

namespace MPCExtensions.Infrastructure.Diagram
{
    internal class DiagramConnection
    {
        private const int ConnectionTouchTickness = 20; //Dimension of connection touchable area
        private const double SelectionLineZoom = 2; //Amount of zooming of selected tile
        private const double LineEdgeRatio = 4.5; //Amount that multiplied with thickness determines how 'far' a line end from the tile
        private readonly PathFinder pathFinder;
        private Polygon arrowEnd;
        private Polygon arrowStart;
        private string color;
        private ConnectionType connectionType = ConnectionType.ArrowTo;
        private ConnectorInfo destinationConnectorInfo;
        private SolidColorBrush highlightBrush;
        private bool isHighlighted;
        private bool isSelected;
        private SolidColorBrush lineBrush;
        private double lineOpacity;
        private ConnectorInfo originConnectorInfo;
        private SolidColorBrush selectedLineBrush;
        private int tickness;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramConnection" /> class.
        /// </summary>
        /// <param name="origin">The tile origin.</param>
        /// <param name="originConnectorInfo">The connector where connection originates</param>
        /// <param name="pointerId">The pointer identifier.</param>
        public DiagramConnection(ConnectableViewItem origin, ConnectorInfo originConnectorInfo, uint pointerId)
        {
            this.originConnectorInfo = originConnectorInfo;
            this.pathFinder = new PathFinder();
            this.PointerId = pointerId;
            this.Origin = origin;
            this.Id = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Gets or sets the connectable targets.
        /// </summary>
        /// <value>
        /// The connectable targets.
        /// </value>
        public List<ConnectableViewItem> ConnectableTargets { get; set; }

        /// <summary>
        /// Gets the optional connection ends.
        /// </summary>
        /// <value>
        /// The connection ends.
        /// </value>
        public List<Polygon> ConnectionEnds { get; } = new List<Polygon>();

        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>
        /// The type of the connection.
        /// </value>
        public ConnectionType ConnectionType
        {
            get => this.connectionType;
            set
            {
                this.connectionType = value;
                //if (this.arrowEnd != null) this.arrowEnd.Visibility = Visibility.Collapsed;
                //if (this.arrowStart != null) this.arrowStart.Visibility = Visibility.Collapsed;
                //if (value != ConnectionType.None) this.UpdateConnectionEnds(this.connectionStartPoint, this.connectionEndPoint);
            }
        }

        /// <summary>
        /// Gets or sets the tile where connection ends.
        /// </summary>
        /// <value>
        /// The destination.
        /// </value>
        public ConnectableViewItem Destination { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this connestion has any connectable targets.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has connectable targets; otherwise, <c>false</c>.
        /// </value>
        public bool HasConnectableTargets => this.ConnectableTargets != null && this.ConnectableTargets.Any();

        /// <summary>
        /// Gets or sets the connection identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether connection is highlighted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is highlighted; otherwise, <c>false</c>.
        /// </value>
        public bool IsHighlighted
        {
            get => this.isHighlighted;
            set
            {
                if (value != this.isHighlighted)
                {
                    this.isHighlighted = value;
                    this.LineConnection.Stroke = value ? this.highlightBrush : this.lineBrush;
                    this.LineConnection.Opacity = value ? 1 : this.lineOpacity;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this connection is visible via hit testing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is hit test visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsHitTestVisible
        {
            get => this.LineConnection.IsHitTestVisible;
            set
            {
                this.LineConnection.IsHitTestVisible = value;
                this.TouchConnection.IsHitTestVisible = value;
                this.arrowStart.IsHitTestVisible = value;
                this.arrowEnd.IsHitTestVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether connection is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                this.isSelected = value;
                this.LineConnection.Stroke = value ? this.selectedLineBrush : this.lineBrush;
                //this.LineConnection.StrokeThickness = value ? this.tickness * SelectionLineZoom : this.tickness;
                this.arrowStart.Fill = value ? this.selectedLineBrush : this.lineBrush;
                this.arrowEnd.Fill = value ? this.selectedLineBrush : this.lineBrush;

                //this.UpdateConnectionEnds(this.connectionStartPoint, this.connectionEndPoint);
            }
        }

        /// <summary>
        /// Gets or sets the connection path.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public Path LineConnection { get; private set; }

        /// <summary>
        /// Gets or sets tile where connection origins.
        /// </summary>
        /// <value>
        /// The origin.
        /// </value>
        public ConnectableViewItem Origin { get; }

        /// <summary>
        /// Gets or sets the pointer identifier for this connection
        /// </summary>
        /// <value>
        /// The pointer identifier.
        /// </value>
        public uint PointerId { get; }

        /// <summary>
        /// Gets or sets the routing mode.
        /// </summary>
        /// <value>
        /// The routing mode.
        /// </value>
        public RoutingMode RoutingMode { get; set; } = RoutingMode.Bezier;

        public Path TouchConnection { get; set; }

        /// <summary>
        /// Gets the origin orientation.
        /// </summary>
        /// <value>
        /// The origin orientation.
        /// </value>
        internal ConnectorOrientation OriginOrientation => this.originConnectorInfo.Orientation;

        /// <summary>
        /// Completes a pending connection.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="destinationConnectorInfo">The destination connector information.</param>
        /// <param name="zoomFactor">The zoom factor.</param>
        public void CompletePendingConnection(ConnectableViewItem target, ConnectorInfo destinationConnectorInfo, double zoomFactor)
        {
            this.destinationConnectorInfo = destinationConnectorInfo;
            this.Destination = target;
            List<Point> points;
            (PathGeometry LineGeometry, PathGeometry TouchGeometry, Point LastPoint) diagramLineUnit;
            if (this.RoutingMode == RoutingMode.Routed)
            {
                points = this.pathFinder.GetConnectionLine(this.originConnectorInfo, destinationConnectorInfo, false);
                diagramLineUnit = this.CreateRoutedLineUnit(points);
            }
            else
            {
                points = this.pathFinder.GetConnectionLine(this.originConnectorInfo, this.destinationConnectorInfo.Position, ConnectorOrientation.None);
                diagramLineUnit = this.CreateBezierLineUnit(points);

                //ToDo: Arrow angle
                //double angle = this.GetBezierCurveAngle(points);
                //System.Diagnostics.Debug.WriteLine($"ANGLE:{angle}");
                //(this.arrowStart.RenderTransform as RotateTransform).Angle = angle;
                //(this.arrowEnd.RenderTransform as RotateTransform).Angle = angle;
            }

            this.LineConnection.Data = diagramLineUnit.LineGeometry;
            this.TouchConnection.Data = diagramLineUnit.TouchGeometry;
            this.TouchConnection.StrokeThickness = ConnectionTouchTickness * zoomFactor;


            this.UpdateConnectionEnds(points[0], diagramLineUnit.LastPoint);
        }

        /// <summary>
        /// Creates a new pending connection.
        /// </summary>
        /// <param name="lineColor">Color of the line.</param>
        /// <param name="selectedColor">Color of the selected.</param>
        /// <param name="highlightColor">Color of the highlight.</param>
        /// <param name="lineThickness">The line thickness.</param>
        /// <param name="opacity">The opacity.</param>
        public void CreateConnection(string lineColor, string selectedColor, string highlightColor, int lineThickness, double opacity)
        {
            this.color = lineColor;
            this.tickness = lineThickness;
            this.selectedLineBrush = new SolidColorBrush(UiUtility.GetColor(selectedColor));
            this.lineBrush = new SolidColorBrush(UiUtility.GetColor(lineColor));
            this.highlightBrush = new SolidColorBrush(UiUtility.GetColor(highlightColor));
            this.lineOpacity = opacity;

            this.LineConnection = new Path
            {
                Stroke = this.lineBrush,
                StrokeThickness = lineThickness,
                Data = this.CreateOriginGeometry(),
                IsHitTestVisible = false,
                Opacity = opacity,
                DataContext = this,
            };

            this.TouchConnection = new Path
            {
                Stroke = new SolidColorBrush(Color.FromArgb(0x00, 0x20, 0x00, 0xFF)), //Touchable brush
                StrokeThickness = ConnectionTouchTickness,
                Data = this.CreateOriginGeometry(),
                IsHitTestVisible = false,
                Opacity = opacity,
                DataContext = this
            };


            this.arrowEnd = new Polygon
            {
                Fill = this.lineBrush,
                IsHitTestVisible = false,
                DataContext = this,
                //RenderTransform = new RotateTransform(),
                //RenderTransformOrigin = new Point(0.5, 0.5)
            };

            this.arrowStart = new Polygon
            {
                Fill = this.lineBrush,
                IsHitTestVisible = false,
                DataContext = this,
                //RenderTransform = new RotateTransform(),
                //RenderTransformOrigin = new Point(0.5, 0.5)
            };
        }

        public event EventHandler<Point> EditOperationRequested;

        /// <summary>
        /// Resets the connection zoom.
        /// </summary>
        /// <param name="controlTileScale">The control tile scale.</param>
        public void ResetZoom(double controlTileScale)
        {
            this.TouchConnection.StrokeThickness = ConnectionTouchTickness * controlTileScale;
        }

        public void SetZoom(double zoomRatio)
        {
            this.TouchConnection.StrokeThickness = 20 * zoomRatio;
        }

        /// <summary>
        /// Shows the connection edit UI.
        /// </summary>
        /// <param name="point">The point.</param>
        public void ShowConnectionEditUi(Point point)
        {
            this.EditOperationRequested?.Invoke(this, point);
        }

        /// <summary>
        /// To the tile connection.
        /// </summary>
        /// <returns></returns>
        public TileConnection ToTileConnection()
        {
            return new TileConnection()
            {
                FromId = this.Origin.Id,
                ToId = this.Destination.Id,
                FromOrientation = this.originConnectorInfo.Orientation,
                ToOrientation = this.destinationConnectorInfo.Orientation,
                Id = this.Id,
                Color = this.color,
                Thickness = this.tickness,
                ConnectionType = this.connectionType,
                RoutingMode = this.RoutingMode
            };
        }

        /// <summary>
        /// Updates this connection when provide tile changes.
        /// </summary>
        /// <param name="updatedTile">The updated tile.</param>
        /// <param name="zoomFactor">The zoom factor.</param>
        public void Update(ConnectableViewItem updatedTile, double zoomFactor)
        {
            if (this.Origin == updatedTile)
            {
                this.originConnectorInfo = this.Origin.CreateDiagramConnection(this.originConnectorInfo.HitTestInfo);
            }

            if (this.Destination == updatedTile)
            {
                this.destinationConnectorInfo = this.Destination.CreateDiagramConnection(this.destinationConnectorInfo.HitTestInfo);
            }

            List<Point> points;
            (PathGeometry LineGeometry, PathGeometry TouchGeometry, Point LastPoint) diagramLineUnit;
            if (this.RoutingMode == RoutingMode.Routed)
            {
                points = this.pathFinder.GetConnectionLine(this.originConnectorInfo, this.destinationConnectorInfo, false);
                diagramLineUnit = this.CreateRoutedLineUnit(points);
            }
            else
            {
                points = this.pathFinder.GetConnectionLine(this.originConnectorInfo, this.destinationConnectorInfo.Position, ConnectorOrientation.None);
                diagramLineUnit = this.CreateBezierLineUnit(points);

                //double angle = this.GetBezierCurveAngle(points);
                //System.Diagnostics.Debug.WriteLine($"ANGLE:{angle}");
                //(this.arrowStart.RenderTransform as RotateTransform).Angle = 0;
                //(this.arrowEnd.RenderTransform as RotateTransform).Angle = 0;
                //Point pt = this.GetBezierMidPoint(points);
                //System.Diagnostics.Debug.WriteLine($"Mid={pt.X}-{pt.Y}");
            }

            this.LineConnection.Data = diagramLineUnit.LineGeometry;
            this.TouchConnection.Data = diagramLineUnit.TouchGeometry;
            this.TouchConnection.StrokeThickness = ConnectionTouchTickness * zoomFactor;

            this.UpdateConnectionEnds(points.First(), diagramLineUnit.LastPoint);
        }

        /// <summary>
        /// Updates drawing of a pending connection.
        /// </summary>
        /// <param name="hitPoint">The hit point.</param>
        public void UpdatePendingConnection(Point hitPoint)
        {
            var points = this.pathFinder.GetConnectionLine(this.originConnectorInfo, hitPoint, ConnectorOrientation.None);
            var diagramLineUnit = this.CreateBezierLineUnit(points);
            this.LineConnection.Data = diagramLineUnit.LineGeometry;
            this.TouchConnection.Data = diagramLineUnit.TouchGeometry;
            this.LineConnection.StrokeDashArray = new DoubleCollection() {1, 1};
        }

        /// <summary>
        /// Called a context operation is requested.
        /// </summary>
        /// <param name="point">The point.</param>
        protected virtual void OnEditOperationRequested(Point point)
        {
            this.EditOperationRequested?.Invoke(this, point);
        }

        /// <summary>
        /// Updates connection line ui details.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The thickness.</param>
        internal void Update(TileConnection connection, double zoomFactor)
        {
            this.color = connection.Color;
            this.tickness = connection.Thickness;
            this.LineConnection.StrokeThickness = this.tickness;
            this.lineBrush = new SolidColorBrush(UiUtility.GetColor(this.color));
            this.ConnectionType = connection.ConnectionType;
            this.RoutingMode = connection.RoutingMode;
            this.IsSelected = false;
            this.LineConnection.StrokeDashCap = PenLineCap.Round;

            //Redraw entire connection using updated params
            this.CompletePendingConnection(this.Destination, this.destinationConnectorInfo, zoomFactor);
        }

        private double Angle(double px1, double py1, double px2, double py2)
        {
            // Negate X and Y values
            double pxRes = px2 - px1;
            double pyRes = py2 - py1;
            double angle = 0.0;
            // Calculate the angleror
            if (pxRes == 0.0)
            {
                if (pxRes == 0.0)
                    angle = 0.0;
                else if (pyRes > 0.0) angle = Math.PI / 2.0;
                else
                    angle = Math.PI * 3.0 / 2.0;
            }
            else if (pyRes == 0.0)
            {
                if (pxRes > 0.0)
                    angle = 0.0;
                else
                    angle = Math.PI;
            }
            else
            {
                if (pxRes < 0.0)
                    angle = Math.Atan(pyRes / pxRes) + Math.PI;
                else if (pyRes < 0.0) angle = Math.Atan(pyRes / pxRes) + (2 * Math.PI);
                else
                    angle = Math.Atan(pyRes / pxRes);
            }
            // Convert to degrees
            angle = angle * 180 / Math.PI;
            return angle;
        }


        /// <summary>
        /// Creates a bezier line unit.
        /// </summary>
        /// <param name="points">The connection points.</param>
        /// <returns></returns>
        private (PathGeometry LineGeometry, PathGeometry TouchGeometry, Point LastPoint) CreateBezierLineUnit(List<Point> points)
        {
            PathGeometry lineGeometry = new PathGeometry();
            PathGeometry touchGeometry = new PathGeometry();

            Point fp = points.First();
            Point lp = points.Last();

            double lineEdge = this.tickness * LineEdgeRatio;

            //Moves origin and destination point to create the necessary edge for arrows
            if (this.connectionType == ConnectionType.ArrowToAndFrom || this.connectionType == ConnectionType.ArrowTo)
            {
                lp = points[points.Count - 1];
                switch (this.destinationConnectorInfo.Orientation)
                {
                    case ConnectorOrientation.None:
                        break;
                    case ConnectorOrientation.Left:
                        points[points.Count - 1] = new Point(lp.X - lineEdge, lp.Y);
                        break;
                    case ConnectorOrientation.Top:
                        points[points.Count - 1] = new Point(lp.X, lp.Y - lineEdge);
                        break;
                    case ConnectorOrientation.Right:
                        points[points.Count - 1] = new Point(lp.X + lineEdge, lp.Y);
                        break;
                    case ConnectorOrientation.Bottom:
                        points[points.Count - 1] = new Point(lp.X, lp.Y + lineEdge);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (this.connectionType == ConnectionType.ArrowToAndFrom || this.connectionType == ConnectionType.ArrowFrom)
            {
                fp = points[0];
                switch (this.originConnectorInfo.Orientation)
                {
                    case ConnectorOrientation.None:
                        break;
                    case ConnectorOrientation.Left:
                        points[0] = new Point(fp.X - lineEdge, fp.Y);
                        break;
                    case ConnectorOrientation.Top:
                        points[0] = new Point(fp.X, fp.Y - lineEdge);
                        break;
                    case ConnectorOrientation.Right:
                        points[0] = new Point(fp.X + lineEdge, fp.Y);
                        break;
                    case ConnectorOrientation.Bottom:
                        points[0] = new Point(fp.X, fp.Y + lineEdge);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (points.Count > 0)
            {
                PathFigure lineFigure = new PathFigure {StartPoint = points[0]};
                PathFigure touchFigure = new PathFigure {StartPoint = points[0]};
                try
                {
#if !UseBezier //http://www.blackwasp.co.uk/WPFBezierSegments.aspx)

                QuadraticBezierSegment polyLineSegment = new QuadraticBezierSegment();
                QuadraticBezierSegment polyLineTouchSegment = new QuadraticBezierSegment();

                polyLineSegment.Point1 = points[2];
                polyLineSegment.Point2 = points[3];
                //polyLineSegment.Point3 = points[3];
                polyLineTouchSegment.Point1 = points[2];
                polyLineTouchSegment.Point2 = points[3];
                //polyLineTouchSegment.Point3 = points[3];
                lineFigure.Segments.Add(polyLineSegment);
                lineGeometry.Figures.Add(lineFigure);
                touchFigure.Segments.Add(polyLineTouchSegment);
                touchGeometry.Figures.Add(touchFigure);

#else
                    BezierSegment polyLineSegment = new BezierSegment();
                    BezierSegment polyLineTouchSegment = new BezierSegment();
                    if (points.Count == 4)
                    {
                        polyLineSegment.Point1 = points[1];
                        polyLineSegment.Point2 = points[2];
                        polyLineSegment.Point3 = points[3];
                        polyLineTouchSegment.Point1 = points[1];
                        polyLineTouchSegment.Point2 = points[2];
                        polyLineTouchSegment.Point3 = points[3];
                    }
                    else
                    {
                        polyLineSegment.Point1 = points[0];
                        polyLineSegment.Point2 = points[1];
                        polyLineSegment.Point3 = points[1];
                        polyLineTouchSegment.Point1 = points[0];
                        polyLineTouchSegment.Point2 = points[1];
                        polyLineTouchSegment.Point3 = points[1];
                    }

                    lineFigure.Segments.Add(polyLineSegment);
                    lineGeometry.Figures.Add(lineFigure);
                    touchFigure.Segments.Add(polyLineTouchSegment);
                    touchGeometry.Figures.Add(touchFigure);
#endif
                }
                catch
                {
                }
            }

            points[0] = fp;
            points[points.Count - 1] = lp;
            return (lineGeometry, touchGeometry, points.LastOrDefault());
        }

        /// <summary>
        /// Creates the diagram origin geometry.
        /// </summary>
        /// <returns></returns>
        private Geometry CreateOriginGeometry()
        {
            List<Point> points = this.pathFinder.GetConnectionLine(this.originConnectorInfo, this.originConnectorInfo.Position, this.originConnectorInfo.Orientation);
            var diagramLineUnit = this.CreateRoutedLineUnit(points);
            return diagramLineUnit.LineGeometry;
        }

        /// <summary>
        /// Creates a routed line geometry
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns></returns>
        private (PathGeometry LineGeometry, PathGeometry TouchGeometry, Point LastPoint) CreateRoutedLineUnit(List<Point> points)
        {
            PathGeometry lineGeometry = new PathGeometry();
            PathGeometry touchGeometry = new PathGeometry();

            Point fp = points.First();
            Point lp = points.Last();
            double lineEdge = 20;


            if (this.connectionType == ConnectionType.ArrowToAndFrom || this.connectionType == ConnectionType.ArrowTo)
            {
                lp = points[points.Count - 1];
                switch (this.destinationConnectorInfo.Orientation)
                {
                    case ConnectorOrientation.None:
                        break;
                    case ConnectorOrientation.Left:
                        points[points.Count - 1] = new Point(lp.X - lineEdge, lp.Y);
                        break;
                    case ConnectorOrientation.Top:
                        points[points.Count - 1] = new Point(lp.X, lp.Y - lineEdge);
                        break;
                    case ConnectorOrientation.Right:
                        points[points.Count - 1] = new Point(lp.X + lineEdge, lp.Y);
                        break;
                    case ConnectorOrientation.Bottom:
                        points[points.Count - 1] = new Point(lp.X, lp.Y + lineEdge);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (this.connectionType == ConnectionType.ArrowToAndFrom || this.connectionType == ConnectionType.ArrowFrom)
            {
                fp = points[0];
                switch (this.originConnectorInfo.Orientation)
                {
                    case ConnectorOrientation.None:
                        break;
                    case ConnectorOrientation.Left:
                        points[0] = new Point(fp.X - lineEdge, fp.Y);
                        break;
                    case ConnectorOrientation.Top:
                        points[0] = new Point(fp.X, fp.Y - lineEdge);
                        break;
                    case ConnectorOrientation.Right:
                        points[0] = new Point(fp.X + lineEdge, fp.Y);
                        break;
                    case ConnectorOrientation.Bottom:
                        points[0] = new Point(fp.X, fp.Y + lineEdge);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (points.Count > 0)
            {
                PathFigure lineFigure = new PathFigure {StartPoint = points[0]};
                PathFigure touchFigure = new PathFigure {StartPoint = points[0]};

                PolyLineSegment polyLineSegment = new PolyLineSegment();
                PolyLineSegment polyLineTouchSegment = new PolyLineSegment();

                foreach (Point point in points.Skip(1))
                {
                    polyLineSegment.Points.Add(point);
                    polyLineTouchSegment.Points.Add(point);
                }

                lineFigure.Segments.Add(polyLineSegment);
                lineGeometry.Figures.Add(lineFigure);
                touchFigure.Segments.Add(polyLineTouchSegment);
                touchGeometry.Figures.Add(touchFigure);
            }

            points[0] = fp;
            points[points.Count - 1] = lp;

            return (lineGeometry, touchGeometry, points.LastOrDefault());
        }


        private double GetBezierCurveAngle(List<Point> points)
        {
            double x = 0, y = 0;
            double xold = 0, yold = 0;
            double angle = 0;

            points.RemoveAt(1);

            for (double t = 0.99; t < 1.001; t += 0.01)
            {
                x = ((1 - t) * (1 - t) * points[0].X) + (2 * t * (1 - t) * points[1].X) + (t * t * points[2].X);
                //this statement is used to determine the x coordinate of the curve. 

                y = ((1 - t) * (1 - t) * points[0].Y) + (2 * t * (1 - t) * points[1].Y) + (t * t * points[2].Y);
                //this statement is used to determine the y coordinate of the curve. 

                x = Math.Round(x, 3);
                y = Math.Round(y, 3);
                Point oldPoint = new Point(xold, yold);
                Point newPoint = new Point(x, y);
                angle = Math.Round(this.Angle(xold, yold, x, y), 3);
                xold = x;
                yold = y;
            }

            return angle;
        }


        private Point GetBezierMidPoint(List<Point> points)
        {
            double x = 0, y = 0;


            points.RemoveAt(0);

            double t = 0.5;


            x = ((1 - t) * (1 - t) * points[0].X) + (2 * t * (1 - t) * points[1].X) + (t * t * points[2].X);
            //this statement is used to determine the x coordinate of the curve. 

            y = ((1 - t) * (1 - t) * points[0].Y) + (2 * t * (1 - t) * points[1].Y) + (t * t * points[2].Y);
            //this statement is used to determine the y coordinate of the curve. 

            x = Math.Round(x, 3);
            y = Math.Round(y, 3);

            return new Point(x, y);
        }

        /// <summary>
        /// Defines the points descibing the polygon used for connection ends.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="isStart">if set to <c>true</c> indicates that points refers to connection start</param>
        /// <param name="orientation">The orientation.</param>
        /// <returns></returns>
        private PointCollection GetConnectionEndPoints(Point origin, bool isStart, ConnectorOrientation orientation)
        {
            PointCollection points = new PointCollection();
            double zoom = this.isSelected ? SelectionLineZoom * 0.6 : 1;
            double connectionEndsSize = this.tickness * 5; //Size of arrows

            switch (orientation)
            {
                case ConnectorOrientation.None:
                    return points;
                case ConnectorOrientation.Left:
                    points.Add(new Point(origin.X, origin.Y));
                    points.Add(new Point((origin.X - connectionEndsSize * zoom), origin.Y - (connectionEndsSize * zoom) / 2));
                    points.Add(new Point((origin.X - connectionEndsSize * zoom), origin.Y + (connectionEndsSize * zoom) / 2));
                    break;
                case ConnectorOrientation.Top:
                    points.Add(new Point(origin.X, origin.Y));
                    points.Add(new Point(origin.X - (connectionEndsSize * zoom) / 2, origin.Y - (connectionEndsSize * zoom)));
                    points.Add(new Point(origin.X + (connectionEndsSize * zoom) / 2, origin.Y - (connectionEndsSize * zoom)));
                    break;
                case ConnectorOrientation.Right:
                    points.Add(new Point(origin.X, origin.Y));
                    points.Add(new Point((origin.X + connectionEndsSize * zoom), origin.Y - (connectionEndsSize * zoom) / 2));
                    points.Add(new Point((origin.X + connectionEndsSize * zoom), origin.Y + (connectionEndsSize * zoom) / 2));
                    break;
                case ConnectorOrientation.Bottom:
                    points.Add(new Point(origin.X, origin.Y));
                    points.Add(new Point(origin.X - (connectionEndsSize * zoom) / 2, origin.Y + (connectionEndsSize * zoom)));
                    points.Add(new Point(origin.X + (connectionEndsSize * zoom) / 2, origin.Y + (connectionEndsSize * zoom)));
                    break;
            }

            return points;
        }

        /// <summary>
        /// Called when diagram is double tapped.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoubleTappedRoutedEventArgs"/> instance containing the event data.</param>
        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.IsHighlighted = false;
            this.IsSelected = false;
            e.Handled = true;
        }


        /// <summary>
        /// Updates the connection end points.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        private void UpdateConnectionEnds(Point startPoint, Point endPoint)
        {
            this.arrowEnd.Visibility = Visibility.Collapsed;
            this.arrowStart.Visibility = Visibility.Collapsed;

            //Arrow Start
            if (this.ConnectionType == ConnectionType.ArrowFrom || this.ConnectionType == ConnectionType.ArrowToAndFrom)
            {
                this.arrowStart.Points = this.GetConnectionEndPoints(startPoint, true, this.originConnectorInfo.Orientation);
                this.arrowStart.Visibility = Visibility.Visible;
            }

            //End
            if (this.ConnectionType == ConnectionType.ArrowTo || this.ConnectionType == ConnectionType.ArrowToAndFrom)
            {
                this.arrowEnd.Points = this.GetConnectionEndPoints(endPoint, false, this.destinationConnectorInfo.Orientation);
                this.arrowEnd.Visibility = Visibility.Visible;
            }

            if (!this.ConnectionEnds.Contains(this.arrowEnd)) this.ConnectionEnds.Add(this.arrowEnd);
            if (!this.ConnectionEnds.Contains(this.arrowStart)) this.ConnectionEnds.Add(this.arrowStart);
        }
    }
}