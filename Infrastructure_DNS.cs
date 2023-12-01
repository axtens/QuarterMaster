using DNS.Client;

using System.Threading.Tasks;

namespace QuarterMaster.Infrastructure
{
    public static class NameServer
    {
        public static ClientRequest GetClientRequestor(string dnsServer) => new ClientRequest(dnsServer);
        public static async Task<ClientResponse> GetClientResponseAsync(ClientRequest clientRequest) => (ClientResponse)await clientRequest.Resolve();
    }
}