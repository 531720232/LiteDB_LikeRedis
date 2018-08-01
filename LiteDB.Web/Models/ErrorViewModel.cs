using System;

namespace LiteDB.Web.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
    public class Key_Value
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}