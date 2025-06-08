using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Core.Models;

public class Visitor
{
    [Column("id")] public Guid Id { get; set; } = Guid.NewGuid();

    [Column("ip_address")]
    public string IpAddress { get; set; } = string.Empty;
    
    [Column("country")]
    public string Country { get; set; } = string.Empty;

    [Column("visit_date")]
    public DateTime VisitDate { get; set; } = DateTime.Now;
    
    [Column("page_url")]
    public string PageUrl { get; set; } = string.Empty;
}