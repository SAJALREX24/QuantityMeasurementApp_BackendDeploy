using QuantityMeasurementAppModelLayer.Entity;

namespace QuantityMeasurementAppRepositoryLayer.Interface
{
    public interface IQuantityMeasurementRepository
    {
        void Register(UserEntity user);
        UserEntity? GetUserbyEmail(string email);
        UserEntity? GetUserById(int id);
        IEnumerable<QuantityMeasurementEntity> GetMyDatabase(int userId);
        void SaveToDatabase(QuantityMeasurementEntity quantity);
        void DeleteHistory(int userId);
        IEnumerable<QuantityMeasurementEntity> GetHistoryByOperation(int userId, string operationType);
        IEnumerable<QuantityMeasurementEntity> GetHistoryByType(int userId, string measurementType);
        object GetStats(int userId);
    }
}
