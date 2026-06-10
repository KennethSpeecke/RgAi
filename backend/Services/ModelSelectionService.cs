namespace RgAi.Backend.Services;

public sealed class ModelSelectionService
{
    private readonly object _lock = new();
    public string CurrentModel { get; private set; }

    public ModelSelectionService(string initialModel)
    {
        CurrentModel = initialModel;
    }

    public void SetCurrentModel(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException("Model name cannot be empty.", nameof(modelName));
        }

        lock (_lock)
        {
            CurrentModel = modelName;
        }
    }
}
