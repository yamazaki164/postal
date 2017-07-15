namespace AwsLambdaPostal

open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.Json
open Amazon.S3
open Amazon.S3.Model

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

    let getZipFileStream (): Stream = 
        let req = HttpWebRequest.Create(@"http://www.post.japanpost.jp/zipcode/dl/oogaki/zip/ken_all.zip")
        let result = req.GetResponseAsync().Result
        result.GetResponseStream()

    let tmpFile: String = Path.Combine([Path.GetTempPath(); Path.GetRandomFileName()] |> List.toArray)

    let downloadedZipFile (): String =
        use file = new FileStream(tmpFile, FileMode.OpenOrCreate)
        getZipFileStream().CopyTo(file)
        tmpFile

    let zip (): ZipArchive = ZipFile.OpenRead(downloadedZipFile())

    let csv (): ZipArchiveEntry = zip().GetEntry("KEN_ALL.CSV")
    let encSjis: Encoding = Encoding.GetEncoding(932)
    
    let parsePostal (): Postal list =
        seq{ use reader = new StreamReader(csv().Open(), encSjis)
            while not reader.EndOfStream do
                let str = reader.ReadLine()
                let srcByte = encSjis.GetBytes(str)
                let destByte = Encoding.Convert(encSjis, Encoding.UTF8, srcByte)
                let s = Encoding.UTF8.GetString(destByte)
                let sa = s.Trim().Split(',') |> Array.map (fun s -> s.TrimStart('"').TrimEnd('"').Trim())
                yield Postal(sa)
        } |> Seq.toList

    let groupedPostal (): (String * Postal list) list = List.groupBy (fun (x : Postal) -> x.PostalCodeShort) (parsePostal())

    let getBucketName: String = "postalcodes"

    let getJsonName (code: String): String = code + ".json"

    let putItem (item : String * Postal list): unit =
        let code = fst item
        let plist = snd item
        let req = PutObjectRequest()
        req.BucketName <- getBucketName
        req.Key <- getJsonName(code)
        req.ContentBody <- JsonConvert.SerializeObject(plist)
        let res = s3client.PutObjectAsync(req)
        ()

    let putJson (context: ILambdaContext) =
        groupedPostal() |> List.iter putItem

    let getItem (code: String) =
        let req = GetObjectRequest()
        req.BucketName <- getBucketName
        req.Key <- getJsonName(code)
        s3client.GetObjectAsync(req)

    let handler (input: InputValue) (context: ILambdaContext) = 
        input.postal

    [<EntryPoint>]
    let main argv =
        0
