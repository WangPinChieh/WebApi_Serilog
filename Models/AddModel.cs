using System;

namespace WebApi_Serilog.Models
{
    public class AddModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}