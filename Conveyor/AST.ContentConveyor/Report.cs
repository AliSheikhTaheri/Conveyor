namespace AST.ContentConveyor
{
    public class Report
    {
        public Report(int nodeId, ActionTypes actionType, ObjectTypes objectType)
        {
            NodeId = nodeId;
            ActionType = actionType;
            ObjectType = objectType;
        }

        public int NodeId { get; private set; }

        public ActionTypes ActionType { get; private set; }

        public ObjectTypes ObjectType { get; private set; }
    }
}
