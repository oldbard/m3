using System.Threading.Tasks;

namespace Requests
{
    public interface IRequestAsync
    {
        bool IsProcessing { get; }

        Task<IResultAsync> Process();
    }

    public interface IResultAsync { }
}