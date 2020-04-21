using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Npgsql;

namespace WebApplication1.DataAccess
{
    public class UsersRepository: ConnectionClass
    {
        public UsersRepository() : base()
        { }

        public void AddUser(string email, string name, string surname)
        {
            string sql = "INSERT into users (email, name, surname, lastloggedin) values (@email, @name, @surname, @lastloggedin)";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@surname", surname);
            cmd.Parameters.AddWithValue("@lastloggedin", DateTime.Now); ;


            MyConnection.Open();
            cmd.ExecuteNonQuery();
            MyConnection.Close();
        }

        public bool DoesEmailExist(string email)
        {
            string sql = "Select Count(*) from users where email = @email";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);

            MyConnection.Open();

            bool result = Convert.ToBoolean(cmd.ExecuteScalar());

            MyConnection.Close();

            return result;
        }

        public void UpdateLastLoggedIn(string email)
        {

            string sql = "update users set lastloggedin = @lastloggedin where email = @email";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@lastloggedin", DateTime.Now); ;

            MyConnection.Open();
            cmd.ExecuteNonQuery();
            MyConnection.Close();

        }

    }
}