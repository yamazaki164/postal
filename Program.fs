namespace AwsLambdaPostal

open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.Json
open Amazon.S3

[<assembly:LambdaSerializer(typeof<JsonSerializer>)>]
do ()

module Program = 
    open System
    open System.IO
    open System.IO.Compression
    open System.Net
    open System.Text
    open Newtonsoft.Json
    open Model.Postal

    Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
    
    type InputValue = {
        [<JsonProperty("postal")>]
        postal : String
    }

    let s3client = new Amazon.S3.AmazonS3Client()

    let getZipFileStream () = 
        let req = HttpWebRequest.Create(@"http://www.post.japanpost.jp/zipcode/dl/oogaki/zip/ken_all.zip")
        let result = req.GetResponseAsync().Result
        result.GetResponseStream()

    let tmpFile = Path.Combine([Path.GetTempPath(); Path.GetRandomFileName()] |> List.toArray)

    let downloadedZipFile () =
        use file = new FileStream(tmpFile, FileMode.OpenOrCreate)
        getZipFileStream().CopyTo(file)
        tmpFile

    let zip () = ZipFile.OpenRead(downloadedZipFile())
    // let zip () = ZipFile.OpenRead(@"C:\Users\hiroshi.yamazaki\works\postal\ken_all.zip")

    let csv () = zip().GetEntry("KEN_ALL.CSV")
    let encSjis = Encoding.GetEncoding(932)
    
    let parsePostal () =
        seq{ use reader = new StreamReader(csv().Open(), encSjis)
            while not reader.EndOfStream do
                let str = reader.ReadLine()
                let srcByte = encSjis.GetBytes(str)
                let destByte = Encoding.Convert(encSjis, Encoding.UTF8, srcByte)
                let s = Encoding.UTF8.GetString(destByte)
                let sa = s.Trim().Split(',') |> Array.map (fun s -> s.TrimStart('"').TrimEnd('"').Trim())
                yield Postal(sa)
        } |> Seq.toList

    let groupedPostal () = List.groupBy (fun (x : Postal) -> x.PostalCodeShort) (parsePostal())

    let handler (input: InputValue) (context: ILambdaContext) = 
        input.postal

    [<EntryPoint>]
    let main argv =
        0
