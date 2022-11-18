namespace SDG.Provider.Services.Translation;

public interface ITranslationService : IService
{
    string language { get; }
}
