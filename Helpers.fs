namespace KnowIT.AzFunctions.Helpers

open System

[<RequireQualifiedAccess>]
module Environment =

    let getVariable name =
        Environment.GetEnvironmentVariable(name)
        |> Option.ofObj

    let getVariables =
        Environment.GetEnvironmentVariables()
        |> Seq.cast<Collections.DictionaryEntry>
        |> Seq.map (fun e -> (string e.Key , string e.Value))
        |> readOnlyDict

    let getPrefixVariables (prefix: string) =
        let prefix = prefix + "_"
        getVariables
        |> Seq.filter (fun kv ->
            kv.Key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
        |> Seq.map (fun kv ->
            (kv.Key.Substring(prefix.Length).TrimStart().ToUpper(), kv.Value))
        |> readOnlyDict



module HttpRequest =
    open Microsoft.AspNetCore.Http

    let tryQueryValues name (request: HttpRequest) =
        match request.Query.TryGetValue(name) with
        | false, _ -> None
        | true, values -> Some (values.ToArray())

    let queryValues name (request: HttpRequest) =
        tryQueryValues name request
        |> Option.defaultValue Array.empty

    let tryHeaderValues name (request: HttpRequest) =
        match request.Headers.GetCommaSeparatedValues(name) with
        | [||] -> None
        | values -> Some values

    let headerValues name (request: HttpRequest) =
        request.Headers.GetCommaSeparatedValues(name)

    let tryQueryOrHeaderValues name (request: HttpRequest) =
        tryQueryValues name request
        |> Option.orElse (tryHeaderValues name request)

    let queryOrHeaderValues name (request: HttpRequest) =
        tryQueryOrHeaderValues name request
        |> Option.defaultValue Array.empty

    let combineQueryAndHeaderValues name (request: HttpRequest) =
        queryValues name request
        |> Array.append (headerValues name request)
        |> Array.distinct

