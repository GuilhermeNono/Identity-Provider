using System.Net;

namespace Idp.Domain.Errors.Exceptions.Http;

public interface IHttpException
{
    public HttpStatusCode StatusCode { get; }
    public string? Code { get; }
}