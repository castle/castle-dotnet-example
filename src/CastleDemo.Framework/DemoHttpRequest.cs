using System.Collections.Specialized;
using System.Web;

namespace CastleDemo.Framework
{
    /// <summary>
    /// A tiny <see cref="HttpRequestBase"/> stand-in so the console sample can build a
    /// request context the same way an ASP.NET (System.Web) application would.
    /// </summary>
    internal sealed class DemoHttpRequest : HttpRequestBase
    {
        private readonly NameValueCollection _headers;
        private readonly HttpCookieCollection _cookies;
        private readonly string _userHostAddress;

        public DemoHttpRequest(NameValueCollection headers, HttpCookieCollection cookies, string userHostAddress)
        {
            _headers = headers;
            _cookies = cookies;
            _userHostAddress = userHostAddress;
        }

        public override NameValueCollection Headers => _headers;

        public override HttpCookieCollection Cookies => _cookies;

        public override string UserHostAddress => _userHostAddress;
    }
}
