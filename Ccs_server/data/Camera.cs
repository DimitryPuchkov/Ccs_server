using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ccs_server.data
{
    public class Camera
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("name", TypeName = "character varying")]

        public string Name { get; set; }

        [Column("address", TypeName = "character varying")]
        public string Address { get; set; }

        [Column("login", TypeName = "character varying")]
        public string Login { get; set; }

        [Column("password", TypeName = "character varying")]
        public string Password { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }


        [Column("port")]
        public int Port { get; set; }

        [Column("fbx1")]
        public int Fbx1 { get; set; }

        [Column("fbx2")]
        public int Fbx2 { get; set; }

        [Column("fby1")]
        public int Fby1 { get; set; }

        [Column("fby2")]
        public int Fby2 { get; set; }

        [Column("sbx1")]
        public int Sbx1 { get; set; }

        [Column("sbx2")]
        public int Sbx2 { get; set; }

        [Column("sby1")]
        public int Sby1 { get; set; }

        [Column("sby2")]
        public int Sby2 { get; set; }

        public User User { get; set; }


    }
}
