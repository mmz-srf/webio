﻿// <auto-generated />

#nullable disable

namespace Tpc.WebIO.Migrations
{
    using System;
    using global::WebIO.DataAccess.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    [DbContext(typeof(AppDbContext))]
    partial class DeviceContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.AdminUser", b =>
                {
                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Email");

                    b.ToTable("AdminUsers");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.ChangeLogEntryEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullInfo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Summary")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Timestamp");

                    b.ToTable("ChangeLog");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.DeviceDenormalizedProperties", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("InterfaceCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("DevicesDenormalized");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.DeviceEntity", b =>
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
                        .IsRequired()
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
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.DevicePropertyValueEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ID");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.ToTable("DeviceProperties");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.InterfaceDenormalizedProperties", b =>
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

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.InterfaceEntity", b =>
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
                        .IsRequired()
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
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId", "Name")
                        .IsUnique();

                    b.ToTable("Interfaces");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.InterfacePropertyValueEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ID");

                    b.Property<Guid>("InterfaceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("InterfaceId");

                    b.ToTable("InterfaceProperties");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.StreamEntity", b =>
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
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("InterfaceId", "Name")
                        .IsUnique();

                    b.ToTable("Streams");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.StreamPropertyValueEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("ID");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid>("StreamId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("StreamId");

                    b.ToTable("StreamProperties");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.DevicePropertyValueEntity", b =>
                {
                    b.HasOne("WebIO.DataAccess.EntityFrameworkCore.Entities.DeviceEntity", "Device")
                        .WithMany("Properties")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.InterfacePropertyValueEntity", b =>
                {
                    b.HasOne("WebIO.DataAccess.EntityFrameworkCore.Entities.InterfaceEntity", "Interface")
                        .WithMany("Properties")
                        .HasForeignKey("InterfaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Interface");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.StreamPropertyValueEntity", b =>
                {
                    b.HasOne("WebIO.DataAccess.EntityFrameworkCore.Entities.StreamEntity", "Stream")
                        .WithMany("Properties")
                        .HasForeignKey("StreamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stream");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.DeviceEntity", b =>
                {
                    b.Navigation("Properties");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.InterfaceEntity", b =>
                {
                    b.Navigation("Properties");
                });

            modelBuilder.Entity("WebIO.DataAccess.EntityFrameworkCore.Entities.StreamEntity", b =>
                {
                    b.Navigation("Properties");
                });
#pragma warning restore 612, 618
        }
    }
}
