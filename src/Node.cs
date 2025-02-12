using System.Collections.Generic;
using Dec;

namespace Arbor
{
    public enum Result
    {
        Success,
        Working,
        Failure,
    }

    public abstract partial class Node : Dec.IRecordable
    {
        private IEnumerator<Result> currentWorker;
        internal Dictionary<Arbor.BaseEventDec, List<System.Delegate>> eventActions;
        internal Dictionary<Arbor.BasePropertyDec, object> properties;

        private bool initted;

        public void Init()
        {
            if (initted)
            {
                Dbg.Err("Initted multiple times");
            }

            InitFields();

            initted = true;
        }

        public Result Update()
        {
            if (!initted)
            {
                Dbg.Err("Not initted");
            }

            var tree = Tree.Current.Value;
            tree.stack.Add(this);

            // get it in the tree in the right order
            int activeIndex = tree.active.Count;
            tree.active.Add(this);

            bool moved;
            try
            {
                if (currentWorker == null)
                {
                    currentWorker = Worker().GetEnumerator();
                }

                moved = currentWorker.MoveNext();
            }
            catch (System.Exception e)
            {
                Dbg.Ex(e);
                moved = false;
            }
            tree.stack.RemoveAt(tree.stack.Count - 1);

            if (!moved)
            {
                Dbg.Err("Worker didn't exit properly, assuming failure");
                try
                {
                    Reset();
                }
                catch (System.Exception e)
                {
                    Dbg.Ex(e);
                }

                tree.active[activeIndex] = null; // nope, not active anymore
                return Result.Failure;
            }

            var result = currentWorker.Current;
            if (result != Result.Working)
            {
                // we done now
                try
                {
                    Reset();
                }
                catch (System.Exception e)
                {
                    Dbg.Ex(e);
                }
                tree.active[activeIndex] = null; // nope, not active anymore
            }

            return result;
        }

        internal void EventAttach_Internal(Arbor.BaseEventDec eve, System.Delegate deleg)
        {
            if (eventActions == null)
            {
                eventActions = new Dictionary<Arbor.BaseEventDec, List<System.Delegate>>();
            }

            if (!eventActions.TryGetValue(eve, out var actions))
            {
                actions = new List<System.Delegate>();
                eventActions[eve] = actions;
            }

            actions.Add(deleg);
        }

        internal void PropertyAttach_Internal(Arbor.BasePropertyDec eve, object data)
        {
            if (properties == null)
            {
                properties = new Dictionary<Arbor.BasePropertyDec, object>();
            }

            properties[eve] = data;
        }

        public abstract IEnumerable<Result> Worker();

        public virtual void Reset()
        {
            // already done, stop recursiving
            if (currentWorker == null)
            {
                return;
            }

            currentWorker = null;
            ResetFields();
        }

        public virtual void InitFields() { }
        public virtual void ResetFields() { }

        public virtual void Record(Recorder recorder)
        {
            recorder.Record(ref currentWorker, nameof(currentWorker));
            recorder.Record(ref eventActions, nameof(eventActions));
            recorder.Record(ref properties, nameof(properties));

            if (recorder.Mode == Recorder.Direction.Read)
            {
                // we actually just mark this as initted in the hopes that the behavior tree is set up properly; this all needs to vanish when we kill serializable behavior trees
                initted = true;
            }
        }
    }
}
