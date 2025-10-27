using System;

namespace Backend.Common;

public class Response<T>//Generic
{
    public bool IsSuccess { get; }//Indicates whether the request was successful (true) or failed (false).
    public T Data { get; set; }//Holds the actual data returned by the API. Could be null if failure.
    public string? Error { get; set; }//Holds error information if the request failed.
    public string? Message { get; set; }//Optional custom message for success or additional info.


    //Initializes a new Response<T> object with the four properties.
    public Response(bool isSuccess, T data, string? error, string? message)
    {
        IsSuccess = isSuccess;//IsSuccess is readonly, so it can only be set in the constructor.
        Data = data;
        Error = error;
        Message = message;
    }

    //Shortcut to create a successful response.
    public static Response<T> Success(T data, string? message = "") => new(true, data, null, message);

    public static Response<T> Failure(string error) => new(false, default!, error, null);
}
