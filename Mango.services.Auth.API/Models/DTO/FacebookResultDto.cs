﻿namespace Mango.Services.AuthAPI.Models.DTO
{
    public class FacebookResultDto
    {
        public FacebookData Data { get; set; }
    }
    public class FacebookData
    {
        public bool Is_Valid { get; set; }
        public string User_Id { get; set; }
    }
}
