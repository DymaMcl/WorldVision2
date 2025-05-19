using eUseControl.BusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldVision.BusinessLogic.Implementation;

{
    public class CommentsBL : IComments
{
    // Context pentru baza de date - adaptează după structura ta
    private CommentContext _context;

    public CommentsBL()
    {
        _context = new CommentContext();
    }

    public bool AddComment(Comment comment)
    {
        try
        {
            // Validări suplimentare
            if (string.IsNullOrWhiteSpace(comment.Name) ||
                string.IsNullOrWhiteSpace(comment.Subject) ||
                string.IsNullOrWhiteSpace(comment.Content))
            {
                return false;
            }

            // Setări default
            comment.CreatedDate = DateTime.Now;
            comment.IsApproved = false; // Toate comentariile trebuie moderate

            // Anti-spam: verifică dacă IP-ul poate posta
            if (!CanPostComment(comment.IPAddress))
            {
                return false;
            }

            _context.Comments.Add(comment);
            return _context.SaveChanges() > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public List<Comment> GetAllComments()
    {
        try
        {
            return _context.Comments
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }
        catch (Exception)
        {
            return new List<Comment>();
        }
    }

    public List<Comment> GetApprovedComments()
    {
        try
        {
            return _context.Comments
                .Where(c => c.IsApproved)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }
        catch (Exception)
        {
            return new List<Comment>();
        }
    }

    public List<Comment> GetComments(int page, int pageSize, bool onlyApproved = true)
    {
        try
        {
            var query = _context.Comments.AsQueryable();

            if (onlyApproved)
            {
                query = query.Where(c => c.IsApproved);
            }

            return query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        catch (Exception)
        {
            return new List<Comment>();
        }
    }

    public Comment GetCommentById(int commentId)
    {
        try
        {
            return _context.Comments.FirstOrDefault(c => c.Id == commentId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public bool ApproveComment(int commentId)
    {
        try
        {
            var comment = GetCommentById(commentId);
            if (comment != null)
            {
                comment.IsApproved = true;
                return _context.SaveChanges() > 0;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool RejectComment(int commentId)
    {
        try
        {
            var comment = GetCommentById(commentId);
            if (comment != null)
            {
                comment.IsApproved = false;
                return _context.SaveChanges() > 0;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool DeleteComment(int commentId)
    {
        try
        {
            var comment = GetCommentById(commentId);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                return _context.SaveChanges() > 0;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool UpdateComment(Comment comment)
    {
        try
        {
            var existingComment = GetCommentById(comment.Id);
            if (existingComment != null)
            {
                existingComment.Name = comment.Name;
                existingComment.Email = comment.Email;
                existingComment.Subject = comment.Subject;
                existingComment.Content = comment.Content;
                existingComment.IsApproved = comment.IsApproved;

                return _context.SaveChanges() > 0;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public int GetTotalCommentsCount(bool onlyApproved = true)
    {
        try
        {
            if (onlyApproved)
            {
                return _context.Comments.Count(c => c.IsApproved);
            }
            return _context.Comments.Count();
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public List<Comment> GetPendingComments()
    {
        try
        {
            return _context.Comments
                .Where(c => !c.IsApproved)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }
        catch (Exception)
        {
            return new List<Comment>();
        }
    }

    public List<Comment> SearchComments(string searchTerm, bool onlyApproved = true)
    {
        try
        {
            var query = _context.Comments.AsQueryable();

            if (onlyApproved)
            {
                query = query.Where(c => c.IsApproved);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Subject.ToLower().Contains(searchTerm) ||
                    c.Content.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm));
            }

            return query
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }
        catch (Exception)
        {
            return new List<Comment>();
        }
    }

    public List<Comment> GetCommentsByDateRange(DateTime startDate, DateTime endDate, bool onlyApproved = true)
    {
        try
        {
            var query = _context.Comments.AsQueryable();

            if (onlyApproved)
            {
                query = query.Where(c => c.IsApproved);
            }

            return query
                .Where(c => c.CreatedDate >= startDate && c.CreatedDate <= endDate)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }
        catch (Exception)
        {
            return new List<Comment>();
        }
    }

    public CommentStatsViewModel GetCommentStatistics()
    {
        try
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var stats = new CommentStatsViewModel
            {
                TotalComments = _context.Comments.Count(),
                ApprovedComments = _context.Comments.Count(c => c.IsApproved),
                PendingComments = _context.Comments.Count(c => !c.IsApproved),
                TodayComments = _context.Comments.Count(c => c.CreatedDate >= today),
                ThisWeekComments = _context.Comments.Count(c => c.CreatedDate >= weekStart),
                ThisMonthComments = _context.Comments.Count(c => c.CreatedDate >= monthStart)
            };

            return stats;
        }
        catch (Exception)
        {
            return new CommentStatsViewModel();
        }
    }

    public bool CanPostComment(string ipAddress)
    {
        try
        {
            // Anti-spam: verifică câte comentarii a trimis IP-ul în ultima oră
            var oneHourAgo = DateTime.Now.AddHours(-1);
            var recentComments = _context.Comments
                .Count(c => c.IPAddress == ipAddress && c.CreatedDate >= oneHourAgo);

            // Permite maximum 3 comentarii pe oră per IP
            return recentComments < 3;
        }
        catch (Exception)
        {
            return true; // În caz de eroare, permite
        }
    }

    public List<Comment> GetCommentsByIP(string ipAddress)
    {
        try
        {
            return _context.Comments
                .Where(c => c.IPAddress == ipAddress)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }
        catch (Exception)
        {
            return new List<Comment>();
        }
    }

    public bool MarkAsSpam(int commentId)
    {
        try
        {
            // Pentru moment, marcarea ca spam înseamnă dezaprobarea
            // Poți extinde cu un câmp separat pentru spam
            return RejectComment(commentId);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public List<Comment> GetLatestComments(int count, bool onlyApproved = true)
    {
        try
        {
            var query = _context.Comments.AsQueryable();

            if (onlyApproved)
            {
                query = query.Where(c => c.IsApproved);
            }

            return query
                .OrderByDescending(c => c.CreatedDate)
                .Take(count)
                .ToList();
        }
        catch (Exception)
        {
            return new List<Comment>();
        }
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
}

// Context pentru Entity Framework (adaptează după structura ta)
namespace WorldVision.BusinessLogic.Implementation
{
    public class CommentContext : DbContext
    {
        public CommentContext() : base("DefaultConnection")
        {
        }

        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                        .Property(c => c.Name)
                        .IsRequired()
                        .HasMaxLength(100);

            modelBuilder.Entity<Comment>()
                .Property(c => c.Subject)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Comment>()
                .Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<Comment>()
                .Property(c => c.Email)
                .HasMaxLength(200);

            modelBuilder.Entity<Comment>()
                .Property(c => c.IPAddress)
                .HasMaxLength(45);
        }
    }
}
