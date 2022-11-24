namespace KnowIT

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open System.Text.Json
open Microsoft.Extensions.Logging

module HttpTrigger =
    // Define a record to deserialize body data into.
    type NameContainer =
        { Name: string }

    // For convenience, it's better to have a central place for the literal.
    [<Literal>]
    let Name = "name"

    [<FunctionName("HttpTrigger")>]
    let run ([<HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)>]req: HttpRequest) (log: ILogger) =
        task {
            log.LogInformation("F# HTTP trigger function processed a request.")

            let nameOpt =
                if req.Query.ContainsKey(Name) then
                    Some(req.Query.[Name].[0])
                else
                    None

            use stream = new StreamReader(req.Body)
            let! reqBody = stream.ReadToEndAsync() //|> Async.AwaitTask

            let data =
                try
                    JsonSerializer.Deserialize<NameContainer>(reqBody,
                        JsonSerializerOptions(PropertyNameCaseInsensitive = true))
                with
                | _ -> { Name = "" }

            let name =
                match nameOpt with
                | Some n -> n
                | None -> data.Name

            let responseMessage =
                if (String.IsNullOrWhiteSpace(name)) then
                    {| Result = "Warning"
                       Message = "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response." |}

                else
                    {| Result = "Success"
                       Message = "Hello, " +  name + ". This HTTP triggered function executed successfully." |}

            return OkObjectResult(responseMessage)
        }