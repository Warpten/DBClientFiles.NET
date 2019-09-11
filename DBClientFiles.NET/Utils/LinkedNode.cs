using System.Runtime.CompilerServices;

namespace DBClientFiles.NET.Utils
{
    internal sealed class LinkedNode<T>
    {
        public T Node { get; set; }

        private LinkedNode<T> _previous;
        private LinkedNode<T> _next;
        public LinkedNode<T> Previous
        {
            get => _previous;
            set
            {
                if (_previous != null)
                {
                    _previous._next = value;
                    value._previous = _previous;
                }

                value._next = this;
                _previous = value;
            }
        }

        public LinkedNode<T> Next
        {
            get => _next;
            set
            {
                if (_next != null)
                {
                    _next._previous = value;
                    value._next = _next;
                }

                // Update our node.
                value._previous = this;
                _next = value;
            }
        }
    }

    internal static class LinkedNodeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LinkedNode<T> AsLinkedNode<T>(this T node)
            => new LinkedNode<T>() {
                Node = node
            };
    
        public static LinkedNode<T> Then<T>(this LinkedNode<T> node, T next)
        {
            var nextNode = new LinkedNode<T>() {
                Node = next
            };
            node.Next = nextNode;
            return nextNode;
        }
    }
}
