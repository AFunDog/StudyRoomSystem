using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using StudyRoomSystem.AvaloniaApp.Contacts;

namespace StudyRoomSystem.AvaloniaApp.Services;

/// <summary>
/// 自动往请求头中添加 Token 的委托器
/// </summary>
internal sealed partial class TokenHandler : DelegatingHandler
{
    private ITokenProvider TokenProvider { get; }

    public TokenHandler(ITokenProvider tokenProvider)
    {
        TokenProvider = tokenProvider; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = TokenProvider.Token;
        if (string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
