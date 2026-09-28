using MealPlanner.Models;
using MealPlanner.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.Configurations
{
    public class UnitConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            builder.ToTable("Units");

            builder.HasKey(unit => unit.Id);

            builder.Property(unit => unit.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(unit => unit.Symbol)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasIndex(unit => unit.Symbol)
                .IsUnique();

            builder.Property(unit => unit.MeasurementType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(unit => unit.ConversionFactor)
                .HasPrecision(18, 6)
                .IsRequired();

            builder.HasData(
                new Unit
                {
                    Id = 1,
                    Name = "gramm",
                    Symbol = "g",
                    MeasurementType = MeasurementType.Mass,
                    ConversionFactor = 1m
                },
                new Unit
                {
                    Id = 2,
                    Name = "kilogramm",
                    Symbol = "kg",
                    MeasurementType = MeasurementType.Mass,
                    ConversionFactor = 1000m
                },
                new Unit
                {
                    Id = 3,
                    Name = "milliliter",
                    Symbol = "ml",
                    MeasurementType = MeasurementType.Volume,
                    ConversionFactor = 1m
                },
                new Unit
                {
                    Id = 4,
                    Name = "deciliter",
                    Symbol = "dl",
                    MeasurementType = MeasurementType.Volume,
                    ConversionFactor = 100m
                },
                new Unit
                {
                    Id = 5,
                    Name = "liter",
                    Symbol = "l",
                    MeasurementType = MeasurementType.Volume,
                    ConversionFactor = 1000m
                },
                new Unit
                {
                    Id = 6,
                    Name = "darab",
                    Symbol = "db",
                    MeasurementType = MeasurementType.Count,
                    ConversionFactor = 1m
                }
            );
        }
    }
}