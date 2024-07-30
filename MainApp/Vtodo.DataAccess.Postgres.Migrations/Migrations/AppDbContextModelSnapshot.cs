﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Vtodo.DataAccess.Postgres;

#nullable disable

namespace Vtodo.DataAccess.Postgres.Migrations.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Vtodo.Entities.Models.Account", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Firstname")
                        .HasColumnType("text");

                    b.Property<string>("HashedPassword")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("RegisteredAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<byte[]>("Salt")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("Surname")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.Board", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("ImageHeaderPath")
                        .HasColumnType("text");

                    b.Property<int>("PrioritySort")
                        .HasColumnType("integer");

                    b.Property<long>("ProjectId")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.ConfirmAccountUrl", b =>
                {
                    b.Property<long>("AccountId")
                        .HasColumnType("bigint")
                        .HasColumnOrder(0);

                    b.Property<string>("UrlPart")
                        .HasColumnType("text");

                    b.HasKey("AccountId", "UrlPart");

                    b.ToTable("ConfirmAccountUrls");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.Project", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.ProjectAccountsRoles", b =>
                {
                    b.Property<long>("ProjectId")
                        .HasColumnType("bigint")
                        .HasColumnOrder(0);

                    b.Property<long>("AccountId")
                        .HasColumnType("bigint")
                        .HasColumnOrder(1);

                    b.Property<int>("ProjectRole")
                        .HasColumnType("integer")
                        .HasColumnOrder(2);

                    b.HasKey("ProjectId", "AccountId", "ProjectRole");

                    b.HasIndex("AccountId");

                    b.ToTable("ProjectAccountsRoles");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.ProjectBoardFile", b =>
                {
                    b.Property<long>("ProjectId")
                        .HasColumnType("bigint")
                        .HasColumnOrder(0);

                    b.Property<long>("BoardId")
                        .HasColumnType("bigint")
                        .HasColumnOrder(1);

                    b.Property<string>("FileName")
                        .HasColumnType("text");

                    b.HasKey("ProjectId", "BoardId", "FileName");

                    b.ToTable("ProjectBoardsFiles");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.ProjectFile", b =>
                {
                    b.Property<long>("ProjectId")
                        .HasColumnType("bigint")
                        .HasColumnOrder(0);

                    b.Property<string>("FileName")
                        .HasColumnType("text");

                    b.HasKey("ProjectId", "FileName");

                    b.ToTable("ProjectFiles");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.ProjectTaskFile", b =>
                {
                    b.Property<long>("ProjectId")
                        .HasColumnType("bigint")
                        .HasColumnOrder(0);

                    b.Property<long>("TaskId")
                        .HasColumnType("bigint")
                        .HasColumnOrder(1);

                    b.Property<string>("FileName")
                        .HasColumnType("text");

                    b.HasKey("ProjectId", "TaskId", "FileName");

                    b.ToTable("ProjectTasksFiles");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.RefreshToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("AccountId")
                        .HasColumnType("bigint");

                    b.Property<string>("Device")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ExpireAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Ip")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.TaskM", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("BoardId")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ImageHeaderPath")
                        .HasColumnType("text");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("boolean");

                    b.Property<long?>("ParentTaskId")
                        .HasColumnType("bigint");

                    b.Property<int>("Priority")
                        .HasColumnType("integer");

                    b.Property<int>("PrioritySort")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("BoardId");

                    b.HasIndex("ParentTaskId");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.Board", b =>
                {
                    b.HasOne("Vtodo.Entities.Models.Project", "Project")
                        .WithMany("Boards")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.ConfirmAccountUrl", b =>
                {
                    b.HasOne("Vtodo.Entities.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.ProjectAccountsRoles", b =>
                {
                    b.HasOne("Vtodo.Entities.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Vtodo.Entities.Models.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.TaskM", b =>
                {
                    b.HasOne("Vtodo.Entities.Models.Board", "Board")
                        .WithMany("Tasks")
                        .HasForeignKey("BoardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Vtodo.Entities.Models.TaskM", "ParentTask")
                        .WithMany("ChildrenTasks")
                        .HasForeignKey("ParentTaskId");

                    b.Navigation("Board");

                    b.Navigation("ParentTask");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.Board", b =>
                {
                    b.Navigation("Tasks");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.Project", b =>
                {
                    b.Navigation("Boards");
                });

            modelBuilder.Entity("Vtodo.Entities.Models.TaskM", b =>
                {
                    b.Navigation("ChildrenTasks");
                });
#pragma warning restore 612, 618
        }
    }
}
