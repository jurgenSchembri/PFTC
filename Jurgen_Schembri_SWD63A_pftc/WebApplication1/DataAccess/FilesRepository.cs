using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DataAccess
{
    public class FilesRepository: ConnectionClass
    {
        public FilesRepository() : base() { }

        public void AddFile(string name, string description, string ownerfk, string link)
        {
            string sql = "INSERT into files (name, description, ownerfk, link) values (@name, @description, @ownerfk, @link)";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@ownerfk", ownerfk);
            cmd.Parameters.AddWithValue("@link", link);

            MyConnection.Open();
            cmd.ExecuteNonQuery();
            MyConnection.Close();
        }

        public List<File> GetFiles()
        {
            string sql = "Select Id, Name, Description, Ownerfk from files";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            

            MyConnection.Open();
            List<File> results = new List<File>();

            using (var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    File f = new File();
                    f.Id = reader.GetInt32(0);
                    f.Name = reader.GetString(1);
                    f.Description = reader.GetString(2);
                    f.OwnerFk = reader.GetString(3);
                    results.Add(f);
                }
            }

            MyConnection.Close();

            return results;
        }



        public List<File> GetFiles(string email)
        {
            string sql = "Select Id, Name, Description, Ownerfk from files where ownerfk=@email";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);

            MyConnection.Open();
            List<File> results = new List<File>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    File f = new File();
                    f.Id = reader.GetInt32(0);
                    f.Name = reader.GetString(1);
                    f.Description = reader.GetString(2);
                    f.OwnerFk = reader.GetString(3);
                    results.Add(f);
                }
            }

            MyConnection.Close();

            return results;
        }

        public File GetFile(int id)
        {
            string sql = "Select Id, Name, Description, Ownerfk, Link from files where Id=@id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@id", id);
            MyConnection.Open();
            File file = new File();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    file.Id = reader.GetInt32(0);
                    file.Name = reader.GetString(1);
                    file.Description = reader.GetString(2);
                    file.OwnerFk = reader.GetString(3);
                    file.Link = reader.GetString(4);
                }
            }

            MyConnection.Close();

            return file;
        }

        public void DeleteFile(int id)
        {
            string sql = "Delete from files where Id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@id", id);

            bool connectionOpenedInThisMethod = false;

            if (MyConnection.State == System.Data.ConnectionState.Closed)
            {
                MyConnection.Open();
                connectionOpenedInThisMethod = true;
            }

            if(MyTransaction != null)
            {
                cmd.Transaction = MyTransaction; //to participate in the opened trasaction (somewhere else), assign the Transaction property to the opened transaction
            }
            cmd.ExecuteNonQuery();

            if (connectionOpenedInThisMethod == true)
            {
                MyConnection.Close();
            }
        }
    }
}