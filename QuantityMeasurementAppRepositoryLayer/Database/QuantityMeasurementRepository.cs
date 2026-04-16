using QuantityMeasurementAppModelLayer.Entity;
using QuantityMeasurementAppRepositoryLayer.Data;
using QuantityMeasurementAppRepositoryLayer.Interface;

namespace QuantityMeasurementAppRepositoryLayer.Database;

public class QuantityMeasurementRepository : IQuantityMeasurementRepository
{
    private readonly UserDbContext _context;
    public QuantityMeasurementRepository(UserDbContext context)
    {
        _context = context;
    }

    public void Register(UserEntity user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public UserEntity? GetUserbyEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email);
    }

    public UserEntity? GetUserById(int id)
    {
        return _context.Users.FirstOrDefault(u => u.Id == id);
    }

    public void SaveToDatabase(QuantityMeasurementEntity quantity)
    {
        _context.Quantity.Add(quantity);
        _context.SaveChanges();
    }

    public IEnumerable<QuantityMeasurementEntity> GetMyDatabase(int userId)
    {
        return _context.Quantity.Where(u => u.UserId == userId).ToList();
    }

    public void DeleteHistory(int userId)
    {
        var records = _context.Quantity.Where(u => u.UserId == userId).ToList();
        _context.Quantity.RemoveRange(records);
        _context.SaveChanges();
    }

    public IEnumerable<QuantityMeasurementEntity> GetHistoryByOperation(int userId, string operationType)
    {
        return _context.Quantity
            .Where(u => u.UserId == userId && u.Operation.ToLower() == operationType.ToLower())
            .ToList();
    }

    public IEnumerable<QuantityMeasurementEntity> GetHistoryByType(int userId, string measurementType)
    {
        return _context.Quantity
            .Where(u => u.UserId == userId && u.Category.ToLower() == measurementType.ToLower())
            .ToList();
    }

    public object GetStats(int userId)
    {
        var records = _context.Quantity.Where(u => u.UserId == userId).ToList();
        return new
        {
            TotalOperations = records.Count,
            ByOperation = records.GroupBy(r => r.Operation)
                .Select(g => new { Operation = g.Key, Count = g.Count() }),
            ByCategory = records.GroupBy(r => r.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
        };
    }
}
