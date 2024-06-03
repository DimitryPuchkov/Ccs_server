using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ccs_server.data
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("name", TypeName = "character varying")]

        public string Name { get; set; }

        [Column("email", TypeName = "character varying")]
        public string Email { get; set; }

        [Column("password", TypeName = "character varying")]
        public string Password { get; set; }

    }
}
