namespace Functions
{
    public interface ITransformationSettings
    {
        string OperationName { get; }
        string AcceptHeader { get; }
        string SubjectRetrievalSparqlCommand { get; }
        string ExistingGraphSparqlCommand { get; }

        string FullDataUrlParameterizedString(string dataUrl);
    }
}