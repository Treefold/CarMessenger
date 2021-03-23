using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using CarMessenger.Models;

namespace CarMessenger.Data
{
    public class ChatContext : DbContext
    {
        public ChatContext() : base("DefaultConnection") { } // ChatConnectionString

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Message> Messages {get; set; }
    }
}