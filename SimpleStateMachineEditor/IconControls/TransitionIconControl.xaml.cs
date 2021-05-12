using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleStateMachineEditor.IconControls
{
    /// <summary>
    /// Interaction logic for TransitionIconControl.xaml
    /// </summary>
    public partial class TransitionIconControl : UserControl, INotifyPropertyChanged
    {
        DesignerControl Designer;
        ViewModel.Transition Transition;
        public double TextRotationAngle 
        {
            get => _textRotationAngle;
            private set
            {
                if (_textRotationAngle != value)
                {
                    _textRotationAngle = value;
                    OnPropertyChanged("TextRotationAngle");
                }
            }
        }
        double _textRotationAngle;
        public Point MidpointOfArc
        {
            get => _midpointOfArc;
            set
            {
                if (_midpointOfArc.X != value.X || _midpointOfArc.Y != value.Y)
                {
                    _midpointOfArc = value;
                    OnPropertyChanged("MidpointOfArc");
                }
            }
        }
        Point _midpointOfArc;
        public Point TextVirtualCenterPoint
        {
            get => _textVirtualCenterPoint;
            set
            {
                if (_textVirtualCenterPoint.X != value.X || _textVirtualCenterPoint.Y != value.Y)
                {
                    _textVirtualCenterPoint = value;
                    OnPropertyChanged("TextVirtualCenterPoint");
                }
            }
        }
        Point _textVirtualCenterPoint;
        public event PropertyChangedEventHandler PropertyChanged;




        public TransitionIconControl(DesignerControl designer, ViewModel.Transition transition)
        {
            Designer = designer;
            Transition = transition;

            InitializeComponent();

            Loaded += TransitionIconControl_LoadedHandler;
            Unloaded += TransitionIconControl_UnloadedHandler;
        }

        internal void Draw()
        {
            if (Transition.SourceState == null || Transition.DestinationState == null)
            {
                return;
            }

            DrawConnector();
            PositionText();
        }

        private PathFigure DrawArc()
        {
            PathFigure arc;

            double P1x = Designer.LoadedIcons[Transition.SourceState].CenterPosition.X;
            double P1y = Designer.LoadedIcons[Transition.SourceState].CenterPosition.Y;

            double P2x = Designer.LoadedIcons[Transition.DestinationState].CenterPosition.X;
            double P2y = Designer.LoadedIcons[Transition.DestinationState].CenterPosition.Y;

            if (Transition.SourceState == Transition.DestinationState)
            {
                TextRotationAngle = 0;

                int index = Transition.SourceState.GetRelativePeerPosition(Transition);
                double height = 25 * (index + 1);

                arc = new PathFigure()
                {
                    StartPoint = new Point(P1x - 1, P1y),
                };
                arc.Segments.Add(new ArcSegment()
                {
                    IsLargeArc = true,
                    Point = new Point(P2x + 1, P2y),
                    RotationAngle = 0,
                    Size = new Size(30, height),
                    SweepDirection = SweepDirection.Clockwise,
                });
                arc = arc.GetFlattenedPathFigure();
            }
            else
            {
                double angle = Math.Atan((P2y - P1y) / (P2x - P1x)) / 3.14159 * 180;
                TextRotationAngle = angle;
                int index = Transition.SourceState.GetRelativePeerPosition(Transition);
                double width = 350 * ((index + 1) / 2);
                SweepDirection direction = index % 2 == 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

                if (Transition.SourceState.Id > Transition.DestinationState.Id)
                {
                    //  Reversed
                    direction = direction == SweepDirection.Clockwise ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
                }

                arc = new PathFigure()
                {
                    StartPoint = new Point(P1x, P1y),
                };
                Size pathSize = new Size(Utility.DrawingAids.Distance(P1x, P1y, P2x, P2y), width);
                arc.Segments.Add(new ArcSegment()
                {
                    IsLargeArc = false,
                    Point = new Point(P2x, P2y),
                    RotationAngle = angle,
                    Size = pathSize,
                    SweepDirection = direction,
                });
                arc = arc.GetFlattenedPathFigure(/*3, ToleranceType.Relative*/);
            }

            return arc;
        }

        private void DrawConnector()
        {
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure arc = DrawArc();
            double arcLength = Utility.DrawingAids.LengthOfFigure(arc);
            pathGeometry.Figures.Add(arc);
            pathGeometry.GetPointAtFractionLength(0.5, out Point midpointOfArc, out Point uselessTangent);
            MidpointOfArc = midpointOfArc;
            ConnectorPath.Data = pathGeometry;
            pathGeometry.GetPointAtFractionLength((arcLength - Icons.StateIcon.Radius * 1.5) / arcLength, out Point startPoint, out uselessTangent);
            pathGeometry.GetPointAtFractionLength((arcLength - Icons.StateIcon.Radius) / arcLength, out Point endPoint, out uselessTangent);
            pathGeometry.Figures.Add(Utility.DrawingAids.DrawArrowHead(startPoint, endPoint));
            ConnectorPath.Data = pathGeometry;
        }

        private void EndpointChangedHandler(object sender, EventArgs e)
        {
            if (Transition.SourceState != null && Transition.DestinationState != null)
            {
                Draw();
            }
        }

        private void EndpointPositionChangedHandler(object sender, EventArgs e)
        {
            if (Transition.SourceState != null && Transition.DestinationState != null)
            {
                Draw();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PositionText()
        {
            if (IsMeasureValid)
            {
                TextVirtualCenterPoint = Utility.DrawingAids.NormalizePoint(TextGrid, PositioningGridCenter, new Point(0, 0));
                Point deltaPosition = new Point(Math.Round(MidpointOfArc.X - TextVirtualCenterPoint.X),
                                                Math.Round(MidpointOfArc.Y - TextVirtualCenterPoint.Y));
                TextGrid.Margin = new Thickness(deltaPosition.X, deltaPosition.Y, 0, 0);
            }
            else
            {
                LayoutUpdated += TransitionIconControl_LayoutUpdatedHandler;
            }
        }

        private void TransitionIconControl_LayoutUpdatedHandler(object sender, EventArgs e)
        {
            LayoutUpdated -= TransitionIconControl_LayoutUpdatedHandler;
            PositionText();
        }

        private void TransitionIconControl_LoadedHandler(object sender, RoutedEventArgs e)
        {

            Transition.EndpointChanged += EndpointChangedHandler;
            Transition.EndpointPositionChanged += EndpointPositionChangedHandler;

            if (Transition.SourceState != null && Transition.DestinationState != null)
            {
                Draw();
            }

            TextGrid.SizeChanged += TriggerEventName_SizeChangedHandler;
        }

        private void TransitionIconControl_UnloadedHandler(object sender, RoutedEventArgs e)
        {
            Transition.EndpointChanged -= EndpointChangedHandler;
            Transition.EndpointPositionChanged -= EndpointPositionChangedHandler;
            TextGrid.SizeChanged -= TriggerEventName_SizeChangedHandler;
        }

        private void TriggerEventName_SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            PositionText();
        }
    }
}
