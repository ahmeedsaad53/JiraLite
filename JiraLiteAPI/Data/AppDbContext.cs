using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JiraLiteAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<WorkTask> Tasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<TaskRequest> TaskRequests { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
             : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().HasMany(u => u.ProjectUsers).WithOne(pu => pu.User).HasForeignKey(pu => pu.UserId);
            builder.Entity<ApplicationUser>().HasMany(u => u.Comments).WithOne(c => c.User).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ApplicationUser>().HasMany(u => u.Tasks).WithOne(t => t.AssignedUser).HasForeignKey(t => t.AssignedUserId).OnDelete(DeleteBehavior.SetNull);
            builder.Entity<ApplicationUser>().HasMany(u => u.ActivityLogs).WithOne(al => al.User).HasForeignKey(al => al.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ApplicationUser>().HasMany(u => u.TaskRequests).WithOne(tr => tr.User).HasForeignKey(tr => tr.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Project>().HasMany(p => p.Users).WithOne(pu => pu.Project).HasForeignKey(pu => pu.ProjectId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Project>().HasMany(t=>t.Tasks).WithOne(pu=>pu.Project).HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Cascade); 
            builder.Entity<WorkTask>().HasMany(m=>m.Comments).WithOne(t=>t.Task).HasForeignKey(t => t.TaskId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<WorkTask>().HasMany(a=>a.ActivityLogs).WithOne(t=>t.Task).HasForeignKey(t => t.TaskId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<WorkTask>().HasMany(a => a.Attachments).WithOne(t => t.Task).HasForeignKey(t => t.TaskId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<WorkTask>().HasMany(tr => tr.taskRequests).WithOne(t => t.WorkTask).HasForeignKey(t => t.TaskId).OnDelete(DeleteBehavior.Cascade);

        }




    }

    public class ApplicationUser : IdentityUser
    {
       public string FName { get; set; }
        public string LName { get; set; }
       public List<ProjectUser> ProjectUsers { get; set; }
        public List<Comment> Comments { get; set; }
        public List<WorkTask> Tasks { get; set; }
        public List<ActivityLog> ActivityLogs { get; set; }
        public List<TaskRequest> TaskRequests { get; set; }


    }
}
