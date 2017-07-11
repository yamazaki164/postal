namespace AwsLambdaPostal.Model
module Postal = 
    open System
    open Newtonsoft.Json
    
    [<JsonObject("postal")>]
    type Postal (a : String[]) = class
        let jisCode: string = a.[0]
        let oldPostalCode: String = a.[1]
        let postalCode: String = a.[2]
        let postalCodeShort: String = a.[2].Substring(0, 3)
        let kanaPrefecture: String = a.[3]
        let kanaAddress1: String = a.[4]
        let kanaAddress2: String = a.[5]
        let prefecture: String = a.[6]
        let address1: String = a.[7]
        let address2: String = a.[8]
        let flag1: bool = 
            match a.[9] with
                | "1" -> true
                | _ -> false
        let flag2: bool = 
            match a.[10] with
                | "1" -> true
                | _ -> false
        let flag3: bool = 
            match a.[11] with
                | "1" -> true
                | _ -> false
        let flag4: bool = 
            match a.[12] with
                | "1" -> true
                | _ -> false
        let status: int = Int32.Parse(a.[13])
        let reason: int = Int32.Parse(a.[14])

        [<JsonProperty("jis_code")>]
        member this.JisCode with get() = jisCode

        [<JsonIgnore>]
        member this.OldPostalCode with get() = oldPostalCode

        [<JsonProperty("postal_code")>]
        member this.PostalCode with get() = postalCode
        [<JsonIgnore>]
        member this.PostalCodeShort with get() = postalCodeShort

        [<JsonProperty("kana_prefecture")>]
        member this.KanaPrefecture with get() = kanaPrefecture

        [<JsonProperty("kana_address1")>]
        member this.KanaAddress1 with get() = kanaAddress1

        [<JsonProperty("kana_address2")>]
        member this.KanaAddress2 with get() = kanaAddress2

        [<JsonProperty("prefecture")>]
        member this.Prefecture with get() = prefecture

        [<JsonProperty("address1")>]
        member this.Address1 with get() = address1

        [<JsonProperty("address2")>]
        member this.Address2 with get() = address2

        [<JsonProperty("flag1")>]
        member this.Flag1 with get() = flag1

        [<JsonProperty("flag2")>]
        member this.Flag2 with get() = flag2

        [<JsonProperty("flag3")>]
        member this.Flag3 with get() = flag3

        [<JsonProperty("flag4")>]
        member this.Flag4 with get() = flag4

        [<JsonProperty("status")>]
        member this.Status with get() = status

        [<JsonProperty("reason")>]
        member this.Reason with get() = reason

        end