
namespace Arbor
{
    internal struct BlackboardIdentifier : Dec.IRecordable
    {
        public string bb;
        public string id;

        public void Record(Dec.Recorder recorder)
        {
            recorder.Record(ref bb, nameof(bb));
            recorder.Record(ref id, nameof(id));
        }
    }

    public struct BlackboardParameter<T> : Dec.IRecordable
    {
        BlackboardIdentifier identifier;

        public static BlackboardParameter<T> Tree(string id)
        {
            return new BlackboardParameter<T> { identifier = new BlackboardIdentifier{ bb = "tree", id = id } };
        }
        public static BlackboardParameter<T> Global(string id)
        {
            return new BlackboardParameter<T> { identifier = new BlackboardIdentifier { bb = "global", id = id } };
        }
        public static BlackboardParameter<T> Specific(string bbid, string itemid)
        {
            return new BlackboardParameter<T> { identifier = new BlackboardIdentifier { bb = bbid, id = itemid } };
        }

        public T Get()
        {
            return Arbor.Tree.Current.Value.BlackboardGet<T>(identifier);
        }

        public void Set(T value)
        {
            Arbor.Tree.Current.Value.BlackboardSet<T>(identifier, value);
        }

        public void Register()
        {
            Arbor.Tree.Current.Value.Register<T>(identifier);
        }

        public void RegisterWith(Tree tree)
        {
            tree.Register<T>(identifier);
        }

        public void Record(Dec.Recorder recorder)
        {
            recorder.RecordAsThis(ref identifier);
        }
    }
}
