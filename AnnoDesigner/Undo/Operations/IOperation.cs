namespace AnnoDesigner.Undo.Operations
{
    public interface IOperation
    {
        void Undo();
        void Redo();
    }
}
