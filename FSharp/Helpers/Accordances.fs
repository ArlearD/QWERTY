namespace QWERTYFSharp.Helpers

open System.Collections.Generic
open System
open System.IO
open System.Web.Hosting
open System.Web
open System.Linq

type Accordances(check:List<bool>, dict:Dictionary<string, Dictionary<int, string>>, category:string) =
    let imperative = new QWERTYFSharp.ImperativeBuilder()
    member this.Check = check
    member this.Dict = dict
    member this.Category = category
    
    member this.GetProperty (i:int) = imperative {
        for property in this.Dict do
            for value in property.Value do
                if (value.Key = i) 
                then return property.Key.Replace(' ', '_')
        raise (Exception("Неверный i"))
    }

    member this.GetValue (i:int) = imperative {
        for property in this.Dict do
            for value in property.Value do
                if (value.Key = i) 
                then return value.Value
        raise (Exception("Неверный i"))
    }

    //Image Remover

type Remover() =
    static member RemoveImage (id : int64) =
        let path = HostingEnvironment.MapPath("~/Images/")

        if File.Exists(path + id.ToString() + ".jpg") 
        then File.Delete(path + id.ToString() + ".jpg")

        elif File.Exists(path + id.ToString() + ".jpeg") 
        then File.Delete(path + id.ToString() + "jpeg")

        elif File.Exists(path + id.ToString() + ".png") 
        then File.Delete(path + id.ToString() + ".png")

     //Image Uploader

type Uploader() =
    static member UploadImage (upload : HttpPostedFileBase, id : int64, server : HttpServerUtilityBase) =
        let extension = System.IO.Path.GetFileName(upload.FileName).Split('.').Last()

        let fileName = "~/Images/" + id.ToString() + "." + extension

        upload.SaveAs(server.MapPath(fileName))