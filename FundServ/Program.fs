open System.IO
open FSharp.Data
open FSharp.Data.HttpRequestHeaders

type FundProfiles = JsonProvider<"fund_profiles.json">

let formValues (start: int) (length: int) =
    [ "draw", "1"
      "columns[0][data]", "code"
      "columns[1][data]", "company"
      "columns[2][data]", "fund_id"
      "columns[3][data]", "fund_name"
      "columns[4][data]", "currency"
      "columns[5][data]", "load"
      "columns[6][data]", "product_type"
      "columns[7][data]", "oeo_eligible"
      "columns[8][data]", "cdic_flag"
      "columns[9][data]", "member_institution"
      "columns[10][data]", "settlement"
      "columns[11][data]", "cut_off_time"
      "columns[12][data]", "classification"
      "columns[13][data]", "money_market_flag"
      "columns[14][data]", "commission"
      "columns[15][data]", "pac_swp"
      "columns[16][data]", "distribution"
      "columns[17][data]", "oeo_fund_id"
      "start", string start
      "length", string length
      "action", "fun-datatables-filter"
      "dataTitle", "fund_profiles" ]

[<EntryPoint>]
let main argv =
    let MAX_LENGTH = 7000

    let fundServ =
        Http.Request(
            "https://www.fundserv.com/wp-admin/admin-ajax.php",
            body = FormValues(formValues 0 MAX_LENGTH),
            httpMethod = "POST"
        )

    let fundServ =
        match fundServ.Body with
        | Text s ->
            let fundProfiles = FundProfiles.Parse(s)

            fundProfiles.Data
            |> Array.append (
                [| MAX_LENGTH..MAX_LENGTH .. fundProfiles.RecordsTotal |]
                |> Array.collect (fun start ->
                    let body =
                        Http
                            .Request(
                                "https://www.fundserv.com/wp-admin/admin-ajax.php",
                                body = FormValues(formValues start MAX_LENGTH),
                                httpMethod = "POST"
                            )
                            .Body

                    match body with
                    | Text s -> FundProfiles.Parse(s) |> _.Data)
            )

    let jv = JsonValue.Array(fundServ |> Array.map (_.JsonValue))
    use f = File.CreateText("fund_serv.json")
    jv.WriteTo(f, JsonSaveOptions.None)
    0
