﻿// <auto-generated />
using HN_GenerateProductDescriptionByName.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HN_GenerateProductDescriptionByName.Server.Migrations
{
    [DbContext(typeof(CustomDbContext))]
    [Migration("20240407085741_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("HN_GenerateProductDescriptionByName.Server.ProductDetails", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("CategoryDataType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("PortalCategoryId")
                        .HasColumnType("bigint");

                    b.Property<string>("PortalCategoryName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("detailId")
                        .HasColumnType("bigint");

                    b.Property<string>("detailName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("AppProductDetails");
                });
#pragma warning restore 612, 618
        }
    }
}
