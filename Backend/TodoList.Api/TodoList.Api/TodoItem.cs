using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TodoList.Api
{
    /*
     * Note:
     * Introducing index can help improve the performance of the query, especially on a large collection - since we can avoid doing entire table scan
     * It's important to carefully consider which columns are included in the index (the order also matters!).
     * - IsCompleted should be indexed because it's included in most of the queries
     * - Description should be indexed because we're comparing for duplicates
     */
    [Index(nameof(IsCompleted), nameof(Description))]
    public class TodoItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MinLength(1), MaxLength(1000)] // assumption: to do items are usually succinct, 1000ch is plenty.
        public string Description { get; set; }

        public bool IsCompleted { get; set; }
    }
}
