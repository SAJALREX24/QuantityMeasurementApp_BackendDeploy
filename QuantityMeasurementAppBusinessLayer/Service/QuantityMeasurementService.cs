using QuantityMeasurementAppBusinessLayer.Exception;
using QuantityMeasurementAppBusinessLayer.Interface;
using QuantityMeasurementAppModelLayer.DTOs;
using QuantityMeasurementAppModelLayer.Enums;
using QuantityMeasurementAppModelLayer.Entity;
using QuantityMeasurementAppRepositoryLayer.Interface;
using QuantityMeasurementAppRepositoryLayer.Database;
using Azure.Core.Diagnostics;
using Microsoft.Identity.Client;

namespace QuantityMeasurementAppBusinessLayer.Service
{
    public class QuantityMeasurementService : IQuantityMeasurementService
    {
        private readonly IQuantityMeasurementRepository _repository;

        public QuantityMeasurementService(IQuantityMeasurementRepository repo)
        {
            _repository = repo;
        }

        public bool Compare(QuantityDTO first, QuantityDTO second,int userId)
        {
            Validate(first);
            Validate(second);
            EnsureSameCategory(first, second, "compare");

            double firstBase = ConvertToBase(first);
            double secondBase = ConvertToBase(second);

            var result = Math.Abs(firstBase - secondBase) < 0.0001;
            double Result;
            if(result) Result = 1.0;
            else Result = 2.0;

            SaveHistory(first,second,"Compare",Result,userId);

            return Math.Abs(firstBase - secondBase) < 0.0001;
        }

        public QuantityDTO Add(QuantityDTO first, QuantityDTO second,int userId)
        {
            Validate(first);
            Validate(second);
            EnsureSameCategory(first, second, "add");
            EnsureArithmeticSupported(first.Category, "Addition");

            double resultBase = ConvertToBase(first) + ConvertToBase(second);

            SaveHistory(first,second,"Addition",resultBase,userId);

            return CreateBaseResult(first.Category, resultBase);
        }

        public QuantityDTO Subtract(QuantityDTO first, QuantityDTO second,int userId)
        {
            Validate(first);
            Validate(second);
            EnsureSameCategory(first, second, "subtract");
            EnsureArithmeticSupported(first.Category, "Subtraction");

            double resultBase = ConvertToBase(first) - ConvertToBase(second);

            SaveHistory(first,second,"Subtraction",resultBase,userId);

            return CreateBaseResult(first.Category, resultBase);
        }

        public double Divide(QuantityDTO first, QuantityDTO second, int userId)
        {
            Validate(first);
            Validate(second);

            EnsureSameCategory(first, second, "divide");

            double firstBase = ConvertToBase(first);
            double secondBase = ConvertToBase(second);

            if (Math.Abs(secondBase) < 0.0001)
                throw new QuantityMeasurementException("Cannot divide by zero quantity.");

            double result = firstBase / secondBase;

            SaveHistory(first, second, "Divide", result, userId);

            return result; // ratio (unitless)
        }

        public QuantityDTO Convert(QuantityDTO source, string targetUnit,int userId)
        {
            Validate(source);

            if (string.IsNullOrWhiteSpace(targetUnit))
                throw new QuantityMeasurementException("Target unit is required.");

            double baseValue = ConvertToBase(source);
            string category = NormalizeCategory(source.Category);
            string normalizedTargetUnit = NormalizeUnit(category, targetUnit);
            double convertedValue = ConvertFromBase(category, baseValue, normalizedTargetUnit);

            var second = new QuantityDTO
            {
                Unit = targetUnit,
                Value = 0,
                Category = source.Category
            };
            
            SaveHistory(source,second,"Convert",convertedValue,userId);

            return new QuantityDTO
            {
                Value = convertedValue,
                Unit = normalizedTargetUnit,
                Category = category
            };
        }

        public IEnumerable<QuantityMeasurementEntity> GetHistory(int userId)
        {
            return _repository.GetMyDatabase(userId);
        }

        private void SaveHistory(QuantityDTO first,QuantityDTO second,string opr,double result,int userId)
        {
            // userId == 0 means guest â€” skip saving history
            if (userId == 0) return;

            _repository.SaveToDatabase(new QuantityMeasurementEntity
            {
                UserId = userId,
                Value1 = first.Value,
                Value2 = second.Value,
                Unit1 = first.Unit,
                Unit2 = second.Unit,
                Category = first.Category,
                Operation = opr,
                Result = result,
            });
        }

        private void Validate(QuantityDTO dto)
        {
            if (dto == null)
                throw new QuantityMeasurementException("Quantity cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.Category))
                throw new QuantityMeasurementException("Category is required.");

            if (string.IsNullOrWhiteSpace(dto.Unit))
                throw new QuantityMeasurementException("Unit is required.");

            NormalizeUnit(dto.Category, dto.Unit);
        }

        private void EnsureSameCategory(QuantityDTO first, QuantityDTO second, string operation)
        {
            if (!NormalizeCategory(first.Category).Equals(NormalizeCategory(second.Category), StringComparison.OrdinalIgnoreCase))
                throw new QuantityMeasurementException($"Cannot {operation} different categories.");
        }

        private void EnsureArithmeticSupported(string category, string operationName)
        {
            if (NormalizeCategory(category).Equals("Temperature", StringComparison.OrdinalIgnoreCase))
                throw new QuantityMeasurementException($"{operationName} of temperature is not supported.");
        }

        private QuantityDTO CreateBaseResult(string category, double baseValue)
        {
            string normalizedCategory = NormalizeCategory(category);

            return new QuantityDTO
            {
                Value = baseValue,
                Unit = GetBaseUnit(normalizedCategory),
                Category = normalizedCategory
            };
        }

        private double ConvertToBase(QuantityDTO dto)
        {
            string category = NormalizeCategory(dto.Category);
            string unit = NormalizeUnit(category, dto.Unit);

            return category switch
            
            {
                "Length" => Enum.Parse<LengthUnit>(unit) switch
                {
                    LengthUnit.Inch => dto.Value,
                    LengthUnit.Feet => dto.Value * 12,
                    LengthUnit.Yard => dto.Value * 36,
                    LengthUnit.Centimeter => dto.Value / 2.54,
                    _ => throw new QuantityMeasurementException("Invalid length unit.")
                },

                "Weight" => Enum.Parse<WeightUnit>(unit) switch
                {
                    WeightUnit.Gram => dto.Value,
                    WeightUnit.Kilogram => dto.Value * 1000,
                    WeightUnit.Tonne => dto.Value * 1000000,
                    _ => throw new QuantityMeasurementException("Invalid weight unit.")
                },

                "Volume" => Enum.Parse<VolumeUnit>(unit) switch
                {
                    VolumeUnit.Milliliter => dto.Value,
                    VolumeUnit.Liter => dto.Value * 1000,
                    VolumeUnit.Gallon => dto.Value * 3785.41,
                    _ => throw new QuantityMeasurementException("Invalid volume unit.")
                },

                "Temperature" => Enum.Parse<TemperatureUnit>(unit) switch
                {
                    TemperatureUnit.Celsius => dto.Value,
                    TemperatureUnit.Fahrenheit => (dto.Value - 32) * 5 / 9,
                    TemperatureUnit.Kelvin => dto.Value - 273.15,
                    _ => throw new QuantityMeasurementException("Invalid temperature unit.")
                },

                _ => throw new QuantityMeasurementException("Invalid category.")
            };
        }

        private double ConvertFromBase(string category, double baseValue, string targetUnit)
        {
            return category switch
            {
                "Length" => Enum.Parse<LengthUnit>(targetUnit) switch
                {
                    LengthUnit.Inch => baseValue,
                    LengthUnit.Feet => baseValue / 12,
                    LengthUnit.Yard => baseValue / 36,
                    LengthUnit.Centimeter => baseValue * 2.54,
                    _ => throw new QuantityMeasurementException("Invalid target length unit.")
                },

                "Weight" => Enum.Parse<WeightUnit>(targetUnit) switch
                {
                    WeightUnit.Gram => baseValue,
                    WeightUnit.Kilogram => baseValue / 1000,
                    WeightUnit.Tonne => baseValue / 1000000,
                    _ => throw new QuantityMeasurementException("Invalid target weight unit.")
                },

                "Volume" => Enum.Parse<VolumeUnit>(targetUnit) switch
                {
                    VolumeUnit.Milliliter => baseValue,
                    VolumeUnit.Liter => baseValue / 1000,
                    VolumeUnit.Gallon => baseValue / 3785.41,
                    _ => throw new QuantityMeasurementException("Invalid target volume unit.")
                },

                "Temperature" => Enum.Parse<TemperatureUnit>(targetUnit) switch
                {
                    TemperatureUnit.Celsius => baseValue,
                    TemperatureUnit.Fahrenheit => (baseValue * 9 / 5) + 32,
                    TemperatureUnit.Kelvin => baseValue + 273.15,
                    _ => throw new QuantityMeasurementException("Invalid target temperature unit.")
                },

                _ => throw new QuantityMeasurementException("Invalid category.")
            };
        }

        private string NormalizeCategory(string category)
        {
            string normalizedCategory = category.Trim().ToLower();

            return normalizedCategory switch
            {
                "length" => "Length",
                "weight" => "Weight",
                "volume" => "Volume",
                "temperature" => "Temperature",
                _ => throw new QuantityMeasurementException("Invalid category. Use Length, Weight, Volume, or Temperature.")
            };
        }

        private string NormalizeUnit(string category, string unit)
        {
            string normalizedCategory = NormalizeCategory(category);

            bool isValid = normalizedCategory switch
            {
                "Length" => Enum.TryParse(unit, true, out LengthUnit lengthUnit),
                "Weight" => Enum.TryParse(unit, true, out WeightUnit weightUnit),
                "Volume" => Enum.TryParse(unit, true, out VolumeUnit volumeUnit),
                "Temperature" => Enum.TryParse(unit, true, out TemperatureUnit temperatureUnit),
                _ => false
            };

            if (!isValid)
                throw new QuantityMeasurementException($"Invalid unit '{unit}' for category '{normalizedCategory}'.");

            return normalizedCategory switch
            {
                "Length" => Enum.Parse<LengthUnit>(unit, true).ToString(),
                "Weight" => Enum.Parse<WeightUnit>(unit, true).ToString(),
                "Volume" => Enum.Parse<VolumeUnit>(unit, true).ToString(),
                "Temperature" => Enum.Parse<TemperatureUnit>(unit, true).ToString(),
                _ => unit
            };
        }

        private string GetBaseUnit(string category)
        {
            return NormalizeCategory(category) switch
            {
                "Length" => LengthUnit.Inch.ToString(),
                "Weight" => WeightUnit.Gram.ToString(),
                "Volume" => VolumeUnit.Milliliter.ToString(),
                "Temperature" => TemperatureUnit.Celsius.ToString(),
                _ => "Unknown"
            };
        }
        public void DeleteHistory(int userId)
        {
            _repository.DeleteHistory(userId);
        }

        public IEnumerable<QuantityMeasurementEntity> GetHistoryByOperation(int userId, string operationType)
        {
            return _repository.GetHistoryByOperation(userId, operationType);
        }

        public IEnumerable<QuantityMeasurementEntity> GetHistoryByType(int userId, string measurementType)
        {
            return _repository.GetHistoryByType(userId, measurementType);
        }

        public object GetStats(int userId)
        {
            return _repository.GetStats(userId);
        }

    }
}
