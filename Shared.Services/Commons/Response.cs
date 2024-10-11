using System.Collections.ObjectModel;
using System.Net;

namespace Shared.Services.Commons;

public class Response
{
    private readonly IList<string> _messages = new List<string>();

    public IEnumerable<string> Errors { get; }
    
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    
    public string Message { get; set; }
    
    public object Data { get; set; }
    public Response() => Errors = new ReadOnlyCollection<string>(_messages);

    public Response(object result, string? message, HttpStatusCode statusCode = HttpStatusCode.OK) : this() { 
        Data = result;
        StatusCode = statusCode;
        Message = message?? "Success";
    }

    public bool IsValid() => !Errors.Any();
    public Response AddError(string message)
    {
        _messages.Add(message);
        return this;
    }
}