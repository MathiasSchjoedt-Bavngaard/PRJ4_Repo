﻿using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    public class Account
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public double Weigth { get; set; }
        public string Gender { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
        public byte[] PasswordHash { get; set; } 

        public byte[] PasswordSalt {get; set; }

        public Calender Calender { get; set; }
        public long CalenderId { get; set; }

    }
}
