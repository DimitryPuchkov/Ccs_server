using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ccs_server.data
{
    public class Value
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("camera_id")]
        public int CameraId { get; set; }

        [Column("person_count")]
        public int PersonCount { get; set; }

        [Column("time", TypeName = "timestamptz")]
        public DateTime Time { get; set; }

        public Camera Camera { get; set; }
    }
}
