using System;
using System.Collections.Generic;

namespace RS.COMMON.DTO
{
    public class Result
    {
        public bool Success;
        public List<String> SuccessMessages;
        public List<String> ErrorMessages;

        public Result()
        {
            Success = true;
            SuccessMessages = new List<string>();
            ErrorMessages = new List<string>();
        }
    }
    public class Result<T>: Result
    {
        public T Response { get; set; }
    } 
}