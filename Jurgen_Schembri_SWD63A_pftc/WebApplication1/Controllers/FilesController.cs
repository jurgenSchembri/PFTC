using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using Google.Cloud.Storage.V1;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Auth.OAuth2;
using System.IO;

namespace WebApplication1.Controllers
{
    public class FilesController : Controller
    {
        [Authorize]
        [HttpGet]
        public ActionResult Create()
        { return View(); }

        [Authorize]
        [HttpPost]
        public ActionResult Create(Models.File p, HttpPostedFileBase file)
        {
            //upload image related to product on the bucket
            try
            {
                string link = "";
                var filename = "";
                if (file!= null)
                { 
                #region Uploading file on Cloud Storage
                var storage = StorageClient.Create();
                    using (var f = file.InputStream)
                    {
                        filename = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                        var storageObject = storage.UploadObject("programming-for-the-cloud", filename, null, f);
                        //link = storageObject.MediaLink;
                        link = "https://storage.cloud.google.com/programming-for-the-cloud" + "/" + filename;

                        if (null == storageObject.Acl)
                        {
                            storageObject.Acl = new List<ObjectAccessControl>();
                        }


                        storageObject.Acl.Add(new ObjectAccessControl()
                        {
                            Bucket = "programming-for-the-cloud",
                            Entity = $"user-" + "jurgenschembri08@gmail.com",
                            Role = "READER", //READER
                        });

                        var updatedObject = storage.UpdateObject(storageObject, new UpdateObjectOptions()
                        {
                            // Avoid race conditions.
                            IfMetagenerationMatch = storageObject.Metageneration,
                        });
                    }
                    //store details in a relational db including the filename/link
                    #endregion


                    
                }
                #region Storing details of product in db [INCOMPLETE]
                p.OwnerFk = User.Identity.Name; //"jurgenschembri08@gmail.com"; 

                ServiceAccountCredential credential;
                using (var stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "//jurgen-cloud-project-5f077f2e1ba1.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).UnderlyingCredential as ServiceAccountCredential;
                }

                UrlSigner urlSigner = UrlSigner.FromServiceAccountCredential(credential); //https://googleapis.github.io/google-cloud-dotnet/docs/Google.Cloud.Storage.V1/index.html
                string signedUrl = urlSigner.Sign("programming-for-the-cloud", filename, TimeSpan.FromDays(365));

                p.Link = signedUrl;

                //p.Link = link;
                FilesRepository pr = new FilesRepository();
                pr.AddFile(p.Name, p.Description, p.OwnerFk,p.Link);
                #endregion

                #region Updating Cache with latest list of Products from db
                //enable: after you switch on db
                CacheRepository cr = new CacheRepository();
                cr.UpdateCache(pr.GetFiles(User.Identity.Name));
                #endregion

                new LoggingRepository().Logging("File Uploaded By: " + User.Identity.Name + " At: " + DateTime.Now);

                //PubSubRepository psr = new PubSubRepository();
                //psr.AddToEmailQueue(p); //adding it to queue to be sent as an email later on.
                //ViewBag.Message = "Product created successfully";
            }
            catch (Exception ex)
            {
                new LoggingRepository().ErrorLogging(ex);
                ViewBag.Error = "Product failed to be created; " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        // GET: Products
        [Authorize]
        public ActionResult Index()
        {
            try
            {
                new LoggingRepository().Logging("Getting Items");

                #region get products of db - removed....instead insert next region
                FilesRepository pr = new FilesRepository();
                //var products = pr.GetProducts(); //gets products from db
                #endregion

                #region instead of getting products from DB, to make your application faster , you load them from the cache
                CacheRepository cr = new CacheRepository();
                cr.UpdateCache(pr.GetFiles(User.Identity.Name)); //commented
                var files = cr.GetFilesFromCache();
                #endregion
                return View("Index", files);
            }
            catch(Exception ex)
            {
                new LoggingRepository().ErrorLogging(ex);
            }
            return View("Index");
        }

        [Authorize]
        public ActionResult Delete(int id)
        {
            try
            {
                new LoggingRepository().Logging("Deleting one element");
                FilesRepository fr = new FilesRepository();
                fr.DeleteFile(id);
                CacheRepository cr = new CacheRepository();
                cr.UpdateCache(fr.GetFiles(User.Identity.Name));
            }
            catch(Exception ex)
            {
                new LoggingRepository().ErrorLogging(ex);
            }
            
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteAll(int[] ids)
        {
            //1. Requirement when opening a transaction: Connection has to be opened
            FilesRepository fr = new FilesRepository();
            fr.MyConnection.Open();

            fr.MyTransaction = fr.MyConnection.BeginTransaction(); //from this point onwards all code executed against the db will remain pending

            try
            {
                new LoggingRepository().Logging("Deleting multiple elements");
                foreach (int id in ids)
                {
                    fr.DeleteFile(id);
                }

                fr.MyTransaction.Commit(); //Commit: you are confirming the changes in the db
            }
            catch (Exception ex)
            {
                //Log the exception on the cloud
                new LoggingRepository().ErrorLogging(ex);
                fr.MyTransaction.Rollback(); //Rollback: it will reverse all the changes done within the try-clause in the db
            }

            fr.MyConnection.Close();

            CacheRepository cr = new CacheRepository();
            cr.UpdateCache(fr.GetFiles(User.Identity.Name));

            return RedirectToAction("Index");
        }

        //public void Send(string email)
        //{
        //    PubSubRepository psr = new PubSubRepository();
        //    psr.DownloadEmailFromQueueAndSend(email);
        //    //return RedirectToAction("Index");
        //}

        //[HttpPost]
        //public ActionResult SendEmail(int id, string email)
        //{
        //    ProductsRepository pr = new ProductsRepository();
        //    Product prod = new Product();
        //    prod = pr.GetProduct(id);

        //    CacheRepository cr = new CacheRepository();
        //    cr.UpdateCache(pr.GetProducts(User.Identity.Name));

        //    PubSubRepository psr = new PubSubRepository();
        //    psr.AddToEmailQueue(prod);
        //    psr.DownloadEmailFromQueueAndSend(email);
        //    return RedirectToAction("Index");
        //}

        [Authorize]
        public ActionResult Send(int FieldId)
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Send(FormCollection collection)
        {
            try
            {
                string fileid = collection.GetValue("FieldId").AttemptedValue;
                int fileId = Convert.ToInt32(fileid);
                PubSubRepository psr = new PubSubRepository();
                FilesRepository fr = new FilesRepository();

                Models.File f = fr.GetFile(fileId);
                FileSendTo fst = new FileSendTo();

                string email = collection.GetValue("Email").AttemptedValue;
                string message = collection.GetValue("Message").AttemptedValue;

                fst.Link = f.Link;
                fst.Name = f.Name;
                fst.OwnerFk = f.OwnerFk;
                fst.Message = message;
                fst.Email = email;

                psr.AddToEmailQueue(fst);

                psr.DownloadEmailFromQueueAndSend();

                new LoggingRepository().Logging("File: " + f.Name + " Shared With: " + email + " At: " + DateTime.Now);

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                new LoggingRepository().ErrorLogging(ex);
            }
            return View();
        }
    }
}