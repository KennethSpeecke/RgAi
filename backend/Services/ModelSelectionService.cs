namespace RgAi.Backend.Services;

public sealed class ModelSelectionService
{
    private readonly object _lock = new();
    public string GenerateModel { get; private set; }
    public string EmbeddingModel { get; private set; }

    public ModelSelectionService(string initialModel)
    {
        GenerateModel = initialModel;
    }

    public void SetGenerateModel(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException("Model name cannot be empty.", nameof(modelName));
        }

        lock (_lock)
        {
            GenerateModel = modelName;
        }
    }
    
    public void SetEmbeddingModel(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException("Model name cannot be empty.", nameof(modelName));
        }
        
        lock (_lock)
        {
            EmbeddingModel = modelName;
        }
    }
}
