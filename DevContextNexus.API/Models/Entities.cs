using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevContextNexus.API.Models
{
    [Table("projects")]
    public class Project
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
    }

    [Table("context_files")]
    public class ContextFile
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("project_id")]
        public Guid ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project? Project { get; set; }

        [Required]
        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [Column("last_sha")]
        public string? LastSha { get; set; }

        [Column("public_url")]
        public string? PublicUrl { get; set; }
    }
}
