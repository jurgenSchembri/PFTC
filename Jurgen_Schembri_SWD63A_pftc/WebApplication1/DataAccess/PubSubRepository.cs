using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DataAccess
{
    public class PubSubRepository
    {
        TopicName tn;
        SubscriptionName sn;
        public PubSubRepository()
        {
            tn  = new TopicName("jurgen-cloud-project", "pftc");  //A Queue/Topic will be created to hold the emails to be sent.  It will always have the same name DemoTopic, which you can change
            sn = new SubscriptionName("jurgen-cloud-project", "DemoSubscription1");  //A Subscription will be created to hold which messages were read or not.  It will always have the same name DemoSubscription, which you can change
        }
        private Topic CreateGetTopic()
        {
            PublisherServiceApiClient client = PublisherServiceApiClient.Create();   //We check if Topic exists, if no we create it and return it
            TopicName tn = new TopicName("jurgen-cloud-project", "pftc");
            try
            {
                 return client.GetTopic(tn);
            }
            catch(Exception ex)
            {
                new LoggingRepository().ErrorLogging(ex);
                return client.CreateTopic(tn);
            }
        }

        /// <summary>
        /// Publish method: uploads a message to the queue
        /// </summary>
        /// <param name="p"></param>
        public void AddToEmailQueue(FileSendTo p) 
        {
            PublisherServiceApiClient client = PublisherServiceApiClient.Create();
            var t = CreateGetTopic();

            p.Link = KeyRepository.Encrypt(p.Link);
            p.OwnerFk = KeyRepository.Encrypt(p.OwnerFk);
            p.Message = KeyRepository.Encrypt(p.Message);
            p.Name = KeyRepository.Encrypt(p.Name);
            p.Email = KeyRepository.Encrypt(p.Email);

            string serialized = JsonSerializer.Serialize(p, typeof(FileSendTo));


            List<PubsubMessage> messagesToAddToQueue = new List<PubsubMessage>(); // the method takes a list, so you can upload more than 1 message/item/product at a time
            PubsubMessage msg = new PubsubMessage();
            msg.Data = ByteString.CopyFromUtf8(serialized);

            messagesToAddToQueue.Add(msg);

            client.Publish(t.TopicName, messagesToAddToQueue); //committing to queue
        }


        private Subscription CreateGetSubscription()
        {
            SubscriberServiceApiClient client = SubscriberServiceApiClient.Create();  //We check if Subscription exists, if no we create it and return it
 
            try
            {
               return client.GetSubscription(sn);
            }
            catch(Exception ex)
            {
                new LoggingRepository().ErrorLogging(ex);
                return client.CreateSubscription(sn, tn, null, 30);
            }

        }

        public void DownloadEmailFromQueueAndSend()
        {
            SubscriberServiceApiClient client = SubscriberServiceApiClient.Create();

            var s = CreateGetSubscription(); //This must be called before being able to read messages from Topic/Queue
            var pullResponse = client.Pull(s.SubscriptionName, true, 1); //Reading the message on top; You can read more than just 1 at a time
            if(pullResponse != null)
            {
                try
                {
                    string toDeserialize = pullResponse.ReceivedMessages[0].Message.Data.ToStringUtf8(); //extracting the first message since in the previous line it was specified to read one at a time. if you decide to read more then a loop is needed
                    FileSendTo deserialized = JsonSerializer.Deserialize<FileSendTo>(toDeserialize); //Deserializing since when we published it we serialized it

                    //MailMessage mm = new MailMessage();  //Message on queue/topic will consist of a ready made email with the desired content, you can upload anything which is serializable
                    //mm.To.Add("jurgen_schembri@hotmail.com");
                    //mm.From = new MailAddress("noreply_abcsupplies@gmail.com");
                    //mm.Subject = "New Product In Inventory";
                    //mm.Body = $"Name:{deserialized.Name}; Price {deserialized.Price};";

                    if (deserialized.Name != null)
                    {
                        deserialized.Link = KeyRepository.Decrypt(deserialized.Link);
                        deserialized.OwnerFk = KeyRepository.Decrypt(deserialized.OwnerFk);
                        deserialized.Message = KeyRepository.Decrypt(deserialized.Message);
                        deserialized.Name = KeyRepository.Decrypt(deserialized.Name);
                        deserialized.Email = KeyRepository.Decrypt(deserialized.Email);
                        SendMail(deserialized);
                    }

                    //Send Email with deserialized. Documentation: https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient?view=netframework-4.8

                    List<string> acksIds = new List<string>();
                    acksIds.Add(pullResponse.ReceivedMessages[0].AckId); //after the email is sent successfully you acknolwedge the message so it is confirmed that it was processed

                    client.Acknowledge(s.SubscriptionName, acksIds.AsEnumerable());
                }
                catch(Exception ex)
                {
                    new LoggingRepository().ErrorLogging(ex);
                }
            }
        }

        public void SendMail(FileSendTo fst)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                string email = "jurgenschembri08@gmail.com";
                string password = "vixaiplnjrrybayq";
                NetworkCredential nc = new NetworkCredential(email, password);
                MailMessage mm = new MailMessage();

                mm.Subject = fst.Message;
                mm.From = new MailAddress(fst.OwnerFk);
                mm.Body = fst.Link;
                mm.To.Add(new MailAddress(fst.Email));

                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = nc;
                client.Send(mm);
            }
            catch (Exception ex)
            {
                new LoggingRepository().ErrorLogging(ex);
            }
        }
    }
}