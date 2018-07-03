namespace Functions
{
    public interface ITransformationSettings
    {
        string AcceptHeader { get; }
        string SubjectRetrievalSparqlCommand { get; }
        string ExistingGraphSparqlCommand { get; }

        string ParameterizedString(string dataUrl);
    }
}