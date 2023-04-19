﻿using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(course => course.Id);
            builder.HasIndex(course => course.Id).IsUnique();
            builder.Property(course => course.Id).IsRequired();
            builder.Property(course => course.Id).ValueGeneratedOnAdd();
            builder.Property(course => course.OwnerUsername).HasMaxLength(100);
            builder.Property(course => course.Title).HasMaxLength(2 * Course.MaxTitleSize);
            builder.Property(course => course.Description).HasMaxLength(2 * Course.MaxDescriptionSize);
            builder.Property(course => course.SmartLmsUrl).HasMaxLength(2 * Course.MaxUrlSize);
            builder.Property(course => course.PldUrl).HasMaxLength(2 * Course.MaxUrlSize);
            builder.Property(course => course.Title).HasConversion<StringHexConverter>();
            builder.Property(course => course.Description).HasConversion<StringHexConverter>();
            builder.Property(course => course.SmartLmsUrl).HasConversion<StringHexConverter>();
            builder.Property(course => course.PldUrl).HasConversion<StringHexConverter>();
            builder.ToTable("Course");
        }
    }
}