using System;
using System.Collections.Generic;

namespace Reactive.Yoga {
    public class YogaLayoutController : ReactiveLayoutController {
        #region Properties

        public Overflow Overflow {
            get => _overflow;
            set {
                _overflow = value;
                YogaNode.StyleSetOverflow(_overflow);
                Refresh();
            }
        }

        public Direction Direction {
            get => _direction;
            set {
                _direction = value;
                YogaNode.StyleSetDirection(_direction);
                Refresh();
            }
        }

        public FlexDirection FlexDirection {
            get => _flexDirection;
            set {
                _flexDirection = value;
                YogaNode.StyleSetFlexDirection(_flexDirection);
                Refresh();
            }
        }

        public Wrap FlexWrap {
            get => _flexWrap;
            set {
                _flexWrap = value;
                YogaNode.StyleSetFlexWrap(_flexWrap);
                Refresh();
            }
        }

        public Justify JustifyContent {
            get => _justifyContent;
            set {
                _justifyContent = value;
                YogaNode.StyleSetJustifyContent(_justifyContent);
                Refresh();
            }
        }

        public Align AlignItems {
            get => _alignItems;
            set {
                _alignItems = value;
                YogaNode.StyleSetAlignItems(_alignItems);
                Refresh();
            }
        }

        public Align AlignContent {
            get => _alignContent;
            set {
                _alignContent = value;
                YogaNode.StyleSetAlignContent(_alignContent);
                Refresh();
            }
        }

        public YogaFrame Padding {
            get => _padding;
            set {
                _padding = value;
                RefreshPadding();
                Refresh();
            }
        }

        public YogaVector Gap {
            get => _gap;
            set {
                _gap = value;
                RefreshGap();
                Refresh();
            }
        }

        private Overflow _overflow = Overflow.Visible;
        private Direction _direction = Direction.Inherit;
        private FlexDirection _flexDirection = FlexDirection.Row;
        private Justify _justifyContent = Justify.FlexStart;
        private Align _alignItems = Align.FlexStart;
        private Align _alignContent = Align.Auto;
        private Wrap _flexWrap = Wrap.Wrap;
        private YogaFrame _padding = YogaFrame.Zero;
        private YogaVector _gap = YogaVector.Undefined;

        private void RefreshGap() {
            YogaNode.StyleSetGap(Gutter.Row, _gap.y);
            YogaNode.StyleSetGap(Gutter.Column, _gap.x);
        }

        private void RefreshPadding() {
            YogaNode.StyleSetPadding(Edge.Top, _padding.top);
            YogaNode.StyleSetPadding(Edge.Bottom, _padding.bottom);
            YogaNode.StyleSetPadding(Edge.Left, _padding.left);
            YogaNode.StyleSetPadding(Edge.Right, _padding.right);
        }

        private void RefreshAllProperties() {
            YogaNode.StyleSetOverflow(_overflow);
            YogaNode.StyleSetDirection(_direction);
            YogaNode.StyleSetFlexDirection(_flexDirection);
            YogaNode.StyleSetFlexWrap(_flexWrap);
            YogaNode.StyleSetJustifyContent(_justifyContent);
            YogaNode.StyleSetAlignItems(_alignItems);
            YogaNode.StyleSetAlignContent(_alignContent);
            RefreshGap();
            RefreshPadding();
        }

        #endregion

        #region Context

        public sealed override Type ContextType { get; } = typeof(YogaContext);

        public sealed override object CreateContext() => new YogaContext();

        public sealed override void ProvideContext(object? context) {
            if (context == null) {
                if (_contextNode.GetIsInitialized()) {
                    _contextNode!.RemoveAllChildren();
                }
                _contextNode = default;
                return;
            }
            var c = (YogaContext)context;
            _contextNode = c.YogaNode;
            RefreshAllProperties();
        }

        #endregion

        #region Calculations

        public bool UseIndependentLayout {
            get => _useIndependentLayout;
            set {
                var oldNode = YogaNode;
                _useIndependentLayout = value;
                var newNode = YogaNode;
                ReloadChildrenInternal(newNode, oldNode);
                RefreshAllProperties();
            }
        }

        internal YogaNode YogaNode {
            get {
                YogaNode? node;
                if (UseIndependentLayout) {
                    _layoutNode ??= new();
                    node = _layoutNode;
                } else {
                    node = _contextNode;
                }
                if (node is not { IsInitialized: true }) {
                    throw new Exception("Node was not initialized");
                }
                return node;
            }
        }

        private bool _useIndependentLayout;
        private YogaNode? _layoutNode;
        private YogaNode? _contextNode;

        public sealed override void Recalculate(bool root) {
            ReloadChildrenVisibility();
            if (!root && !UseIndependentLayout) return;
            RecalculateInternal();
        }

        protected virtual void RecalculateInternal() {
            YogaNode.CalculateLayout(Rect.width, Rect.height, Direction);
        }

        public sealed override void ApplyChildren() {
            foreach (var (child, node) in _nodes) {
                child.ApplyTransforms(x => node.ApplyTo(x));
            }
        }

        private void ReloadChildrenVisibility() {
            foreach (var (child, node) in _nodes) {
                node.StyleSetDisplay(child.WithinLayout ? Display.Flex : Display.None);
            }
        }

        public override void ApplySelf(ILayoutItem item) { }

        #endregion

        #region Children

        private readonly Dictionary<ILayoutItem, YogaNode> _nodes = new();
        private IEnumerable<ILayoutItem>? _children;

        public sealed override void ReloadChildren(IEnumerable<ILayoutItem> children) {
            _children = children;
            ReloadChildrenInternal(YogaNode, null);
        }

        private protected virtual void ReloadChildrenInternal(YogaNode node, YogaNode? fromNode) {
            if (_children == null) return;
            fromNode?.RemoveAllChildren();
            node.RemoveAllChildren();
            _nodes.Clear();
            var index = 0;
            foreach (var child in _children) {
                if (child.LayoutModifier is not YogaModifier modifier) continue;
                node.InsertChild(modifier.YogaNode, index);
                _nodes[child] = modifier.YogaNode;
                index++;
            }
        }

        #endregion
    }
}