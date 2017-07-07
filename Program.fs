namespace AwsLambdaPostal

open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.Json

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

    let getZipFileStream = 
        let req = HttpWebRequest.Create(@"http://www.post.japanpost.jp/zipcode/dl/oogaki/zip/ken_all.zip")
        let result = req.GetResponseAsync().Result
        result.GetResponseStream()

    // let downloadedZipFile = 
    //     let file = new FileStream(Path.GetTempFileName(), FileMode.Open)
    //     getZipFileStream.CopyTo(file)
    //     let name = file.Name
    //     file.Dispose()
    //     name

    let downloadedZipFile = @"C:\Users\hiroshi.yamazaki\works\postal\ken_all.zip"

    let createJsonFile (p: Postal) = 
        File.WriteAllText(@"C:\Users\hiroshi.yamazaki\works\postal\tmp\" + p.PostalCode + ".json", JsonConvert.SerializeObject(p))

    let zip = ZipFile.OpenRead(downloadedZipFile)
    let csv = zip.GetEntry("KEN_ALL.CSV")
    let encSjis = Encoding.GetEncoding(932)
    
    let outputPostalJson = 
        Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
        let reader = new StreamReader(csv.Open(), encSjis)

        while not reader.EndOfStream do
            let str = reader.ReadLine()
            let srcByte = encSjis.GetBytes(str)
            let destByte = Encoding.Convert(encSjis, Encoding.UTF8, srcByte)
            let s = Encoding.UTF8.GetString(destByte)
            let sa = s.Trim().Split(',') |> Array.map (fun s -> s.Trim().TrimStart('"').TrimEnd('"'))
            let p = Postal(sa)
            createJsonFile(p)
        reader.Dispose()
    
    let handler(context: ILambdaContext) = 
        Console.WriteLine context.ToString
        Console.WriteLine "Hello World on console"
        printfn "Hello World"
        0

    [<EntryPoint>]
    let main argv =
        outputPostalJson
        0 // return an integer exit code
