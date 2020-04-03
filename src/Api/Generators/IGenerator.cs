using System.Threading;
using System.Threading.Tasks;

namespace Api.Generators
{
    public interface IGenerator
    {
        string[] ValidateTemplate(string code);
        Task<GenerateResult> GenerateDocumentAsync(string code, object model, IResourceManager resourceManager = null, CancellationToken cancellationToken = default);
    }

    public class GenerateResult
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}
