﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WirelessCom.Infrastructure.Persistence;

#nullable disable

namespace WirelessCom.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ClimateDbContext))]
    partial class ClimateDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("WirelessCom.Domain.Models.Entities.RoomClimateReading", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("TEXT");

                    b.Property<double>("Humidity")
                        .HasColumnType("REAL");

                    b.Property<double>("Temperature")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("RoomClimateReadings");
                });
#pragma warning restore 612, 618
        }
    }
}
