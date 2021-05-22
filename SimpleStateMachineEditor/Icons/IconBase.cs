using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SimpleStateMachineEditor.Icons
{
    internal abstract class IconBase : INotifyPropertyChanged, IOleCommandTarget
    {
        internal const int HoverDelay = 250;     // Milliseconds

        public abstract int ContextMenuId { get; }
        internal DesignerControl Designer { get; set; }
        public virtual Size Size { get; set; }
        public ObjectModel.TrackableObject ReferencedObject { get; private set; }
        public virtual Control Body 
        {
            get => _body;
            internal set
            {
                _body = value;
                if (_body != null)
                {
                    if (_body.IsLoaded)
                    {
                        OnBodyLoaded(_body, null);
                    }
                    else
                    {
                        _body.Loaded += OnBodyLoaded;
                    }
                }
            }
        }
        Control _body;
        public FrameworkElement DraggableShape { get; set; }
        protected System.Windows.Point ContextMenuActivationLocation { get; private set; }
        DispatcherTimer MouseHoverTimer;
        public bool IsHidden
        {
            get => _isHidden;
            set
            {
                if (_isHidden != value)
                {
                    _isHidden = value;
                    OnPropertyChanged("IsHidden");
                }
            }
        }
        bool _isHidden;
        public bool IsHovering
        {
            get => _isHovering;
            set
            {
                if (_isHovering != value)
                {
                    bool wasHovering = _isHovering;
                    _isHovering = value;
                    OnPropertyChanged("IsHovering");
                    if (wasHovering && !_isHovering)
                    {
                        OnHoverEnd();
                    }
                }
            }
        }
        bool _isHovering;
        public event PropertyChangedEventHandler PropertyChanged;



        protected IconBase(DesignerControl designer, ObjectModel.TrackableObject referencedObject, System.Windows.Point? center, Size? size)
        {
            Designer = designer;
            ReferencedObject = referencedObject;
            if (size.HasValue)
            {
                Size = size.Value;
            }

            Body = CreateIcon();
            if (center.HasValue)
            {
                CenterPosition = center.Value;
            }

            DraggableShape = CreateDraggableShape();

        }

        public double Bottom => ReferencedObject is ObjectModel.PositionableObject positionableObject ? positionableObject.LeftTopPosition.Y + Size.Height : throw new InvalidOperationException();

        public virtual Point CenterPosition
        {
            get => ReferencedObject is ObjectModel.PositionableObject positionableObject ? new Point(positionableObject.LeftTopPosition.X + Size.Width / 2, positionableObject.LeftTopPosition.Y + Size.Height / 2) : throw new InvalidOperationException();
            set
            {
                if (ReferencedObject is ObjectModel.PositionableObject positionableObject)
                {
                    positionableObject.LeftTopPosition = new System.Windows.Point(Math.Max(0, value.X - Size.Width / 2), Math.Max(0, value.Y - Size.Height / 2));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        protected abstract Control CreateIcon();

        protected abstract FrameworkElement CreateDraggableShape();

        public virtual int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                switch (nCmdID)
                {
                    case PackageIds.DeleteCommandId:
                        Designer.DeleteSelectedIcons();
                        return VSConstants.S_OK;
                    default:
                        return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
                }
            }
            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_UNKNOWNGROUP;
        }

        private void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (!IsHovering)
            {
                MouseHoverTimer.Start();
            }
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            MouseHoverTimer.Stop();
            IsHovering = false;
        }

        protected virtual void OnBodyLoaded(object sender, RoutedEventArgs e)
        {
            Body.Unloaded += OnBodyUnloaded;
            Body.MouseEnter += MouseEnterHandler;
            Body.MouseLeave += MouseLeaveHandler;
            ReferencedObject.PropertyChanged += ReferencedObject_PropertyChangedHandler;

            MouseHoverTimer = new DispatcherTimer(DispatcherPriority.Normal);
            MouseHoverTimer.Interval = new TimeSpan(0, 0, 0, 0, HoverDelay);
            MouseHoverTimer.Tick += OnHover;

            if (ReferencedObject is ObjectModel.PositionableObject positionableObject)
            {
                Body.Margin = new Thickness(positionableObject.LeftTopPosition.X, positionableObject.LeftTopPosition.Y, 0, 0);
            }
        }

        protected virtual void OnBodyUnloaded(object sender, RoutedEventArgs e)
        {
            Body.Unloaded -= OnBodyUnloaded;
            Body.MouseEnter -= MouseEnterHandler;
            Body.MouseLeave -= MouseLeaveHandler;
            ReferencedObject.PropertyChanged -= ReferencedObject_PropertyChangedHandler;
        }

        protected virtual void OnHover(object sender, EventArgs e)
        {
            MouseHoverTimer.Stop();
            IsHovering = true;
        }

        protected virtual void OnHoverEnd() { }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                return VSConstants.S_OK;
            }
            else
            {
                return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_UNKNOWNGROUP;
            }
        }

        public double Right => ReferencedObject is ObjectModel.PositionableObject positionableObject ? positionableObject.LeftTopPosition.X + Size.Width : throw new InvalidOperationException();

        private void ReferencedObject_PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LeftTopPosition")
            {
                if (ReferencedObject is ObjectModel.PositionableObject positionableObject)
                {
                    Body.Margin = new Thickness(positionableObject.LeftTopPosition.X, positionableObject.LeftTopPosition.Y, 0, 0);
                }
                else
                {
                    throw new InvalidProgramException();
                }
            }
        }

        public override string ToString()
        {
            return (ReferencedObject is ObjectModel.NamedObject namedObject) ? namedObject.ToString() : GetType().ToString();
        }
    }
}
