using System.Threading.Tasks;

namespace Api.Generators
{
    public interface IGenerator
    {
        string[] ValidateTemplate(string code);
        Task<(byte[] Content, string ContentType)> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null);
    }
}
