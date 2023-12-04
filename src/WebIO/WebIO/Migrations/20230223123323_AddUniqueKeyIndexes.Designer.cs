﻿// <auto-generated />

#nullable disable

namespace Tpc.WebIO.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;

    [DbContext(typeof(AppDbContext))]
    [Migration("20230223123323_AddUniqueKeyIndexes")]
    partial class AddUniqueKeyIndexes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.ChangeLogEntryEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Summary")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Timestamp");

                    b.ToTable("ChangeLog");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.DeviceDenormalizedProperties", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("InterfaceCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("DevicesDenormalized");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.DeviceEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ID");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Creator")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("DeviceType")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("ModificationComment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime2");

                    b.Property<string>("Modifier")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.DevicePropertyValueEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ID");

                    b.Property<Guid?>("DeviceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Key")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.ToTable("DeviceProperties");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.InterfaceDenormalizedProperties", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("StreamsCountAncillaryReceive")
                        .HasColumnType("int");

                    b.Property<int>("StreamsCountAncillarySend")
                        .HasColumnType("int");

                    b.Property<int>("StreamsCountAudioReceive")
                        .HasColumnType("int");

                    b.Property<int>("StreamsCountAudioSend")
                        .HasColumnType("int");

                    b.Property<int>("StreamsCountVideoReceive")
                        .HasColumnType("int");

                    b.Property<int>("StreamsCountVideoSend")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("InterfacesDenormalized");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.InterfaceEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ID");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Creator")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<string>("InterfaceTemplate")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("ModificationComment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime2");

                    b.Property<string>("Modifier")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId", "Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("Interfaces");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.InterfacePropertyValueEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ID");

                    b.Property<Guid?>("InterfaceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Key")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("InterfaceId");

                    b.ToTable("InterfaceProperties");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.StreamEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ID");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Creator")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("Direction")
                        .HasColumnType("int");

                    b.Property<Guid>("InterfaceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ModificationComment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime2");

                    b.Property<string>("Modifier")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("InterfaceId", "Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("Streams");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.StreamPropertyValueEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ID");

                    b.Property<string>("Key")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid?>("StreamId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("StreamId");

                    b.ToTable("StreamProperties");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.DevicePropertyValueEntity", b =>
                {
                    b.HasOne("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.DeviceEntity", "Device")
                        .WithMany("Properties")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Device");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.InterfacePropertyValueEntity", b =>
                {
                    b.HasOne("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.InterfaceEntity", "Interface")
                        .WithMany("Properties")
                        .HasForeignKey("InterfaceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Interface");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.StreamPropertyValueEntity", b =>
                {
                    b.HasOne("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.StreamEntity", "Stream")
                        .WithMany("Properties")
                        .HasForeignKey("StreamId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Stream");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.DeviceEntity", b =>
                {
                    b.Navigation("Properties");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.InterfaceEntity", b =>
                {
                    b.Navigation("Properties");
                });

            modelBuilder.Entity("Tpc.WebIO.DataAccess.EntityFrameworkCore.Entities.StreamEntity", b =>
                {
                    b.Navigation("Properties");
                });
#pragma warning restore 612, 618
        }
    }
}
