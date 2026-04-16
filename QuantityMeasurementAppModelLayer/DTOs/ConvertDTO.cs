namespace QuantityMeasurementAppModelLayer.DTOs
{
    public class ConvertDTO
    {
        public QuantityDTO? QuantityOne { get; set; }
        public string TargetUnit { get; set; } = string.Empty;
    }
}
