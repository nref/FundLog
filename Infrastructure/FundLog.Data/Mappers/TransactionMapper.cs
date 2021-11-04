using FundLog.Data.Entities;

namespace FundLog.Data.Mappers;

public static class TransactionMapper
{
  public static Model.Transaction? Map(this Transaction? t, Institution i, Category c, CategoryGroup cg, TransactionIdMap map) => t == null ? null : new()
  {
    Id = t.Id,
    Date = t.Date,
    Name = t.Name,
    Pending = t.Pending,
    Amount = t.Amount / 100M,
    Institution = i.Map(),
    Category = c.Map(cg),
    ExternalId = map.ExternalId,
  };

  public static Model.Institution? Map(this Institution? i) => i == null ? null : new()
  {
    Id = i.Id,
    Name = i.Name
  };

  public static Model.Category? Map(this Category? i, CategoryGroup cg) => i == null ? null : new()
  {
    Id = i.Id,
    Name = i.Name,
    Group = cg.Map(),
  };

  public static Model.CategoryGroup? Map(this CategoryGroup? cg) => cg == null ? null : new()
  {
    Id = cg.Id,
    Name = cg.Name
  };

  public static Model.Allocation? Map(this Entities.Allocation a, Category cfrom, Category cto, CategoryGroup cgfrom, CategoryGroup cgto) => a == null ? null : new()
  {
    Id = a.Id,
    Name = "",
    From = cfrom.Map(cgfrom),
    To = cto.Map(cgto),
    Created = a.Created,
    ForMonth = a.ForMonth,
  };
}
