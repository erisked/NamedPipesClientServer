using namedpipes_core_client;

namespace namedpipes_core_server
{
    public interface IRequestHandler
    {
        public Response ProcessRequest(Request r);
    }
}
