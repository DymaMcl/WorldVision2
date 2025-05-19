using System;

public class Comment
{
    public WorldVision.Models.Comments.CommentViewModel.Content Content { get; set; }
    public DateTime CreatedDate { get; set; }
    public WorldVision.Models.Comments.CommentViewModel.Email Email { get; set; }
    public string IPAddress { get; set; }
    public bool IsApproved { get; set; }
    public WorldVision.Models.Comments.CommentViewModel.Name Name { get; set; }
    public WorldVision.Models.Comments.CommentViewModel.Subject Subject { get; set; }
    public int Id { get; internal set; }
}