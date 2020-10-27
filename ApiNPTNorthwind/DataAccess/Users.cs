using System;
using System.Collections.Generic;

namespace ApiNPTNorthwind.DataAccess
{
    public partial class Users
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}
