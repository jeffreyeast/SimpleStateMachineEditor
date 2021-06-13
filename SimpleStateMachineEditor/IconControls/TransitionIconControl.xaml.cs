using Microsoft.VisualStudio;
using SimpleStateMachineEditor.ObjectModel;
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
    public partial class TransitionIconControl : UserControl, INotifyPropertyChanged, ObjectModel.IRemovableObject
    {
        DesignerControl Designer;
        ObjectModel.ITransition Transition;
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
        public event RemovingHandler Removing;




        internal TransitionIconControl(DesignerControl designer, ObjectModel.ITransition transition)
        {
            Designer = designer;
            Transition = transition;

            InitializeComponent();

            Loaded += TransitionIconControl_LoadedHandler;
        }

        private void DragEnter_Handler(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (Transition is ViewModel.Transition &&
                e.Data.GetFormats().Contains(typeof(Icons.ToolWindowActionIcon).ToString()))
            {
                Icons.ToolWindowActionIcon icon = e.Data.GetData(typeof(Icons.ToolWindowActionIcon)) as Icons.ToolWindowActionIcon;
                if (icon.Action.Controller == Designer.Model)
                {
                    e.Effects = DragDropEffects.Copy;
                    (DataContext as Icons.TransitionIcon).IsDropCandidate = true;
                }
            }
            e.Handled = true;
        }

        private void DragLeave_Handler(object sender, DragEventArgs e)
        {
            (DataContext as Icons.TransitionIcon).IsDropCandidate = false;
        }

        private void DragOver_Handler(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (Transition is ViewModel.Transition &&
                e.Data.GetFormats().Contains(typeof(Icons.ToolWindowActionIcon).ToString()))
            {
                Icons.ToolWindowActionIcon icon = e.Data.GetData(typeof(Icons.ToolWindowActionIcon)) as Icons.ToolWindowActionIcon;
                if (icon.Action.Controller == Designer.Model)
                {
                    e.Effects = DragDropEffects.Copy;
                }
            }
            e.Handled = true;
        }

        internal void Draw()
        {
            if (!Designer.LoadedIcons.ContainsKey(Transition.SourceState) || !Designer.LoadedIcons.ContainsKey(Transition.DestinationState))
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

            if (Math.Abs(P1x - P2x) < Icons.StateIcon.IconSize.Width && Math.Abs(P1y - P2y) < Icons.StateIcon.IconSize.Height)
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
            pathGeometry.GetPointAtFractionLength((arcLength - Designer.LoadedIcons[Transition.DestinationState].Radius * 1.5) / arcLength, out Point startPoint, out uselessTangent);
            pathGeometry.GetPointAtFractionLength((arcLength - Designer.LoadedIcons[Transition.DestinationState].Radius) / arcLength, out Point endPoint, out uselessTangent);
            pathGeometry.Figures.Add(Utility.DrawingAids.DrawArrowHead(startPoint, endPoint));
            ConnectorPath.Data = pathGeometry;
        }

        private void Drop_Handler(object sender, DragEventArgs e)
        {
            if (Transition is ViewModel.Transition)
            {
                Icons.ToolWindowActionIcon action = e.Data.GetData(typeof(Icons.ToolWindowActionIcon).ToString()) as Icons.ToolWindowActionIcon;

                if (action != null && action.Action.Controller == Designer.Model)
                {
                    ObjectModel.ITransitionEndpoint originState = Designer.LoadedIcons[Transition.SourceState].CenterPosition.X < Designer.LoadedIcons[Transition.DestinationState].CenterPosition.X ? Transition.SourceState : Transition.DestinationState;
                    (DataContext as Icons.TransitionIcon).ProcessDroppedAction(action.Action, originState, e.GetPosition(Designer.LoadedIcons[originState].Body), true);
                    e.Handled = true;
                }
                (DataContext as Icons.TransitionIcon).IsDropCandidate = false;
            }
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

        private void OnRemoving()
        {
            if (Transition is ViewModel.Transition t)
            {
                t.EndpointChanged -= EndpointChangedHandler;
            }
            Transition.EndpointPositionChanged -= EndpointPositionChangedHandler;
            TextGrid.SizeChanged -= TriggerEventName_SizeChangedHandler;
            Removing?.Invoke(this);
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
            Loaded -= TransitionIconControl_LoadedHandler;
            if (Transition is ViewModel.Transition t)
            {
                t.EndpointChanged += EndpointChangedHandler;
            }
            Transition.EndpointPositionChanged += EndpointPositionChangedHandler;

            if (Transition.SourceState != null && Transition.DestinationState != null)
            {
                Draw();
            }

            TextGrid.SizeChanged += TriggerEventName_SizeChangedHandler;
        }

        private void TriggerEventName_SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            PositionText();
        }

        private void ActionIconLoadedHandler(object sender, RoutedEventArgs e)
        {
            if (sender is ListBoxItem listBoxItem && listBoxItem.Content is Icons.ActionReferenceIcon actionReferenceIcon)
            {
                actionReferenceIcon.ListBoxItem = listBoxItem;
            }
        }

        public void Remove()
        {
            OnRemoving();
        }
    }
}
